/// Copyright (c) 2008 Jeffrey Powers for Fluxcapacity Open Source.
/// Under the MIT License, details: License.txt.

using System;

namespace FluxJpeg.Core
{
    internal class YCbCr 
    {
        public static void toRGB(byte[] colorIn, byte[] colorOut)
        {
            float[] inFloat = new float[3];
            for (int i = 0; i < 3; i++)
                inFloat[i] = colorIn[i] / 255.0f;
            float[] outFloat = toRGB(inFloat);
            for (int i = 0; i < 3; i++)
                colorOut[i] = (byte)(255.0f * outFloat[i]);
        }

        /* YCbCr to RGB range 0 to 1 */
        public static float[] toRGB(float[] data)
        {
            float[] dest = new float[3];

            data[0] *= 255;
            data[1] *= 255;
            data[2] *= 255;

            dest[0] = (float)data[0] + (float)1.402 * ((float)data[2] - (float)128);
            dest[1] = (float)data[0] - (float)0.34414 * ((float)data[1] - (float)128) - (float)0.71414 * ((float)data[2] - (float)128);
            dest[2] = (float)data[0] + (float)1.772 * ((float)data[1] - (float)128);

            dest[0] /= 255;
            dest[1] /= 255;
            dest[2] /= 255;

            if (dest[0] < (float)0)
                dest[0] = 0;
            if (dest[1] < (float)0)
                dest[1] = 0;
            if (dest[2] < (float)0)
                dest[2] = 0;

            if (dest[0] > (float)1)
                dest[0] = 1;
            if (dest[1] > (float)1)
                dest[1] = 1;
            if (dest[2] > (float)1)
                dest[2] = 1;

            return (dest);
        }


        /* RGB to YCbCr range 0-255 */
        public static void fromRGB(byte[] rgb, byte[] ycbcr)
        {
            ycbcr[0] = (byte)((0.299 * (float)rgb[0] + 0.587 * (float)rgb[1] + 0.114 * (float)rgb[2]));
            ycbcr[1] = (byte)(128 + (byte)((-0.16874 * (float)rgb[0] - 0.33126 * (float)rgb[1] + 0.5 * (float)rgb[2])));
            ycbcr[2] = (byte)(128 + (byte)((0.5 * (float)rgb[0] - 0.41869 * (float)rgb[1] - 0.08131 * (float)rgb[2])));
        }


        /* RGB to YCbCr range 0-255 */
        public static float[] fromRGB(float[] data)
        {
            float[] dest = new float[3];

            dest[0] = (float)((0.299 * (float)data[0] + 0.587 * (float)data[1] + 0.114 * (float)data[2]));
            dest[1] = 128 + (float)((-0.16874 * (float)data[0] - 0.33126 * (float)data[1] + 0.5 * (float)data[2]));
            dest[2] = 128 + (float)((0.5 * (float)data[0] - 0.41869 * (float)data[1] - 0.08131 * (float)data[2]));

            return (dest);
        }
    }

}