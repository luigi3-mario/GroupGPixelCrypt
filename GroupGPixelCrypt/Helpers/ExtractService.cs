using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Decrypters;
using GroupGPixelCrypt.Extractors;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Helpers
{
    public class ExtractService
    {
        #region Methods

        public (string text, SoftwareBitmap image) Extract(SoftwareBitmap bitmap)
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(bitmap);
            var header = HeaderManager.ReadHeader(pixels);

            if (!header.hasMessage)
            {
                return (null, null);
            }

            if (header.isText)
            {
                var extractor = new TextExtractor(bitmap);
                return extractor.HasEmbeddedMessage()
                    ? (extractor.ExtractMessageText(), null)
                    : ((string)null, (SoftwareBitmap)null);
            }
            else
            {
                var extractor = new ImageExtractor(bitmap);
                if (!extractor.HasEmbeddedMessage())
                {
                    return (null, null);
                }

                var extractedBitmap = extractor.ExtractMessageBitmap();
                if (header.encryptionUsed)
                {
                    extractedBitmap = this.decryptMessageBitmap(extractedBitmap);
                }

                return (null, extractedBitmap);
            }
        }

        private SoftwareBitmap decryptMessageBitmap(SoftwareBitmap extractedBitmap)
        {
            var msgPixels = PixelBgr8.FromSoftwareBitmap(extractedBitmap);
            var unswapped = ImageDecrypter.DecryptQuadrants(
                msgPixels,
                extractedBitmap.PixelWidth,
                extractedBitmap.PixelHeight);

            var unscrambledBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                extractedBitmap.PixelWidth,
                extractedBitmap.PixelHeight,
                BitmapAlphaMode.Premultiplied);

            PixelBgr8.WriteToSoftwareBitmap(unswapped, unscrambledBitmap);
            return unscrambledBitmap;
        }

        #endregion
    }
}