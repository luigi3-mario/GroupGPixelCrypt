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
            var sb = new StringBuilder(text.Length);
            int ki = 0, klen = keyword.Length;

            foreach (var c in text)
            {
                var shift = keyword[ki % klen] - StegoConstants.FirstLetter;
                var dec = (c - StegoConstants.FirstLetter - shift + StegoConstants.MaxLetterSymbol) %
                          StegoConstants.MaxLetterSymbol;
                sb.Append((char)(dec + StegoConstants.FirstLetter));
                ki++;
            }

            return sb.ToString();
        }

        #endregion
    }
}