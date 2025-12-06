using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Model
{
    public class Embedder
    {
        #region Data members

        private IList<byte> message;
        private SoftwareBitmap visibleImage;
        private TextManager textManager;
        private byte bitsPerChannel;
        private Mode mode;
        private int offset;
        private delegate PixelBgr8 EmbedIntoPixelDelegate(byte[] messageBytes, int index, PixelBgr8 coverPixel);
        private EmbedIntoPixelDelegate embedIntoPixel;
        private const int headerSize = 2;
        private int currentIndex;

        #endregion

        #region Properties

        private byte Mask => Model.Mask.setMaskForUpper(this.bitsPerChannel);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Embedder"/> class.
        /// </summary>
        /// <param name="messageString">The message string.</param>
        /// <param name="bitsPerChannel">The bits per channel.</param>
        /// <param name="visibleImage">The visible image.</param>
        public Embedder(String messageString, byte bitsPerChannel, SoftwareBitmap visibleImage)
        {
            this.textManager = new TextManager();
            this.message = this.textManager.ConvertMessageToBytes(messageString, bitsPerChannel);
            this.visibleImage = visibleImage;
            this.bitsPerChannel = bitsPerChannel;
            this.mode = Mode.Text;
            this.initializEmbedIntoPixelDelegate();
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
            this.initializEmbedIntoPixelDelegate();
        }


        #endregion

        #region Methods

        /// <summary>
        /// Embeds the message.
        /// </summary>
        /// <returns>The image with the message hidden inside</returns>
        public SoftwareBitmap EmbedMessage()
        {
            byte[] messageArray = this.message.ToArray();
            PixelBgr8[] pixelBgr8Array = PixelBgr8.FromSoftwareBitmap(this.visibleImage);
            PixelBgr8[] headerPixels = this.generateHeader();
            Array.Copy(headerPixels, 0, pixelBgr8Array, 0, headerPixels.Length);
            if (this.mode == Mode.Image)
            {
                this.offset = headerSize;
            }
            else
            {
                this.offset = 0;
            }
            for (this.currentIndex = 0; this.currentIndex + this.offset < messageArray.Length; this.currentIndex++)
            {
                pixelBgr8Array[this.currentIndex + headerSize] = this.embedIntoPixel(messageArray, this.currentIndex + this.offset, pixelBgr8Array[this.currentIndex + headerSize]);
            }
            return PixelBgr8.WriteToSoftwareBitmap(pixelBgr8Array, this.visibleImage);
        }

        private void initializEmbedIntoPixelDelegate()
        {
            PixelBgr8 EmbedPixelIntoPixel(byte[] pixelBytes, int index, PixelBgr8 coverPixel)
            {
                byte newBlue = (byte)((coverPixel.Blue & this.Mask) | pixelBytes[index]);
                this.currentIndex++;
                return new PixelBgr8(newBlue, coverPixel.Green, coverPixel.Red, coverPixel.Alpha);
            }

            PixelBgr8 EmbedTextIntoPixel(byte[] messageBytes, int index, PixelBgr8 coverPixel)
            {
                byte newBlue = (byte)((coverPixel.Blue & this.Mask) | messageBytes[index]);
                byte newGreen = (byte)((coverPixel.Blue & this.Mask) | messageBytes[index + 1]);
                byte newRed = (byte)((coverPixel.Blue & this.Mask) | messageBytes[index + 2]);
                this.currentIndex += 3;
                return new PixelBgr8(newBlue, newGreen, newRed, coverPixel.Alpha);
            }

            if (this.mode == Mode.Image)
            {
                this.embedIntoPixel = EmbedPixelIntoPixel;
            }
            else
            {
                this.embedIntoPixel = EmbedTextIntoPixel;
            }
        }

        private PixelBgr8[] generateHeader()
        {
            PixelBgr8 firstPixel = new PixelBgr8(123, 123, 123, 255);
            byte modeSignal = this.mode == Mode.Text ? (byte)1 : (byte)0;
            PixelBgr8 secondPixel = new PixelBgr8(modeSignal, this.bitsPerChannel, 0, 255);//TODO: change red for encryption later
            return new PixelBgr8[] { firstPixel, secondPixel };
        }

        #endregion
    }
}
