using System;
using System.Text;
using GroupGPixelCrypt.Data;

namespace GroupGPixelCrypt.Decrypters
{
    public static class TextDecrypter
    {
        #region Methods

        public static string DecryptBody(string encryptedBody, string keyword)
        {
            if (string.IsNullOrEmpty(encryptedBody))
            {
                throw new ArgumentNullException(nameof(encryptedBody));
            }

            if (string.IsNullOrEmpty(keyword))
            {
                return encryptedBody;
            }

            return removeVigenere(encryptedBody.ToUpperInvariant(), keyword.ToUpperInvariant());
        }

        private static string removeVigenere(string text, string keyword)
        {
            var stringBuilder = new StringBuilder(text.Length);
            int keywordIndex = 0, klen = keyword.Length;

            foreach (var c in text)
            {
                var shift = keyword[keywordIndex % klen] - StegoConstants.FirstLetter;
                var decodedIndex = (c - StegoConstants.FirstLetter - shift + StegoConstants.MaxLetterSymbol) %
                                   StegoConstants.MaxLetterSymbol;
                stringBuilder.Append((char)(decodedIndex + StegoConstants.FirstLetter));
                keywordIndex++;
            }

            return stringBuilder.ToString();
        }

        #endregion
    }
}