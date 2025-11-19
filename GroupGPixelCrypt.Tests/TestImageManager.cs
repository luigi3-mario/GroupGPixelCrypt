using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GroupGPixelCrypt.Model;
using System.Text;
using Windows.Storage;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Tests
{
    [TestClass]
    public class TestImageManager
    {
        [TestMethod]
        public void TestMonochromeImage()
        {
            byte[] expectedBytes = new Byte[]{0,1,0,1,0,1,0,1,0};
            StorageFile imageFile = Windows.ApplicationModel.Package.Current.InstalledLocation
                .GetFileAsync("Assets\\checker.png").AsTask().Result;
            ImageManager imageManager = ImageManager.FromImageFile(imageFile).Result;
            PixelL1[] pixelL1Array = PixelL1.FromSoftwareBitmap(imageManager.SoftwareBitmap);
            byte[] actualBytes = PixelL1.ToByteArray(pixelL1Array);
            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }
    }
}
