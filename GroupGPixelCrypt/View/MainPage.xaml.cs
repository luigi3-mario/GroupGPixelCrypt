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
using GroupGPixelCrypt.Model.image; // needed for PixelBgr8

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

        private async void OpenMessageFileButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.PickImageFile();
            if (file != null)
            {
                await this.viewModel.LoadMessageImage(file);
                await this.SetImageControl(this.messageImage, this.viewModel.MessageBitmap);
                this.messageImage.Visibility = Visibility.Visible;
                this.messageScrollViewer.Visibility = Visibility.Collapsed;
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.viewModel.EmbedMessage();
                if (this.viewModel.TargetBitmap != null)
                {
                    await this.SetImageControl(this.targetImage, this.viewModel.TargetBitmap);
                    Debug.WriteLine("Message embedded successfully.");
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
                if (this.viewModel.SourceBitmap == null)
                    return;

                var extractor = new Extractor(this.viewModel.SourceBitmap);

                if (!extractor.HasEmbeddedMessage())
                {
                    Debug.WriteLine("No embedded message found.");
                    return;
                }

                // Read header from pixel 1 to decide type
                var pixels = PixelBgr8.FromSoftwareBitmap(this.viewModel.SourceBitmap);
                bool isText = (pixels[1].Blue & 0x01) == 1;

                if (isText)
                {
                    string extractedText = extractor.ExtractMessageText();
                    this.messageTextBlock.Text = extractedText;
                    this.messageScrollViewer.Visibility = Visibility.Visible;
                    this.messageImage.Visibility = Visibility.Collapsed;
                    Debug.WriteLine("Extracted text: " + extractedText);
                }
                else
                {
                    var extractedBitmap = extractor.ExtractMessageBitmap();
                    this.viewModel.SetTargetBitmap(extractedBitmap);
                    await this.SetImageControl(this.targetImage, extractedBitmap);
                    this.messageScrollViewer.Visibility = Visibility.Collapsed;
                    this.messageImage.Visibility = Visibility.Visible;
                    Debug.WriteLine("Extraction completed (image).");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Extraction failed: {ex.Message}");
            }
        }

        private async void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.TargetBitmap == null)
                return;

            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "output_image"
            };
            picker.FileTypeChoices.Add("PNG Image", new[] { ".png" });
            picker.FileTypeChoices.Add("BMP Image", new[] { ".bmp" });

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder;

                    if (file.FileType.ToLower() == ".bmp")
                        encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                    else
                        encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                    encoder.SetSoftwareBitmap(this.viewModel.TargetBitmap);
                    await encoder.FlushAsync();
                }
            }
        }

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

        private async Task SetImageControl(Image control, SoftwareBitmap bitmap)
        {
            if (bitmap == null)
                return;

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
