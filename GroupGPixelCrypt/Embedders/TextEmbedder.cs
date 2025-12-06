using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Data;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Embedders
{
    public sealed class TextEmbedder
    {
        #region Data members

        private readonly SoftwareBitmap sourceImage;
        private readonly string messageText;
        private readonly byte bpcc;
        private readonly bool encryptionUsed;

        #endregion

        #region Constructors

        public TextEmbedder(string messageText, SoftwareBitmap sourceImage,
            byte bitsPerChannel = StegoConstants.MinBitsPerChannel,
            bool encryptionUsed = false)
        {
            this.messageText = messageText ?? throw new ArgumentNullException(nameof(messageText));
            this.sourceImage =
                ImageManager.ConvertToCorrectFormat(sourceImage ??
                                                    throw new ArgumentNullException(nameof(sourceImage)));

            if (bitsPerChannel < StegoConstants.MinBitsPerChannel || bitsPerChannel > StegoConstants.MaxBitsPerChannel)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerChannel));
            }

            this.bpcc = bitsPerChannel;
            this.encryptionUsed = encryptionUsed;
        }

        #endregion

        #region Methods

        public SoftwareBitmap EmbedMessage()
        {
            var cleanedLetters = cleanMessageText(this.messageText);
            var symbols = convertToSymbols(cleanedLetters);
            var bitStream = buildBitStream(symbols);

            var srcPixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
            var capacity = calculateCapacity(srcPixels.Length, this.bpcc);

            if (bitStream.Count > capacity)
            {
                var neededBpcc = Math.Ceiling((double)bitStream.Count /
                                              ((srcPixels.Length - StegoConstants.ReservedHeaderPixels) *
                                               StegoConstants.ChannelsPerPixel));
                throw new ArgumentException(
                    $"Message too large. Needs {bitStream.Count} bits but only {capacity} available. " +
                    $"Minimum BPCC required: {neededBpcc}.");
            }

            HeaderManager.WriteHeader(srcPixels, true, this.bpcc, this.encryptionUsed);

            var bitIndex = 0;
            for (var i = StegoConstants.ReservedHeaderPixels; i < srcPixels.Length && bitIndex < bitStream.Count; i++)
            {
                srcPixels[i].Red = this.embedBitsIntoChannel(srcPixels[i].Red, ref bitIndex, bitStream);
                if (bitIndex >= bitStream.Count)
                {
                    break;
                }

                srcPixels[i].Green = this.embedBitsIntoChannel(srcPixels[i].Green, ref bitIndex, bitStream);
                if (bitIndex >= bitStream.Count)
                {
                    break;
                }

                srcPixels[i].Blue = this.embedBitsIntoChannel(srcPixels[i].Blue, ref bitIndex, bitStream);
            }

            var output = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                this.sourceImage.PixelWidth,
                this.sourceImage.PixelHeight,
                BitmapAlphaMode.Premultiplied);

            PixelBgr8.WriteToSoftwareBitmap(srcPixels, output);
            return output;
        }

        private static char[] cleanMessageText(string text)
        {
            return text.ToUpperInvariant().Where(char.IsLetter).ToArray();
        }

        private static List<int> convertToSymbols(char[] letters)
        {
            var symbols = new List<int>(letters.Length + 1);
            foreach (var c in letters)
            {
                symbols.Add(c - StegoConstants.FirstLetter + 1);
            }

            symbols.Add(StegoConstants.TerminatorSymbol);
            return symbols;
        }

        private static List<int> buildBitStream(List<int> symbols)
        {
            var bits = new List<int>();
            foreach (var sym in symbols)
            {
                for (var bit = StegoConstants.SymbolBitLength - 1; bit >= 0; bit--)
                {
                    bits.Add((sym >> bit) & 1);
                }
            }

            return bits;
        }

        private static long calculateCapacity(long pixelCount, byte bpcc)
        {
            var usablePix = pixelCount - StegoConstants.ReservedHeaderPixels;
            return usablePix * (bpcc * StegoConstants.ChannelsPerPixel);
        }

        private byte embedBitsIntoChannel(byte channel, ref int bitIndex, List<int> bitStream)
        {
            var result = channel;
            for (var i = this.bpcc - 1; i >= 0 && bitIndex < bitStream.Count; i--)
            {
                var bit = bitStream[bitIndex++];
                result = (byte)((result & ~(1 << i)) | (bit << i));
            }

            return result;
        }

        #endregion
    }
}