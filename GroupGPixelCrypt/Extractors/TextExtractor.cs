using System;
using System.Collections.Generic;
using System.Text;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Data;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Extractors
{
    public sealed class TextExtractor
    {
        #region Data members

        private readonly SoftwareBitmap embeddedImage;

        #endregion

        #region Constructors

        public TextExtractor(SoftwareBitmap embeddedImage)
        {
            this.embeddedImage = ImageManager.ConvertToCorrectFormat(
                embeddedImage ?? throw new ArgumentNullException(nameof(embeddedImage)));
        }

        #endregion

        #region Methods

        public bool HasEmbeddedMessage()
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
            if (pixels.Length == 0)
            {
                return false;
            }

            var marker = pixels[0];
            return marker.Red == StegoConstants.MarkerValue &&
                   marker.Green == StegoConstants.MarkerValue &&
                   marker.Blue == StegoConstants.MarkerValue;
        }

        public string ExtractMessageText()
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
            if (pixels.Length == 0)
            {
                throw new InvalidOperationException("Image is empty.");
            }

            var marker = pixels[0];
            if (marker.Red != StegoConstants.MarkerValue ||
                marker.Green != StegoConstants.MarkerValue ||
                marker.Blue != StegoConstants.MarkerValue)
            {
                throw new InvalidOperationException("No embedded message found (marker pixel missing).");
            }

            var bpcc = Math.Max(StegoConstants.MinBitsPerChannel,
                Math.Min(StegoConstants.MaxBitsPerChannel, (int)pixels[1].Green));

            var isText = (pixels[1].Blue & StegoConstants.LsbMask) == 1;
            if (!isText)
            {
                throw new InvalidOperationException("Header indicates embedded message is not text.");
            }

            var bitStream = new List<int>(bpcc * StegoConstants.ChannelsPerPixel *
                                          (pixels.Length - StegoConstants.ReservedHeaderPixels));

            for (var pix = StegoConstants.ReservedHeaderPixels; pix < pixels.Length; pix++)
            {
                appendLowerBitsMsbFirst(pixels[pix].Red, bpcc, bitStream);
                appendLowerBitsMsbFirst(pixels[pix].Green, bpcc, bitStream);
                appendLowerBitsMsbFirst(pixels[pix].Blue, bpcc, bitStream);
            }

            var stringBuilder = new StringBuilder();
            var bitstreamPosition = 0;
            while (bitstreamPosition + StegoConstants.SymbolBitLength <= bitStream.Count)
            {
                var sym = 0;
                for (var i = 0; i < StegoConstants.SymbolBitLength; i++)
                {
                    sym = (sym << 1) | (bitStream[bitstreamPosition++] & 1);
                }

                if (sym == StegoConstants.TerminatorSymbol)
                {
                    break;
                }

                if (sym >= 1 && sym <= StegoConstants.MaxLetterSymbol)
                {
                    stringBuilder.Append((char)(StegoConstants.FirstLetter + sym - 1));
                }
            }

            return stringBuilder.ToString();
        }

        private static void appendLowerBitsMsbFirst(byte channel, int bpcc, List<int> bitStream)
        {
            for (var i = bpcc - 1; i >= 0; i--)
            {
                var bit = (channel >> i) & 1;
                bitStream.Add(bit);
            }
        }

        #endregion
    }
}