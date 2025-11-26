using System;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;
using GroupGPixelCrypt.Model.Image;

public sealed class Embedder
{
    #region Data members

    private readonly SoftwareBitmap messageImage;
    private readonly SoftwareBitmap sourceImage;

    #endregion

    #region Constructors

    public Embedder(SoftwareBitmap messageImage, SoftwareBitmap sourceImage)
    {
        this.messageImage =
            ImageManager.ConvertToCorrectFormat(messageImage ?? throw new ArgumentNullException(nameof(messageImage)));
        this.sourceImage =
            ImageManager.ConvertToCorrectFormat(sourceImage ?? throw new ArgumentNullException(nameof(sourceImage)));

        if (this.messageImage.PixelWidth > this.sourceImage.PixelWidth ||
            this.messageImage.PixelHeight > this.sourceImage.PixelHeight)
        {
            throw new ArgumentException("Message image file exceeds the dimensions of the source image. Cannot embed.");
        }
    }

    #endregion

    #region Methods

    public SoftwareBitmap EmbedMessage()
    {
        var srcPixels = PixelBgr8.FromSoftwareBitmap(this.sourceImage);
        var msgPixels = PixelL1.FromSoftwareBitmap(this.messageImage);

        var srcWidth = this.sourceImage.PixelWidth;
        var msgWidth = this.messageImage.PixelWidth;
        var msgHeight = this.messageImage.PixelHeight;

        srcPixels[0].Red = 123;
        srcPixels[0].Green = 123;
        srcPixels[0].Blue = 123;

        for (var y = 0; y < msgHeight; y++)
        {
            for (var x = 0; x < msgWidth; x++)
>>>>>>> 29a97f9b8f88ec95f974e16c43839cda8e332ba9
            {
                var srcIndex = y * srcWidth + x;

                if (srcIndex == 0)
                {
                    continue;
                }

                var msgIndex = y * msgWidth + x;

                var blue = srcPixels[srcIndex].Blue;
                var msgBit = msgPixels[msgIndex].Luma;

                srcPixels[srcIndex].Blue = (byte)((blue & 0xFE) | msgBit);
            }
        }

        var result = new SoftwareBitmap(BitmapPixelFormat.Bgra8, this.sourceImage.PixelWidth,
            this.sourceImage.PixelHeight,
            BitmapAlphaMode.Premultiplied);

        PixelBgr8.WriteToSoftwareBitmap(srcPixels, result);
        return result;
    }

    #endregion
}