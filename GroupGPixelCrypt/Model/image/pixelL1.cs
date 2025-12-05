using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace GroupGPixelCrypt.Model.image
{
    public class PixelL1
    {
        public byte Luma { get; }

        public PixelL1(byte value)
        {
            if (value > 1 || value < 0)
                throw new ArgumentException("Can only be 0 or 1");
            this.Luma = value;
        }

        public static PixelL1[] FromSoftwareBitmap(SoftwareBitmap source)
        {
            Debug.WriteLine($"[PixelL1] Entered FromSoftwareBitmap. Source={(source == null ? "NULL" : "OK")}");

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                source.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
            {
                Debug.WriteLine("[PixelL1] Converting to BGRA8 Premultiplied format.");
                source = SoftwareBitmap.Convert(source, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            var result = new PixelL1[width * height];

            var raw = new byte[height * width * 4];
            source.CopyToBuffer(raw.AsBuffer());

            int stride = width * 4;
            Debug.WriteLine($"[PixelL1] Using stride={stride}, width={width}, height={height}");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * stride + x * 4;
                    byte blue = raw[idx + 0];
                    byte green = raw[idx + 1];
                    byte red = raw[idx + 2];
                    var average = (red + green + blue) / 3.0f;
                    result[y * width + x] = new PixelL1((byte)(average >= 128 ? 1 : 0));
                }
            }

            Debug.WriteLine($"[PixelL1] Returning array length={result.Length}");
            return result;
        }

        public static SoftwareBitmap ToSoftwareBitmap(PixelL1[] pixels, int width, int height)
        {
            if (pixels.Length != width * height)
                throw new ArgumentException("Pixel array length does not match dimensions.");

            var bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
            var buffer = new byte[width * height * 4];

            int nullCount = 0;
            for (var i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] == null)
                {
                    nullCount++;
                    // keep buffer default (black, opaque) for any nulls
                    continue;
                }
                var v = pixels[i].Luma == 1 ? (byte)255 : (byte)0;
                var idx = i * 4;
                buffer[idx + 0] = v;
                buffer[idx + 1] = v;
                buffer[idx + 2] = v;
                buffer[idx + 3] = 255;
            }

            Debug.WriteLine($"[PixelL1.ToSoftwareBitmap] Null pixels encountered={nullCount}");
            bitmap.CopyFromBuffer(buffer.AsBuffer());
            return bitmap;
        }
    }
}
