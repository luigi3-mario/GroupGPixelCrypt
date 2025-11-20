using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace GroupGPixelCrypt.Model.image
{
    /// <summary>
    /// represents one pixel in an image
    /// </summary>
    public sealed class PixelBgr8
    {
        private static int numberOfChannels = 4;

        /// <summary>
        /// Gets or sets the channels.
        /// </summary>
        /// <value>
        /// The channels.
        /// </value>
        public byte[] Channels => new byte[] { this.Blue, this.Green, this.Red, this.Alpha };

        /// <summary>
        /// Gets or sets the red.
        /// </summary>
        /// <value>
        /// The red.
        /// </value>
        public byte Red { get; set; }

        /// <summary>
        /// Gets or sets the green.
        /// </summary>
        /// <value>
        /// The green.
        /// </value>
        public byte Green { get; set; }

        /// <summary>
        /// Gets or sets the blue.
        /// </summary>
        /// <value>
        /// The blue.
        /// </value>
        public byte Blue { get; set; }

        /// <summary>
        /// Gets or sets the alpha.
        /// </summary>
        /// <value>
        /// The alpha.
        /// </value>
        public byte Alpha { get; set; }

        public PixelBgr8(byte blue, byte green, byte red, byte alpha)
        {
            this.Blue = blue;
            this.Green = green;
            this.Red = red;
            this.Alpha = alpha;
        }

        public static PixelBgr8[] FromSoftwareBitmap(SoftwareBitmap source)
        {
            source = ImageManager.ConvertToCorrectFormat(source);
            byte[] sourcePixels = new byte[source.PixelWidth * source.PixelHeight * numberOfChannels];
            PixelBgr8[] result = new PixelBgr8[source.PixelWidth * source.PixelHeight];
            source.CopyToBuffer(sourcePixels.AsBuffer());
            for (int i = 0; i * numberOfChannels < sourcePixels.Length; i++)
            {
                result[i] = singlePixelFromByteArray(sourcePixels, i);
            }

            return result;
        }

        /// <summary>
        /// Singles the pixel from byte array.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static PixelBgr8 singlePixelFromByteArray(byte[] sourcePixels, int index)
        {
            byte blue = sourcePixels[index * numberOfChannels];
            byte green = sourcePixels[index * numberOfChannels + 1];
            byte red = sourcePixels[index * numberOfChannels + 2];
            byte alpha = sourcePixels[index * numberOfChannels + 3];
            return new PixelBgr8(blue, green, red, alpha);
        }

        public static SoftwareBitmap WriteToSoftwareBitmap(PixelBgr8[] source, SoftwareBitmap original)
        {
            original.CopyFromBuffer(ToByteArray(source).AsBuffer());
            return original;
        }

        public static byte[] ToByteArray(PixelBgr8[] source)
        {
            byte[] result = new byte[source.Length * numberOfChannels];
            for (int i = 0; i * numberOfChannels < result.Length; i++)
            {
                result[i * numberOfChannels] = source[i].Blue;
                result[i * numberOfChannels + 1] = source[i].Green;
                result[i * numberOfChannels + 2] = source[i].Red;
                result[i * numberOfChannels + 3] = source[i].Alpha;
            }

            return result;
        }

        public static PixelBgr8 whitePixel()
        {
            return new PixelBgr8(255, 255, 255, 255);
        }

        public static PixelBgr8 fromPixelL1(PixelL1 pixel)
        {
            return new PixelBgr8(pixel.Luma, pixel.Luma, pixel.Luma, 255);
        }
    }
}
