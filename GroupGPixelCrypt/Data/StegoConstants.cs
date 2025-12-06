namespace GroupGPixelCrypt.Data
{
    public static class StegoConstants
    {
        public const byte MinBitsPerChannel = 1;
        public const byte MaxBitsPerChannel = 8;
        public const int ReservedHeaderPixels = 2;
        public const int ChannelsPerPixel = 3;
        public const int SymbolBitLength = 5;
        public const char FirstLetter = 'A';
        public const int TerminatorSymbol = 0;
        public const int MaxLetterSymbol = 26;

        public const byte MarkerValue = 123;
        public const byte LsbMask = 0x01;
        public const byte ClearLsbMask = 0xFE;
        public const byte AlphaOpaque = 255;

        public const int BytesPerPixelBgra8 = 4;
    }
}