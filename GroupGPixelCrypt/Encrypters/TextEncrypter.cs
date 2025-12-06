using System;
using System.Text;
using GroupGPixelCrypt.Data;

namespace GroupGPixelCrypt.Encrypters
{
    public class TextEncrypter
    {
        #region Methods

        public string EncryptMessage(string keyword, string lettersOnlyBody)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentNullException(nameof(keyword));
            }

            if (string.IsNullOrEmpty(lettersOnlyBody))
            {
                throw new ArgumentNullException(nameof(lettersOnlyBody));
            }

            var key = keyword.ToUpperInvariant();
            var body = lettersOnlyBody.ToUpperInvariant();
            var encryptedBody = ApplyVigenere(body, key);
            return key + "#KEY#" + encryptedBody + "#-.-#";
        }

        internal static string ApplyVigenere(string text, string keyword)
        {
            var sb = new StringBuilder(text.Length);
            int ki = 0, klen = keyword.Length;

            foreach (var c in text)
            {
                var shift = keyword[ki % klen] - StegoConstants.FirstLetter;
                var encrypt = (c - StegoConstants.FirstLetter + shift) % StegoConstants.MaxLetterSymbol;
                sb.Append((char)(encrypt + StegoConstants.FirstLetter));
                ki++;
            }

            return sb.ToString();
        }

        #endregion
    }
}