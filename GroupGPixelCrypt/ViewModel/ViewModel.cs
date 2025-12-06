using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using GroupGPixelCrypt.Data;
using GroupGPixelCrypt.Decrypters;
using GroupGPixelCrypt.Embedders;
using GroupGPixelCrypt.Encrypters;
using GroupGPixelCrypt.Extractors;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Data members

        private SoftwareBitmap sourceBitmap;
        private SoftwareBitmap messageBitmap;
        private string messageText;
        private SoftwareBitmap targetBitmap;
        private SoftwareBitmap encryptedMessageBitmap;
        private bool encryptionUsed;
        private string keyword;
        private string encryptedMessage;

        #endregion

        #region Properties

        public SoftwareBitmap SourceBitmap
        {
            get => this.sourceBitmap;
            private set
            {
                this.sourceBitmap = value;
                this.notify(nameof(this.SourceBitmap), nameof(this.CanEmbed), nameof(this.CanExtract));
            }
        }

        public SoftwareBitmap MessageBitmap
        {
            get => this.messageBitmap;
            private set
            {
                this.messageBitmap = value;
                this.notify(nameof(this.MessageBitmap), nameof(this.CanEmbed));
            }
        }

        public string MessageText
        {
            get => this.messageText;
            private set
            {
                this.messageText = value;
                this.notify(nameof(this.MessageText), nameof(this.CanEmbed));
            }
        }

        public SoftwareBitmap TargetBitmap
        {
            get => this.targetBitmap;
            private set
            {
                this.targetBitmap = value;
                this.notify(nameof(this.TargetBitmap), nameof(this.CanSave));
            }
        }

        public SoftwareBitmap EncryptedMessageBitmap
        {
            get => this.encryptedMessageBitmap;
            private set
            {
                this.encryptedMessageBitmap = value;
                this.notify(nameof(this.EncryptedMessageBitmap));
            }
        }

        public bool EncryptionUsed
        {
            get => this.encryptionUsed;
            set
            {
                if (this.encryptionUsed != value)
                {
                    this.encryptionUsed = value;
                    this.notify(nameof(this.EncryptionUsed));
                }
            }
        }

        public bool UnencryptedMode
        {
            get => !this.EncryptionUsed;
            set
            {
                this.EncryptionUsed = !value;
                this.notify(nameof(this.UnencryptedMode), nameof(this.EncryptionUsed));
            }
        }

        public string Keyword
        {
            get => this.keyword;
            set
            {
                this.keyword = value;
                this.notify(nameof(this.Keyword));
            }
        }

        public string EncryptedMessage
        {
            get => this.encryptedMessage;
            private set
            {
                this.encryptedMessage = value;
                this.notify(nameof(this.EncryptedMessage));
            }
        }

        public int BitsPerChannel { get; set; } = 2;

        public bool CanEmbed =>
            this.SourceBitmap != null &&
            (this.MessageBitmap != null || !string.IsNullOrEmpty(this.MessageText));

        public bool CanExtract => this.SourceBitmap != null;
        public bool CanSave => this.TargetBitmap != null;

        #endregion

        #region Methods

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task LoadSourceImage(StorageFile file)
        {
            var manager = await ImageManager.FromImageFile(file);
            this.SourceBitmap = manager.SoftwareBitmap;
        }

        public async Task LoadMessageImage(StorageFile file)
        {
            var manager = await ImageManager.FromImageFile(file);
            this.MessageBitmap = manager.SoftwareBitmap;
        }

        public async Task LoadMessageText(StorageFile file)
        {
            this.MessageText = await FileIO.ReadTextAsync(file);
        }

        public void EmbedMessage()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.MessageText))
                {
                    var cleaned = cleanLettersOnly(this.MessageText);

                    string textToEmbed;
                    if (this.EncryptionUsed && !string.IsNullOrEmpty(this.Keyword))
                    {
                        var encrypter = new TextEncrypter();
                        this.EncryptedMessage = encrypter.EncryptMessage(this.Keyword, cleaned);

                        var idxKey = this.EncryptedMessage.IndexOf("#KEY#", StringComparison.Ordinal);
                        var idxEnd = this.EncryptedMessage.LastIndexOf("#-.-#", StringComparison.Ordinal);
                        textToEmbed = this.EncryptedMessage.Substring(idxKey + StegoConstants.KeyMarkerLength,
                            idxEnd - (idxKey + StegoConstants.KeyMarkerLength));
                    }
                    else
                    {
                        textToEmbed = cleaned;
                    }

                    var embedder = new TextEmbedder(textToEmbed, this.SourceBitmap, (byte)this.BitsPerChannel,
                        this.EncryptionUsed);
                    this.TargetBitmap = embedder.EmbedMessage();
                }
                else if (this.MessageBitmap != null)
                {
                    var toEmbed = this.MessageBitmap;

                    if (this.EncryptionUsed)
                    {
                        var padded = ImageManager.PadToMatch(this.MessageBitmap, this.SourceBitmap);
                        var encrypted = ImageEncrypter.SwapQuadrants(padded);
                        this.MessageBitmap = encrypted;
                        this.EncryptedMessageBitmap = encrypted;
                        toEmbed = encrypted;
                    }

                    var embedder = new ImageEmbedder(toEmbed, this.SourceBitmap, (byte)this.BitsPerChannel,
                        this.EncryptionUsed);
                    this.TargetBitmap = embedder.EmbedMessage();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ViewModel] EmbedMessage failed: {ex.Message}");
            }
        }

        public SoftwareBitmap ExtractMessage()
        {
            try
            {
                var header = HeaderManager.ReadHeader(PixelBgr8.FromSoftwareBitmap(this.SourceBitmap));
                if (!header.hasMessage)
                {
                    return null;
                }

                if (header.isText)
                {
                    var extractor = new TextExtractor(this.SourceBitmap);
                    var extractedBody = extractor.ExtractMessageText();

                    if (header.encryptionUsed)
                    {
                        if (!string.IsNullOrEmpty(this.Keyword))
                        {
                            this.EncryptedMessage = this.Keyword.ToUpperInvariant() + "#KEY#" + extractedBody + "#-.-#";
                            this.MessageText = TextDecrypter.DecryptBody(extractedBody, this.Keyword);
                        }
                        else
                        {
                            this.EncryptedMessage = extractedBody;
                            this.MessageText = extractedBody;
                        }
                    }
                    else
                    {
                        this.MessageText = extractedBody;
                    }

                    return null;
                }
                else
                {
                    var extractor = new ImageExtractor(this.SourceBitmap);
                    var extracted = extractor.ExtractMessageBitmap();

                    if (header.encryptionUsed && extracted != null)
                    {
                        return ImageDecrypter.DecryptQuadrants(extracted);
                    }

                    return extracted;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ViewModel] ExtractMessage failed: {ex.Message}");
                return null;
            }
        }

        public async Task SaveTargetImageAsync()
        {
            if (this.TargetBitmap == null)
            {
                return;
            }

            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeChoices.Add("PNG Image", new[] { ".png" });
            picker.SuggestedFileName = "embedded_output";

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    encoder.SetSoftwareBitmap(this.TargetBitmap);
                    await encoder.FlushAsync();
                }
            }
        }

        private void notify(params string[] names)
        {
            if (names == null)
            {
                return;
            }

            foreach (var n in names)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
            }
        }

        private static string cleanLettersOnly(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var upper = input.ToUpperInvariant();
            var buffer = new char[upper.Length];
            var count = 0;

            foreach (var c in upper)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    buffer[count++] = c;
                }
            }

            return new string(buffer, 0, count);
        }

        #endregion
    }
}