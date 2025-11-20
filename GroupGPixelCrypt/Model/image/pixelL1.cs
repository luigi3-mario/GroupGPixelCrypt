using System;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace GroupGPixelCrypt.Model.image
{
    public class PixelL1
    {
        private static int bgraChannels = 4;

        public byte Luma { get; }

        public PixelL1(byte value)
        {
            if (value > 1 || value < 0)
                throw new ArgumentException("Luma must be 0 or 1");
            this.Luma = value;
        }

        public static PixelL1[] FromSoftwareBitmap(SoftwareBitmap source)
        {
            source = ImageManager.ConvertToCorrectFormat(source);
            byte[] sourcePixels = new byte[source.PixelWidth * source.PixelHeight * bgraChannels];
            PixelL1[] result = new PixelL1[source.PixelWidth * source.PixelHeight];
            source.CopyToBuffer(sourcePixels.AsBuffer());

            for (int i = 0; i < result.Length; i++)
            {
                byte blue = sourcePixels[i * bgraChannels];
                byte green = sourcePixels[i * bgraChannels + 1];
                byte red = sourcePixels[i * bgraChannels + 2];
                float avg = (red + green + blue) / 3f;
                result[i] = new PixelL1((byte)(avg >= 128 ? 1 : 0));
            }

            return result;
        }

        public static byte[] ToByteArray(PixelL1[] pixels)
        {
            byte[] result = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
                result[i] = pixels[i].Luma;
            return result;
        }

        public static PixelL1[] FromByteArray(byte[] bytes)
        {
            PixelL1[] result = new PixelL1[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                result[i] = new PixelL1(bytes[i]);
            return result;
        }

        public static SoftwareBitmap ToSoftwareBitmap(PixelL1[] pixels, int width, int height)
        {
            var bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
            var buffer = new byte[width * height * 4];

            for (int i = 0; i < pixels.Length; i++)
            {
                byte v = (byte)(pixels[i].Luma == 1 ? 255 : 0); // map 1 → white, 0 → black
                buffer[i * 4 + 0] = v; // B
                buffer[i * 4 + 1] = v; // G
                buffer[i * 4 + 2] = v; // R
                buffer[i * 4 + 3] = 255; // A
            }

            bitmap.CopyFromBuffer(buffer.AsBuffer());
            return bitmap;
        }

    }
}
