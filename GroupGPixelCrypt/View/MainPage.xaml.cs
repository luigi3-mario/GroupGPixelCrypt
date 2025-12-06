using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupGPixelCrypt.View
{
    public sealed partial class MainPage
    {
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
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Extraction failed: {ex.Message}");
            }
        }

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
            {
                return;
            }

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
        }

        #endregion
    }
}