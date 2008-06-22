using System;

namespace GisSharpBlog.NetTopologySuite
{
    public class XmlTestErrorEventArgs : EventArgs
    {
        private readonly Exception m_objException;

        public XmlTestErrorEventArgs(Exception ex)
        {
            m_objException = ex;
        }

        public Exception Thrown
        {
            get { return m_objException; }
        }
    }

    public delegate void XmlTestErrorEventHandler(object sender, XmlTestErrorEventArgs args);

    /// <summary>
    /// Summary description for XmlTestExceptionManager.
    /// </summary>
    public class XmlTestExceptionManager
    {
        public static event XmlTestErrorEventHandler ErrorEvent;

        public static void Publish(Exception ex)
        {
            if (ErrorEvent != null)
            {
                ErrorEvent(typeof (XmlTestExceptionManager), new XmlTestErrorEventArgs(ex));
            }
        }
    }
}