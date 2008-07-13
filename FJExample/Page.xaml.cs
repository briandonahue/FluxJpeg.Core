using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FluxJpeg.Core.Decoder;
using FluxJpeg.Core;
using System.IO;
using FluxJpeg.Core.Filtering;
using FluxJpeg.Core.Encoder;
using System.Windows.Media.Imaging;

namespace FJExample
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image files (*.jpg)|*.jpg";
            if (ofd.ShowDialog() != true) return;

            MemoryStream outStream = new MemoryStream();

            // Copy the image to a buffer to hand to Silverlight
            Stream fileStream = ofd.SelectedFile.OpenRead();
            byte[] fileBuffer = new byte[(int)fileStream.Length];
            fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
            fileStream.Seek(0, SeekOrigin.Begin);
            MemoryStream inStream = new MemoryStream(fileBuffer);

            BitmapImage imageIn = new BitmapImage();
            imageIn.SetSource(inStream);
            InputImage.Source = imageIn;

            using (fileStream)
            {
                JpegDecoder decoder = new JpegDecoder(fileStream);
                DecodedJpeg jpeg = decoder.Decode();
                ImageResizer resizer = new ImageResizer(jpeg.Image);
                FluxJpeg.Core.Image small =
                    resizer.Resize(320, ResamplingFilters.NearestNeighbor);
                JpegEncoder encoder = new JpegEncoder(small, 90, outStream);
                encoder.Encode();

                BitmapImage image = new BitmapImage();
                outStream.Seek(0, SeekOrigin.Begin);
                image.SetSource(outStream);
                OutputImage.Source = image;
            }

        }
    }
}
