using System;
using System.Collections.Specialized;
using System.IO;

namespace GisSharpBlog.NetTopologySuite
{
    public class XmlTestController
    {
        private StringCollection _fileNames;

        private readonly XmlTestDocument _currentDoc;

        public XmlTestController()
        {
            _fileNames = new StringCollection();
            _currentDoc = new XmlTestDocument();
        }

        public StringCollection FileNames
        {
            get { return _fileNames; }
        }

        public void ResetFiles()
        {
            if (_fileNames != null)
            {
                _fileNames.Clear();
            }
        }

        public void Reset()
        {
            if (_currentDoc != null)
            {
                _currentDoc.ResetTests();
            }

            ResetFiles();
        }

        public Boolean RunFile(Int32 index)
        {
            if (_fileNames != null && _fileNames.Count > 0)
            {
                if (index >= 0 && index < _fileNames.Count)
                {
                    String fileName = _fileNames[index];

                    if (_currentDoc != null && _currentDoc.LoadFile(fileName))
                    {
                        XmlTestCollection listTests = _currentDoc.CurrentTests;

                        if (listTests != null && listTests.Count > 0)
                        {
                            return listTests.RunTests();
                        }
                    }
                }
            }

            return false;
        }

        public Boolean GetFiles(String directory)
        {
            if (_fileNames == null)
            {
                _fileNames = new StringCollection();
            }

            try
            {
                String[] dirs = Directory.GetFiles(directory, "*.xml");

                foreach (String dir in dirs)
                {
                    _fileNames.Add(dir);
                }

                return true;
            }
            catch (Exception ex)
            {
                XmlTestExceptionManager.Publish(ex);
            }

            return false;
        }

        public XmlTestCollection Load(String filePath)
        {
            if (_currentDoc != null)
            {
                if (_currentDoc.LoadFile(filePath))
                {
                    return _currentDoc.CurrentTests;
                }
            }

            return null;
        }
    }
}