using GroupGPixelCrypt.Model.image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

    namespace GroupGPixelCrypt.Model
    {
        public sealed class ImageEmbedder
        {
            private readonly SoftwareBitmap sourceImage;
            private readonly SoftwareBitmap messageImage;
            private readonly bool encryptionUsed;

            public ImageEmbedder(SoftwareBitmap messageImage, SoftwareBitmap sourceImage, bool encryptionUsed = false)
            {
                this.messageImage = ImageManager.ConvertToCorrectFormat(messageImage ?? throw new ArgumentNullException(nameof(messageImage)));
                this.sourceImage = ImageManager.ConvertToCorrectFormat(sourceImage ?? throw new ArgumentNullException(nameof(sourceImage)));

                if (this.messageImage.PixelWidth > this.sourceImage.PixelWidth ||
                    this.messageImage.PixelHeight > this.sourceImage.PixelHeight)
                    throw new ArgumentException("Message image exceeds source dimensions.");

                this.encryptionUsed = encryptionUsed;
            }

            public SoftwareBitmap EmbedMessage()
            {
                var srcPixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
                var msgPixels = PixelL1.FromSoftwareBitmap(this.messageImage);

                // If encryption is requested, swap quadrants
                if (this.encryptionUsed)
                {
                    msgPixels = EncryptQuadrants(msgPixels, this.messageImage.PixelWidth, this.messageImage.PixelHeight);
                }

                // Pixel 0 marker
                srcPixels[0].Red = 123;
                srcPixels[0].Green = 123;
                srcPixels[0].Blue = 123;

                // Pixel 1 header: explicitly mark as image (Blue LSB = 0)
                srcPixels[1].Red = 0;
                srcPixels[1].Green = 0;
                srcPixels[1].Blue = 0; // force even, LSB=0

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

                var result = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                    this.sourceImage.PixelWidth,
                    this.sourceImage.PixelHeight,
                    BitmapAlphaMode.Premultiplied);

                PixelBgr8.WriteToSoftwareBitmap(srcPixels, result);
                return result;
            }

            // Quadrant swap encryption
            private PixelL1[] EncryptQuadrants(PixelL1[] pixels, int width, int height)
            {
                int halfWidth = width / 2;
                int halfHeight = height / 2;
                var encrypted = new PixelL1[pixels.Length];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex = y * width + x;
                        int newX = x, newY = y;

                        if (x < halfWidth && y < halfHeight) // Q1 → Q4
                        {
                            newX = x + halfWidth;
                            newY = y + halfHeight;
                        }
                        else if (x >= halfWidth && y < halfHeight) // Q2 → Q3
                        {
                            newX = x - halfWidth;
                            newY = y + halfHeight;
                        }
                        else if (x < halfWidth && y >= halfHeight) // Q3 → Q2
                        {
                            newX = x + halfWidth;
                            newY = y - halfHeight;
                        }
                        else if (x >= halfWidth && y >= halfHeight) // Q4 → Q1
                        {
                            newX = x - halfWidth;
                            newY = y - halfHeight;
                        }

                        int destIndex = newY * width + newX;
                        encrypted[destIndex] = pixels[srcIndex];
                    }
                }

                return encrypted;
            }
        }
    }



