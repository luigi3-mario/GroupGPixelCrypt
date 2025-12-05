using System;
using System.Diagnostics;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Embedders
{
    /// <summary>
    /// Embeds a monochrome message image into a source image.
    /// </summary>
    public sealed class ImageEmbedder
    {
        private readonly SoftwareBitmap sourceImage;
        private readonly SoftwareBitmap messageImage;

        public ImageEmbedder(SoftwareBitmap messageImage, SoftwareBitmap sourceImage)
        {
            this.messageImage = ImageManager.ConvertToCorrectFormat(messageImage ?? throw new ArgumentNullException(nameof(messageImage)));
            this.sourceImage = ImageManager.ConvertToCorrectFormat(sourceImage ?? throw new ArgumentNullException(nameof(sourceImage)));

            Debug.WriteLine($"[ImageEmbedder] Source size={this.sourceImage.PixelWidth}x{this.sourceImage.PixelHeight}");
            Debug.WriteLine($"[ImageEmbedder] Message size={this.messageImage.PixelWidth}x{this.messageImage.PixelHeight}");

            if (this.messageImage.PixelWidth > this.sourceImage.PixelWidth ||
                this.messageImage.PixelHeight > this.sourceImage.PixelHeight)
                throw new ArgumentException("Message image exceeds source dimensions.");
        }

        public SoftwareBitmap EmbedMessage()
        {
            Debug.WriteLine("[ImageEmbedder] Starting EmbedMessage.");

            var srcPixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
            var msgPixels = PixelL1.FromSoftwareBitmap(this.messageImage);

            Debug.WriteLine($"[ImageEmbedder] srcPixels length={srcPixels?.Length}, msgPixels length={msgPixels?.Length}");

            // Pixel 0 marker
            srcPixels[0].Red = 123;
            srcPixels[0].Green = 123;
            srcPixels[0].Blue = 123;

            // Pixel 1 header: mark as image (Blue LSB = 0)
            srcPixels[1].Red = 0;
            srcPixels[1].Green = 0;
            srcPixels[1].Blue = 0;

            for (int y = 0; y < this.messageImage.PixelHeight; y++)
            {
                for (int x = 0; x < this.messageImage.PixelWidth; x++)
                {
                    int srcIndex = y * this.sourceImage.PixelWidth + x;
                    if (srcIndex == 0 || srcIndex == 1) continue;

                    int msgIndex = y * this.messageImage.PixelWidth + x;
                    byte msgBit = (byte)(msgPixels[msgIndex].Luma & 1);

                    srcPixels[srcIndex].Blue = (byte)((srcPixels[srcIndex].Blue & 0xFE) | msgBit);
                }
            }

            Debug.WriteLine("[ImageEmbedder] Finished embedding bits into source pixels.");

            var result = new SoftwareBitmap(
                BitmapPixelFormat.Bgra8,
                this.sourceImage.PixelWidth,
                this.sourceImage.PixelHeight,
                BitmapAlphaMode.Premultiplied);

            PixelBgr8.WriteToSoftwareBitmap(srcPixels, result);

            Debug.WriteLine("[ImageEmbedder] Returning embedded SoftwareBitmap.");
            return result;
        }
    }
}
