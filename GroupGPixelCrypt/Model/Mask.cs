using System;

namespace GroupGPixelCrypt.Model
{
    public class Mask
    {

        public Mask()
        {
        }

        public static byte SetMaskForUpper(byte bitsPerChannel)
        {
            switch (bitsPerChannel)
            {
                case 1:
                    return 0b11111110;
                case 2:
                    return 0b11111100;
                case 3:
                    return 0b11111000;
                case 4:
                    return 0b11110000;
                case 5:
                    return 0b11100000;
                case 6:
                    return 0b11000000;
                case 7:
                    return 0b10000000;
                case 8:
                    return 0b00000000;
                default:
                    throw new ArgumentException("bitsPerChannel must be between 1 and 8");
            }
        }

        public static byte SetMaskForLower(byte bitsPerChannel)
        {
            return (byte)(~(int)(SetMaskForUpper(bitsPerChannel)));
        }
    }
}