using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace GroupGPixelCrypt.Model.image
{
    /// <summary>
    ///     Represents a monochrome pixel (0 = black, 1 = white).
    /// </summary>
    public class PixelL1
    {
        #region Properties

        public byte Luma { get; }

        #endregion

        #region Constructors

        public PixelL1(byte value)
        {
            if (value > 1 || value < 0)
            {
                throw new ArgumentException("Can only be 0 or 1");
            }

            this.Luma = value;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts a SoftwareBitmap into an array of PixelL1 (thresholded to 0/1).
        /// </summary>
        public static PixelL1[] FromSoftwareBitmap(SoftwareBitmap source)
        {
            source = ImageManager.ConvertToCorrectFormat(source);
            var bgraPixels = PixelBgr8.FromSoftwareBitmap(source);
            var result = new PixelL1[source.PixelWidth * source.PixelHeight];
            for (var i = 0; i < bgraPixels.Length; i++)
            {
                result[i] = FromBgra8(bgraPixels[i]);
            }

            return result;
        }

        /// <summary>
        ///     Converts a BGRA pixel into a PixelL1 (0/1 based on average brightness).
        /// </summary>
        public static PixelL1 FromBgra8(PixelBgr8 input)
        {
            var average = (input.Red + input.Green + input.Blue) / 3.0f;
            return new PixelL1((byte)(average >= 128 ? 1 : 0));
        }

        public static byte[] ToByteArray(PixelL1[] pixelL1Array)
        {
            var result = new byte[pixelL1Array.Length];
            for (var i = 0; i < pixelL1Array.Length; i++)
            {
                result[i] = pixelL1Array[i].Luma;
            }

            return result;
        }

        /// <summary>
        ///     Converts an array of PixelL1 into a BGRA8 monochrome SoftwareBitmap.
        ///     0 => black (0,0,0), 1 => white (255,255,255).
        /// </summary>
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
                var v = pixels[i].Luma == 1 ? (byte)255 : (byte)0;
                var idx = i * 4;
                buffer[idx + 0] = v; // B
                buffer[idx + 1] = v; // G
                buffer[idx + 2] = v; // R
                buffer[idx + 3] = 255; // A
            }

            bitmap.CopyFromBuffer(buffer.AsBuffer());
            return bitmap;
        }

        #endregion
    }
}