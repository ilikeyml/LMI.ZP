using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace VisionWorkshop
{
    public class ImageConvert
    {
        public static ImageSource BitmapToImageSource(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        source.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
        }
        public static AvlNet.Point3D TransPoint3DToRealWorld(AvlNet.Point3D inPoint3D, Gocator.GocatorContext mContext)
        {
            float x = (float)(inPoint3D.X * mContext.XResolution + mContext.XOffset);
            float y = (float)(inPoint3D.Y * mContext.YResolution + mContext.YOffset);
            float z = (float)(inPoint3D.Z * mContext.ZResolution + mContext.ZOffset);
            return new AvlNet.Point3D(x, y, z);
        }
        public static string Point3DToString(AvlNet.Point3D inPoint3D)
        {
            return $"X,{inPoint3D.X},Y,{inPoint3D.Y},Z,{inPoint3D.Z}";
        }
        public static unsafe AvlNet.Image ZValueToDepthImage(ushort[] zvalue, Gocator.GocatorContext gocatorContext)
        {
            fixed (ushort* pointer = zvalue)
            {
                //width* depth *component type size
                return new AvlNet.Image(gocatorContext.Width, gocatorContext.Height, gocatorContext.Width*1*sizeof(ushort), AvlNet.PlainType.UInt16, 1,(IntPtr)pointer );
            }
        }
        public static unsafe AvlNet.Image ByteToIntensityBitmap(byte[] intensity, Gocator.GocatorContext gocatorContext)
        {
            return new AvlNet.Image(gocatorContext.Width, gocatorContext.Height, gocatorContext.Width * 1 * sizeof(byte), AvlNet.PlainType.UInt8,1, intensity);
        }
        public static  unsafe AvlNet.Image XGapFill(ushort[] zvalue, Gocator.GocatorContext gocatorContext, int FillingScale)
        {
            int width = gocatorContext.Width;
            int height = gocatorContext.Height;
            int FilledWidth = width * FillingScale;
            ushort[] newdata = new ushort[height * FilledWidth];


            for (int i = 0; i < zvalue.Length; i++)
            {
                for (int k = 0; k < FillingScale; k++)
                {
                    newdata[k + i * FillingScale] = zvalue[i];
                }
            }

            fixed (ushort* pointer = newdata)
            {
                //width* depth *component type size
                return new AvlNet.Image(FilledWidth, height, FilledWidth * 1 * sizeof(ushort), AvlNet.PlainType.UInt16, 1, (IntPtr)pointer);
            }

        }
        public static double CalcZGap(AvlNet.Point3D Top, AvlNet.Point3D Bottom, Gocator.GocatorContext gocatorContext)
        {
            return (Top.Z - Bottom.Z);
        }

    }
}
