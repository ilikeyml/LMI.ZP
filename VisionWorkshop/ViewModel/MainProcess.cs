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
using AvlNet.ExecutorSerialization;
using Gocator;
namespace VisionWorkshop
{
    public class MainProcess : ViewModel
    {
        private string _rootPath;
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
            ImageSource = new BitmapImage(new Uri(@"C:\Users\ilike\source\repos\LMI.ZP\VisionWorkshop\Images\lmitechnologieslogo-cube.png"));
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
        public ImageInfo DisplayImage { get; set; } = new ImageInfo();
        public ImageInfo CurrentImage { get; set; } = new ImageInfo();
        public ImageInfo LastImage { get; set; } = new ImageInfo();
        public List<ImageInfo> ImageQueue { get; set; } = new List<ImageInfo>();
        public AvlNet.Image[] ToolBackgroundImages { get; set; } = new AvlNet.Image[0];
        #endregion

        #region Macro Filter Params
        CoordinateSystem2D coordinateSystem = new CoordinateSystem2D();

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
            return true;
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
            throw new NotImplementedException();
        }
        private void LineEditor02Excute()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Line03 editor
        public ICommand LineEditor03 { get; set; }
        private bool LineEditor03CanExcute()
        {
            throw new NotImplementedException();
        }
        private void LineEditor03Excute()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Match
        public ICommand Match { get; set; }
        private bool MatchCanExcute()
        {
            return true;
        }
        private void MatchExcute()
        {
            CreateEdgeModel();
        }
        #endregion



        #region debug func
        void DebugLoadImage()
        {
            string debugImageFilePath = $@"{RootPath}\LMI\Debug\DebugImage.png";
            if (ChkFileExist(debugImageFilePath))
            {
                ImageQueue.Clear();
                ImageInfo loadImage = new ImageInfo
                {
                    Image = System.Drawing.Image.FromFile(debugImageFilePath),
                    Info = $"Image for debug"
                };
                ImageSource = ImageConvert.BitmapToImageSource((Bitmap)loadImage.Image);
                CurrentImage = loadImage;
                ImageQueue.Add(CurrentImage);
                if (ImageQueue.Count > 0)
                {
                    ToolBackgroundImages = new AvlNet.Image[ImageQueue.Count];
                    for (int i = 0; i < ImageQueue.Count; i++)
                    {
                        AvlNet.Image outImage = new AvlNet.Image();
                        float outA = 0;
                        float outB = 0;
                        AVL.NormalizeImage(new AvlNet.Image((Bitmap)ImageQueue[i].Image), 0, 255, 0, 0, out outImage, out outA, out outB);
                        ToolBackgroundImages[i] = outImage;
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
                string edgeModelParamFilePath = $@"{RootPath}\LMI\FeatureModel\EdgeModelParam.avdata";
                string edgeModelFilePath = $@"{RootPath}\LMI\FeatureModel\EdgeModel.avdata";
                if (ChkFileExist(edgeModelParamFilePath))
                {
                    EdgeModelDesigner edgeModelDesigner = new EdgeModelDesigner();
                    edgeModelDesigner.ExpertMode = true;
                    edgeModelDesigner.Backgrounds = ToolBackgroundImages;
                    edgeModelDesigner.ShowEdges = true;
                    edgeModelDesigner.LoadParameters(edgeModelParamFilePath);
                    if (edgeModelDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        edgeModelDesigner.SaveParameters(edgeModelParamFilePath);
                        EdgeModel edgeModel = new EdgeModel();
                        edgeModel = edgeModelDesigner.GetEdgeModel();
                        AVL.SaveEdgeModel(edgeModel, edgeModelFilePath);
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
            SegmentFittingFieldArrayDesigner segmentFittingFieldArrayDesigner = new SegmentFittingFieldArrayDesigner();
            segmentFittingFieldArrayDesigner.Backgrounds = ToolBackgroundImages;
            segmentFittingFieldArrayDesigner.Message = $"Fit Line Editor 01";
            segmentFittingFieldArrayDesigner.
            segmentFittingFieldArrayDesigner.ShowDialog();
            AVL.saveo
        }
        #endregion

    }
}
