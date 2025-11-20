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
            PixelBgr8[] bgraPixels = PixelBgr8.FromSoftwareBitmap(source);
            PixelL1[] result = new PixelL1[source.PixelWidth * source.PixelHeight]; 
            for (int i = 0; i < bgraPixels.Length; i++)
            {
                result[i] = FromBgra8(bgraPixels[i]);
            }
            return result;
        }

        /// <summary>
        /// Turns a bgra8 pixel into a PixelL1.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static PixelL1 FromBgra8(PixelBgr8 input)
        {
            float average = (input.Red + input.Green + input.Blue) / 3.0f;
            return new PixelL1((byte)(average >= 128 ? 1 : 0));
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
