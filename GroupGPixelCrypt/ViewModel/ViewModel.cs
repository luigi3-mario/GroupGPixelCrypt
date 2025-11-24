using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace GroupGPixelCrypt.ViewModel
{
    public class MainViewModel
    {
        #region Properties

        public SoftwareBitmap SourceBitmap { get; private set; }
        public SoftwareBitmap MessageBitmap { get; private set; }

        public SoftwareBitmap TargetBitmap { get; private set; }

        #endregion

        #region Methods

        public async Task LoadSourceImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                this.SourceBitmap = manager.SoftwareBitmap;
            }
        }

        public async Task LoadMessageImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                this.MessageBitmap = manager.SoftwareBitmap;
            }
        }

        public void EmbedMessage(int bitsPerChannel = 1)
        {
            if (this.SourceBitmap != null && this.MessageBitmap != null)
            {
                var embedder = new Embedder(this.MessageBitmap, this.SourceBitmap);
                this.TargetBitmap = embedder.EmbedMessage();
            }
        }

        public void SetTargetBitmap(SoftwareBitmap bitmap)
        {
            this.TargetBitmap = bitmap;
        }

        #endregion
    }
}