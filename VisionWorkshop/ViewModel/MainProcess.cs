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
using Gocator;
namespace VisionWorkshop
{
    public class MainProcess : ViewModel
    {
        #region AVL Helper
        private readonly ProgramMacrofilters macros;
        private CoordinateSystem2D CoordinateSystem = new CoordinateSystem2D();
        private SegmentFittingField SegmentFittingField_01 = new SegmentFittingField();
        #endregion

        public MainProcess()
        {
            RootPath = Environment.CurrentDirectory;
            Run = new RelayCommand(RunExcute, RunCanExcute);
            Stop = new RelayCommand(StopExcute, StopCanExcute);
            LineEditor01 = new RelayCommand(LineEditor01Excute, LineEditor01CanExcute);
            LineEditor02 = new RelayCommand(LineEditor02Excute, LineEditor02CanExcute);
            LineEditor03 = new RelayCommand(LineEditor03Excute, LineEditor03CanExcute);
            Match = new RelayCommand(MatchExcute, MatchCanExcute);
            Debug = new RelayCommand(DebugExcute, DebugCanExcute);
            ImageSource = new BitmapImage(new Uri(ConfigPath.LogoPath));
            try
            {
                string avsProjectPath = ConfigPath.AVProgramPath;
                macros = ProgramMacrofilters.Create(avsProjectPath);
                macros.DeserializeCoordinateSystem(ConfigPath.CoordinationSystemPath, out CoordinateSystem);
                macros.DeserializeFittingField(ConfigPath.SegmentFittingFieldPath_01, out SegmentFittingField_01);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        #region Gocator
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
        #endregion

        #region Image manipulation
        public ImageSource ImageSource { get; set; }
        public ImageInfo LastImage { get; set; } = new ImageInfo();
        public List<ImageInfo> ImageQueue { get; set; } = new List<ImageInfo>();
        public bool bFlag { get; set; } = false;
        public AvlNet.Image[] ToolBackgroundImagesNorm { get; set; } = new AvlNet.Image[0];
        public AvlNet.Image[] ToolBackgroundImages { get; set; } = new AvlNet.Image[0];
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
        }
        #endregion

        #region Line01 editor
        public ICommand LineEditor01 { get; set; }
        private bool LineEditor01CanExcute()
        {
            return ImageQueue.Count>0;
            //return bFlag;
        }
        private void LineEditor01Excute()
        {
            FitLineEditor_01();
        }
        #endregion

        #region Line02 editor
        public ICommand LineEditor02 { get; set; }
        private bool LineEditor02CanExcute()
        {
            return ImageQueue.Count>0;
            //return bFlag;
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
            return ImageQueue.Count>0;
            //return bFlag;
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
            return ImageQueue.Count>0;
            //return bFlag;
        }
        private void MatchExcute()
        {
            CreateEdgeModel();
        }
        #endregion

        #region debug func
        void DebugLoadImage()
        {
            string debugImageFilePath = ConfigPath.DebugImagePath;
            if (ChkFileExist(debugImageFilePath))
            {
                ImageQueue.Clear();
                ImageInfo loadImage = new ImageInfo
                {
                    Image = new AvlNet.Image(new Bitmap(debugImageFilePath)),
                    Info = $"Image for debug"
                };
                DisplayImage = new AvlNet.Image((Bitmap)loadImage.Image.CreateBitmap().Clone());
   
                ImageSource = ImageConvert.BitmapToImageSource(DisplayImage.CreateBitmap());
                ImageQueue.Add(loadImage);
                if (ImageQueue.Count > 0)
                {
                    ToolBackgroundImagesNorm = new AvlNet.Image[ImageQueue.Count];
                    ToolBackgroundImages = new AvlNet.Image[ImageQueue.Count];
                    for (int i = 0; i < ImageQueue.Count; i++)
                    {
                        AvlNet.Image outImage = new AvlNet.Image();
                        float outA = 0;
                        float outB = 0;
                        AVL.NormalizeImage(ImageQueue[i].Image, 0, 255, 0, 0, out outImage, out outA, out outB);
                        ToolBackgroundImagesNorm[i] = outImage;
                        ToolBackgroundImages[i] = ImageQueue[i].Image;
                    }
                }
                else
                {
                    MessageBox.Show("No Debug Image");
                }
            }
        }
        #endregion

        #region match func
        void CreateEdgeModel()
        {
            string edgeModelParamFilePath = ConfigPath.EdgeModelParamPath;
            string edgeModelFilePath = ConfigPath.EdgeModelPath;
            if (ChkFileExist(edgeModelParamFilePath))
            {
                EdgeModelDesigner edgeModelDesigner = new EdgeModelDesigner();
                edgeModelDesigner.ExpertMode = true;
                edgeModelDesigner.Backgrounds = ToolBackgroundImagesNorm;
                edgeModelDesigner.ShowEdges = true;
                edgeModelDesigner.LoadParameters(edgeModelParamFilePath);
                if (edgeModelDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    edgeModelDesigner.SaveParameters(edgeModelParamFilePath);
                    EdgeModel edgeModel = new EdgeModel();
                    edgeModel = edgeModelDesigner.GetEdgeModel();
                    AVL.SaveEdgeModel(edgeModel, edgeModelFilePath);
                    CoordinateSystem2D? coordinationSystem = new CoordinateSystem2D();
                    AvlNet.Path[] edgePath;
                    AvlNet.Point2D? point2D;
                    float? MatchScore = -1;
                    AvlNet.Rectangle2D? matchRect;
                    macros.Match(ToolBackgroundImagesNorm[0], edgeModel, out coordinationSystem, out edgePath, out point2D, out MatchScore, out matchRect);
 
                    foreach (var item in edgePath)
                    {
                        AVL.DrawPath(ref DisplayImage, item, coordinationSystem, Pixel.Green, new DrawingStyle(DrawingMode.Fast, 1, 1, false, PointShape.Cross, 2));
                    }
                    AVL.DrawPoint(ref DisplayImage, point2D.Value, Pixel.Blue, new DrawingStyle(DrawingMode.Fast, 1, 2, true, PointShape.Cross, 50));
                    macros.SerializeCoordinateSystem(ConfigPath.CoordinationSystemPath, coordinationSystem.Value);
                    ImageSource = ImageConvert.BitmapToImageSource(DisplayImage.CreateBitmap());
                    MessageBox.Show("New Match Template Saved");
                }
            }
            else
            {
                MessageBox.Show("Load File Error");
            }
        }
        #endregion

        #region FitLine 1
        void FitLineEditor_01()
        {
            SegmentFittingFieldDesigner segmentFittingFieldDesigner = new SegmentFittingFieldDesigner();
            segmentFittingFieldDesigner.Message = $"Fit line 01";
            segmentFittingFieldDesigner.Title = $"Fit line 01";
            segmentFittingFieldDesigner.Backgrounds = ToolBackgroundImages;
            segmentFittingFieldDesigner.SegmentFittingField = SegmentFittingField_01;
            segmentFittingFieldDesigner.CoordinateSystems = new CoordinateSystem2D[] { CoordinateSystem };
            if (segmentFittingFieldDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SegmentFittingField_01 = segmentFittingFieldDesigner.SegmentFittingField;
                macros.SerializeFittingField(SegmentFittingField_01, ConfigPath.SegmentFittingFieldPath_01);

            
                MessageBox.Show("Fitting field saved");
            }
        }
        #endregion

    }
}
