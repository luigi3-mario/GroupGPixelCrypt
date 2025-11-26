using GroupGPixelCrypt.Model.image;
using GroupGPixelCrypt.Model;
using System;
using Windows.Graphics.Imaging;

public sealed class Embedder
{
    private readonly SoftwareBitmap messageImage;
    private readonly SoftwareBitmap sourceImage;

    public Embedder(SoftwareBitmap messageImage, SoftwareBitmap sourceImage)
    {
        this.messageImage = ImageManager.ConvertToCorrectFormat(messageImage ?? throw new ArgumentNullException(nameof(messageImage)));
        this.sourceImage = ImageManager.ConvertToCorrectFormat(sourceImage ?? throw new ArgumentNullException(nameof(sourceImage)));

        if (this.messageImage.PixelWidth > this.sourceImage.PixelWidth ||
            this.messageImage.PixelHeight > this.sourceImage.PixelHeight)
        {
            throw new ArgumentException("Message image file exceeds the dimensions of the source image. Cannot embed.");
        }
    }

    public SoftwareBitmap EmbedMessage()
    {
        var srcPixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
        var msgPixels = PixelL1.FromSoftwareBitmap(this.messageImage);

        int srcWidth = sourceImage.PixelWidth;
        int msgWidth = messageImage.PixelWidth;
        int msgHeight = messageImage.PixelHeight;

        // ✅ Mark first pixel as (123,123,123)
        srcPixels[0].Red = 123;
        srcPixels[0].Green = 123;
        srcPixels[0].Blue = 123;

        // ✅ Embed message bits starting from pixel index 1 (skip marker)
        for (int y = 0; y < msgHeight; y++)
        {
            for (int x = 0; x < msgWidth; x++)
            {
                int srcIndex = y * srcWidth + x;
                if (srcIndex == 0) continue; // skip marker

                int msgIndex = y * msgWidth + x;

                byte blue = srcPixels[srcIndex].Blue;
                byte msgBit = msgPixels[msgIndex].Luma; // 0 or 1

                srcPixels[srcIndex].Blue = (byte)((blue & 0xFE) | msgBit);
            }
        }

        var result = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                                        sourceImage.PixelWidth,
                                        sourceImage.PixelHeight,
                                        BitmapAlphaMode.Premultiplied);

        PixelBgr8.WriteToSoftwareBitmap(srcPixels, result);
        return result;
    }
}
