using System;
using System.Diagnostics;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Encrypters
{
    public static class ImageEncrypter
    {
        // Monochrome path (kept for compatibility)
        public static PixelL1[] EncryptQuadrants(PixelL1[] pixels, int width, int height)
        {
            if (pixels == null) throw new ArgumentNullException(nameof(pixels));
            if (pixels.Length != width * height) throw new ArgumentException("Pixel array size does not match dimensions.");

            var result = new PixelL1[pixels.Length];
            int halfW = width / 2;
            int halfH = height / 2;

            Debug.WriteLine($"[EncryptQuadrants:L1] width={width}, height={height}, halfW={halfW}, halfH={halfH}");

            int countAssigned = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = y * width + x;
                    int newX = x, newY = y;

                    bool top = y < halfH;
                    bool left = x < halfW;

                    if (top && left) { newX = x + halfW; newY = y + halfH; }
                    else if (top && !left) { newX = x - halfW; newY = y + halfH; }
                    else if (!top && left) { newX = x + halfW; newY = y - halfH; }
                    else { newX = x - halfW; newY = y - halfH; }

                    int dstIndex = newY * width + newX;
                    result[dstIndex] = pixels[srcIndex];
                    countAssigned++;
                }
            }

            Debug.WriteLine($"[EncryptQuadrants:L1] Assigned {countAssigned} of {pixels.Length} pixels.");
            return result;
        }

        // Color path (use this for full-color encryption)
        public static PixelBgr8[] EncryptQuadrants(PixelBgr8[] pixels, int width, int height)
        {
            if (pixels == null) throw new ArgumentNullException(nameof(pixels));
            if (pixels.Length != width * height) throw new ArgumentException("Pixel array size does not match dimensions.");

            var result = new PixelBgr8[pixels.Length];
            int halfW = width / 2;
            int halfH = height / 2;

            Debug.WriteLine($"[EncryptQuadrants:BGR8] width={width}, height={height}, halfW={halfW}, halfH={halfH}");

            int countAssigned = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = y * width + x;
                    int newX = x, newY = y;

                    bool top = y < halfH;
                    bool left = x < halfW;

                    if (top && left) { newX = x + halfW; newY = y + halfH; }
                    else if (top && !left) { newX = x - halfW; newY = y + halfH; }
                    else if (!top && left) { newX = x + halfW; newY = y - halfH; }
                    else { newX = x - halfW; newY = y - halfH; }

                    int dstIndex = newY * width + newX;
                    result[dstIndex] = pixels[srcIndex];
                    countAssigned++;
                }
            }

            Debug.WriteLine($"[EncryptQuadrants:BGR8] Assigned {countAssigned} of {pixels.Length} pixels.");
            return result;
        }
    }
}
