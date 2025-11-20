using System;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model;
using GroupGPixelCrypt.Model.image;
using System.Diagnostics;

public class Embedder
{
    private readonly byte[] messageBytes;
    private readonly SoftwareBitmap visibleImage;
    private readonly byte bitsPerChannel;
    private readonly Mode mode;

    public Embedder(SoftwareBitmap messageImage, SoftwareBitmap visibleImage)
    {
        if (messageImage == null) throw new ArgumentNullException(nameof(messageImage));
        if (visibleImage == null) throw new ArgumentNullException(nameof(visibleImage));

        this.bitsPerChannel = 1;
        this.visibleImage = visibleImage;
        this.mode = Mode.Image;

        // Convert message image to L1 pixel array and then to byte array
        var pixelL1Array = PixelL1.FromSoftwareBitmap(messageImage);
        this.messageBytes = PixelL1.ToByteArray(pixelL1Array);
    }

    public SoftwareBitmap EmbedMessage()
    {
        var pixels = PixelBgr8.FromSoftwareBitmap(this.visibleImage);

        // Create header and insert at start of pixel array
        var header = GenerateHeader();
        Array.Copy(header, 0, pixels, 0, header.Length);

        int pixelIndex = header.Length; // start embedding after header

        // Convert message bytes into bits
        var messageBits = new System.Collections.Generic.List<byte>();
        foreach (byte b in messageBytes)
        {
<<<<<<< HEAD
            this.textManager = new TextManager();
            this.message = this.textManager.GetMessage(messageString, bitsPerChannel);
            this.visibleImage = visibleImage;
            this.bitsPerChannel = bitsPerChannel;
            this.mode = Mode.Text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Embedder"/> class.
        /// </summary>
        /// <param name="messageImage">The message image.</param>
        /// <param name="visibleImage">The visible image.</param>
        public Embedder(SoftwareBitmap messageImage, SoftwareBitmap visibleImage)
        {
            
            this.bitsPerChannel = 1;
            this.visibleImage = visibleImage;
            PixelL1[] pixelL1Array = PixelL1.FromSoftwareBitmap(messageImage);
            this.message = PixelL1.ToByteArray(pixelL1Array).ToList();
            this.mode = Mode.Image;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Embeds the message.
        /// </summary>
        /// <returns>The image with the message hidden inside</returns>
        public SoftwareBitmap embedMessage()
        {
            PixelBgr8[] pixelBgr8Array = PixelBgr8.FromSoftwareBitmap(this.visibleImage);
            PixelBgr8[] headerPixels = this.generateHeader();
            Array.Copy(headerPixels, 0, pixelBgr8Array, 0, headerPixels.Length);
            if (this.mode == Mode.Image)
=======
            for (int i = 0; i < 8; i++)
>>>>>>> 29a97f9b8f88ec95f974e16c43839cda8e332ba9
            {
                messageBits.Add((byte)((b >> i) & 1)); // extract each bit
            }
        }

        if (messageBits.Count > pixels.Length - header.Length)
        {
            Debug.WriteLine("Warning: Message is too large to embed fully in this image!");
        }

        // Embed each bit of the message into the LSB of the pixel blue channel
        for (int bitIndex = 0; bitIndex < messageBits.Count && pixelIndex < pixels.Length; bitIndex++, pixelIndex++)
        {
            byte bit = messageBits[bitIndex];
            PixelBgr8 original = pixels[pixelIndex];

            pixels[pixelIndex] = new PixelBgr8(
                (byte)((original.Blue & 0xFE) | bit),
                original.Green,
                original.Red,
                original.Alpha
            );

           
        }

        return PixelBgr8.WriteToSoftwareBitmap(pixels, this.visibleImage);
    }


    private PixelBgr8[] GenerateHeader()
    {
        int messageWidth = (int)Math.Ceiling(Math.Sqrt(messageBytes.Length));
        int messageHeight = messageWidth;

        // First pixel: marker
        var firstPixel = new PixelBgr8(123, 123, 123, 255);

        // Second pixel: mode and bits per channel, plus low bytes of width/height
        byte modeSignal = (byte)(this.mode == Mode.Text ? 1 : 0);
        var secondPixel = new PixelBgr8(
            modeSignal,                 // Blue: mode
            this.bitsPerChannel,        // Green: bits per channel
            (byte)(messageWidth & 0xFF),  // Red: low byte width
            (byte)(messageHeight & 0xFF)  // Alpha: low byte height
        );

        // Third pixel: high bytes of width/height
        var thirdPixel = new PixelBgr8(
            (byte)((messageWidth >> 8) & 0xFF),   // Blue: high byte width
            (byte)((messageHeight >> 8) & 0xFF),  // Green: high byte height
            0, 0
        );

        return new[] { firstPixel, secondPixel, thirdPixel };
    }
}
