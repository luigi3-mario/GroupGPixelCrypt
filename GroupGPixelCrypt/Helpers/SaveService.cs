using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GroupGPixelCrypt.Helpers
{
    public class SaveService
    {
        #region Methods

        public async Task SaveAsync(SoftwareBitmap bitmap)
        {
            var picker = createFileSavePicker();
            var file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoderId = getEncoderId(file.FileType);
                var encoder = await BitmapEncoder.CreateAsync(encoderId, stream);
                encoder.SetSoftwareBitmap(bitmap);
                await encoder.FlushAsync();
            }
        }

        private static FileSavePicker createFileSavePicker()
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "output_image"
            };
            picker.FileTypeChoices.Add("PNG Image", new[] { ".png" });
            picker.FileTypeChoices.Add("BMP Image", new[] { ".bmp" });
            return picker;
        }

        private static Guid getEncoderId(string fileType)
        {
            return fileType.ToLower() == ".bmp"
                ? BitmapEncoder.BmpEncoderId
                : BitmapEncoder.PngEncoderId;
        }

        #endregion
    }
}