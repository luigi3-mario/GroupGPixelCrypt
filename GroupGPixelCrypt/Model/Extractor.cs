using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
=======
>>>>>>> 29a97f9b8f88ec95f974e16c43839cda8e332ba9
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

<<<<<<< HEAD
        private Mode mode;
        private PixelBgr8[] pixels;
        private byte bitsPerChannel;
        private StringBuilder extractedStringMessage;
        private const int BitsPerChar = 8;
        private const int HeaderLength = 2;
        private int bitsExtracted;
        private ushort currentValue;
        private int width;
        private int height;
=======
    /// <summary>
    /// Extracts the hidden message bytes from the embedded image.
    /// Also sets MessageWidth and MessageHeight from the header.
    /// </summary>
    public byte[] ExtractMessageBytes()
    {
        var pixels = PixelBgr8.FromSoftwareBitmap(this.embeddedImage);
>>>>>>> 29a97f9b8f88ec95f974e16c43839cda8e332ba9

        if (pixels.Length < 3)
            throw new InvalidOperationException("Image is too small to contain a header.");

        // Read header
        var secondPixel = pixels[1];
        var thirdPixel = pixels[2];

<<<<<<< HEAD
        private byte Mask => Model.Mask.SetMaskForLower(this.bitsPerChannel);
=======
        this.MessageWidth = secondPixel.Red | (thirdPixel.Blue << 8);
        this.MessageHeight = secondPixel.Alpha | (thirdPixel.Green << 8);
        int totalMessageBytes = MessageWidth * MessageHeight;
>>>>>>> 29a97f9b8f88ec95f974e16c43839cda8e332ba9

        // Step 1: Extract bits from Blue channel, skipping header
        List<byte> bits = new List<byte>();
        for (int i = 3; i < pixels.Length; i++)
        {
<<<<<<< HEAD
            this.pixels = PixelBgr8.FromSoftwareBitmap(sourceImage);
            this.bitsExtracted = 0;
            this.extractedStringMessage = new StringBuilder();
            this.setMode();
            this.width = sourceImage.PixelWidth;
            this.height = sourceImage.PixelHeight;
=======
            bits.Add((byte)(pixels[i].Blue & 0x01));
>>>>>>> 29a97f9b8f88ec95f974e16c43839cda8e332ba9
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

<<<<<<< HEAD
        /// <summary>
        /// Extracts the string.
        /// </summary>
        /// <returns></returns>
        public String ExtractString()
        {
            this.extractedStringMessage = new StringBuilder();
            this.currentValue = 0;
            for (int i = HeaderLength; i < this.pixels.Length; i++)
            {
                PixelBgr8 currentPixel = this.pixels[i];
                if (this.extractStringFromPixelAndCheckIfDone(currentPixel))
                {
                    this.extractedStringMessage.ToString().Substring(0, this.extractedStringMessage.Length - 5);
                }
            }
            return this.extractedStringMessage.ToString();
        }

        private bool extractStringFromPixelAndCheckIfDone(PixelBgr8 currentPixel)
        {
            foreach (byte channel in currentPixel.Channels)
            {
                byte extractedBits = (byte)(channel & this.Mask);
                this.currentValue = (ushort)(this.currentValue << this.bitsPerChannel);
                this.currentValue = (ushort)(this.currentValue | extractedBits);
                byte currentChar = (byte)(this.bitsExtracted % (byte.MaxValue + 1));
                this.bitsExtracted += this.bitsPerChannel;
                if (this.bitsExtracted >= BitsPerChar)
                {
                    this.extractedStringMessage.Append((char)currentChar);
                    if (this.isFinishedExtractingString())
                    {
                        return true;
                    }
                    this.bitsExtracted -= BitsPerChar;
                    this.currentValue = (ushort)(this.currentValue >> BitsPerChar);
                }
            }
            return false;
        }

        /// <summary>
        /// Extracts the image.
        /// </summary>
        /// <returns></returns>
        public SoftwareBitmap extractImage()
        {
            PixelL1 [] extractedPixels = new PixelL1[(this.pixels.Length)];
            for(int i = 0; i < this.pixels.Length; i++)
            {
                PixelBgr8 pixel = this.pixels[i];
                byte color = (byte)(pixel.Blue & this.Mask);
                extractedPixels[i] = new PixelL1(color);
            }
            byte[] byteArray = PixelL1.ToByteArray(extractedPixels);
            SoftwareBitmap result = new SoftwareBitmap(BitmapPixelFormat.Bgra8, this.width, this.height);
            result.CopyFromBuffer(byteArray.AsBuffer());
            return result;
        }

        private bool isFinishedExtractingString()
        {
            if(this.extractedStringMessage.Length >= 5)
            {
                string lastFiveChars = this.extractedStringMessage.ToString().Substring(this.extractedStringMessage.Length - 5, 5);
                return lastFiveChars.Equals("#-.-#");
            }
            return false;
        }

        #endregion
=======
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
>>>>>>> 29a97f9b8f88ec95f974e16c43839cda8e332ba9
    }
}
