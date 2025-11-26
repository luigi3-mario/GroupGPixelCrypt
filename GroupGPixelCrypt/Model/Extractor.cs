using GroupGPixelCrypt.Model.image;
using GroupGPixelCrypt.Model;
using System;
using Windows.Graphics.Imaging;

public class Extractor
{
    private readonly SoftwareBitmap embeddedImage;

    public Extractor(SoftwareBitmap embeddedImage)
    {
        if (embeddedImage == null) throw new ArgumentNullException(nameof(embeddedImage));
        this.embeddedImage = ImageManager.ConvertToCorrectFormat(embeddedImage);
    }

    /// <summary>
    /// Extracts the hidden message as a monochrome SoftwareBitmap.
    /// </summary>
    public SoftwareBitmap ExtractMessageBitmap()
    {
        var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);

        if (pixels.Length == 0)
            throw new InvalidOperationException("Image is empty.");

        // ✅ Check marker pixel
        var marker = pixels[0];
        if (marker.Red != 123 || marker.Green != 123 || marker.Blue != 123)
            throw new InvalidOperationException("No embedded message found (marker pixel missing).");

        int width = embeddedImage.PixelWidth;
        int height = embeddedImage.PixelHeight;

        var resultPixels = new PixelL1[pixels.Length];

        // ✅ Extract LSB from blue channel for every pixel
        for (int i = 0; i < pixels.Length; i++)
        {
            byte bit = (byte)(pixels[i].Blue & 0x01);
            resultPixels[i] = new PixelL1(bit);
        }

        // ✅ Convert to monochrome bitmap
        return PixelL1.ToSoftwareBitmap(resultPixels, width, height);
    }
}