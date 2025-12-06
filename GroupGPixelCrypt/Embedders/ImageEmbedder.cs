using System;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Data;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Embedders
{
    public sealed class ImageEmbedder
    {
        #region Data members

        private readonly SoftwareBitmap sourceImage;
        private readonly SoftwareBitmap messageImage;
        private readonly byte bpcc;
        private readonly bool encryptionUsed;

        #endregion

        #region Constructors

        public ImageEmbedder(SoftwareBitmap messageImage, SoftwareBitmap sourceImage, byte bpcc = 1,
            bool encryptionUsed = false)
        {
            this.messageImage =
                ImageManager.ConvertToCorrectFormat(messageImage ??
                                                    throw new ArgumentNullException(nameof(messageImage)));
            this.sourceImage =
                ImageManager.ConvertToCorrectFormat(sourceImage ??
                                                    throw new ArgumentNullException(nameof(sourceImage)));

            if (bpcc < 1 || bpcc > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bpcc));
            }

            this.bpcc = bpcc;
            this.encryptionUsed = encryptionUsed;

            this.validateDimensions();
        }

        #endregion

        #region Methods

        public SoftwareBitmap EmbedMessage()
        {
            var sourcePixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
            var msgPixels = PixelL1.FromSoftwareBitmap(this.messageImage);

            HeaderManager.WriteHeader(sourcePixels, false, this.bpcc, this.encryptionUsed);

            for (var y = 0; y < this.messageImage.PixelHeight; y++)
            {
                for (var x = 0; x < this.messageImage.PixelWidth; x++)
                {
                    var sourceIndex = getIndex(x, y, this.sourceImage.PixelWidth);
                    if (sourceIndex == 0 || sourceIndex == 1)
                    {
                        continue;
                    }

                    var msgIndex = getIndex(x, y, this.messageImage.PixelWidth);
                    var msgBit = extractMessageBit(msgPixels[msgIndex]);

                    sourcePixels[sourceIndex].Blue = embedBitIntoBlue(sourcePixels[sourceIndex].Blue, msgBit);
                }
            }

            var result = new SoftwareBitmap(
                BitmapPixelFormat.Bgra8,
                this.sourceImage.PixelWidth,
                this.sourceImage.PixelHeight,
                BitmapAlphaMode.Premultiplied);

            PixelBgr8.WriteToSoftwareBitmap(sourcePixels, result);

            return result;
        }

        private void validateDimensions()
        {
            if (this.messageImage.PixelWidth > this.sourceImage.PixelWidth ||
                this.messageImage.PixelHeight > this.sourceImage.PixelHeight)
            {
                throw new ArgumentException("Message image exceeds source dimensions.");
            }
        }

        private static byte extractMessageBit(PixelL1 pixel)
        {
            return (byte)(pixel.Luma & 1);
        }

        private static byte embedBitIntoBlue(byte blueChannel, byte bit)
        {
            return (byte)((blueChannel & StegoConstants.ClearLsbMask) | bit);
        }

        private static int getIndex(int x, int y, int stride)
        {
            return y * stride + x;
        }

        #endregion
    }
}