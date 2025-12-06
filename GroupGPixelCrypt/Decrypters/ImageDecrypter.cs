using System;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Decrypters
{
    public static class ImageDecrypter
    {
        #region Methods

        public static PixelBgr8[] DecryptQuadrants(PixelBgr8[] source, int width, int height)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Invalid dimensions.");

            }

            if (source.Length != width * height)
            {
                throw new ArgumentException("Pixel array size mismatch.");
            }

            var destination = new PixelBgr8[source.Length];

            var halfWidth = width / 2;
            var halfHeight = height / 2;

            int topLeftX0 = 0, topLeftX1 = halfWidth;
            int topLeftY0 = 0, topLeftY1 = halfHeight;

            int topRightX0 = halfWidth, topRightX1 = width;
            int topRightY0 = 0, topRightY1 = halfHeight;

            int bottomLeftX0 = 0, bottomLeftX1 = halfWidth;
            int bottomLeftY0 = halfHeight, bottomLeftY1 = height;

            int bottomRightX0 = halfWidth, bottomRightX1 = width;
            int bottomRightY0 = halfHeight, bottomRightY1 = height;

            copyRegion(source, destination, bottomRightX0, bottomRightY0, topLeftX0, topLeftY0,
                bottomRightX1 - bottomRightX0, bottomRightY1 - bottomRightY0, width);
            copyRegion(source, destination, topLeftX0, topLeftY0, bottomRightX0, bottomRightY0,
                topLeftX1, topLeftY1, width);
            copyRegion(source, destination, bottomLeftX0, bottomLeftY0, topRightX0, topRightY0,
                bottomLeftX1, bottomLeftY1 - bottomLeftY0, width);
            copyRegion(source, destination, topRightX0, topRightY0, bottomLeftX0, bottomLeftY0,
                topRightX1 - topRightX0, topRightY1, width);

            return destination;
        }

        private static void copyRegion(
            PixelBgr8[] source,
            PixelBgr8[] destination,
            int sourceX, int sourceY,
            int destinationX, int destinationY,
            int regionWidth, int regionHeight,
            int stride)
        {
            for (var y = 0; y < regionHeight; y++)
            {
                var sourceRow = (sourceY + y) * stride;
                var destinationRow = (destinationY + y) * stride;

                for (var x = 0; x < regionWidth; x++)
                {
                    var sourceIndex = sourceRow + sourceX + x;
                    var destinationIndex = destinationRow + destinationX + x;
                    destination[destinationIndex] = source[sourceIndex];
                }
            }
        }

        #endregion
    }
}