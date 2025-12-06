using System;
using System.Diagnostics;
using GroupGPixelCrypt.Data;

namespace GroupGPixelCrypt.Model.image
{
    public static class HeaderManager
    {
        #region Methods

        public static void WriteHeader(PixelBgr8[] pixels, bool isText, byte bpcc, bool encryptionUsed)
        {
            if (pixels == null || pixels.Length < StegoConstants.ReservedHeaderPixels)
            {
                throw new ArgumentException("Pixel array too small for header.");
            }

            pixels[0] = new PixelBgr8(
                StegoConstants.MarkerValue,
                StegoConstants.MarkerValue,
                StegoConstants.MarkerValue,
                StegoConstants.AlphaOpaque);

            var second = pixels[1];

            second.Red = (byte)((second.Red & StegoConstants.ClearLsbMask) | (encryptionUsed ? 1 : 0));
            second.Green = bpcc;
            second.Blue = (byte)((second.Blue & StegoConstants.ClearLsbMask) | (isText ? 1 : 0));

            pixels[1] = second;

            Debug.WriteLine(
                $"[HeaderManager] Header written: isText={isText}, bpcc={bpcc}, encryption={encryptionUsed}");
        }

        public static (bool hasMessage, bool isText, byte bpcc, bool encryptionUsed) ReadHeader(PixelBgr8[] pixels)
        {
            if (pixels == null || pixels.Length < StegoConstants.ReservedHeaderPixels)
            {
                throw new ArgumentException("Pixel array too small for header.");
            }

            var hasMessage = pixels[0].Red == StegoConstants.MarkerValue &&
                             pixels[0].Green == StegoConstants.MarkerValue &&
                             pixels[0].Blue == StegoConstants.MarkerValue;

            var encryptionUsed = (pixels[1].Red & StegoConstants.LsbMask) == 1;
            var bpcc = pixels[1].Green;
            var isText = (pixels[1].Blue & StegoConstants.LsbMask) == 1;

            Debug.WriteLine(
                $"[HeaderManager] Read header: hasMessage={hasMessage}, isText={isText}, bpcc={bpcc}, encryption={encryptionUsed}");
            return (hasMessage, isText, bpcc, encryptionUsed);
        }

        #endregion
    }
}