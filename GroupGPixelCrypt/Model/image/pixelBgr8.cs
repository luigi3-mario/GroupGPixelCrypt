using System;
using System.Runtime.InteropServices;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace GroupGPixelCrypt.Model.Image
{
    /// <summary>
    /// Represents one BGRA pixel (Blue, Green, Red, Alpha).
    /// </summary>
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

        /// <summary>
        /// Returns the pixel channels as a tuple (no allocation).
        /// </summary>
        public (byte Blue, byte Green, byte Red, byte Alpha) Channels => (Blue, Green, Red, Alpha);

        /// <summary>
        /// Converts a SoftwareBitmap into an array of PixelBgr8.
        /// </summary>
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
                    raw[i * 4 + 0], // Blue
                    raw[i * 4 + 1], // Green
                    raw[i * 4 + 2], // Red
                    raw[i * 4 + 3]  // Alpha
                );
            }
            return result;
        }

        /// <summary>
        /// Writes an array of PixelBgr8 back into a SoftwareBitmap.
        /// </summary>
        public static SoftwareBitmap WriteToSoftwareBitmap(PixelBgr8[] pixels, SoftwareBitmap target)
        {
            if (pixels.Length != target.PixelWidth * target.PixelHeight)
                throw new ArgumentException("Pixel array size does not match bitmap dimensions.");

            byte[] raw = ToByteArray(pixels);
            target.CopyFromBuffer(raw.AsBuffer());
            return target;
        }

        /// <summary>
        /// Converts PixelBgr8 array into raw BGRA byte array.
        /// </summary>
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
