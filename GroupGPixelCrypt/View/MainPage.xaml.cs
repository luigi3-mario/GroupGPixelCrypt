using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
<<<<<<< HEAD
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

=======

namespace GroupGPixelCrypt.View
{
    public sealed partial class MainPage
    {
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this.viewModel;
        }

        #endregion

        #region Methods

        private async void OpenSourceImageButton_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
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
=======
            var file = await this.pickImageFile();
            if (file != null)
            {
                await this.viewModel.LoadSourceImage(file);
                await this.setImageControl(this.sourceImage, this.viewModel.SourceBitmap);
            }
        }

        private async void OpenMessageFileButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.pickMessageFile();
            if (file != null)
            {
                var fileExtension = file.FileType.ToLowerInvariant();

                if (fileExtension == ".png" || fileExtension == ".bmp")
                {
                    await this.viewModel.LoadMessageImage(file);
                    await this.setImageControl(this.messageImage, this.viewModel.MessageBitmap);

                    this.messageImage.Visibility = Visibility.Visible;
                    this.messagePreviewScrollViewer.Visibility = Visibility.Collapsed;
                    this.messagePreviewTextBlock.Text = string.Empty;
                }
                else if (fileExtension == ".txt")
                {
                    await this.viewModel.LoadMessageText(file);

                    this.messagePreviewTextBlock.Text = this.viewModel.MessageText ?? string.Empty;
                    this.messagePreviewScrollViewer.Visibility = Visibility.Visible;
                    this.messageImage.Visibility = Visibility.Collapsed;
                }
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
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

                // First, extract image (this sets MessageWidth/MessageHeight internally)
                SoftwareBitmap extractedBitmap = extractor.ExtractImage();

                // Now you can safely read width and height
                var msgWidth = extractor.Width;
                var msgHeight = extractor.Height;


                await this.SetImageControl(this.targetImage, extractedBitmap);

                Debug.WriteLine($"Extraction happened)");
=======
            try
            {
                this.viewModel.EncryptionUsed = this.encryptedRadioButton.IsChecked == true;
                this.viewModel.EmbedMessage();

                if (this.viewModel.TargetBitmap != null)
                {
                    await this.setImageControl(this.targetImage, this.viewModel.TargetBitmap);
                    this.targetImage.Visibility = Visibility.Visible;
                    this.messageScrollViewer.Visibility = Visibility.Collapsed;
                }

                if (this.viewModel.EncryptionUsed && this.viewModel.EncryptedPreviewBitmap != null)
                {
                    await this.setImageControl(this.encryptedOutputImage, this.viewModel.EncryptedPreviewBitmap);
                    this.encryptedOutputImage.Visibility = Visibility.Visible;
                }
                else
                {
                    this.encryptedOutputImage.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Embedding failed: {ex.Message}");
            }
        }

        private async void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = this.viewModel.ExtractMessage();

                if (result != null)
                {
                    await this.setImageControl(this.targetImage, result);
                    this.messageScrollViewer.Visibility = Visibility.Collapsed;
                    this.messageImage.Visibility = Visibility.Visible;
                    this.targetImage.Visibility = Visibility.Visible;
                }
                else if (!string.IsNullOrEmpty(this.viewModel.MessageText))
                {
                    this.messageTextBlock.Text = this.viewModel.MessageText;
                    this.messageScrollViewer.Visibility = Visibility.Visible;
                    this.messageImage.Visibility = Visibility.Collapsed;
                    this.targetImage.Visibility = Visibility.Collapsed;
                }
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Extraction failed: {ex.Message}");
            }
        }

<<<<<<< HEAD

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
=======
        private async void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.viewModel.SaveTargetImageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save failed: {ex.Message}");
            }
        }

        private async Task<StorageFile> pickImageFile()
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
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

<<<<<<< HEAD
        /// <summary>
        ///     Display a SoftwareBitmap in an Image control
        /// </summary>
        private async Task SetImageControl(Image control, SoftwareBitmap bitmap)
        {
            if (bitmap == null)
=======
        private async Task<StorageFile> pickMessageFile()
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

        private async Task setImageControl(Image control, SoftwareBitmap bitmap)
        {
            if (control == null || bitmap == null)
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
            {
                return;
            }

<<<<<<< HEAD
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
=======
            try
            {
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[setImageControl] Failed: {ex.Message}");
            }
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        }

        #endregion
    }
}