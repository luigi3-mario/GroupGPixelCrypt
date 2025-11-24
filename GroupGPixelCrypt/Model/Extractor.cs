using System;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;
using GroupGPixelCrypt.Model.Image;

public class Extractor
{
    #region Data members

    private readonly SoftwareBitmap embeddedImage;

    #endregion

    #region Constructors

    public Extractor(SoftwareBitmap embeddedImage)
    {
        if (embeddedImage == null)
        {
            throw new ArgumentNullException(nameof(embeddedImage));
        }

        this.embeddedImage = ImageManager.ConvertToCorrectFormat(embeddedImage);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Extracts the hidden message as a monochrome SoftwareBitmap.
    /// </summary>
    public SoftwareBitmap ExtractMessageBitmap()
    {
        var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);

        if (pixels.Length == 0)
        {
            throw new InvalidOperationException("Image is empty.");
        }

        var marker = pixels[0];
        if (marker.Red != 123 || marker.Green != 123 || marker.Blue != 123)
        {
            throw new InvalidOperationException("No embedded message found (marker pixel missing).");
        }

        var width = this.embeddedImage.PixelWidth;
        var height = this.embeddedImage.PixelHeight;

        var resultPixels = new PixelL1[pixels.Length];

        for (var i = 0; i < pixels.Length; i++)
        {
            var bit = (byte)(pixels[i].Blue & 0x01);
            resultPixels[i] = new PixelL1(bit);
        }

        return PixelL1.ToSoftwareBitmap(resultPixels, width, height);
    }

    #endregion
}