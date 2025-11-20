using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using GroupGPixelCrypt.Model;
using GroupGPixelCrypt.Model.image;
using GroupGPixelCrypt.ViewModel;

namespace GroupGPixelCrypt
{
    public sealed partial class MainPage : Page
    {
        #region Data members

        private readonly MainViewModel viewModel = new MainViewModel();

        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private async void OpenSourceImageButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.PickImageFile();
            if (file != null)
            {
                await this.viewModel.LoadSourceImage(file);
                await this.SetImageControl(this.sourceImage, this.viewModel.SourceBitmap);
            }
        }

        private async void OpenMessageImageButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.PickImageFile();
            if (file != null)
            {
                await this.viewModel.LoadMessageImage(file);
                await this.SetImageControl(this.messageImage, this.viewModel.MessageBitmap);
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.EmbedMessage();
            if (this.viewModel.TargetBitmap != null)
            {
                await this.SetImageControl(this.targetImage, this.viewModel.TargetBitmap);
            }
        }

        private async void extractButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.viewModel.SourceBitmap == null)
                {
                    return;
                }

                var extractor = new Extractor(this.viewModel.SourceBitmap);

                // First, extract bytes (this sets MessageWidth/MessageHeight internally)
                var extractedBytes = extractor.ExtractMessageBytes();

                // Now you can safely read width and height
                var msgWidth = extractor.MessageWidth;
                var msgHeight = extractor.MessageHeight;

                var pixels = PixelL1.FromByteArray(extractedBytes);
                var extractedBitmap = PixelL1.ToSoftwareBitmap(pixels, msgWidth, msgHeight);

                await this.SetImageControl(this.targetImage, extractedBitmap);

                Debug.WriteLine($"Extraction happened)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Extraction failed: {ex.Message}");
            }
        }


        private async void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.TargetBitmap == null)
            {
                return;
            }

            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "embedded_image"
            };
            picker.FileTypeChoices.Add("PNG", new[] { ".png" });

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(
                        BitmapEncoder.PngEncoderId, stream);
                    encoder.SetSoftwareBitmap(this.viewModel.TargetBitmap);
                    await encoder.FlushAsync();
                }
            }
        }

        /// <summary>
        ///     Helper method to pick a source image
        /// </summary>
        private async Task<StorageFile> PickImageFile()
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

        /// <summary>
        ///     Display a SoftwareBitmap in an Image control
        /// </summary>
        private async Task SetImageControl(Image control, SoftwareBitmap bitmap)
        {
            if (bitmap == null)
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

        #endregion
    }
}