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
        private string m_strFilter       = null;
        private bool m_bVerbose          = true;
        private bool m_bException        = true;
        private bool m_bInteractive      = false;

        #endregion

        #region Constructors and Destructor

        public TestInfo(string filter)
        {
            m_strFilter = filter;
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

        /// <summary>
        /// The name of the only operation to report on, or <c>null</c> to report on all of them.
        /// </summary>
        public string Filter
        {
            get => m_strFilter;

            set => m_strFilter = value;
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
