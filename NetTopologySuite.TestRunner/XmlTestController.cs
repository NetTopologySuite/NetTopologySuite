using System;
using System.Collections.Specialized;
using System.IO;

namespace Open.Topology.TestRunner
{
	/// <summary>
	/// Summary description for XmlTestController.
	/// </summary>
	public class XmlTestController
	{
	    private XmlTestDocument  m_objCurrentDoc = null;

		public XmlTestController()
		{
            FileNames = new StringCollection();
            m_objCurrentDoc = new XmlTestDocument();
		}

        public StringCollection FileNames { get; private set; } = null;

	    public void ResetFiles()
	    {
	        if (FileNames != null)
	            FileNames.Clear();
	    }

	    public void Reset()
        {
            if (m_objCurrentDoc != null)
                m_objCurrentDoc.ResetTests();

            ResetFiles();
        }

        public bool RunFile(int index)
        {
            if (FileNames != null && FileNames.Count > 0)
            {
                if (index >= 0 && index < FileNames.Count)
                {
                    string fileName = FileNames[index];
                    if (m_objCurrentDoc != null && m_objCurrentDoc.LoadFile(fileName))
                    {
                        XmlTestCollection listTests = m_objCurrentDoc.CurrentTests;
                        if (listTests != null && listTests.Count > 0)
                            return listTests.RunTests();
                    }
                }
            }
            return false;
        }

        public bool GetFiles(string directory)
        {
            if (FileNames == null)
                FileNames = new StringCollection();

            try
            {
                string[] dirs = Directory.GetFiles(directory, "*.xml");
                foreach (string dir in dirs)
                    FileNames.Add(dir);
                return true;
            }
            catch (Exception ex)
            {
                XmlTestExceptionManager.Publish(ex);
            }

            return false;
        }

        public XmlTestCollection Load(string filePath)
        {
            if (m_objCurrentDoc != null)
                if (m_objCurrentDoc.LoadFile(filePath))
                    return m_objCurrentDoc.CurrentTests;
             return null;
        }
	}
}
