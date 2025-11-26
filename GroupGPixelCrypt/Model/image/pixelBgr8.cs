using System;
using System.Runtime.InteropServices;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace GroupGPixelCrypt.Model.image
{
    [StructLayout(LayoutKind.Sequential)]
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
            source = ImageManager.ConvertToCorrectFormat(source);
            int pixelCount = source.PixelWidth * source.PixelHeight;
            byte[] raw = new byte[pixelCount * 4];
            source.CopyToBuffer(raw.AsBuffer());

            var result = new PixelBgr8[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                result[i] = new PixelBgr8(
                    raw[i * 4 + 0],
                    raw[i * 4 + 1],
                    raw[i * 4 + 2],
                    raw[i * 4 + 3]);
            }
            return result;
        }

        public static SoftwareBitmap WriteToSoftwareBitmap(PixelBgr8[] pixels, SoftwareBitmap target)
        {
            if (pixels.Length != target.PixelWidth * target.PixelHeight)
                throw new ArgumentException("Pixel array size does not match bitmap dimensions.");

            byte[] raw = ToByteArray(pixels);
            target.CopyFromBuffer(raw.AsBuffer());
            return target;
        }

        public static byte[] ToByteArray(PixelBgr8[] pixels)
        {
            byte[] raw = new byte[pixels.Length * 4];
            for (int i = 0; i < pixels.Length; i++)
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
