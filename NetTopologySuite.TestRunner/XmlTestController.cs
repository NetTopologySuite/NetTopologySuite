using System;
using System.Collections.Specialized;
using System.IO;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite
{
    public class XmlTestController<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private StringCollection _fileNames;

        private readonly XmlTestDocument<TCoordinate> _currentDoc;

        public XmlTestController()
        {
            _fileNames = new StringCollection();
            _currentDoc = new XmlTestDocument<TCoordinate>();
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

        public Boolean RunFile(Int32 index, XmlTestDocument<TCoordinate>.CreateCoordinateFactory createCoordinateFactory, XmlTestDocument<TCoordinate>.CreateCoordinateSequenceFactory createCoordinateSequenceFactory)
        {
            if (_fileNames != null && _fileNames.Count > 0)
            {
                if (index >= 0 && index < _fileNames.Count)
                {
                    String fileName = _fileNames[index];

                    if (_currentDoc != null && _currentDoc.LoadFile(fileName, createCoordinateFactory, createCoordinateSequenceFactory))
                    {
                        XmlTestCollection<TCoordinate> listTests = _currentDoc.CurrentTests;

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

        public XmlTestCollection<TCoordinate> Load(String filePath, XmlTestDocument<TCoordinate>.CreateCoordinateFactory createCoordinateFactory, XmlTestDocument<TCoordinate>.CreateCoordinateSequenceFactory createCoordinateSequenceFactory)
        {
            if (_currentDoc != null)
            {
                if (_currentDoc.LoadFile(filePath, createCoordinateFactory, createCoordinateSequenceFactory))
                {
                    return _currentDoc.CurrentTests;
                }
            }

            return null;
        }
    }
}