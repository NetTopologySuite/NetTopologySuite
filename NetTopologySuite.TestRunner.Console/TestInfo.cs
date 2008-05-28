using System;
using GisSharpBlog.NetTopologySuite;

namespace GisSharpBlog.NetTopologySuite.Console
{
    /// <summary>
    /// Summary description for TestInfo.
    /// </summary>
    public class TestInfo
    {
        #region Private Members

        private String _strFileName;
        private String _strDirectory;
        private XmlTestType _enumFilter = XmlTestType.None;
        private Boolean _bVerbose = true;
        private Boolean _bException = true;
        private Boolean _bInteractive;

        #endregion

        #region Constructors and Destructor

        public TestInfo(XmlTestType filter)
        {
            _enumFilter = filter;
        }

        #endregion

        #region Public Properties

        public String FileName
        {
            get { return _strFileName; }

            set { _strFileName = value; }
        }

        public String Directory
        {
            get { return _strDirectory; }

            set { _strDirectory = value; }
        }

        public XmlTestType Filter
        {
            get { return _enumFilter; }

            set { _enumFilter = value; }
        }

        public Boolean Verbose
        {
            get { return _bVerbose; }

            set { _bVerbose = value; }
        }

        public Boolean Exception
        {
            get { return _bException; }

            set { _bException = value; }
        }

        public Boolean Interactive
        {
            get { return _bInteractive; }

            set { _bInteractive = value; }
        }

        #endregion
    }
}