using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace GroupGPixelCrypt.Model.image
{
    public class PixelL1
    {
        private static int bgraChannels = 4;

        /// <summary>
        /// Gets or sets the channels.
        /// </summary>
        /// <value>
        /// The channels.
        /// </value>
        public byte Luma { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelL1"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PixelL1(byte value)
        {
            if (value > 1 || value < 0)
            {
                throw new ArgumentException("Can only be 0 or 1");
            }
            this.Luma = value;
        }

        /// <summary>
        /// Takes in a SoftwareBitmap and converts it to an array of PixelL1.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static PixelL1[] FromSoftwareBitmap(SoftwareBitmap source)
        {
            source = ImageManager.ConvertToCorrectFormat(source);
            byte[] sourcePixels = new byte[source.PixelWidth * source.PixelHeight * bgraChannels];
            PixelL1[] result = new PixelL1[source.PixelWidth * source.PixelHeight]; 
            source.CopyToBuffer(sourcePixels.AsBuffer());
            for (int i = 0; i * bgraChannels < sourcePixels.Length; i++)
            {
                byte blue = sourcePixels[i * bgraChannels];
                byte green = sourcePixels[i * bgraChannels + 1];
                byte red = sourcePixels[i * bgraChannels + 2];
                float average = (red + green + blue) / 3.0f;
                result[i] = new PixelL1((byte)(average >= 128 ? 1 : 0));
            }
            return result;
        }

        public static byte[] ToByteArray(PixelL1[] pixelL1Array)
        {
            byte[] result = new byte[pixelL1Array.Length];
            for (int i = 0; i < pixelL1Array.Length; i++)
            {
                result[i] = pixelL1Array[i].Luma;
            }
            return result;
        }
    }
}
