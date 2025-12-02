using GroupGPixelCrypt.Model;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace GroupGPixelCrypt.ViewModel
{
    public class MainViewModel
    {
        public SoftwareBitmap SourceBitmap { get; private set; }
        public SoftwareBitmap MessageBitmap { get; set; }
        public SoftwareBitmap TargetBitmap { get; private set; }

        public string MessageText { get; set; }
        public byte BitsPerChannel { get; set; } = 1;
        public bool EncryptionUsed { get; set; } = false;

        public async Task LoadSourceImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                SourceBitmap = manager.SoftwareBitmap;
            }
        }

        public async Task LoadMessageImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                MessageBitmap = manager.SoftwareBitmap;
            }
        }

        public void SetTargetBitmap(SoftwareBitmap bitmap)
        {
            TargetBitmap = bitmap;
        }

        public void EmbedMessage()
        {
            if (SourceBitmap == null) return;

            if (MessageBitmap != null)
            {
                var embedder = new Embedder(MessageBitmap, SourceBitmap);
                TargetBitmap = embedder.EmbedMessage();
            }
            else if (!string.IsNullOrEmpty(MessageText))
            {
                var embedder = new Embedder(MessageText, SourceBitmap, BitsPerChannel, EncryptionUsed);
                TargetBitmap = embedder.EmbedMessage();
            }
        }

        public SoftwareBitmap ExtractImageFromSource()
        {
            if (SourceBitmap == null) return null;
            var extractor = new Extractor(SourceBitmap);
            return extractor.ExtractMessageBitmap();
        }

        public string ExtractTextFromSource()
        {
            if (SourceBitmap == null) return null;
            var extractor = new Extractor(SourceBitmap);
            return extractor.ExtractMessageText();
        }
    }
}
