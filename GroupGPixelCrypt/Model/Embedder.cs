using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Model
{
    public sealed class Embedder
    {
        #region Data members

        private readonly SoftwareBitmap sourceImage;
        private readonly SoftwareBitmap messageImage; // only used in image mode
        private readonly string messageText; // only used in text mode
        private readonly Mode mode;
        private readonly byte bpcc; // bits per color channel (1-8) for text mode
        private readonly bool encryptionUsed; // 0/1 in header (encryption not implemented here)

        #endregion

        #region Constructors

        // Image embedding constructor (monochrome message image)
        public Embedder(SoftwareBitmap messageImage, SoftwareBitmap sourceImage)
        {
            if (messageImage == null)
            {
                throw new ArgumentNullException(nameof(messageImage));
            }

            if (sourceImage == null)
            {
                throw new ArgumentNullException(nameof(sourceImage));
            }

            this.messageImage = ImageManager.ConvertToCorrectFormat(messageImage);
            this.sourceImage = ImageManager.ConvertToCorrectFormat(sourceImage);

            // Requirement: message image must not exceed source dimensions
            if (this.messageImage.PixelWidth > this.sourceImage.PixelWidth ||
                this.messageImage.PixelHeight > this.sourceImage.PixelHeight)
            {
                throw new ArgumentException(
                    "Message image file exceeds the dimensions of the source image. Cannot embed.");
            }

            this.mode = Mode.Image;
            this.bpcc = 1; // unused for image mode, but set default
            this.encryptionUsed = false;
            this.messageText = null;
        }

        // Text embedding constructor
        public Embedder(string messageText, SoftwareBitmap sourceImage, byte bitsPerChannel = 1,
            bool encryptionUsed = false)
        {
            if (messageText == null)
            {
                throw new ArgumentNullException(nameof(messageText));
            }

            if (sourceImage == null)
            {
                throw new ArgumentNullException(nameof(sourceImage));
            }

            if (bitsPerChannel < 1 || bitsPerChannel > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerChannel));
            }

            this.messageText = messageText;
            this.sourceImage = ImageManager.ConvertToCorrectFormat(sourceImage);
            this.bpcc = bitsPerChannel;
            this.encryptionUsed = encryptionUsed;
            this.mode = Mode.Text;
            this.messageImage = null;
        }

        #endregion

        #region Methods

        #region Public API

        /// <summary>
        ///     Embed the message (image or text) into the source image and return the resulting SoftwareBitmap.
        /// </summary>
        public SoftwareBitmap EmbedMessage()
        {
            return this.mode == Mode.Image ? this.EmbedImageMessage() : this.EmbedTextMessage();
        }

        #endregion

        #region Private helpers - Image embedding (RESTORED TO ORIGINAL MAPPING)

        private SoftwareBitmap EmbedImageMessage()
        {
            // Convert pixel arrays
            var srcPixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
            var msgPixels = PixelL1.FromSoftwareBitmap(this.messageImage);

            var srcWidth = this.sourceImage.PixelWidth;
            var srcHeight = this.sourceImage.PixelHeight;

            var msgWidth = this.messageImage.PixelWidth;
            var msgHeight = this.messageImage.PixelHeight;

            var srcTotal = srcPixels.Length;
            var msgTotal = msgPixels.Length;

            // Ensure message fits within dimensions (we already checked in ctor)
            if (msgWidth > srcWidth || msgHeight > srcHeight)
            {
                throw new ArgumentException(
                    "Message image file exceeds the dimensions of the source image. Cannot embed.");
            }

            // Pixel 0 marker (123,123,123)
            srcPixels[0].Red = 123;
            srcPixels[0].Green = 123;
            srcPixels[0].Blue = 123;
            srcPixels[0].Alpha = 255;

            // NOTE: For IMAGE mode we do NOT reserve pixel 1 for header so that the
            // message image maps to the same (x,y) coordinates as the source (backwards-compatible).
            // This preserves the expected visual placement and works with your extractor that reads LSB(blue)
            // from every pixel. (Text mode still writes header into pixel1.)

            // Embed message bits by matching message (x,y) to source (x,y).
            // This mirrors the original working implementation and keeps the message aligned.
            for (var y = 0; y < msgHeight; y++)
            {
                for (var x = 0; x < msgWidth; x++)
                {
                    var srcIndex = y * srcWidth + x;

                    // preserve pixel 0 marker
                    if (srcIndex == 0)
                    {
                        // this would only happen for x==0 && y==0; skip it (marker already set)
                        continue;
                    }

                    var msgIndex = y * msgWidth + x;
                    var blue = srcPixels[srcIndex].Blue;
                    var msgBit = msgPixels[msgIndex].Luma; // 0 or 1

                    // write message bit into LSB of blue channel (only change blue LSB)
                    srcPixels[srcIndex].Blue = (byte)((blue & 0xFE) | (msgBit & 1));
                }
            }

            // Create output SoftwareBitmap and write pixel data
            var result = new SoftwareBitmap(BitmapPixelFormat.Bgra8, this.sourceImage.PixelWidth,
                this.sourceImage.PixelHeight, BitmapAlphaMode.Premultiplied);

            PixelBgr8.WriteToSoftwareBitmap(srcPixels, result);
            return result;
        }

        #endregion

        #endregion

        #region Private helpers - Text embedding (unchanged)

        private SoftwareBitmap EmbedTextMessage()
        {
            // Prepare text: keep original for display, but encoding only alphabetic characters
            var cleanedLetters = this.messageText
                .ToUpperInvariant()
                .Where(char.IsLetter)
                .ToArray();

            // Build symbol list: A=1 ... Z=26. Append end-of-message marker value 0.
            var symbols = new List<int>(cleanedLetters.Length + 1);
            foreach (var c in cleanedLetters)
            {
                symbols.Add(c - 'A' + 1); // 1..26
            }

            symbols.Add(0); // end-of-message marker

            // Convert each symbol (0..26) into 5 bits (msb first)
            var bitStream = new List<int>(symbols.Count * 5);
            foreach (var sym in symbols)
            {
                for (var bit = 4; bit >= 0; bit--)
                {
                    bitStream.Add((sym >> bit) & 1);
                }
            }

            // Pixel arrays
            var srcPixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
            var totalPixels = srcPixels.Length;

            // Capacity check: usable pixels = totalPixels - 2 (header)
            long usablePixels = totalPixels - 2;
            var capacityBits = usablePixels * (this.bpcc * 3); // R,G,B channels

            if (bitStream.Count > capacityBits)
            {
                // compute minimum bpcc required (ceil)
                var neededBpccPerChannel = Math.Ceiling((double)bitStream.Count / (usablePixels * 3));
                throw new ArgumentException(
                    $"Message too large. Needs {bitStream.Count} bits but only {capacityBits} available.");
            }

            // Write header:
            // Pixel 0 = (123,123,123)
            srcPixels[0].Red = 123;
            srcPixels[0].Green = 123;
            srcPixels[0].Blue = 123;
            srcPixels[0].Alpha = 255;

            // Pixel 1:
            // - Red LSB = encryption flag
            // - Green = BPCC (1..8)
            // - Blue LSB = 1 (text)
            {
                var r = srcPixels[1].Red;
                var g = srcPixels[1].Green;
                var b = srcPixels[1].Blue;

                r = (byte)((r & 0xFE) | (this.encryptionUsed ? 1 : 0));
                g = this.bpcc; // store bpcc value directly in green
                b = (byte)((b & 0xFE) | 1); // 1 = text

                srcPixels[1].Red = r;
                srcPixels[1].Green = g;
                srcPixels[1].Blue = b;
            }

            // Masks from helper
            var upperMask = Mask.SetMaskForUpper(this.bpcc); // keeps upper bits, zeroes lower bpcc bits

            // Now embed the bitStream into pixels sequentially starting at pixel index 2.
            var bitIndex = 0;
            for (var i = 2; i < totalPixels && bitIndex < bitStream.Count; i++)
            {
                // For each channel (Red, Green, Blue) in that order, take up to bpcc bits and write them
                srcPixels[i].Red = this.EmbedBitsIntoChannel(srcPixels[i].Red, ref bitIndex, bitStream, upperMask);
                if (bitIndex >= bitStream.Count)
                {
                    break;
                }

                srcPixels[i].Green = this.EmbedBitsIntoChannel(srcPixels[i].Green, ref bitIndex, bitStream, upperMask);
                if (bitIndex >= bitStream.Count)
                {
                    break;
                }

                srcPixels[i].Blue = this.EmbedBitsIntoChannel(srcPixels[i].Blue, ref bitIndex, bitStream, upperMask);
            }

            // Create output SoftwareBitmap and write pixel data
            var output = new SoftwareBitmap(BitmapPixelFormat.Bgra8, this.sourceImage.PixelWidth,
                this.sourceImage.PixelHeight, BitmapAlphaMode.Premultiplied);

            PixelBgr8.WriteToSoftwareBitmap(srcPixels, output);
            return output;
        }

        /// <summary>
        ///     Embeds up to bpcc bits into the channel byte using provided mask for upper bits.
        ///     Returns the modified channel value.
        /// </summary>
        private byte EmbedBitsIntoChannel(byte channel, ref int bitIndex, List<int> bitStream, byte upperMask)
        {
            // Clear out the lower bpcc bits first
            var cleared = (byte)(channel & upperMask);

            // Build the new lower-bit value
            var value = 0;
            var bitsFilled = 0;
            for (var i = 0; i < this.bpcc && bitIndex < bitStream.Count; i++)
            {
                // We will store bits in LSB positions in the order they appear
                var bit = bitStream[bitIndex++] & 1;
                value |= bit << (this.bpcc - 1 - i); // accumulate MSB-first in this chunk
                bitsFilled++;
            }

            if (bitsFilled == 0)
            {
                return channel;
            }

            // Align the accumulated bits into the lower positions:
            // value currently is in high-aligned inside bpcc bits; shift right to LSB positions
            if (bitsFilled < this.bpcc)
            {
                var shift = this.bpcc - bitsFilled;
                value = value >> shift;
            }

            var maskLower = (1 << this.bpcc) - 1;
            var finalLower = value & maskLower;

            var result = (byte)(cleared | finalLower);
            return result;
        }

        #endregion
    }
}