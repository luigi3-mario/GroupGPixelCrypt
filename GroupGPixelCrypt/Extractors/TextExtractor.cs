using System;
using System.Collections.Generic;
using System.Text;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Model
{
    public sealed class TextExtractor
    {
        private readonly SoftwareBitmap embeddedImage;

        public TextExtractor(SoftwareBitmap embeddedImage)
        {
            this.embeddedImage = ImageManager.ConvertToCorrectFormat(embeddedImage ?? throw new ArgumentNullException(nameof(embeddedImage)));
        }

        public bool HasEmbeddedMessage()
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
            if (pixels.Length == 0) return false;

            var marker = pixels[0];
            return marker.Red == 123 && marker.Green == 123 && marker.Blue == 123;
        }

        public string ExtractMessageText()
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
            if (pixels.Length == 0)
                throw new InvalidOperationException("Image is empty.");

            var marker = pixels[0];
            if (marker.Red != 123 || marker.Green != 123 || marker.Blue != 123)
                throw new InvalidOperationException("No embedded message found (marker pixel missing).");

            // Header (pixel 1):
            var bpcc = Math.Max(1, Math.Min(8, (int)pixels[1].Green));
            var isText = (pixels[1].Blue & 0x01) == 1;
            if (!isText)
                throw new InvalidOperationException("Header indicates embedded message is not text.");

            var bitStream = new List<int>(bpcc * 3 * (pixels.Length - 2));
            for (var pix = 2; pix < pixels.Length; pix++)
            {
                AppendLowerBitsMsbFirst(pixels[pix].Red, bpcc, bitStream);
                AppendLowerBitsMsbFirst(pixels[pix].Green, bpcc, bitStream);
                AppendLowerBitsMsbFirst(pixels[pix].Blue, bpcc, bitStream);
            }

            var sb = new StringBuilder();
            int bitPos = 0;
            while (bitPos + 5 <= bitStream.Count)
            {
                int sym = 0;
                for (int i = 0; i < 5; i++)
                    sym = (sym << 1) | (bitStream[bitPos++] & 1);

                if (sym == 0) break; // terminator
                if (sym >= 1 && sym <= 26)
                    sb.Append((char)('A' + sym - 1));
            }

            return sb.ToString();
        }

        private static void AppendLowerBitsMsbFirst(byte channel, int bpcc, List<int> bitStream)
        {
            for (int i = bpcc - 1; i >= 0; i--)
            {
                int bit = (channel >> i) & 1;
                bitStream.Add(bit);
            }
        }
    }
}
