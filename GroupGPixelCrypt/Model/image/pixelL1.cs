using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace GroupGPixelCrypt.Model.image
{
    public class PixelL1
    {
        #region Properties

        public byte Luma { get; }

        #endregion

        #region Constructors

        public PixelL1(byte value)
        {
            if (value > 1)
            {
                throw new ArgumentException("Can only be 0 or 1");
            }

            this.Luma = value;
        }

        #endregion

        #region Methods

        public static PixelL1[] FromSoftwareBitmap(SoftwareBitmap source)
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
            var result = new PixelL1[width * height];

            var raw = new byte[height * width * 4];
            source.CopyToBuffer(raw.AsBuffer());

            var stride = width * 4;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var idx = getIndex(x, y, stride);
                    var blue = raw[idx + 0];
                    var green = raw[idx + 1];
                    var red = raw[idx + 2];
                    var average = calculateAverage(red, green, blue);
                    result[y * width + x] = createPixelFromAverage(average);
                }
            }

            return result;
        }

        public static SoftwareBitmap ToSoftwareBitmap(PixelL1[] pixels, int width, int height)
        {
            if (pixels.Length != width * height)
            {
                throw new ArgumentException("Pixel array length does not match dimensions.");
            }

            var bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
            var buffer = new byte[width * height * 4];

            for (var i = 0; i < pixels.Length; i++)
            {
                writePixelToBuffer(pixels[i], buffer, i * 4);
            }

            bitmap.CopyFromBuffer(buffer.AsBuffer());
            return bitmap;
        }

        private static int getIndex(int x, int y, int stride)
        {
            return y * stride + x * 4;
        }

        private static float calculateAverage(byte red, byte green, byte blue)
        {
            return (red + green + blue) / 3.0f;
        }

        private static PixelL1 createPixelFromAverage(float average)
        {
            return new PixelL1((byte)(average >= 128 ? 1 : 0));
        }

<<<<<<< HEAD
        /// <summary>
        /// Converts to bytearray.
        /// </summary>
        /// <param name="pixelL1Array">The pixel l1 array.</param>
        /// <returns></returns>
        public static byte[] ToByteArray(PixelL1[] pixelL1Array)
=======
        private static void writePixelToBuffer(PixelL1 pixel, byte[] buffer, int idx)
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        {
            var v = pixel.Luma == 1 ? (byte)255 : (byte)0;
            buffer[idx + 0] = v;
            buffer[idx + 1] = v;
            buffer[idx + 2] = v;
            buffer[idx + 3] = 255;
        }

<<<<<<< HEAD
        /// <summary>
        /// Converts an array of bytes to an array of PixelL1.
        /// </summary>
        /// <param name="byteArray">The byte array.</param>
        /// <returns></returns>
        public static PixelL1[] FromByteArray(byte[] byteArray)
        {
            PixelL1[] result = new PixelL1[byteArray.Length];
            for (int i = 0; i < byteArray.Length; i++)
            {
                result[i] = new PixelL1(byteArray[i]);
            }

            return result;
        }

        public static SoftwareBitmap ToSoftwareBitmap(PixelL1[] pixelArray, int width, int height)
        {
            PixelBgr8[] bgraPixels = new PixelBgr8[pixelArray.Length];
            foreach (PixelL1 pixelL1 in pixelArray)
            {
                bgraPixels[Array.IndexOf(pixelArray, pixelL1)] = PixelBgr8.fromPixelL1(pixelL1);
            }
            SoftwareBitmap result = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height);
            return PixelBgr8.WriteToSoftwareBitmap(bgraPixels, result);
        }
=======
        #endregion
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
    }
}