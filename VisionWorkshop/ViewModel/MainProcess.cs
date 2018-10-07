using System;
using System.Windows.Input;
using VisionWorkshop.Command;
using AvlNet.Designers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using AvlNet;
using AdaptiveVision;
using System.Threading;
using Gocator;
using System.Windows.Threading;
namespace VisionWorkshop
{
    public class MainProcess : ViewModel
    {
        #region AVL Members
        private readonly ProgramMacrofilters macros;
        AvlNet.Image displayImage = new AvlNet.Image();
        AvlNet.Image outDepthImage = new AvlNet.Image();
        Line2D? outLine01 = new Line2D();
        Line2D? outLine02 = new Line2D();
        Line2D? outLine03 = new Line2D();
        Point2D? outIntersectionPoint01 = new Point2D();
        Point2D? outIntersectionPoint02 = new Point2D();
        AvlNet.Path[] outMatchEdges = new AvlNet.Path[0];
        Point2D? outMatchPoint = new Point2D();
        int SurfaceWidth = 0;
        int SurfaceHeight = 0;
        //Crop Surface by region
        Point2D? outPoint01Offset = new Point2D();
        Point2D? outPoint02Offset = new Point2D();
        Point3D? outPoint3D01 = new Point3D();
        Point3D? outPoint3D02 = new Point3D();
        #endregion

