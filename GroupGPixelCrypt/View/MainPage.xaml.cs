using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using GroupGPixelCrypt.ViewModel;

namespace GroupGPixelCrypt.View
{
    public sealed partial class MainPage
    {
        #region Properties

        public MainViewModel ViewModel { get; } = new MainViewModel();

        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this.ViewModel;
        }

        #endregion

        #region Methods

        private async void OpenSourceImageButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.pickImageFile();
            if (file != null)
            {
                await this.ViewModel.LoadSourceImage(file);
                await this.setImageControl(this.sourceImage, this.ViewModel.SourceBitmap);
            }
        }

        private async void OpenMessageFileButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.pickMessageFile();
            if (file != null)
            {
                var ext = file.FileType.ToLowerInvariant();
                if (ext == ".png" || ext == ".bmp")
                {
                    await this.ViewModel.LoadMessageImage(file);
                    await this.setImageControl(this.messageImage, this.ViewModel.MessageBitmap);
                    this.messageImage.Visibility = Visibility.Visible;
                    this.messagePreviewScrollViewer.Visibility = Visibility.Collapsed;
                    this.messagePreviewTextBlock.Text = string.Empty;
                }
                else if (ext == ".txt")
                {
                    await this.ViewModel.LoadMessageText(file);
                    this.messagePreviewTextBlock.Text = this.ViewModel.MessageText ?? string.Empty;
                    this.messagePreviewScrollViewer.Visibility = Visibility.Visible;
                    this.messageImage.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ViewModel.EncryptionUsed && !string.IsNullOrEmpty(this.ViewModel.MessageText))
                {
                    var inputBox = new TextBox { PlaceholderText = "Keyword" };
                    var dialog = new ContentDialog
                    {
                        Title = "Enter Encryption Keyword",
                        Content = inputBox,
                        PrimaryButtonText = "OK",
                        CloseButtonText = "Cancel"
                    };

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        this.ViewModel.Keyword = inputBox.Text;
                    }
                    else
                    {
                        return;
                    }
                }

                this.ViewModel.EmbedMessage();

                if (this.ViewModel.TargetBitmap != null)
                {
                    await this.setImageControl(this.targetImage, this.ViewModel.TargetBitmap);
                    this.targetImage.Visibility = Visibility.Visible;
                    this.messageScrollViewer.Visibility = Visibility.Collapsed;
                }

                if (this.ViewModel.EncryptedMessageBitmap != null)
                {
                    await this.setImageControl(this.encryptedImage, this.ViewModel.EncryptedMessageBitmap);
                    this.encryptedImage.Visibility = Visibility.Visible;
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
                var result = this.ViewModel.ExtractMessage();

                if (result != null)
                {
                    await this.setImageControl(this.targetImage, result);
                    this.messageScrollViewer.Visibility = Visibility.Collapsed;
                    this.messageImage.Visibility = Visibility.Visible;
                    this.targetImage.Visibility = Visibility.Visible;
                }
                else if (!string.IsNullOrEmpty(this.ViewModel.MessageText))
                {
                    this.messageTextBlock.Text = this.ViewModel.MessageText;
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
                await this.ViewModel.SaveTargetImageAsync();
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
                    bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
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