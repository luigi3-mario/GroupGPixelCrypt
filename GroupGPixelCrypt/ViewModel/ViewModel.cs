using GroupGPixelCrypt.Model;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace GroupGPixelCrypt.ViewModel
{
    public class MainViewModel
    {
        public SoftwareBitmap SourceBitmap { get; private set; }
        public SoftwareBitmap MessageBitmap { get; private set; }
        public SoftwareBitmap TargetBitmap { get; private set; }

        /// <summary>
        /// Load a source image file into SourceBitmap
        /// </summary>
        public async Task LoadSourceImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                SourceBitmap = manager.SoftwareBitmap;
            }
        }

        /// <summary>
        /// Load a message image file into MessageBitmap
        /// </summary>
        public async Task LoadMessageImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                MessageBitmap = manager.SoftwareBitmap;
            }
        }

        /// <summary>
        /// Embed the message into the source image
        /// </summary>
        public void EmbedMessage(int bitsPerChannel = 1)
        {
            if (SourceBitmap != null && MessageBitmap != null)
            {
                Embedder embedder = new Embedder(MessageBitmap, SourceBitmap);
                TargetBitmap = embedder.EmbedMessage();
            }
        }
    }
}