        public MainProcess()
        {
            RootPath = Environment.CurrentDirectory;
            _context = SynchronizationContext.Current;
            string msg = $"System start ";
            Logger(msg);
            #region Gocator Initi
            gocator.DeviceStatusEvent += Gocator_DeviceStatusEvent;
            gocator.OnDataReceivedEvent += Gocator_OnDataReceivedEvent;
            gocator.InitialAcq();
            #endregion
            #region UI Binding
            Run = new RelayCommand(RunExcute, RunCanExcute);
            Stop = new RelayCommand(StopExcute, StopCanExcute);
            LineEditor01 = new RelayCommand(LineEditor01Excute, LineEditor01CanExcute);
            LineEditor02 = new RelayCommand(LineEditor02Excute, LineEditor02CanExcute);
            LineEditor03 = new RelayCommand(LineEditor03Excute, LineEditor03CanExcute);
            Match = new RelayCommand(MatchExcute, MatchCanExcute);
            Debug = new RelayCommand(DebugExcute, DebugCanExcute);
            ImageSource = new BitmapImage(new Uri(ConfigPath.LogoPath));
            #endregion
            #region AVL RunTime
            try
            {
                string avsProjectPath = ConfigPath.AVProgramPath;
                macros = ProgramMacrofilters.Create(avsProjectPath);
                //macros.DeserializeCoordinateSystem(ConfigPath.CoordinationSystemPath, out CoordinateSystem);
                //macros.DeserializeFittingField(ConfigPath.SegmentFittingFieldPath_01, out SegmentFittingField_01);
                //AVL.LoadEdgeModel(ConfigPath.EdgeModelPath, out EdgeModel);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            #endregion
        }

        #region Gocator Private menber
        readonly GocatorDevice gocator = new GocatorDevice("127.0.0.1", 32);
        private void Gocator_OnDataReceivedEvent(object sender, object e)
        {
            List<ushort[]> rawDataSet = (List<ushort[]>)e;
            foreach (var item in rawDataSet)
            {
                //save to surface
                Surface tempSurface = new Surface(gocator.mContext.Width, gocator.mContext.Height, item);
                tempSurface.SetOffsetAndScales(gocator.mContext.XOffset, gocator.mContext.XResolution, gocator.mContext.YOffset, gocator.mContext.YResolution, gocator.mContext.ZOffset, gocator.mContext.ZResolution);

                string generateSurfaceName = $"{ConfigPath.ImageDataPath}Surface{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.avdata";

                AVL.SaveSurface(tempSurface, generateSurfaceName);
                AVLRun(tempSurface);

  

            }
            Logger("Surface data saved");

  
        }
        private void Gocator_DeviceStatusEvent(object sender, object e)
        {
            Logger(e.ToString());
        }
        #endregion

        #region file and directory
        private string rootPath;
        public string RootPath
        {
            get { return rootPath; }
            set
            {
                rootPath = Environment.CurrentDirectory;
            }
        }
        bool ChkFileExist(string filePath)
        {
            return File.Exists(filePath);
        }
        public string LoggerInfo { get; set; }
        #endregion

        #region UI Context
        SynchronizationContext _context;
        #endregion

        #region Image manipulation
        public ImageSource ImageSource { get; set; }
        private AvlNet.Image DisplayImage = new AvlNet.Image();
        #endregion

        // <Button Style = "{StaticResource CustomButton}" Content="Run" Command="{Binding Run}"/>
        //<Button Style = "{StaticResource CustomButton}" Content="Stop"/>
        //<Label Style = "{StaticResource CustomLabel}" Content="Debug"/>
        //<Button Style = "{StaticResource CustomButton}" Content="LineEditor01"/>
        //<Button Style = "{StaticResource CustomButton}" Content="LineEditor02"/>
        //<Button Style = "{StaticResource CustomButton}" Content="LineEditor03"/>
        //<Button Style = "{StaticResource CustomButton}" Content="Match"/>

        #region run command
        public ICommand Run { get; set; }
        private bool RunCanExcute()
        {
            return true;
        }
        private void RunExcute()
        {
            RunFunc();
        }
        #endregion

        #region stop command
        public ICommand Stop { get; set; }
        private bool StopCanExcute()
        {
            return true;
        }
        private void StopExcute()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region debug command
        public ICommand Debug { get; set; }
        private bool DebugCanExcute()
        {
            return false;
        }
        private void DebugExcute()
        {
            //DebugLoadImage();
            return;
        }
        #endregion

        #region Line01 editor
        public ICommand LineEditor01 { get; set; }
        private bool LineEditor01CanExcute()
        {
            //return ImageQueue.Count > 0;
            //return bFlag;
            return false;
        }
        private void LineEditor01Excute()
        {
            //FitLineEditor_01();
            return;
        }
        #endregion

        #region Line02 editor
        public ICommand LineEditor02 { get; set; }
        private bool LineEditor02CanExcute()
        {
            //return ImageQueue.Count > 0;
            //return bFlag;
            return false;
        }
        private void LineEditor02Excute()
        {
            return;
        }
        #endregion

        #region Line03 editor
        public ICommand LineEditor03 { get; set; }
        private bool LineEditor03CanExcute()
        {
            //return ImageQueue.Count > 0;
            //return bFlag;
            return false;
        }
        private void LineEditor03Excute()
        {
            return;
        }
        #endregion

        #region Match
        public ICommand Match { get; set; }
        private bool MatchCanExcute()
        {
            //return ImageQueue.Count > 0;
            //return bFlag;
            return false;
        }
        private void MatchExcute()
        {
            //CreateEdgeModel();
            return;
        }
        #endregion

        #region debug func
        void DebugLoadImage()
        {
            //string debugImageFilePath = ConfigPath.DebugImagePath;
            //if (ChkFileExist(debugImageFilePath))
            //{
            //    ImageQueue.Clear();
            //    ImageInfo loadImage = new ImageInfo
            //    {
            //        Image = new AvlNet.Image(),
            //        Info = $"Image for debug"
            //    };
            //    DisplayImage = new AvlNet.Image(loadImage.Image);
            //    ImageSource = ImageConvert.BitmapToImageSource(DisplayImage.CreateBitmap());
            //    ImageQueue.Add(loadImage);
            //    if (ImageQueue.Count > 0)
            //    {
            //        ToolBackgroundImagesNorm = new AvlNet.Image[ImageQueue.Count];
            //        ToolBackgroundImages = new AvlNet.Image[ImageQueue.Count];
            //        for (int i = 0; i < ImageQueue.Count; i++)
            //        {
            //            AvlNet.Image outImage = new AvlNet.Image();
            //            float outA = 0;
            //            float outB = 0;
            //            AVL.NormalizeImage(ImageQueue[i].Image, 0, 255, 0, 0, out outImage, out outA, out outB);
            //            ToolBackgroundImagesNorm[i] = outImage;
            //            ToolBackgroundImages[i] = ImageQueue[i].Image;
            //        }
            //        ToolBackgroundImagesNorm[0].Save(@"C:\ToolBackgroundImagesNorm.png");
            //        ToolBackgroundImages[0].Save(@"C:\ToolBackgroundImages.png");
            //    }
            //    else
            //    {
            //        MessageBox.Show("No Debug Image");
            //    }
            //}
        }
        #endregion

        #region match func
        void CreateEdgeModel()
        {
            //string edgeModelParamFilePath = ConfigPath.EdgeModelParamPath;
            //string edgeModelFilePath = ConfigPath.EdgeModelPath;
            //if (ChkFileExist(edgeModelParamFilePath))
            //{
            //    EdgeModelDesigner edgeModelDesigner = new EdgeModelDesigner();
            //    edgeModelDesigner.ExpertMode = true;
            //    edgeModelDesigner.Backgrounds = ToolBackgroundImagesNorm;
            //    edgeModelDesigner.ShowEdges = true;
            //    edgeModelDesigner.LoadParameters(edgeModelParamFilePath);
            //    if (edgeModelDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        edgeModelDesigner.SaveParameters(edgeModelParamFilePath);
            //        EdgeModel = edgeModelDesigner.GetEdgeModel();
            //        AVL.SaveEdgeModel(EdgeModel, edgeModelFilePath);
            //        CoordinateSystem2D? coordinationSystem = new CoordinateSystem2D();
            //        AvlNet.Path[] edgePath;
            //        AvlNet.Point2D? point2D;
            //        float? MatchScore = -1;
            //        AvlNet.Rectangle2D? matchRect;
            //        macros.Match(ToolBackgroundImagesNorm[0], EdgeModel, out coordinationSystem, out edgePath, out point2D, out MatchScore, out matchRect);
            //        foreach (var item in edgePath)
            //        {
            //            AVL.DrawPath(ref DisplayImage, item, coordinationSystem, Pixel.Green, new DrawingStyle(DrawingMode.Fast, 1, 1, false, PointShape.Cross, 2));
            //        }
            //        AVL.DrawPoint(ref DisplayImage, point2D.Value, Pixel.Blue, new DrawingStyle(DrawingMode.Fast, 1, 2, true, PointShape.Cross, 50));
            //        AVL.DrawString(ref DisplayImage, $"Match Score : { MatchScore}", new Location(100, 100), Anchor2D.TopLeft, Pixel.DarkGreen, new DrawingStyle(DrawingMode.Fast, 1, 10, false, PointShape.Circle, 8), 50, 0);
            //        macros.SerializeCoordinateSystem(ConfigPath.CoordinationSystemPath, coordinationSystem.Value);
            //        ImageSource = ImageConvert.BitmapToImageSource(DisplayImage.CreateBitmap());
            //        MessageBox.Show("New Match Template Saved");
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Load File Error");
            //}
        }
        #endregion

        #region FitLine 1
        void FitLineEditor_01()
        {
            //SegmentFittingFieldDesigner segmentFittingFieldDesigner = new SegmentFittingFieldDesigner();
            //segmentFittingFieldDesigner.Message = $"Fit line 01";
            //segmentFittingFieldDesigner.Title = $"Fit line 01";
            //segmentFittingFieldDesigner.Backgrounds = ToolBackgroundImages;
            //segmentFittingFieldDesigner.SegmentFittingField = SegmentFittingField_01;
            //segmentFittingFieldDesigner.CoordinateSystems = new CoordinateSystem2D[] { CoordinateSystem };
            //if (segmentFittingFieldDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    SegmentFittingField_01 = segmentFittingFieldDesigner.SegmentFittingField;
            //    macros.SerializeFittingField(SegmentFittingField_01, ConfigPath.SegmentFittingFieldPath_01);
            //    Line2D? line_01 = new Line2D();
            //    Point2D?[] points = new Point2D?[0];
            //    ToolBackgroundImages[0].Save(@"C:\ssss.png");
            //    macros.FitLine01(ToolBackgroundImages[0], CoordinateSystem, SegmentFittingField_01, out line_01, out points);
            //    AVL.DrawLine(ref DisplayImage, line_01.Value, Pixel.DarkGreen, new DrawingStyle(DrawingMode.Fast, 1, 2, false, PointShape.Circle, 2));
            //    MessageBox.Show("Fitting field saved");
            //}
        }
        #endregion

        #region Run func
        void RunFunc()
        {
            gocator.StartAcq();
        }
        #endregion

        #region AVL MainProcess
        void AVLRun(Surface inSurface)
        {
            macros.DiagnosticMode = true;
            macros.DetectIntersectionPoints(inSurface, out outLine01, out outLine02,
                out outLine03, out outIntersectionPoint01, out outIntersectionPoint02, out outMatchEdges, out outMatchPoint,out outDepthImage,
                out SurfaceWidth, out SurfaceHeight
                );
            macros.CropSurfaceByIntersectionPoints(outIntersectionPoint01, outIntersectionPoint02, outLine02, SurfaceWidth, SurfaceHeight, inSurface,
              out outPoint01Offset, out outPoint02Offset
                );

            macros.GetPoint3DZValue(outPoint01Offset, outPoint02Offset, outDepthImage, out outPoint3D01, out outPoint3D02);

            Point3D newPoint3D01 = ImageConvert.TransPoint3DToRealWorld(outPoint3D01.Value, gocator.mContext);
            Point3D newPoint3D02 = ImageConvert.TransPoint3DToRealWorld(outPoint3D02.Value, gocator.mContext);
            string P1Msg = $"P1,{ImageConvert.Point3DToString(newPoint3D01)}";
            string P2Msg = $"P2,{ImageConvert.Point3DToString(newPoint3D02)}";
            File.AppendAllText(@"C:\result01.csv", P1Msg + Environment.NewLine);
            File.AppendAllText(@"C:\result02.csv", P2Msg + Environment.NewLine);
            Logger(P1Msg);
            Logger(P2Msg);
            AVLGraphics(true);
        }

        void AVLGraphics(bool graphicsSwitch)
        {
            if (graphicsSwitch)
            {
                displayImage = new AvlNet.Image(outDepthImage.CreateBitmap());
                
                //Match Edges
                foreach (var item in outMatchEdges)
                {
                    AVL.DrawPath(ref displayImage, item, Pixel.Green, new DrawingStyle(DrawingMode.Fast, 1, 1, false, PointShape.Circle, 2));
                }
                //Match Point
                AVL.DrawPoint(ref displayImage, outMatchPoint.Value, Pixel.Green, new DrawingStyle(DrawingMode.Fast, 1, 5, false, PointShape.Cross, 60));
                // Intersection Point 1 
                AVL.DrawPoint(ref displayImage, outIntersectionPoint01.Value, Pixel.Fuchsia, new DrawingStyle(DrawingMode.Fast, 1, 5, false, PointShape.Cross, 120));
                // Intersection Point 2
                AVL.DrawPoint(ref displayImage, outIntersectionPoint02.Value, Pixel.Purple, new DrawingStyle(DrawingMode.Fast, 1, 5, false, PointShape.Cross, 120));
                //Fit line
                AVL.DrawLine(ref displayImage, outLine01.Value, Pixel.Yellow, new DrawingStyle(DrawingMode.Fast, 1, 2, false, PointShape.Circle, 2));
                AVL.DrawLine(ref displayImage, outLine02.Value, Pixel.Yellow, new DrawingStyle(DrawingMode.Fast, 1, 1, false, PointShape.Circle, 2));
                AVL.DrawLine(ref displayImage, outLine03.Value, Pixel.Yellow, new DrawingStyle(DrawingMode.Fast, 1, 2, false, PointShape.Circle, 2));

                _context.Post(new SendOrPostCallback((args) => {
                    ImageSource = ImageConvert.BitmapToImageSource(displayImage.CreateBitmap());

                }), null);
            }
        }
        #endregion

        #region logger
        void Logger(string msg)
        {
            //Dispatcher.CurrentDispatcher.InvokeAsync(() => {
            //    LoggerInfo += $" {msg}~~~~~~~~@{DateTime.Now.ToString("HH:mm:ss yyyy-MM-dd")}{Environment.NewLine}";
            //});
            object obj = new object();
            lock (obj)
            {
                _context.Post(new SendOrPostCallback((e) => {
                    LoggerInfo += $" {msg}~~~~~~~~@{DateTime.Now.ToString("HH:mm:ss yyyy-MM-dd")}{Environment.NewLine}";
                }), new object());
            }
        }
        #endregion
    }
}
