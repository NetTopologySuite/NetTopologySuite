using Open.Topology.TestRunner;

namespace ConsoleTestRunner
{
	/// <summary>
	/// Summary description for TestInfo.
	/// </summary>
    public class TestInfo
    {
        #region Private Members

        #endregion

        #region Constructors and Destructor
        
        public TestInfo(XmlTestType filter)
        {
            Filter = filter;
        }

        #endregion

        #region Public Properties

        public string FileName { get; set; } = null;

        public string Directory { get; set; } = null;

        public XmlTestType Filter { get; set; } = XmlTestType.None;

        public bool Verbose { get; set; } = true;

        public bool Exception { get; set; } = true;

        public bool Interactive { get; set; } = false;

        #endregion

    }

}
