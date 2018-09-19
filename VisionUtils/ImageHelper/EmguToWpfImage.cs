using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Gocator;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
namespace VisionUtils
{
    public class EmguToWpfImage
    {

        public static unsafe Mat ZValuesToMat(ushort[] zValues, GocatorContext mContext)
        {
            int width = mContext.Width;
            int height = mContext.Height;
            int[] size = new int[] { height, width };
            fixed (ushort* p = zValues)
            {
                return new Mat(size, DepthType.Cv16U, (IntPtr)p, null);
            }
        }

        /// <summary>
        /// Trans zvalues to PNG 16bit
        /// </summary>
        /// <param name="zValues"></param>
        /// <param name="mContext"></param>
        /// <returns></returns>
        /// 
        public static unsafe Image<Gray, ushort> ZValuesToDepthPng(ushort[] zValues, GocatorContext mContext)
        {
            int width = mContext.Width;
            int height = mContext.Height;
            int[] size = new int[] { height, width };
            fixed (ushort* p = zValues)
            {
                Mat depthPng = new Mat(size, DepthType.Cv16U, (IntPtr)p, null);

                //depthPng.ToImage<Rgb, ushort>().Save(@"C:\colorpng.png");
                //Image<Gray, ushort> trh = depthPng.ToImage<Gray, ushort>().ThresholdBinary(new Gray(10000), new Gray(32768));
                //trh.Save(@"C:\ trs.png");
                return depthPng.ToImage<Gray, ushort>();
            }
        }
        /// <summary>
        /// Trans zvalues to colormap
        /// </summary>
        /// <returns></returns>
        public static unsafe Image<Rgb, Byte> ZValueToColorMap(ushort[] zValues, GocatorContext mContext)
        {

            Mat mat = ZValuesToMat(zValues, mContext);
            var maxValue = default(ushort);
            var minValue = ushort.MaxValue;
            var nullValue = default(ushort);
            Parallel.For(0, zValues.Length, (index) =>
            {
                var tempZ = zValues[index];
                if (tempZ != nullValue)
                {
                    if (tempZ > maxValue)
                    {
                        maxValue = tempZ;
                    }
                    if (tempZ < minValue)
                    {
                        minValue = tempZ;
                    }
                }
            });
            Color[] colors = new Color[zValues.Length];
            Image<Rgb, Byte> image = new Image<Rgb, byte>(mContext.Width, mContext.Height, new Rgb(255, 255, 255));
            Parallel.For(0, zValues.Length, (index) =>
            {
                colors[index] = ToColor(zValues[index], minValue, maxValue, nullValue);
            });

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    Rgb tempColor = new Rgb(colors[j + image.Width *i]);
                    image[i,j] = tempColor;
                }
            }
            return image;

        }

        /// <summary>
        /// Trans zValues to color
        /// </summary>
        /// <param name="ZValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="nullValue"></param>
        /// <returns></returns>
        public static Color ToColor(ushort ZValue, ushort minValue, ushort maxValue, ushort nullValue)
        {
            Color CC = new Color();
            int Steps = 255 + 1;
            int Edges = 7;
            int Total_Steps = Steps * Edges;
            double valueRange = maxValue - minValue;
            CC = Color.FromArgb(0, 0, 0);
            if (maxValue > minValue)
            {
                if (ZValue != nullValue)
                {
                    Int32 x = (Int32)(Total_Steps * (ZValue - minValue) / valueRange);
                    if (x < 0 * Steps)
                        CC = Color.FromArgb(0, 0, 0);
                    else if (x < 1 * Steps)
                        CC = Color.FromArgb(0, 0, x - 0 * Steps);
                    else if (x < 2 * Steps)
                        CC = Color.FromArgb(0, x - 1 * Steps, Steps - 1);
                    else if (x < 3 * Steps)
                        CC = Color.FromArgb(0, Steps - 1, 3 * Steps - x - 1);
                    else if (x < 4 * Steps)
                        CC = Color.FromArgb(x - 3 * Steps, Steps - 1, 0);
                    else if (x < 5 * Steps)
                        CC = Color.FromArgb(Steps - 1, 5 * Steps - x - 1, 0);
                    else if (x < 6 * Steps)
                        CC = Color.FromArgb(Steps - 1, 0, x - 5 * Steps);
                    else if (x < 7 * Steps)
                        CC = Color.FromArgb(Steps - 1, x - 6 * Steps, Steps - 1);
                    else
                        CC = Color.FromArgb(Steps - 1, Steps - 1, Steps - 1);
                }
            }
            return CC;
        }


        /// <summary>
        /// Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(ptr); //release the HBitmap

                return bs;
            }
        }
    }
}
