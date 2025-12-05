

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Model
{
        public sealed class TextEmbedder
        {
            private readonly SoftwareBitmap sourceImage;
            private readonly string messageText;
            private readonly byte bpcc;
            private readonly bool encryptionUsed;

            public TextEmbedder(string messageText, SoftwareBitmap sourceImage, byte bitsPerChannel = 1, bool encryptionUsed = false)
            {
                this.messageText = messageText ?? throw new ArgumentNullException(nameof(messageText));
                this.sourceImage = ImageManager.ConvertToCorrectFormat(sourceImage ?? throw new ArgumentNullException(nameof(sourceImage)));

                if (bitsPerChannel < 1 || bitsPerChannel > 8)
                    throw new ArgumentOutOfRangeException(nameof(bitsPerChannel));

                this.bpcc = bitsPerChannel;
                this.encryptionUsed = encryptionUsed;
            }

            public SoftwareBitmap EmbedMessage()
            {
                // Clean text: only letters, uppercase
                var cleanedLetters = this.messageText.ToUpperInvariant().Where(char.IsLetter).ToArray();
                var symbols = new List<int>(cleanedLetters.Length + 1);
                foreach (var c in cleanedLetters) symbols.Add(c - 'A' + 1);
                symbols.Add(0); // terminator

                // Convert to bitstream (5 bits per symbol)
                var bitStream = new List<int>();
                foreach (var sym in symbols)
                    for (int bit = 4; bit >= 0; bit--)
                        bitStream.Add((sym >> bit) & 1);

                var srcPixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
                long usablePix = srcPixels.Length - 2;
                long capacity = usablePix * (this.bpcc * 3);

                if (bitStream.Count > capacity)
                {
                    var neededBpcc = Math.Ceiling((double)bitStream.Count / (usablePix * 3));
                    throw new ArgumentException(
                        $"Message too large. Needs {bitStream.Count} bits but only {capacity} available. " +
                        $"Minimum BPCC required: {neededBpcc}.");
                }

                // Pixel 0 marker
                srcPixels[0].Red = 123;
                srcPixels[0].Green = 123;
                srcPixels[0].Blue = 123;

                // Pixel 1 header
                srcPixels[1].Red = (byte)((srcPixels[1].Red & 0xFE) | (this.encryptionUsed ? 1 : 0));
                srcPixels[1].Green = this.bpcc;
                srcPixels[1].Blue = (byte)((srcPixels[1].Blue & 0xFE) | 1); // 1 = text

                int bitIndex = 0;
                for (int i = 2; i < srcPixels.Length && bitIndex < bitStream.Count; i++)
                {
                    srcPixels[i].Red = embedBitsIntoChannel(srcPixels[i].Red, ref bitIndex, bitStream);
                    if (bitIndex >= bitStream.Count) break;

                    srcPixels[i].Green = embedBitsIntoChannel(srcPixels[i].Green, ref bitIndex, bitStream);
                    if (bitIndex >= bitStream.Count) break;

                    srcPixels[i].Blue = embedBitsIntoChannel(srcPixels[i].Blue, ref bitIndex, bitStream);
                }

                var output = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                    this.sourceImage.PixelWidth,
                    this.sourceImage.PixelHeight,
                    BitmapAlphaMode.Premultiplied);

                PixelBgr8.WriteToSoftwareBitmap(srcPixels, output);
                return output;
            }

            // Helper: embed bpcc bits MSB-first into channel
            private byte embedBitsIntoChannel(byte channel, ref int bitIndex, List<int> bitStream)
            {
                byte result = channel;
                for (int i = this.bpcc - 1; i >= 0 && bitIndex < bitStream.Count; i--)
                {
                    int bit = bitStream[bitIndex++];
                    result = (byte)((result & ~(1 << i)) | (bit << i));
                }
                return result;
            }
        }
    }


