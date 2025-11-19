using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Audio;
using Windows.Storage;
using Windows.Storage.Streams;

namespace GroupGPixelCrypt.Model
{
    public class ImageManager
    {

        #region Properties

        public SoftwareBitmap SoftwareBitmap { get; private set; }
        public const int BytesPerPixelBgra8 = 4;

        #endregion

        #region Constructors

        public static async Task<ImageManager> FromImageFile(StorageFile imageFile)
        {
            IRandomAccessStream stream = await imageFile.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            return new ImageManager(softwareBitmap);
        }

        private ImageManager(SoftwareBitmap softwareBitmap)
        {
            ConvertToCorrectFormat(softwareBitmap);
            Debug.WriteLine(softwareBitmap.PixelHeight.ToString() + softwareBitmap.PixelWidth.ToString());
            this.SoftwareBitmap = softwareBitmap;
        }

        /// <summary>
        /// Converts to correct format.
        /// </summary>
        /// <param name="softwareBitmap">The software bitmap.</param>
        /// <returns></returns>
        public static SoftwareBitmap ConvertToCorrectFormat(SoftwareBitmap softwareBitmap)
        {
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            return softwareBitmap;
        }
        #endregion
    }
}
