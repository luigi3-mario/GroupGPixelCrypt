using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupGPixelCrypt.Helpers
{
    public static class ImageFileHelper
    {
        #region Methods

        public static async Task<StorageFile> PickImageFileAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            return await picker.PickSingleFileAsync();
        }

        public static async Task<StorageFile> PickMessageFileAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".txt");

            return await picker.PickSingleFileAsync();
        }

        public static async Task SetImageControlAsync(Image control, SoftwareBitmap bitmap)
        {
            if (control == null || bitmap == null)
            {
                return;
            }

            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                bitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                bitmap = SoftwareBitmap.Convert(bitmap,
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied);
            }

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bitmap);
            control.Source = source;
        }

        public static void HandleException(string context, Exception ex)
        {
            Debug.WriteLine($"{context} failed: {ex.Message}");
        }

        public static bool IsImageFile(string ext)
        {
            return ext == ".png" || ext == ".bmp";
        }

        public static bool IsTextFile(string ext)
        {
            return ext == ".txt";
        }

        #endregion
    }
}