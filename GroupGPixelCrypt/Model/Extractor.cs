using System;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

public class Extractor
{
    private readonly SoftwareBitmap embeddedImage;

    public int MessageWidth { get; private set; }
    public int MessageHeight { get; private set; }

    public Extractor(SoftwareBitmap embeddedImage)
    {
        if (embeddedImage == null) throw new ArgumentNullException(nameof(embeddedImage));
        this.embeddedImage = embeddedImage;
    }

    /// <summary>
    /// Extracts the hidden message bytes from the embedded image.
    /// Also sets MessageWidth and MessageHeight from the header.
    /// </summary>
    public byte[] ExtractMessageBytes()
    {
        var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);

        if (pixels.Length < 3)
            throw new InvalidOperationException("Image is too small to contain a header.");

        // Read header
        var secondPixel = pixels[1];
        var thirdPixel = pixels[2];

        this.MessageWidth = secondPixel.Red | (thirdPixel.Blue << 8);
        this.MessageHeight = secondPixel.Alpha | (thirdPixel.Green << 8);
        int totalMessageBytes = MessageWidth * MessageHeight;

        // Step 1: Extract bits from Blue channel, skipping header
        List<byte> bits = new List<byte>();
        for (int i = 3; i < pixels.Length; i++)
        {
            bits.Add((byte)(pixels[i].Blue & 0x01));
        }

        // Step 2: Pack bits into bytes
        byte[] messageBytes = new byte[totalMessageBytes];
        for (int b = 0; b < totalMessageBytes; b++)
        {
            byte val = 0;
            for (int bit = 0; bit < 8; bit++)
            {
                int bitIndex = b * 8 + bit;
                if (bitIndex < bits.Count)
                    val |= (byte)(bits[bitIndex] << bit);
            }
            messageBytes[b] = val;
        }

        return messageBytes;
    }

    /// <summary>
    /// Convenience method to directly get a SoftwareBitmap of the extracted message.
    /// </summary>
    public SoftwareBitmap ExtractMessageBitmap()
    {
        var messageBytes = ExtractMessageBytes();
        var pixels = PixelL1.FromByteArray(messageBytes);
        return PixelL1.ToSoftwareBitmap(pixels, MessageWidth, MessageHeight);
    }
}
