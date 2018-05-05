using System;

namespace Open.Topology.TestRunner
{
    public class XmlTestErrorEventArgs : EventArgs
    {
        private Exception m_objException = null;

        public XmlTestErrorEventArgs(Exception ex)
        {
            m_objException = ex;
        }

        public Exception Thrown
        {
            get
            {
                return m_objException;
            }
        }
    }

    public delegate void XmlTestErrorEventHandler(object sender, XmlTestErrorEventArgs args);

	/// <summary>
	/// Summary description for XmlTestExceptionManager.
	/// </summary>
	public class XmlTestExceptionManager
	{
		public XmlTestExceptionManager()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        public static event XmlTestErrorEventHandler ErrorEvent;

        public static void Publish(Exception ex)
        {
            if (ErrorEvent != null)
                ErrorEvent(typeof(XmlTestExceptionManager), new XmlTestErrorEventArgs(ex));
        }
	}
}
