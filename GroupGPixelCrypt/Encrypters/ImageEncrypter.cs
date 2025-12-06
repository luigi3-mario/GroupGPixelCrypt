using System;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Encrypters
{
    public static class ImageEncrypter
    {
        #region Methods

        public static SoftwareBitmap SwapQuadrants(SoftwareBitmap source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var pixels = PixelBgr8.FromSoftwareBitmap(source);
            var width = source.PixelWidth;
            var height = source.PixelHeight;

            var encryptedPixels = EncryptQuadrants(pixels, width, height);

            var output = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
            PixelBgr8.WriteToSoftwareBitmap(encryptedPixels, output);
            return output;
        }

        public static PixelBgr8[] EncryptQuadrants(PixelBgr8[] pixels, int width, int height)
        {
            validatePixels(pixels, width, height);

            var result = new PixelBgr8[pixels.Length];
            var halfWidth = width / 2;
            var halfHeight = height / 2;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var srcIndex = getIndex(x, y, width);

                    int newX, newY;
                    mapCoordinates(x, y, halfWidth, halfHeight, out newX, out newY);

                    var dstIndex = getIndex(newX, newY, width);
                    result[dstIndex] = pixels[srcIndex];
                }
            }

            return result;
        }

        private static void validatePixels(PixelBgr8[] pixels, int width, int height)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (pixels.Length != width * height)
            {
                throw new ArgumentException("Pixel array size does not match dimensions.");
            }
        }

        private static int getIndex(int x, int y, int stride)
        {
            return y * stride + x;
        }

        private static void mapCoordinates(int x, int y, int halfWidth, int halfHeight, out int newX, out int newY)
        {
            var isTopHalf = y < halfHeight;
            var isLeftHalf = x < halfWidth;

            if (isTopHalf && isLeftHalf)
            {
                newX = x + halfWidth;
                newY = y + halfHeight;
            }
            else if (isTopHalf)
            {
                newX = x - halfWidth;
                newY = y + halfHeight;
            }
            else if (isLeftHalf)
            {
                newX = x + halfWidth;
                newY = y - halfHeight;
            }
            else
            {
                newX = x - halfWidth;
                newY = y - halfHeight;
            }
        }

        #endregion
    }
}