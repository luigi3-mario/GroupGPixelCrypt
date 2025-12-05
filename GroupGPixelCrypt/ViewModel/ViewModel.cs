using GroupGPixelCrypt.Model;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using GroupGPixelCrypt.Model.image;

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
                this.SourceBitmap = manager.SoftwareBitmap;
            }
        }

        public async Task LoadMessageImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                this.MessageBitmap = manager.SoftwareBitmap;
                this.MessageText = null; // clear text if image is loaded
            }
        }

        public async Task LoadMessageText(StorageFile file)
        {
            if (file != null)
            {
                this.MessageText = await FileIO.ReadTextAsync(file);
                this.MessageBitmap = null; // clear image if text is loaded
            }
        }

        public void SetTargetBitmap(SoftwareBitmap bitmap)
        {
            this.TargetBitmap = bitmap;
        }

        

       

        
    }
}
