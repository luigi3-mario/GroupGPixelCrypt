using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace GroupGPixelCrypt.Model.image
{
    public struct PixelBgr8
    {
        public byte Blue { get; set; }
        public byte Green { get; set; }
        public byte Red { get; set; }
        public byte Alpha { get; set; }

        public PixelBgr8(byte blue, byte green, byte red, byte alpha)
        {
            Blue = blue;
            Green = green;
            Red = red;
            Alpha = alpha;
        }

        public static PixelBgr8[] FromSoftwareBitmap(SoftwareBitmap source)
        {
            Debug.WriteLine($"[PixelBgr8] Entered FromSoftwareBitmap. Source={(source == null ? "NULL" : "OK")}");

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                source.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
            {
                Debug.WriteLine("[PixelBgr8] Converting to BGRA8 Premultiplied format.");
                source = SoftwareBitmap.Convert(source, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            var result = new PixelBgr8[width * height];

            var raw = new byte[height * width * 4];
            source.CopyToBuffer(raw.AsBuffer());

            int stride = width * 4;
            Debug.WriteLine($"[PixelBgr8] Using stride={stride}, width={width}, height={height}");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * stride + x * 4;
                    result[y * width + x] = new PixelBgr8(
                        raw[idx + 0],
                        raw[idx + 1],
                        raw[idx + 2],
                        raw[idx + 3]
                    );
                }
            }

            Debug.WriteLine($"[PixelBgr8] Returning array length={result.Length}");
            return result;
        }

        public static SoftwareBitmap WriteToSoftwareBitmap(PixelBgr8[] pixels, SoftwareBitmap target)
        {
            if (pixels.Length != target.PixelWidth * target.PixelHeight)
                throw new ArgumentException("Pixel array size does not match bitmap dimensions.");

            var raw = ToByteArray(pixels);
            target.CopyFromBuffer(raw.AsBuffer());
            return target;
        }

        public static byte[] ToByteArray(PixelBgr8[] pixels)
        {
            var raw = new byte[pixels.Length * 4];
            for (var i = 0; i < pixels.Length; i++)
            {
                raw[i * 4 + 0] = pixels[i].Blue;
                raw[i * 4 + 1] = pixels[i].Green;
                raw[i * 4 + 2] = pixels[i].Red;
                raw[i * 4 + 3] = pixels[i].Alpha;
            }
            return raw;
        }
    }
}
