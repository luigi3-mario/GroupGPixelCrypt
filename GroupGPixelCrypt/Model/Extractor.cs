using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Model
{
    /// <summary>
    /// Extracts hidden data from an image
    /// </summary>
    public class Extractor
    {
        #region Data members

        private Mode mode;
        private PixelBgr8[] pixels;
        private byte bitsPerChannel;
        private StringBuilder extractedStringMessage;
        private const int BitsPerChar = 8;
        private const int HeaderLength = 2;
        private int bitsExtracted;
        private ushort currentValue;

        #endregion

        #region Properties
        public int Width { get; }
        public int Height { get; }

        private byte Mask => Model.Mask.SetMaskForLower(this.bitsPerChannel);

        #endregion

        #region Constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="Extractor"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public Extractor(SoftwareBitmap sourceImage)
        {
            this.pixels = PixelBgr8.FromSoftwareBitmap(sourceImage);
            this.bitsExtracted = 0;
            this.extractedStringMessage = new StringBuilder();
            this.setMode();
            this.Width = sourceImage.PixelWidth;
            this.Height = sourceImage.PixelHeight;
        }

        public void setMode()
        {
            PixelBgr8 firstPixel = this.pixels[0];
            PixelBgr8 secondPixel = this.pixels[1];
            if (firstPixel.Blue != 123 || firstPixel.Green != 123 || firstPixel.Blue != 123)
            {
                throw new ActionNotSupportedException("This image has no secret message!");
                
            }
            byte modeByte = (byte)(secondPixel.Blue & Model.Mask.SetMaskForLower(1));
            if (modeByte == 0)
            {
                this.mode = Mode.Image;
            }
            else
            {
                this.mode = Mode.Text;
            }

            this.bitsPerChannel = secondPixel.Green;

        }

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
        public SoftwareBitmap ExtractImage()
        {
            PixelL1 [] extractedPixels = new PixelL1[(this.pixels.Length)];
            for(int i = 0; i < this.pixels.Length; i++)
            {
                PixelBgr8 pixel = this.pixels[i];
                byte color = (byte)(pixel.Blue & this.Mask);
                extractedPixels[i] = new PixelL1(color);
            }
            byte[] byteArray = PixelL1.ToByteArray(extractedPixels);
            SoftwareBitmap result = new SoftwareBitmap(BitmapPixelFormat.Bgra8, this.Width, this.Height);
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
    }
}
