using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace GroupGPixelCrypt.Model.image
{
    public struct PixelBgr8
    {
        #region Properties

        public byte Blue { get; set; }
        public byte Green { get; set; }
        public byte Red { get; set; }
        public byte Alpha { get; set; }

        #endregion

        #region Constructors

        public PixelBgr8(byte blue, byte green, byte red, byte alpha)
        {
            this.Blue = blue;
            this.Green = green;
            this.Red = red;
            this.Alpha = alpha;
        }

        #endregion

        #region Methods

        public static PixelBgr8[] FromSoftwareBitmap(SoftwareBitmap source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                source.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
            {
                source = SoftwareBitmap.Convert(source, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            var width = source.PixelWidth;
            var height = source.PixelHeight;
            var result = new PixelBgr8[width * height];

            var raw = new byte[height * width * 4];
            source.CopyToBuffer(raw.AsBuffer());

            var stride = width * 4;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var idx = getIndex(x, y, stride);
                    result[y * width + x] = createPixel(raw, idx);
                }
            }

            return result;
        }

        public static SoftwareBitmap WriteToSoftwareBitmap(PixelBgr8[] pixels, SoftwareBitmap target)
        {
            if (pixels.Length != target.PixelWidth * target.PixelHeight)
            {
                throw new ArgumentException("Pixel array size does not match bitmap dimensions.");
            }

            var raw = ToByteArray(pixels);
            target.CopyFromBuffer(raw.AsBuffer());
            return target;
        }

        public static byte[] ToByteArray(PixelBgr8[] pixels)
        {
            var raw = new byte[pixels.Length * 4];
            for (var i = 0; i < pixels.Length; i++)
            {
                writePixelToRaw(pixels[i], raw, i * 4);
            }

            return raw;
        }

        private static int getIndex(int x, int y, int stride)
        {
            return y * stride + x * 4;
        }

        private static PixelBgr8 createPixel(byte[] raw, int idx)
        {
            return new PixelBgr8(
                raw[idx + 0],
                raw[idx + 1],
                raw[idx + 2],
                raw[idx + 3]
            );
        }

        private static void writePixelToRaw(PixelBgr8 pixel, byte[] raw, int idx)
        {
            raw[idx + 0] = pixel.Blue;
            raw[idx + 1] = pixel.Green;
            raw[idx + 2] = pixel.Red;
            raw[idx + 3] = pixel.Alpha;
        }

        #endregion
    }
}