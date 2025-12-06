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
            var stringBuilder = new StringBuilder(text.Length);
            int keywordIndex = 0, klen = keyword.Length;

            foreach (var c in text)
            {
                var shift = keyword[keywordIndex % klen] - StegoConstants.FirstLetter;
                var encrypt = (c - StegoConstants.FirstLetter + shift) % StegoConstants.MaxLetterSymbol;
                stringBuilder.Append((char)(encrypt + StegoConstants.FirstLetter));
                keywordIndex++;
            }

            return stringBuilder.ToString();
        }

        #endregion
    }
}