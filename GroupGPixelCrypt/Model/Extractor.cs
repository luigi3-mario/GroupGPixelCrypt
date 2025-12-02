using System;
using System.Collections.Generic;
using System.Text;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model;
using GroupGPixelCrypt.Model.image;

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
    ///     High-level extractor that detects header and returns either an image (SoftwareBitmap) or a text string.
    ///     Use ExtractMessageBitmap() for image extraction, ExtractMessageText() for text extraction.
    /// </summary>
    public bool HasEmbeddedMessage()
    {
        var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
        if (pixels.Length == 0)
        {
            return false;
        }

        var marker = pixels[0];
        return marker.Red == 123 && marker.Green == 123 && marker.Blue == 123;
    }

    /// <summary>
    ///     Extracts a monochrome bitmap from the embedded image by reading the LSB of the blue channel for every pixel.
    ///     This returns a full-size bitmap (same dimensions as the source) as required by the spec.
    /// </summary>
    public SoftwareBitmap ExtractMessageBitmap()
    {
        var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
        if (pixels.Length == 0)
        {
            throw new InvalidOperationException("Image is empty.");
        }

        // Check marker
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

    /// <summary>
    ///     Extracts embedded text. Uses header on pixel 1 to determine BPCC and encryption flag.
    ///     Stops when it finds the end-of-message symbol (0).
    ///     Returns the decoded alphabetic-only text (no terminator).
    /// </summary>
    public string ExtractMessageText()
    {
        var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
        if (pixels.Length == 0)
        {
            throw new InvalidOperationException("Image is empty.");
        }

        // Marker check
        var marker = pixels[0];
        if (marker.Red != 123 || marker.Green != 123 || marker.Blue != 123)
        {
            throw new InvalidOperationException("No embedded message found (marker pixel missing).");
        }

        // Read header from pixel 1
        var r = pixels[1].Red;
        var g = pixels[1].Green;
        var b = pixels[1].Blue;

        var encryptionFlag = (r & 0x01) == 1;
        var bpcc = Math.Max(1, Math.Min(8, (int)g)); // clamp to 1..8
        var isText = (b & 0x01) == 1;

        if (!isText)
        {
            throw new InvalidOperationException("Header indicates embedded message is not text.");
        }

        // Build bit stream by reading pixels starting at index 2
        var bitStream = new List<int>();

        var totalPixels = pixels.Length;
        for (var pix = 2; pix < totalPixels; pix++)
        {
            // For each channel R,G,B, read lower bpcc bits in LSB-first order
            this.AppendLowerBitsLsbFirst(pixels[pix].Red, bpcc, bitStream);
            this.AppendLowerBitsLsbFirst(pixels[pix].Green, bpcc, bitStream);
            this.AppendLowerBitsLsbFirst(pixels[pix].Blue, bpcc, bitStream);
        }

        // Now interpret bitStream as sequence of 5-bit symbols (MSB first per symbol)
        var symbols = new List<int>();
        var bitPos = 0;
        while (bitPos + 5 <= bitStream.Count)
        {
            var sym = 0;
            // reconstruct MSB-first
            for (var i = 0; i < 5; i++)
            {
                sym = (sym << 1) | (bitStream[bitPos++] & 1);
            }

            symbols.Add(sym);
            if (sym == 0) // terminator
            {
                break;
            }
        }

        // Convert symbols to letters (A=1..Z=26). Stop at terminator (0)
        var sb = new StringBuilder();
        foreach (var s in symbols)
        {
            if (s == 0)
            {
                break;
            }

            if (s >= 1 && s <= 26)
            {
                var c = (char)('A' + s - 1);
                sb.Append(c);
            }
            // ignore unexpected values
        }

        var extracted = sb.ToString();

        // If encryptionFlag is true -> decrypt here (not implemented). For now just return extracted.
        // If you implement Vigenere (or other) decryption, do it here and return decrypted + show encrypted separately in UI.
        if (encryptionFlag)
        {
            // Placeholder: return the encrypted string as well, or perform decryption if you have the key.
            // throw new NotImplementedException("Encryption indicated in header but decryption not implemented.");
        }

        return extracted;
    }

    // Helper: append the lower 'bpcc' bits of 'channel' to bitStream in LSB-first order
    private void AppendLowerBitsLsbFirst(byte channel, int bpcc, List<int> bitStream)
    {
        for (var i = 0; i < bpcc; i++)
        {
            var bit = (channel >> i) & 1;
            bitStream.Add(bit);
        }
    }

    #endregion
}

