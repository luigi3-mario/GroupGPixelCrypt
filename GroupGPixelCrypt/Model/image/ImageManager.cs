using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using GroupGPixelCrypt.Model.image;

public class ImageManager
{
    public SoftwareBitmap SoftwareBitmap { get; private set; }
    public const int BytesPerPixelBgra8 = 4;

    public static async Task<ImageManager> FromImageFile(StorageFile imageFile)
    {
        IRandomAccessStream stream = await imageFile.OpenAsync(FileAccessMode.Read);
        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
        SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
        return new ImageManager(softwareBitmap);
    }

    private ImageManager(SoftwareBitmap softwareBitmap)
    {
        softwareBitmap = ConvertToCorrectFormat(softwareBitmap);
        Debug.WriteLine($"{softwareBitmap.PixelHeight}x{softwareBitmap.PixelWidth}");
        this.SoftwareBitmap = softwareBitmap;
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
}