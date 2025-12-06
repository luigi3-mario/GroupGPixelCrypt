using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using GroupGPixelCrypt.Data;

namespace GroupGPixelCrypt.Model.image
{
    public class ImageManager
    {
        #region Properties

        public SoftwareBitmap SoftwareBitmap { get; private set; }

        #endregion

        #region Constructors

        private ImageManager(SoftwareBitmap softwareBitmap)
        {
            softwareBitmap = ConvertToCorrectFormat(softwareBitmap);
            Debug.WriteLine($"{softwareBitmap.PixelHeight}x{softwareBitmap.PixelWidth}");
            this.SoftwareBitmap = softwareBitmap;
        }

        #endregion

        #region Methods

        public static async Task<ImageManager> FromImageFile(StorageFile imageFile)
        {
            var stream = await imageFile.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            return new ImageManager(softwareBitmap);
        }

        public static SoftwareBitmap ConvertToCorrectFormat(SoftwareBitmap softwareBitmap)
        {
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(
                    softwareBitmap,
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied);
            }

            return softwareBitmap;
        }

        public static SoftwareBitmap PadToMatch(
            SoftwareBitmap messageBitmap,
            SoftwareBitmap sourceBitmap,
            byte padBlue = 0,
            byte padGreen = 0,
            byte padRed = 0,
            byte padAlpha = StegoConstants.AlphaOpaque)
        {
            if (messageBitmap == null)
            {
                throw new ArgumentNullException(nameof(messageBitmap));
            }

            if (sourceBitmap == null)
            {
                throw new ArgumentNullException(nameof(sourceBitmap));
            }

            messageBitmap = ConvertToCorrectFormat(messageBitmap);
            sourceBitmap = ConvertToCorrectFormat(sourceBitmap);

            var targetWidth = sourceBitmap.PixelWidth;
            var targetHeight = sourceBitmap.PixelHeight;

            if (messageBitmap.PixelWidth > targetWidth || messageBitmap.PixelHeight > targetHeight)
            {
                throw new ArgumentException("Message image exceeds source dimensions.");
            }

            var padded = new SoftwareBitmap(BitmapPixelFormat.Bgra8, targetWidth, targetHeight,
                BitmapAlphaMode.Premultiplied);
            var rawOut = new byte[targetWidth * targetHeight * StegoConstants.BytesPerPixelBgra8];

            for (var i = 0; i < rawOut.Length; i += StegoConstants.BytesPerPixelBgra8)
            {
                rawOut[i + 0] = padBlue;
                rawOut[i + 1] = padGreen;
                rawOut[i + 2] = padRed;
                rawOut[i + 3] = padAlpha;
            }

            var rawMsg = new byte[messageBitmap.PixelWidth * messageBitmap.PixelHeight *
                                  StegoConstants.BytesPerPixelBgra8];
            messageBitmap.CopyToBuffer(rawMsg.AsBuffer());

            var msgStride = messageBitmap.PixelWidth * StegoConstants.BytesPerPixelBgra8;
            var outStride = targetWidth * StegoConstants.BytesPerPixelBgra8;

            var copyWidth = messageBitmap.PixelWidth;
            var copyHeight = messageBitmap.PixelHeight;

            for (var y = 0; y < copyHeight; y++)
            {
                var srcRow = y * msgStride;
                var dstRow = y * outStride;

                for (var x = 0; x < copyWidth; x++)
                {
                    var srcIdx = srcRow + x * StegoConstants.BytesPerPixelBgra8;
                    var dstIdx = dstRow + x * StegoConstants.BytesPerPixelBgra8;

                    rawOut[dstIdx + 0] = rawMsg[srcIdx + 0];
                    rawOut[dstIdx + 1] = rawMsg[srcIdx + 1];
                    rawOut[dstIdx + 2] = rawMsg[srcIdx + 2];
                    rawOut[dstIdx + 3] = rawMsg[srcIdx + 3];
                }
            }

            padded.CopyFromBuffer(rawOut.AsBuffer());
            return padded;
        }

        #endregion
    }
}