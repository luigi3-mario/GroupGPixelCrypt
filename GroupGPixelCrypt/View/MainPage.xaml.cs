using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using GroupGPixelCrypt.Embedders;
using GroupGPixelCrypt.Encrypters;
using GroupGPixelCrypt.Model;
using GroupGPixelCrypt.Model.image;
using GroupGPixelCrypt.ViewModel;

namespace GroupGPixelCrypt.View
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
            var file = await this.pickImageFile();
            if (file != null)
            {
                await this.viewModel.LoadSourceImage(file);
                await this.setImageControl(this.sourceImage, this.viewModel.SourceBitmap);
            }
        }

        private async void OpenMessageFileButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.pickMessageFile(); // supports .png, .bmp, .txt
            if (file != null)
            {
                var ext = file.FileType?.ToLowerInvariant();

                if (ext == ".png" || ext == ".bmp")
                {
                    // Message is an image
                    await this.viewModel.LoadMessageImage(file);
                    await this.setImageControl(this.messageImage, this.viewModel.MessageBitmap);

                    this.messageImage.Visibility = Visibility.Visible;
                    this.messagePreviewScrollViewer.Visibility = Visibility.Collapsed;
                    this.messagePreviewTextBlock.Text = string.Empty;

                    Debug.WriteLine("Loaded message image.");
                }
                else if (ext == ".txt")
                {
                    // Message is text
                    await this.viewModel.LoadMessageText(file);

                    this.messagePreviewTextBlock.Text = this.viewModel.MessageText ?? string.Empty;
                    this.messagePreviewScrollViewer.Visibility = Visibility.Visible;
                    this.messageImage.Visibility = Visibility.Collapsed;

                    this.viewModel.MessageBitmap = null; // ensure text path is used

                    Debug.WriteLine("Loaded message text.");
                }
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.viewModel.SourceBitmap == null)
                    return;

                this.viewModel.EncryptionUsed = this.encryptedRadioButton.IsChecked == true;

                if (this.viewModel.MessageBitmap != null)
                {
                    // Step 1: Embed message image into source
                    var embedder = new ImageEmbedder(this.viewModel.MessageBitmap, this.viewModel.SourceBitmap);
                    var embeddedBitmap = embedder.EmbedMessage();
                    this.viewModel.SetTargetBitmap(embeddedBitmap);

                    // Show embedded image in Output grid
                    await this.setImageControl(this.targetImage, embeddedBitmap);
                    this.targetImage.Visibility = Visibility.Visible;
                    this.messageScrollViewer.Visibility = Visibility.Collapsed;

                    // Step 2: If encryption is selected, encrypt the embedded image
                    if (this.viewModel.EncryptionUsed)
                    {
                        Debug.WriteLine($"[Embed] embeddedBitmap is {(embeddedBitmap == null ? "NULL" : "OK")}.");
                        if (embeddedBitmap != null)
                        {
                            Debug.WriteLine(
                                $"[Embed] Size: {embeddedBitmap.PixelWidth}x{embeddedBitmap.PixelHeight}, Format={embeddedBitmap.BitmapPixelFormat}, Alpha={embeddedBitmap.BitmapAlphaMode}");
                        }

                        // Use PixelBgr8 to preserve color
                        var pixels = PixelBgr8.FromSoftwareBitmap(embeddedBitmap);
                        var encryptedPixels = ImageEncrypter.EncryptQuadrants(
                            pixels,
                            embeddedBitmap.PixelWidth,
                            embeddedBitmap.PixelHeight
                        );

                        // Rebuild a full-color SoftwareBitmap
                        var encryptedBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                            embeddedBitmap.PixelWidth,
                            embeddedBitmap.PixelHeight,
                            BitmapAlphaMode.Premultiplied);

                        PixelBgr8.WriteToSoftwareBitmap(encryptedPixels, encryptedBitmap);

                        // Show in encrypted output column
                        await this.setImageControl(this.encryptedOutputImage, encryptedBitmap);
                        this.encryptedOutputImage.Visibility = Visibility.Visible;
                        Debug.WriteLine(
                            $"[Encrypt] encryptedOutputImage.Source is {(this.encryptedOutputImage.Source == null ? "NULL" : "SET")}");

                        // Optionally promote to TargetBitmap if you want Save/Extract to use encrypted
                        this.viewModel.SetTargetBitmap(encryptedBitmap);
                    }

                }
                else if (!string.IsNullOrEmpty(this.viewModel.MessageText))
                {
                    // Text embedding path (unchanged)
                    var bpcc = await this.askBitsPerChannelAsync();
                    this.viewModel.BitsPerChannel = bpcc;

                    var embedder = new TextEmbedder(this.viewModel.MessageText, this.viewModel.SourceBitmap,
                        this.viewModel.BitsPerChannel, this.viewModel.EncryptionUsed
                    );

                    var embeddedBitmap = embedder.EmbedMessage();
                    this.viewModel.SetTargetBitmap(embeddedBitmap);

                    await this.setImageControl(this.targetImage, embeddedBitmap);
                    this.targetImage.Visibility = Visibility.Visible;
                    this.messageScrollViewer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Debug.WriteLine("No message selected. Load an image or a text file.");
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
                // Choose the bitmap that actually contains the embedded message
                var bitmapToExtract = this.viewModel.TargetBitmap ?? this.viewModel.SourceBitmap;
                if (bitmapToExtract == null)
                {
                    Debug.WriteLine("No bitmap available for extraction.");
                    return;
                }

                // Optional: log header to confirm flags
                this.LogHeader(bitmapToExtract);

                var pixels = PixelBgr8.FromSoftwareBitmap(bitmapToExtract);
                var isText = (pixels[1].Blue & 0x01) == 1;

                if (isText)
                {
                    var extractor = new TextExtractor(bitmapToExtract);
                    if (!extractor.HasEmbeddedMessage())
                    {
                        Debug.WriteLine("No embedded text message found.");
                        return;
                    }

                    var extractedText = extractor.ExtractMessageText();
                    this.messageTextBlock.Text = extractedText ?? string.Empty;

                    this.messageScrollViewer.Visibility = Visibility.Visible;
                    this.messageImage.Visibility = Visibility.Collapsed;
                    this.targetImage.Visibility = Visibility.Collapsed;

                    var encryptionFlag = (pixels[1].Red & 0x01) == 1;

                    Debug.WriteLine("Extracted text: " + extractedText);
                }
                else
                {
                    var extractor = new ImageExtractor(bitmapToExtract);
                    if (!extractor.HasEmbeddedMessage())
                    {
                        Debug.WriteLine("No embedded image message found.");
                        return;
                    }

                    var extractedBitmap = extractor.ExtractMessageBitmap();
                    this.viewModel.SetTargetBitmap(extractedBitmap);
                    await this.setImageControl(this.targetImage, extractedBitmap);

                    this.messageScrollViewer.Visibility = Visibility.Collapsed;
                    this.messageImage.Visibility = Visibility.Visible;
                    this.targetImage.Visibility = Visibility.Visible;

                    var encryptionFlag = (pixels[1].Red & 0x01) == 1;

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
            {
                return;
            }

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
                    {
                        encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                    }
                    else
                    {
                        encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    }

                    encoder.SetSoftwareBitmap(this.viewModel.TargetBitmap);
                    await encoder.FlushAsync();
                }
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

            // Allow both image and text for the message
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".txt");

            return await picker.PickSingleFileAsync();
        }

        private async Task setImageControl(Image control, SoftwareBitmap bitmap)
        {
            if (control == null) { Debug.WriteLine("[setImageControl] control is NULL"); return; }
            if (bitmap == null) { Debug.WriteLine("[setImageControl] bitmap is NULL"); return; }

            try
            {
                if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                    bitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    bitmap = SoftwareBitmap.Convert(bitmap,
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied);
                    Debug.WriteLine("[setImageControl] Converted bitmap to BGRA8 Premultiplied.");
                }

                var source = new SoftwareBitmapSource();
                await source.SetBitmapAsync(bitmap);
                control.Source = source;

                Debug.WriteLine("[setImageControl] Image set successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[setImageControl] Failed: {ex.Message}");
            }
        }


        /// <summary>
        ///     Popup dialog to select BPCC (1–8).
        /// </summary>
        private async Task<byte> askBitsPerChannelAsync()
        {
            var comboBox = new ComboBox
            {
                Width = 120,
                Margin = new Thickness(0, 10, 0, 0)
            };

            for (var i = 1; i <= 8; i++)
            {
                comboBox.Items.Add(i);
            }

            comboBox.SelectedIndex = 0; // default = 1

            var dialog = new ContentDialog
            {
                Title = "Select Bits Per Channel (1–8)",
                Content = comboBox,
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot // ensure dialog shows
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return (byte)(int)comboBox.SelectedItem;
            }

            return 1; // fallback default
        }

        private void LogHeader(SoftwareBitmap bmp)
        {
            try
            {
                var p = PixelBgr8.FromSoftwareBitmap(bmp);
                if (p.Length < 2)
                {
                    Debug.WriteLine("Header log: too few pixels.");
                    return;
                }

                Debug.WriteLine($"P0: R{p[0].Red} G{p[0].Green} B{p[0].Blue}");
                Debug.WriteLine($"P1: R{p[1].Red} G{p[1].Green} B{p[1].Blue} | BlueLSB={p[1].Blue & 1}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Header log failed: {ex.Message}");
            }
        }

        #endregion
    }
}