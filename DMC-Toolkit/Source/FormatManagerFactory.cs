using System.IO;

namespace NIST.DMC
{
    public class FormatManagerFactory
    {
        /// <summary>
        /// Factory that creates the right FormatManager instance based on a file's extension
        /// </summary>
        /// <param name="file">Path to the file for which a FormatManager is required</param>
        /// <returns>Instance of the appropriate FormatManager or null if the format is not supported</returns>
        public static FormatManager GetInstance(string file)
        {
            FormatManager NewFormatManager = null;
            if (Path.HasExtension(file))
            {
                switch (Path.GetExtension(file).ToLower())
                {
                    case ".stp":
                        NewFormatManager = new STEP4PLOT();
                        break;
                    case ".qif":
                        NewFormatManager = new QIF4PLOT();
                        break;
                    case ".ncc":
                        NewFormatManager = new GCODE4PLOT();
                        break;
                    case ".pdf":
                        NewFormatManager = new PDF4PLOT();
                        break;
                    default:
                        break;
                }
            }
            return NewFormatManager;
        }
    }
}
