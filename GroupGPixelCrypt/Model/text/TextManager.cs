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

        private string message;
        private char cipherChar;
        private char messageChar;
        private const int CharLength = 8;
        private const int ByteLength = 8;

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
        /// Gets the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="bitsPerChannel">The bits per channel.</param>
        /// <returns>The bytes for the encoder to encode</returns>
        public IList<byte> ConvertMessageToBytes(String message, int bitsPerChannel)
        {
            string messageWithTerminator = message + "#-.-#";
            return this.BreakDownText(messageWithTerminator, bitsPerChannel);
        }

        private IList<byte> BreakDownText(String message, int bitsPerChannel)
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
            byte maskOneBit = getMask(1);
            byte nextValue;
            IList<byte> result = new List<byte>();
            for (int i = 0; i < CharLength; i++)
            {
                nextValue = (byte)((input << i) & maskOneBit);
                result.Add(nextValue);
            }
            return result;
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
                    combinedByte |= (byte)(chunk[i] >> (ByteLength - bitsPerChannel + i));
                }

                result.Add(combinedByte);
            }

            return result;
        }

        private char decryptChar(string cipher, string message, int index)
        {
            this.setupChars(cipher, message, index);
            short cipherOffset = this.charAddNumber();
            return (char) (this.messageChar - cipherOffset);
        }

        private void setupChars(string cipher, string message, int index)
        {
            this.cipherChar = cipher[index % cipher.Length];
            this.messageChar = message[index];
        }

        private short charAddNumber()
        {
            if (!char.IsLetter(this.cipherChar))
            {
                throw new ArgumentException("cipher can only contain letters");
            }
            short cipherOffset = (short)(char.ToUpper(this.cipherChar) - 'A');
            return cipherOffset;
        }

        private char encryptChar(string cipher, string message, int index)
        {
            this.setupChars(cipher, message, index);
            short cipherOffset = this.charAddNumber();
            return (char)(this.messageChar + cipherOffset);
        }

        private string decryptMessage(string cipher, string message)
        {
            StringBuilder result = new StringBuilder(message.Length);
            for (int i = 0; i < message.Length; i++)
            {
                result.Append(this.decryptChar(cipher, message, i));
            }
            return result.ToString();
        }

        /// <summary>
        /// Checks for encryption and decrypts. If no encryption is found, returns the input as is.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public string CheckForEncryptionAndDecrypt(string input)
        {
            if (input.Contains("#KEY#"))
            {
                string[] parts = input.Split("#KEY#", 2);
                string cipher = parts[0];
                string message = parts[1];
                return this.decryptMessage(cipher, message);
            }
            else
            {
                return input;
            }
        }

        public static byte getMask(int bitsPerChannel)
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
