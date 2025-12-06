using System;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Data;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Extractors
{
    public sealed class ImageExtractor
    {
        private readonly SoftwareBitmap embeddedImage;

        public ImageExtractor(SoftwareBitmap embeddedImage)
        {
            this.embeddedImage =
                ImageManager.ConvertToCorrectFormat(embeddedImage ??
                                                    throw new ArgumentNullException(nameof(embeddedImage)));
        }

        public bool HasEmbeddedMessage()
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
            return validateImageNotEmpty(pixels) && isMarkerPixel(pixels[0]);
        }


        public SoftwareBitmap ExtractMessageBitmap()
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
            if (!validateImageNotEmpty(pixels))
            {
                throw new InvalidOperationException("Image is empty.");
            }

            if (!isMarkerPixel(pixels[0]))
            {
                throw new InvalidOperationException("No embedded message found (marker pixel missing).");
            }

            var width = this.embeddedImage.PixelWidth;
            var height = this.embeddedImage.PixelHeight;

            var resultPixels = new PixelL1[pixels.Length];
            for (var i = 0; i < pixels.Length; i++)
            {
                var bit = extractBitFromBlue(pixels[i].Blue);
                resultPixels[i] = createPixelFromBit(bit);
            }

            return PixelL1.ToSoftwareBitmap(resultPixels, width, height);
        }

        private static bool validateImageNotEmpty(PixelBgr8[] pixels)
        {
            return pixels != null && pixels.Length > 0;
        }

        private static bool isMarkerPixel(PixelBgr8 pixel)
        {
            return pixel.Red == StegoConstants.MarkerValue &&
                   pixel.Green == StegoConstants.MarkerValue &&
                   pixel.Blue == StegoConstants.MarkerValue;
        }

        private static byte extractBitFromBlue(byte blueChannel)
        {
            return (byte)(blueChannel & StegoConstants.LsbMask);
        }

        private static PixelL1 createPixelFromBit(byte bit)
        {
            return new PixelL1(bit);
        }
    }
}
