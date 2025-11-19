using System;
using System.Collections.Generic;
using System.Linq;
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

        public static PixelBgr8[] FromArray(SoftwareBitmap source)
        {
            source = ImageManager.ConvertToCorrectFormat(source);
            byte[] sourcePixels = new byte[source.PixelWidth * source.PixelHeight * numberOfChannels];
            PixelBgr8[] result = new PixelBgr8[source.PixelWidth * source.PixelHeight];
            source.CopyToBuffer(sourcePixels.AsBuffer());
            for (int i = 0; i * numberOfChannels < sourcePixels.Length; i++)
            {
                byte blue = sourcePixels[i * numberOfChannels];
                byte green = sourcePixels[i * numberOfChannels + 1];
                byte red = sourcePixels[i * numberOfChannels + 2];
                byte alpha = sourcePixels[i * numberOfChannels + 3];
                result[i] = new PixelBgr8(blue, green, red, alpha);
            }
            return result;
        }
    }
}
