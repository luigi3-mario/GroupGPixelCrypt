using System;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Model
{
    public sealed class ImageExtractor
    {
        private readonly SoftwareBitmap embeddedImage;

        public ImageExtractor(SoftwareBitmap embeddedImage)
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

        public SoftwareBitmap ExtractMessageBitmap()
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
            if (pixels.Length == 0)
                throw new InvalidOperationException("Image is empty.");

            var marker = pixels[0];
            if (marker.Red != 123 || marker.Green != 123 || marker.Blue != 123)
                throw new InvalidOperationException("No embedded message found (marker pixel missing).");

            var width = this.embeddedImage.PixelWidth;
            var height = this.embeddedImage.PixelHeight;

            var resultPixels = new PixelL1[pixels.Length];
            for (var i = 0; i < pixels.Length; i++)
            {
                var bit = (byte)(pixels[i].Blue & 0x01);
                resultPixels[i] = new PixelL1(bit);
            }

            return PixelL1.ToSoftwareBitmap(resultPixels, width, height);
        }
    }
}