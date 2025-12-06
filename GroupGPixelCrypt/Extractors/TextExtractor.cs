using System;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using GroupGPixelCrypt.Data;
using GroupGPixelCrypt.Model.image;

namespace GroupGPixelCrypt.Extractors
{
    public sealed class TextExtractor
    {
        private readonly SoftwareBitmap source;

        public TextExtractor(SoftwareBitmap source)
        {
            this.source = ImageManager.ConvertToCorrectFormat(source ?? throw new ArgumentNullException(nameof(source)));
        }

        public string ExtractMessageText()
        {
            var pixels = PixelBgr8.FromSoftwareBitmap(this.source);
            var header = HeaderManager.ReadHeader(pixels);
            var bpcc = header.bpcc;

            var bits = new List<int>();
            for (var i = StegoConstants.ReservedHeaderPixels; i < pixels.Length; i++)
            {
                readChannel(bits, pixels[i].Red, bpcc);
                readChannel(bits, pixels[i].Green, bpcc);
                readChannel(bits, pixels[i].Blue, bpcc);
            }

            var symbols = new List<int>();
            for (var i = 0; i + StegoConstants.SymbolBitLength <= bits.Count; i += StegoConstants.SymbolBitLength)
            {
                var symbol = 0;
                for (var j = 0; j < StegoConstants.SymbolBitLength; j++)
                {
                    symbol = (symbol << 1) | bits[i + j];
                }
                if (symbol == StegoConstants.TerminatorSymbol)
                {
                    break;
                }
                symbols.Add(symbol);
            }

            var chars = new char[symbols.Count];
            for (var i = 0; i < symbols.Count; i++)
            {
                chars[i] = (char)(StegoConstants.FirstLetter + symbols[i] - 1);
            }

            return new string(chars);
        }

        private static void readChannel(List<int> bits, byte channel, byte bpcc)
        {
            for (var i = bpcc - 1; i >= 0; i--)
            {
                bits.Add((channel >> i) & 1);
            }
        }
    }
}
