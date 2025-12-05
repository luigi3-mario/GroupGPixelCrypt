using System;
using System.Collections.Generic;

namespace GroupGPixelCrypt.Model.text
{
    public class TextManager
    {
        #region Data members

        private const int CharLength = 8;
        private const int ByteLength = 8;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextManager" /> class.
        /// </summary>
        public TextManager()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="bitsPerChannel">The bits per channel.</param>
        /// <returns>The bytes for the encoder to encode</returns>
        public IList<byte> GetMessage(string message, int bitsPerChannel)
        {
            var messageWithTerminator = message + "#-.-#";
            return this.breakDownText(messageWithTerminator, bitsPerChannel);
        }

        private IList<byte> breakDownText(string message, int bitsPerChannel)
        {
            IList<byte> brokenDownChar;
            var result = new List<byte>();
            foreach (var messageChar in message)
            {
                brokenDownChar = this.breakDownChar(messageChar);
                result.AddRange(brokenDownChar);
            }

            return this.combineBits(result, bitsPerChannel);
        }

        private IList<byte> breakDownChar(char input)
        {
            var maskOneBit = GetMask(1);
            byte nextValue;
            IList<byte> result = new List<byte>();
            for (var i = 0; i < CharLength; i++)
            {
                nextValue = (byte)((input << i) & maskOneBit);
                result.Add(nextValue);
            }

            return result;
        }

        private IList<byte> combineBits(IList<byte> bits, int bitsPerChannel)
        {
            var chunks = bits.ChunkBy(bitsPerChannel);
            var result = new List<byte>();
            foreach (var chunk in chunks)
            {
                byte combinedByte = 0;
                for (var i = 0; i < chunk.Count; i++)
                {
                    combinedByte |= (byte)(chunk[i] >> (ByteLength - bitsPerChannel + i));
                }

                result.Add(combinedByte);
            }

            return result;
        }

        public static byte GetMask(int bitsPerChannel)
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

        #endregion
    }
}