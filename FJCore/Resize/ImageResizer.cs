/// Copyright (c) 2008 Jeffrey Powers for Fluxcapacity Open Source.
/// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using System.Text;
using FluxJpeg.Core.Filtering;

namespace FluxJpeg.Core
{
    public class ResizeNotNeededException : Exception {  }
    public class ResizeProgressChangedEventArgs : EventArgs { public double Progress; }

    public class ImageResizer
    {
        private ResizeProgressChangedEventArgs progress = new ResizeProgressChangedEventArgs();
        public event EventHandler<ResizeProgressChangedEventArgs> ProgressChanged;

        private Image _input;

        public ImageResizer(Image input)
        {
            _input = input;
        }

        public static bool ResizeNeeded(FluxJpeg.Core.Image image, int maxEdgeLength)
        {
            double scale = (image.Width > image.Height) ? 
                (double)maxEdgeLength / image.Width : 
                (double)maxEdgeLength / image.Height;

            return scale < 1.0; // true if we must downscale
        }

        public Image ResizeToScale(int maxEdgeLength, ResamplingFilters technique)
        {
            double scale = 0;

            if (_input.Width > _input.Height)
                scale = (double)maxEdgeLength / _input.Width;
            else
                scale = (double)maxEdgeLength / _input.Height;

            if (scale >= 1.0)
                throw new ResizeNotNeededException();
            else
                return ResizeToScale(scale, technique);
        }

        public Image ResizeToScale(int maxWidth, int maxHeight, ResamplingFilters technique)
        {
            double wFrac = (double)maxWidth / _input.Width;
            double hFrac = (double)maxHeight / _input.Height;
            double scale = 0;

            // Make the image as large as possible, while 
            // fitting in the supplied box and
            // obeying the aspect ratio

            if (wFrac < hFrac) { scale = wFrac; }
            else { scale = hFrac; }

            if (scale >= 1.0)
                throw new ResizeNotNeededException();
            else
                return ResizeToScale(scale, technique);
        }

        public Image ResizeToScale(double scale, ResamplingFilters technique)
        {
            int height = (int)(scale * _input.Height);
            int width = (int)(scale * _input.Width);

            Filter resizeFilter;

            resizeFilter = GetResizeFilter(technique);

            return PerformResize(resizeFilter, width, height);
        }

        private Image PerformResize(Filter resizeFilter, int width, int height)
        {
            return new Image(_input.ColorModel, resizeFilter.Apply(_input.Raster, width, height));
        }

        private Filter GetResizeFilter(ResamplingFilters technique)
        {
            Filter resizeFilter;
            switch (technique)
            {
                case ResamplingFilters.NearestNeighbor:
                    resizeFilter = new NNResize();
                    break;
                case ResamplingFilters.LowpassAntiAlias:
                    resizeFilter = new LowpassResize();
                    break;
                default:
                    throw new NotSupportedException();
            }
            resizeFilter.ProgressChanged += ResizeProgressChanged;
            return resizeFilter;
        }

        public Image Resize(int width, int height, ResamplingFilters technique)
        {
            var resizeFilter = GetResizeFilter(technique);
            return PerformResize(resizeFilter, width, height);
        }

        void ResizeProgressChanged(object sender, Filtering.FilterProgressEventArgs e)
        {
            progress.Progress = e.Progress;
            if (ProgressChanged != null) ProgressChanged(this, progress);
        }

    }
}
