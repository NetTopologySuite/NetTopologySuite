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
        private StringCollection m_listFileNames = null;

        private XmlTestDocument  m_objCurrentDoc = null;

		public XmlTestController()
		{
            m_listFileNames = new StringCollection();
            m_objCurrentDoc = new XmlTestDocument();
		}

        public StringCollection FileNames
        {
            get
            {
                return m_listFileNames;
            }
        }

        public void ResetFiles()
        {
            if (m_listFileNames != null)
                m_listFileNames.Clear();            
       }

        public void Reset()
        {
            if (m_objCurrentDoc != null)
                m_objCurrentDoc.ResetTests();
            
            ResetFiles();
        }

        public bool RunFile(int index)
        {
            if (m_listFileNames != null && m_listFileNames.Count > 0)
            {
                if (index >= 0 && index < m_listFileNames.Count)
                {
                    string fileName = m_listFileNames[index];
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
            if (m_listFileNames == null)
                m_listFileNames = new StringCollection();
            
            try
            {
                string[] dirs = Directory.GetFiles(directory, "*.xml");
                foreach (string dir in dirs) 
                    m_listFileNames.Add(dir);                
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
