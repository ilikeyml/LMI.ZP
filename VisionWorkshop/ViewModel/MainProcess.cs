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
using System.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using VisionWorkshop.Utils;
using System.Text;
namespace VisionWorkshop
{
    public class MainProcess : ViewModel
    {

        #region Graphics
        System.Drawing.Color LineColor = System.Drawing.Color.FromArgb(50, 255, 0);

        #endregion

        #region Socket Server
        AsyncTcpServer server;
        #endregion  
        #region AVL Members
        private readonly SinvoProgramMacrofilters macros;
        AvlNet.Image displayImageTop = new AvlNet.Image();
        AvlNet.Image displayImageBottom = new AvlNet.Image();
        AvlNet.Image TopIntensityImage = new AvlNet.Image();
        AvlNet.Image BottomIntensityImage = new AvlNet.Image();
        AvlNet.Image TopDepthImage = new AvlNet.Image();
        AvlNet.Image BottomDepthImage = new AvlNet.Image();
        int SurfaceWidth = 0;
        int SurfaceHeight = 0;

        #endregion
        #region ctor
        public MainProcess()
        {
            RootPath = Environment.CurrentDirectory;
            _context = SynchronizationContext.Current;
            string msg = $"System start ";
            Logger(msg);
            server = new AsyncTcpServer(9999);
            server.Encoding = Encoding.UTF8;
            server.ClientConnected +=
            new EventHandler<TcpClientConnectedEventArgs>(server_ClientConnected);
            server.ClientDisconnected +=
            new EventHandler<TcpClientDisconnectedEventArgs>(server_ClientDisconnected);
            server.PlaintextReceived +=
            new EventHandler<TcpDatagramReceivedEventArgs<string>>(server_PlaintextReceived);
            server.Start();
            Logger("Server listening");
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
            ImageSourceTop = new BitmapImage(new Uri(ConfigPath.LogoPath));
            ImageSourceBottom = new BitmapImage(new Uri(ConfigPath.LogoPath));
            #endregion
            #region AVL RunTime
            try
            {
                string avsProjectPath = ConfigPath.AVProgramPath;
                macros = SinvoProgramMacrofilters.Create(avsProjectPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            #endregion
        }
        private void server_PlaintextReceived(object sender, TcpDatagramReceivedEventArgs<string> e)
        {
            throw new NotImplementedException();
        }
        private void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            Logger($" {e.TcpClient.Client.RemoteEndPoint} Disconnect");
        }
        private void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            Logger($" {e.TcpClient.Client.RemoteEndPoint} Connect");
        }
        #endregion
        #region Gocator Event menber
        readonly GocatorDevice gocator = new GocatorDevice("127.0.0.1", 33);
        private void Gocator_OnDataReceivedEvent(object sender, object e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<TopBottomSurface> rawDataSet = (List<TopBottomSurface>)e;
            foreach (var item in rawDataSet)
            {
                //save to surface
                Surface topSurface = new Surface(gocator.mContextTop.Width, gocator.mContextTop.Height, item.TopSurfaceData);
                SurfaceConvert.ScaleSurface(ref topSurface, gocator.mContextTop);
                Surface bottomSurface = new Surface(gocator.mContextBottom.Width, gocator.mContextBottom.Height, item.BottomSurfaceData);
                SurfaceConvert.ScaleSurface(ref bottomSurface, gocator.mContextBottom);
                TopDepthImage = ImageConvert.ZValueToDepthImage(item.TopSurfaceData, gocator.mContextTop);
                BottomDepthImage = ImageConvert.ZValueToDepthImage(item.BottomSurfaceData, gocator.mContextBottom);
                TopIntensityImage = ImageConvert.ByteToIntensityBitmap(item.TopSurfaceIntensityData, gocator.mContextTop);
                BottomIntensityImage = ImageConvert.ByteToIntensityBitmap(item.BottomSurfaceIntensityData, gocator.mContextBottom);
                
                AVLRun(topSurface, bottomSurface, TopDepthImage, BottomDepthImage, TopIntensityImage, BottomIntensityImage);

                server.SendAll("-1000");
                sw.Stop();
                Logger($"{sw.ElapsedMilliseconds}");
                Logger("Send Data to Client -1000");

                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string generateSurfaceTopName = $"{ConfigPath.ImageDataPathTop}TopSurface{timeStamp}.avdata";
                AVL.SaveSurface(topSurface, generateSurfaceTopName);
                string generateSurfaceBottomName = $"{ConfigPath.ImageDataPathBottom}BottomSurface{timeStamp}.avdata";
                AVL.SaveSurface(bottomSurface, generateSurfaceBottomName);
            }
            Logger("Surface data saved");

            //gocator.StartAcq();
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
        #region UI  Sync Context
        SynchronizationContext _context;
        #endregion
        #region Image manipulation
        public ImageSource ImageSourceTop { get; set; }
        public ImageSource ImageSourceBottom { get; set; }
        private AvlNet.Image DisplayImage = new AvlNet.Image();
        private float? TopPointValue;
        private float? BottomPointValue;
        private float? TopRefValue;
        private float? BottomRefValue;
        private Point2D? TopPoint;
        private Point2D? BottomPoint;
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
            return true;
        }
        private void DebugExcute()
        {
            DebugLoadImage();
            //return;
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

            //delete all files
            var botData = Directory.GetFiles(ConfigPath.ImageDataPathBottom);
            foreach (var item in botData)
            {
                File.Delete(item);
            }
            botData = Directory.GetFiles(ConfigPath.ImageDataPathTop);
            foreach (var item in botData)
            {
                File.Delete(item);
            }

            botData = Directory.GetFiles(ConfigPath.TestResultPath);
            foreach (var item in botData)
            {
                File.Delete(item);
            }

            Logger("Files delete");
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
        void AVLRun(Surface topSurface, Surface bottomSurface, AvlNet.Image topDepthImage, AvlNet.Image bottomDepthImage, AvlNet.Image topIntensityImage, AvlNet.Image bottomIntensityImage)
        {
          
            double dis01 = -1000;
            double dis02 = -1000;

            macros.TopPoint(topIntensityImage, topDepthImage, out TopPointValue,out TopPoint);

            macros.BottomPoint(topDepthImage, topIntensityImage, out BottomPointValue,out BottomPoint);

            macros.TopRef(bottomIntensityImage, bottomDepthImage, out TopRefValue);
            macros.BottomRef(bottomIntensityImage, bottomDepthImage, out BottomRefValue);

            Point3D topPoint = ImageConvert.TransPoint3DToRealWorld(new Point3D(TopPoint.Value.X, TopPoint.Value.Y, TopPointValue.Value), gocator.mContextTop);

            Point3D bottomPoint  = ImageConvert.TransPoint3DToRealWorld(new Point3D(BottomPoint.Value.X, BottomPoint.Value.Y, BottomPointValue.Value), gocator.mContextTop);

            Point3D topRefPoint = ImageConvert.TransPoint3DToRealWorld(new Point3D(0, 0, TopRefValue.Value), gocator.mContextBottom);

            Point3D bottomRefPoint = ImageConvert.TransPoint3DToRealWorld(new Point3D(0, 0, BottomRefValue.Value), gocator.mContextBottom);

            dis01 = ImageConvert.CalcZGap(topPoint, topRefPoint, gocator.mContextTop);
            dis02 = ImageConvert.CalcZGap(bottomPoint, bottomRefPoint, gocator.mContextTop);


            string P1Msg = $"P1,{ImageConvert.Point3DToString(topPoint)},Distance,{dis01},Ref,{TopRefValue.Value}";
            string P2Msg = $"P2,{ImageConvert.Point3DToString(bottomPoint)},Distance,{dis02},Ref,{ BottomRefValue.Value}";
            File.AppendAllText($"{ConfigPath.TestResultPath}result01.csv", P1Msg + Environment.NewLine);
            File.AppendAllText($"{ConfigPath.TestResultPath}result02.csv", P2Msg + Environment.NewLine);
            Logger(P1Msg);
            Logger(P2Msg);
            _context.Post(new SendOrPostCallback((e) =>
            {
                AVLGraphics(true);
            }), new object());

 

        }
        void AVLGraphics(bool graphicsSwitch)
        {
            if (graphicsSwitch)
            {
                displayImageTop = new AvlNet.Image(TopIntensityImage.CreateBitmap());
                displayImageBottom = new AvlNet.Image(BottomIntensityImage.CreateBitmap());
                //Match Edges
                //foreach (var item in outMatchEdges)
                //{
                //    AVL.DrawPath(ref displayImageTop, item, Pixel.Green, new DrawingStyle(DrawingMode.Fast, 1, 1, false, PointShape.Circle, 2));
                //}
                //Match Point
                //AVL.DrawPoint(ref displayImageTop, outMatchPoint.Value, Pixel.Green, new DrawingStyle(DrawingMode.Fast, 1, 5, false, PointShape.Cross, 60));
                // Intersection Point 1 
                // AVL.DrawPoint(ref displayImageTop, outIntersectionPoint01.Value, Pixel.Fuchsia, new DrawingStyle(DrawingMode.Fast, 1, 5, false, PointShape.Cross, 120));
                // Intersection Point 2
                // AVL.DrawPoint(ref displayImageTop, outIntersectionPoint02.Value, Pixel.Purple, new DrawingStyle(DrawingMode.Fast, 1, 5, false, PointShape.Cross, 120));
                //Fit line
                //AVL.DrawLine(ref displayImageTop, outLine01.Value, Pixel.Yellow, new DrawingStyle(DrawingMode.Fast, 1, 2, false, PointShape.Circle, 2));
                //AVL.DrawLine(ref displayImageTop, outLine02.Value, Pixel.Yellow, new DrawingStyle(DrawingMode.Fast, 1, 1, false, PointShape.Circle, 2));
                //AVL.DrawLine(ref displayImageTop, outLineMid.Value, new Pixel(LineColor), new DrawingStyle(DrawingMode.Fast, 1, 2, false, PointShape.Circle, 2));
                //AVL.DrawPoint(ref displayImageTop, MidPoint01.Value, Pixel.Fuchsia, new DrawingStyle(DrawingMode.Fast, 1, 5, false, PointShape.Cross, 120));
                // AVL.DrawPoint(ref displayImageTop, MidPoint02.Value, Pixel.Fuchsia, new DrawingStyle(DrawingMode.Fast, 1, 5, false, PointShape.Cross, 120));
                //displayImageTop.Save($@"{ConfigPath.ImageDataPathTop}{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.png");
                
                ImageSourceTop = ImageConvert.BitmapToImageSource(displayImageTop.CreateBitmap());
                ImageSourceBottom = ImageConvert.BitmapToImageSource(displayImageBottom.CreateBitmap());

            }
        }
        #endregion
        #region logger
        void Logger(string msg)
        {
            object obj = new object();
            lock (obj)
            {
                _context.Post(new SendOrPostCallback((e) =>
                {
                    LoggerInfo = $" [{DateTime.Now.ToString("HH:mm:ss")}]:{msg}{Environment.NewLine}" + LoggerInfo;
                }), new object());
            }
        }
        #endregion
    }
}
