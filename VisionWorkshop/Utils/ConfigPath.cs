using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionWorkshop
{
    public class ConfigPath
    {
        public static readonly string RootPath = Environment.CurrentDirectory;

        #region AVProgram
        public static readonly string AVProgramPath = $@"{RootPath}\LMI\AVProgram\SinvoProgram.avproj";
        #endregion

        #region Feature Model
        public static readonly string CoordinationSystemPath = $@"{RootPath}\LMI\FeatureModel\CoordinationSystem.avdata";
        public static readonly string EdgeModelPath = $@"{RootPath}\LMI\FeatureModel\EdgeModel.avdata";
        public static readonly string EdgeModelParamPath = $@"{RootPath}\LMI\FeatureModel\EdgeModelParam.avdata";
        public static readonly string SegmentFittingFieldPath_01 = $@"{RootPath}\LMI\FeatureModel\SegmentFittingFieldPath_01.avdata";
        #endregion

        #region debug data
        public static readonly string DebugImagePath = $@"{RootPath}\LMI\Debug\DebugImage.png";
        public static readonly string LogoPath = $@"{RootPath}\LMI\Debug\lmitechnologieslogo-cube.png";
        #endregion

        #region Image Data
        public static readonly string ImageDataPathTop = $@"{RootPath}\LMI\ImageData\Top\";
        public static readonly string ImageDataPathBottom = $@"{RootPath}\LMI\ImageData\Bottom\";
        #endregion

        #region TestResult
        public static readonly string TestResultPath = $@"{RootPath}\LMI\TestResult\";
        #endregion

    }
}
