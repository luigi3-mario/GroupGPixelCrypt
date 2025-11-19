using System;
using System.Collections.Generic;
using System.Linq;
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

        #endregion

        #region Properties

        private byte Mask => Model.Mask.setMaskForUpper(this.bitsPerChannel);

        #endregion

        #region Constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="Extractor"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public Extractor(SoftwareBitmap sourceImage)
        {

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

        #endregion
    }
}
