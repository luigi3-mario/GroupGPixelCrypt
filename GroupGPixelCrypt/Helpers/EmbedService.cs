using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Embedders;
using GroupGPixelCrypt.Encrypters;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Helpers
{
    public class EmbedService
    {
        public SoftwareBitmap EmbedText(SoftwareBitmap source, string text, byte bitsPerChannel, bool encryptionUsed)
        {
            var embedder = new TextEmbedder(text, source, bitsPerChannel, encryptionUsed);
            return embedder.EmbedMessage();
        }

        public SoftwareBitmap EncryptMessageBitmap(SoftwareBitmap padded)
        {
            var msgPixels = PixelBgr8.FromSoftwareBitmap(padded);
            var swapped = ImageEncrypter.EncryptQuadrants(msgPixels, padded.PixelWidth, padded.PixelHeight);

            var finalMessage = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                padded.PixelWidth, padded.PixelHeight, BitmapAlphaMode.Premultiplied);

            PixelBgr8.WriteToSoftwareBitmap(swapped, finalMessage);
            return finalMessage;
        }
    }
}