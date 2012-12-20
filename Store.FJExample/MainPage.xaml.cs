using System;
using System.IO;
using FluxJpeg.Core;
using FluxJpeg.Core.Decoder;
using FluxJpeg.Core.Encoder;
using FluxJpeg.Core.Filtering;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Store.FJExample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker
                                 {
                                     ViewMode = PickerViewMode.Thumbnail,
                                     SuggestedStartLocation = PickerLocationId.PicturesLibrary
                                 };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");

            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Display input image
                var imageIn = new BitmapImage();
                await imageIn.SetSourceAsync(fileStream);
                InputImage.Source = imageIn;

                // Rewind
                fileStream.Seek(0);

                // Decode
                var jpegIn = new JpegDecoder(fileStream.AsStreamForRead()).Decode();

                if (!ImageResizer.ResizeNeeded(jpegIn.Image, 320))
                {
                    OutputImage.Source = null;
                    OutputText.Text = "No resize necessary.";
                    return;
                }

                // Resize
                var jpegOut = new DecodedJpeg(
                    new ImageResizer(jpegIn.Image)
                        .ResizeToScale(320, ResamplingFilters.NearestNeighbor),
                    jpegIn.MetaHeaders); // Retain EXIF details

                // Encode
                using (var outStream = new InMemoryRandomAccessStream())
                {
                    new JpegEncoder(jpegOut, 90, outStream.AsStreamForWrite()).Encode();

                    // Display 
                    outStream.Seek(0);
                    var image = new BitmapImage();
                    await image.SetSourceAsync(outStream);
                    OutputImage.Source = image;
                }
            }
        }
    }
}
