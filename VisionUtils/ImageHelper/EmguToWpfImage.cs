using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Gocator;
using System;
using System.Runtime.InteropServices;
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
        public static Image<Rgb,Byte> ZValueToColorMap(Mat mat)
        {
            //mat.ToImage<Emgu.CV.Structure.Rgb, ushort>
            
            return null;
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
