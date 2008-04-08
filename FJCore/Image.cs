﻿/// Copyright (c) 2008 Jeffrey Powers for Fluxcapacity Open Source.
/// Under the MIT License, details: License.txt.

using System;
#if WIN
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace FluxJpeg.Core
{
    public struct ColorModel
    {
        public ColorSpace colorspace;
        public bool Opaque;
    }

    public enum ColorSpace { Gray, YCbCr, RGB }

    public class Image
    {
        private ColorModel _cm;
        private byte[][,] _raster;

        public byte[][,] Raster { get { return _raster; } }
        public ColorModel ColorModel { get { return _cm; } }

        /// <summary> X density (dots per inch).</summary>
        public double DensityX { get; set; }
        /// <summary> Y density (dots per inch).</summary>
        public double DensityY { get; set; }

        public int ComponentCount { get { return _raster.Length; } }

        /// <summary>
        /// Converts the colorspace of an image (in-place)
        /// </summary>
        /// <param name="cs">Colorspace to convert into</param>
        /// <returns>Self</returns>
        public Image ChangeColorSpace(ColorSpace cs)
        {
            // Colorspace is already correct
            if (_cm.colorspace == cs) return this;

            byte[] ycbcr = new byte[3];

            if (_cm.colorspace == ColorSpace.YCbCr && cs == ColorSpace.RGB)
            {
                byte[] rgb = new byte[3];

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        ycbcr[0] = (byte)_raster[0][x, y]; // 0 is LUMA
                        ycbcr[1] = (byte)_raster[1][x, y]; // 1 is BLUE
                        ycbcr[2] = (byte)_raster[2][x, y];

                        YCbCr.toRGB(ycbcr, rgb);

                        _raster[0][x, y] = rgb[0];
                        _raster[1][x, y] = rgb[1];
                        _raster[2][x, y] = rgb[2];
                    }

                _cm.colorspace = ColorSpace.RGB;
            }
            else if (_cm.colorspace == ColorSpace.Gray && cs == ColorSpace.YCbCr)
            {
                // To convert to YCbCr, we just add two 128-filled chroma channels

                byte[,] Cb = new byte[width, height];
                byte[,] Cr = new byte[width, height];

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        Cb[x, y] = 128; Cr[x, y] = 128;
                    }

                _raster = new byte[][,] { _raster[0], Cb, Cr };

                _cm.colorspace = ColorSpace.YCbCr;
            }
            else if (_cm.colorspace == ColorSpace.Gray && cs == ColorSpace.RGB)
            {
                ChangeColorSpace(ColorSpace.YCbCr);
                ChangeColorSpace(ColorSpace.RGB);
            }

            return this;
        }

        private int width; private int height;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Image(ColorModel cm, byte[][,] raster)
        {
            width = raster[0].GetLength(0);
            height = raster[0].GetLength(1);

            _cm = cm;
            _raster = raster;
        }

        public static byte[][,] CreateRaster(int width, int height, int bands)
        {
            // Create the raster
            byte[][,] raster = new byte[bands][,];
            for (int b = 0; b < bands; b++)
                raster[b] = new byte[width, height];
            return raster;
        }

        delegate void ConvertColor(byte[] colorIn, byte[] colorOut);

        #if WIN
        public Bitmap ToBitmap()
        {
            ConvertColor ColorConverter;

            switch(_cm.colorspace)
            {
                case ColorSpace.YCbCr:
                    ColorConverter = YCbCr.toRGB;
                    break;
                default:
                    throw new Exception("Colorspace not supported yet.");
            }

            int _width = width;
            int _height = height;

            Bitmap bitmap = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);

            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] outColor = new byte[3];
            byte[] inColor = new byte[3];

            unsafe
            {
                int i = 0;

                byte* ptrBitmap = (byte*)bmData.Scan0;

                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        inColor[0] = (byte)_raster[0][x, y];
                        inColor[1] = (byte)_raster[1][x, y];
                        inColor[2] = (byte)_raster[2][x, y];

                        ColorConverter(inColor, outColor);

                        ptrBitmap[2] = outColor[0];
                        ptrBitmap[1] = outColor[1];
                        ptrBitmap[0] = outColor[2];

                        ptrBitmap[3] = 255; /* 100% opacity */
                        ptrBitmap += 4;     // advance to the next pixel
                        i++;                // "
                    }
                }
            }

            bitmap.UnlockBits(bmData);

            return bitmap;

        }
        #endif

    }
}
