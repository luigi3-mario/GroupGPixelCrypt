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
using GroupGPixelCrypt.Model.image;

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

        public SoftwareBitmap resize(SoftwareBitmap image, int newWidth, int newHeight)
        {
            PixelBgr8 [] pixels = PixelBgr8.FromSoftwareBitmap(image);
            List<PixelBgr8> resizedPixels = new List<PixelBgr8>();
            IList<PixelBgr8> currentRow = new List<PixelBgr8>();
            for (int i = 0; i < image.PixelHeight; i++)
            {
                int pixelIndex = i * image.PixelWidth;
                currentRow = pixels.ToList().GetRange(pixelIndex, pixelIndex + image.PixelWidth);
                currentRow = this.resizeRow(currentRow, newWidth);
                resizedPixels.AddRange(currentRow);
            }

            byte[]resizedBytes = PixelBgr8.ToByteArray(resizedPixels.ToArray());
            SoftwareBitmap newImage = SoftwareBitmap.CreateCopyFromBuffer(resizedBytes.AsBuffer(),
                BitmapPixelFormat.Bgra8, newWidth, newHeight);
            return newImage;
        }

        private IList<PixelBgr8> resizeRow(IList<PixelBgr8> row, int newWidth)
        {
            for (int i = 0; i < newWidth - row.Count; i++)
            {
                row.Add(PixelBgr8.whitePixel());
            }

            return row;
        }
        #endregion
    }
}
