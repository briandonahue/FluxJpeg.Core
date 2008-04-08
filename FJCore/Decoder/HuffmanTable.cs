/// Copyright (c) 2008 Jeffrey Powers for Fluxcapacity Open Source.
/// Under the MIT License, details: License.txt.

using System;

using FluxJpeg.Core.IO;

namespace FluxJpeg.Core.Decoder
{
    internal class HuffmanTable
    {
        public static int HUFFMAN_MAX_TABLES = 4;

        private short[] huffcode = new short[256];
        private short[] huffsize = new short[256];
        private short[] valptr = new short[16];
        private short[] mincode = {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,-1,-1};
        private short[] maxcode = {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};

        private short[] huffval;
        private short[] bits;

        public static byte JPEG_DC_TABLE = 0;
        public static byte JPEG_AC_TABLE = 1;

        private short lastk = 0;

        internal HuffmanTable(JpegHuffmanTable table)
        {
            // WARNING: TODO: was huffcode instead of huffval!!?

            huffval = table.Values;
            bits = table.Lengths;

            GenerateSizeTable();
            GenerateCodeTable();
            GenerateDecoderTables();
        }

        /// <summary>See Figure C.1</summary>
        private void GenerateSizeTable()
        {
            short index = 0;
            for (short i = 0; i < bits.Length; i++)
            {
                for (short j = 0; j < bits[i]; j++)
                {
                    huffsize[index] = (short)(i + 1);
                    index++;
                }
            }
            lastk = index;
        }

        /// <summary>See Figure C.2</summary>
        private void GenerateCodeTable()
        {
            short k = 0;
            short si = huffsize[0];
            short code = 0;
            for (short i = 0; i < huffsize.Length; i++)
            {
                while (huffsize[k] == si)
                {
                    huffcode[k] = code;
                    code++;
                    k++;
                }
                code <<= 1;
                si++;
            }
        }

        /// <summary>See figure F.15</summary>
        private void GenerateDecoderTables()
        {
            short bitcount = 0;
            for (int i = 0; i < 16; i++)
            {
                if (bits[i] != 0)
                    valptr[i] = bitcount;
                for (int j = 0; j < bits[i]; j++)
                {
                    if (huffcode[j + bitcount] < mincode[i] || mincode[i] == -1)
                        mincode[i] = huffcode[j + bitcount];

                    if (huffcode[j + bitcount] > maxcode[i])
                        maxcode[i] = huffcode[j + bitcount];
                }
                if (mincode[i] != -1)
                    valptr[i] = (short)(valptr[i] - mincode[i]);
                bitcount += bits[i];
            }
        }

        /// <summary>Figure F.12</summary>
        public static int Extend(int diff, int t)
        {
            // here we use bitshift to implement 2^ ... 
            // NOTE: Math.Pow returns 0 for negative powers, which occassionally happen here!

            int Vt = 1 << t - 1;
            // WAS: int Vt = (int)Math.Pow(2, (t - 1));

            if (diff < Vt)
            {
                Vt = (-1 << t) + 1;
                diff = diff + Vt;
            }
            return diff;
        }

        /// <summary>Figure F.16 - Reads the huffman code bit-by-bit.</summary>
        public int Decode(JPEGBinaryReader JPEGStream)
        {
            int i = 0;
            short code = (short)JPEGStream.ReadBits(1);
            while (code > maxcode[i])
            {
                i++;
                code <<= 1;
                code |= (short)JPEGStream.ReadBits(1);
            }
            int val = huffval[code + (valptr[i])];
            if (val < 0)
                val = 256 + val;
            return val;
        }
    }

}
