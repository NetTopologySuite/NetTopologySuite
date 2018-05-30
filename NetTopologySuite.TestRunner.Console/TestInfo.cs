using Open.Topology.TestRunner;

namespace ConsoleTestRunner
{
	/// <summary>
	/// Summary description for TestInfo.
	/// </summary>
    public class TestInfo
    {
        #region Private Members

        private string m_strFileName     = null;
        private string m_strDirectory    = null;
        private XmlTestType m_enumFilter = XmlTestType.None;
        private bool m_bVerbose          = true;
        private bool m_bException        = true;
        private bool m_bInteractive      = false;

        #endregion

        #region Constructors and Destructor

        public TestInfo(XmlTestType filter)
        {
            m_enumFilter = filter;
        }

        #endregion

        #region Public Properties

        public string FileName
        {
            get => m_strFileName;

            set => m_strFileName = value;
        }

        public string Directory
        {
            get => m_strDirectory;

            set => m_strDirectory = value;
        }

        public XmlTestType Filter
        {
            get => m_enumFilter;

            set => m_enumFilter = value;
        }

        public bool Verbose
        {
            get => m_bVerbose;

            set => m_bVerbose = value;
        }

        public bool Exception
        {
            get => m_bException;

            set => m_bException = value;
        }

        public bool Interactive
        {
            get => m_bInteractive;

            set => m_bInteractive = value;
        }

        #endregion

    }

}
