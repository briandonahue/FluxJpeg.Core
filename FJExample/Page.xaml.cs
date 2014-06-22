using FluxJpeg.Core;
using FluxJpeg.Core.Decoder;
using FluxJpeg.Core.Encoder;
using FluxJpeg.Core.Filtering;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FJExample
{
    public partial class Page : UserControl
    {
        DecodedJpeg jpegOut;

        public Page()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Image files (*.jpg)|*.jpg" };

            if (ofd.ShowDialog() != true) return;

            Stream fileStream = ofd.File.OpenRead();

            // Display input image
            Stream inStream = new MemoryStream(new BinaryReader(fileStream).ReadBytes((int)fileStream.Length));
            BitmapImage imageIn = new BitmapImage();
            imageIn.SetSource(inStream);
            InputImage.Source = imageIn;

            // Rewind
            fileStream.Seek(0, SeekOrigin.Begin);

            using (fileStream)
            {
                // Decode
                DecodedJpeg jpegIn = new JpegDecoder(fileStream).Decode();

                if (!ImageResizer.ResizeNeeded(jpegIn.Image, 320))
                {
                    OutputImage.Source = null;
                    OutputText.Text = "No resize necessary.";
                    return;
                }

                // Resize
                jpegOut = new DecodedJpeg(
                    new ImageResizer(jpegIn.Image)
                        .ResizeToScale(320, ResamplingFilters.NearestNeighbor),
                    jpegIn.MetaHeaders); // Retain EXIF details

                // Encode
                MemoryStream outStream = new MemoryStream();
                new JpegEncoder(jpegOut, 90, outStream).Encode();

                // Display 
                outStream.Seek(0, SeekOrigin.Begin);
                BitmapImage image = new BitmapImage();
                image.SetSource(outStream);
                OutputImage.Source = image;
            }

        }

        /// <summary>
        /// Prompts the user to save the loaded Image locally
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Return if no image was loaded
            if (jpegOut == null)
                return;

            // Create an show dialog
            var dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                // After the resize, we can now inspect the PPI values
                var ppiX = jpegOut.Image.DensityX;
                var ppiY = jpegOut.Image.DensityX;
                Debug.WriteLine("DPI: {0}, {1}", ppiX, ppiY);

                // Get the file
                using (var fileStream = dialog.OpenFile())
                {
                    new JpegEncoder(jpegOut, 100, fileStream).Encode();
                }
            }
        }
    }
}
