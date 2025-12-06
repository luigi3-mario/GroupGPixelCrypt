<<<<<<< HEAD
﻿using GroupGPixelCrypt.Model;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace GroupGPixelCrypt.ViewModel
{
    public class MainViewModel
    {
        public SoftwareBitmap SourceBitmap { get; private set; }
        public SoftwareBitmap MessageBitmap { get; private set; }
        public SoftwareBitmap TargetBitmap { get; private set; }

        /// <summary>
        /// Load a source image file into SourceBitmap
        /// </summary>
=======
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using GroupGPixelCrypt.Embedders;
using GroupGPixelCrypt.Helpers;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Data members

        private SoftwareBitmap sourceBitmap;
        private SoftwareBitmap messageBitmap;
        private SoftwareBitmap targetBitmap;
        private SoftwareBitmap encryptedPreviewBitmap;
        private string messageText;

        private readonly EmbedService embedService = new EmbedService();
        private readonly ExtractService extractService = new ExtractService();
        private readonly SaveService saveService = new SaveService();

        #endregion

        #region Properties

        public SoftwareBitmap SourceBitmap
        {
            get => this.sourceBitmap;
            private set
            {
                if (this.setProperty(ref this.sourceBitmap, value, nameof(this.SourceBitmap)))
                {
                    this.OnPropertyChanged(nameof(this.CanEmbed));
                    this.OnPropertyChanged(nameof(this.CanExtract));
                }
            }
        }

        public SoftwareBitmap MessageBitmap
        {
            get => this.messageBitmap;
            set
            {
                if (this.setProperty(ref this.messageBitmap, value, nameof(this.MessageBitmap)))
                {
                    this.OnPropertyChanged(nameof(this.CanEmbed));
                }
            }
        }

        public SoftwareBitmap TargetBitmap
        {
            get => this.targetBitmap;
            private set
            {
                if (this.setProperty(ref this.targetBitmap, value, nameof(this.TargetBitmap)))
                {
                    this.OnPropertyChanged(nameof(this.CanSave));
                    this.OnPropertyChanged(nameof(this.CanExtract));
                }
            }
        }

        public SoftwareBitmap EncryptedPreviewBitmap
        {
            get => this.encryptedPreviewBitmap;
            private set =>
                this.setProperty(ref this.encryptedPreviewBitmap, value, nameof(this.EncryptedPreviewBitmap));
        }

        public string MessageText
        {
            get => this.messageText;
            set
            {
                if (this.setProperty(ref this.messageText, value, nameof(this.MessageText)))
                {
                    this.OnPropertyChanged(nameof(this.CanEmbed));
                }
            }
        }

        public byte BitsPerChannel { get; set; } = 1;
        public bool EncryptionUsed { get; set; } = false;

        public bool CanEmbed => this.SourceBitmap != null &&
                                (this.MessageBitmap != null || !string.IsNullOrEmpty(this.MessageText));

        public bool CanExtract => this.SourceBitmap != null || this.TargetBitmap != null;

        public bool CanSave => this.TargetBitmap != null;

        #endregion

        #region Methods

        public event PropertyChangedEventHandler PropertyChanged;

>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        public async Task LoadSourceImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
<<<<<<< HEAD
                SourceBitmap = manager.SoftwareBitmap;
            }
=======
                this.SourceBitmap = manager.SoftwareBitmap;
            }
        }

        public async Task LoadMessageImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                this.MessageBitmap = manager.SoftwareBitmap;
                this.MessageText = null;
            }
        }

        public async Task LoadMessageText(StorageFile file)
        {
            if (file != null)
            {
                this.MessageText = await FileIO.ReadTextAsync(file);
                this.MessageBitmap = null;
            }
        }

        public void SetTargetBitmap(SoftwareBitmap bitmap)
        {
            this.TargetBitmap = bitmap;
        }

        public void EmbedMessage()
        {
            if (this.SourceBitmap == null)
            {
                return;
            }

            if (this.MessageBitmap != null)
            {
                var padded = ImageManager.PadToMatch(this.MessageBitmap, this.SourceBitmap);
                var finalMessage = padded;

                if (this.EncryptionUsed)
                {
                    finalMessage = this.embedService.EncryptMessageBitmap(padded);
                    this.EncryptedPreviewBitmap = finalMessage;
                }
                else
                {
                    this.EncryptedPreviewBitmap = null;
                }

                var embedder = new ImageEmbedder(finalMessage, this.SourceBitmap, 1, this.EncryptionUsed);
                var embeddedBitmap = embedder.EmbedMessage();
                this.SetTargetBitmap(embeddedBitmap);
            }
            else if (!string.IsNullOrEmpty(this.MessageText))
            {
                this.EncryptedPreviewBitmap = null;
                var embeddedBitmap = this.embedService.EmbedText(this.SourceBitmap, this.MessageText,
                    this.BitsPerChannel, this.EncryptionUsed);
                this.SetTargetBitmap(embeddedBitmap);
            }
        }

        public SoftwareBitmap ExtractMessage()
        {
            var bitmapToExtract = this.TargetBitmap ?? this.SourceBitmap;
            if (bitmapToExtract == null)
            {
                return null;
            }

            var (text, image) = this.extractService.Extract(bitmapToExtract);
            if (text != null)
            {
                this.MessageText = text;
            }

            if (image != null)
            {
                this.MessageBitmap = image;
            }

            return image;
        }

        public async Task SaveTargetImageAsync()
        {
            if (this.TargetBitmap != null)
            {
                await this.saveService.SaveAsync(this.TargetBitmap);
            }
        }

        private bool setProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
>>>>>>> f9088511ec8202dfa11ee13c0b476e53e6bc4ef6
        }

        /// <summary>
        /// Load a message image file into MessageBitmap
        /// </summary>
        public async Task LoadMessageImage(StorageFile file)
        {
            if (file != null)
            {
                var manager = await ImageManager.FromImageFile(file);
                MessageBitmap = manager.SoftwareBitmap;
            }
        }

        /// <summary>
        /// Embed the message into the source image
        /// </summary>
        public void EmbedMessage(int bitsPerChannel = 1)
        {
            if (SourceBitmap != null && MessageBitmap != null)
            {
                Embedder embedder = new Embedder(MessageBitmap, SourceBitmap);
                TargetBitmap = embedder.EmbedMessage();
            }
        }
    }
}