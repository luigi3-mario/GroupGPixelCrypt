using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace GroupGPixelCrypt.Model
{
    public class TextManager
    {
        #region Data members

        private string inputText;
        private const int CharLength = 8;
        private const int byteLength = 8;

        #endregion

        #region Properties

        #endregion

        #region Constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="TextManager"/> class.
        /// </summary>
        public TextManager()
        {

        }

        #endregion

        #region Methods        
        /// <summary>
        /// Breaks down bytes of text into a series of numbers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="bitsPerChannel">The bits per channel.</param>
        /// <returns>the bits of the text broken down and reassembled into groups of length bitsPerChannel</returns>
        public IList<byte> BreakDownText(String message, int bitsPerChannel)
        {
            IList<byte> brokenDownChar;
            List<byte> result = new List<byte>();//We may not use IList here because we need to use AddRange
            foreach (char messageChar in message.ToCharArray())
            {
                brokenDownChar = this.breakDownChar(messageChar);
                result.AddRange(brokenDownChar);
            }

            return this.combineBits(result, bitsPerChannel);
        }
        private IList<byte> breakDownChar(char input)
        {
            byte maskOneBit = this.getMask(1);
            byte nextValue;
            IList<byte> result = new List<byte>();
            for (int i = 0; i < CharLength; i++)
            {
                nextValue = (byte)((input << i) & maskOneBit);
                result.Add(nextValue);
            }
            return result;
        }

        private byte getMask(int bitsPerChannel)
        {
            switch (bitsPerChannel)
            {
                case 1:
                    return 0xff;
                case 2:
                    return 0x7f;
                case 3:
                    return 0x3f;
                case 4:
                    return 0x1f;
                case 5:
                    return 0x0f;
                case 6:
                    return 0x07;
                case 7:
                    return 0x03;
                case 8:
                    return 0x01;
                default:
                    throw new ArgumentException("bitsPerChannel must be between 1 and 8");
            }
        }

        private IList<byte> combineBits(IList<byte> bits, int bitsPerChannel)
        {
            ICollection<IList<byte>> chunks = bits.ChunkBy(bitsPerChannel);
            List<byte> result = new List<byte>();
            foreach (IList<byte> chunk in chunks)
            {
                byte combinedByte = 0;
                for (int i = 0; i < chunk.Count; i++)
                {
                    combinedByte |= (byte)(chunk[i] >> (byteLength - bitsPerChannel + i));
                }

                result.Add(combinedByte);
            }

            return result;
        }

        

        #endregion
    }
}
