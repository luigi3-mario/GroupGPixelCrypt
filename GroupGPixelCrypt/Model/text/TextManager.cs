using System;
using System.Collections.Generic;

namespace GroupGPixelCrypt.Model.text
{
    public class TextManager
    {
        #region Data members

<<<<<<< HEAD
        private string message;
        private char cipherChar;
        private char messageChar;
=======
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        private const int CharLength = 8;
        private const int ByteLength = 8;

        #endregion

        #region Methods

        public IList<byte> GetMessage(string message, int bitsPerChannel)
        {
<<<<<<< HEAD
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
=======
            var messageWithTerminator = appendTerminator(message);
            return this.breakDownText(messageWithTerminator, bitsPerChannel);
        }

        private static string appendTerminator(string message)
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        {
            return message + "#-.-#";
        }

<<<<<<< HEAD
        private IList<byte> BreakDownText(String message, int bitsPerChannel)
=======
        private IList<byte> breakDownText(string message, int bitsPerChannel)
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        {
            var result = new List<byte>();
            foreach (var messageChar in message)
            {
                var brokenDownChar = this.breakDownChar(messageChar);
                result.AddRange(brokenDownChar);
            }

            return this.combineBits(result, bitsPerChannel);
        }

        private IList<byte> breakDownChar(char input)
        {
            var maskOneBit = GetMask(1);
            var result = new List<byte>();
            for (var i = 0; i < CharLength; i++)
            {
                result.Add(shiftAndMaskChar(input, i, maskOneBit));
            }

            return result;
        }

        private static byte shiftAndMaskChar(char input, int shift, byte mask)
        {
            return (byte)((input << shift) & mask);
        }

        private IList<byte> combineBits(IList<byte> bits, int bitsPerChannel)
        {
            var chunks = bits.ChunkBy(bitsPerChannel);
            var result = new List<byte>();
            foreach (var chunk in chunks)
            {
<<<<<<< HEAD
                byte combinedByte = 0;
                for (int i = 0; i < chunk.Count; i++)
                {
                    combinedByte |= (byte)(chunk[i] >> (ByteLength - bitsPerChannel + i));
                }

                result.Add(combinedByte);
=======
                result.Add(combineChunk(chunk, bitsPerChannel));
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
            }

            return result;
        }

<<<<<<< HEAD
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
=======
        private static byte combineChunk(IList<byte> chunk, int bitsPerChannel)
        {
            byte combinedByte = 0;
            for (var i = 0; i < chunk.Count; i++)
            {
                combinedByte |= (byte)(chunk[i] >> (ByteLength - bitsPerChannel + i));
            }

            return combinedByte;
        }

        public static byte GetMask(int bitsPerChannel)
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        {
            switch (bitsPerChannel)
            {
                case 1: return 0xff;
                case 2: return 0x7f;
                case 3: return 0x3f;
                case 4: return 0x1f;
                case 5: return 0x0f;
                case 6: return 0x07;
                case 7: return 0x03;
                case 8: return 0x01;
                default: throw new ArgumentException("bitsPerChannel must be between 1 and 8");
            }
        }

        #endregion
    }
}