using System;
using System.IO;
using FluxJpeg.Core.Decoder;
using FluxJpeg.Core;
using FluxJpeg.Core.Filtering;
using System.Diagnostics;
using System.Drawing;

namespace FJUnit
{
    class Program
    {
        static string output = "output.jpg", input = "../../geneserath.jpg";
        static int resizeTo = 640;

        static void Main(string[] args)
        {
            Console.WriteLine("FJCore Unit Test");

            PrintHeader();

            BasicResize();
        }

        private static void BasicResize()
        {
            Bitmap resized = Resize(input, resizeTo);

            // Save to disk.  This invokes GDI+ for JPEG Encode since
            // the encoder isn't included in FJCore yet.
            resized.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);

            // Show results
            Process.Start(new ProcessStartInfo(output));
        }

        static Bitmap Resize(string pathIn, int edge)
        {
            JpegDecoder decoder = new JpegDecoder(File.Open(pathIn, FileMode.Open));
            DecodedJpeg jpeg = decoder.Decode();
            ImageResizer resizer = new ImageResizer(jpeg.Image);
            return resizer.Resize(edge, ResamplingFilters.NearestNeighbor).ToBitmap();
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            #if DYNAMIC_IDCT
            Console.WriteLine("IDCT: Dynamic CIL");
            #else
            Console.WriteLine("IDCT: Pure C#");
            #endif
            Console.ForegroundColor = ConsoleColor.Gray;
        }

    }
}
