using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using GroupGPixelCrypt.Model;

namespace GroupGPixelCrypt.ViewModel
{
    public class ViewModel
    {
        #region Data Members

        private ImageManager imageManager;

        #endregion

        #region Properties

        public SoftwareBitmap Pixels => this.imageManager?.SoftwareBitmap;

        #endregion

        #region Constructors

        public ViewModel()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the image.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        public async Task UpdateImage(StorageFile imageFile)
        {
            this.imageManager = await ImageManager.FromImageFile(imageFile);
        }

        #endregion
    }
}
