using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Data;

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

        public static SoftwareBitmap ToSoftwareBitmap(PixelBgr8[] pixels, int width, int height)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (pixels.Length != width * height)
            {
                throw new ArgumentException("Pixel array size does not match dimensions.");
            }

            var raw = ToByteArray(pixels);

            var bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
            bitmap.CopyFromBuffer(raw.AsBuffer());
            return bitmap;
        }

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

            var raw = new byte[height * width * StegoConstants.BytesPerPixelBgra8];
            source.CopyToBuffer(raw.AsBuffer());

            var stride = width * StegoConstants.BytesPerPixelBgra8;

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
            var raw = new byte[pixels.Length * StegoConstants.BytesPerPixelBgra8];
            for (var i = 0; i < pixels.Length; i++)
            {
                writePixelToRaw(pixels[i], raw, i * StegoConstants.BytesPerPixelBgra8);
            }

            return raw;
        }

        private static int getIndex(int x, int y, int stride)
        {
            return y * stride + x * StegoConstants.BytesPerPixelBgra8;
        }

        private static PixelBgr8 createPixel(byte[] raw, int idx)
        {
            return new PixelBgr8(
                raw[idx + StegoConstants.ChannelBlue],
                raw[idx + StegoConstants.ChannelGreen],
                raw[idx + StegoConstants.ChannelRed],
                raw[idx + StegoConstants.ChannelAlpha]
            );
        }

        private static void writePixelToRaw(PixelBgr8 pixel, byte[] raw, int idx)
        {
            raw[idx + StegoConstants.ChannelBlue] = pixel.Blue;
            raw[idx + StegoConstants.ChannelGreen] = pixel.Green;
            raw[idx + StegoConstants.ChannelRed] = pixel.Red;
            raw[idx + StegoConstants.ChannelAlpha] = pixel.Alpha;
        }

        #endregion
    }
}