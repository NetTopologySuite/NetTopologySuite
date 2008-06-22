using System;
using System.IO;
using System.Xml;
using SysConsole = System.Console;

namespace GisSharpBlog.NetTopologySuite.Console
{
    /// <summary>
    /// Summary description for TestOptionsParser.
    /// </summary>
    public class TestOptionsParser
    {
        private Boolean _bIsDefault;

        public TestOptionsParser()
        {
            _bIsDefault = false;
        }

        public Boolean IsDefault
        {
            get { return _bIsDefault; }
        }

        public TestInfoCollection ParseProject(String projectFile)
        {
            if (!File.Exists(projectFile))
            {
                throw new ArgumentException("The file does not exits or the 'projectFile' is not valid.",
                                            "projectFile");
            }

            try
            {
                TestInfoCollection collection = new TestInfoCollection();

                XmlDocument xmldoc = new XmlDocument();

                xmldoc.Load(projectFile);

                XmlElement root = xmldoc.DocumentElement;

                // Now, handle the "case" nodes
                XmlNodeList elemList = xmldoc.GetElementsByTagName("test");
                for (Int32 i = 0; i < elemList.Count; i++)
                {
                    XmlNode element = elemList[i];
                    if (element != null)
                    {
                        XmlTestType filterType = XmlTestType.None;
                        Boolean bDisplayException = true;
                        Boolean bVerbose = true;
                        Boolean bInteractive = false;

                        XmlAttributeCollection attributes = element.Attributes;
                        if (attributes != null && attributes.Count > 0)
                        {
                            XmlAttribute attFilter = attributes["filter"];
                            if (attFilter != null)
                            {
                                filterType = ParseXmlTestType(attFilter.InnerText);
                            }

                            XmlAttribute attException = attributes["exception"];
                            if (attException != null)
                            {
                                bDisplayException = Boolean.Parse(attException.InnerText);
                            }

                            XmlAttribute attVerbose = attributes["verbose"];
                            if (attVerbose != null)
                            {
                                bVerbose = Boolean.Parse(attVerbose.InnerText);
                            }

                            XmlAttribute attInteractive = attributes["interactive"];
                            if (attInteractive != null)
                            {
                                bInteractive = Boolean.Parse(attInteractive.InnerText);
                            }
                        }

                        XmlNodeList elemFiles = element.SelectNodes("files/file");
                        if (elemFiles != null)
                        {
                            for (Int32 j = 0; j < elemFiles.Count; j++)
                            {
                                XmlNode xmlFile = elemFiles[j];
                                if (xmlFile != null)
                                {
                                    TestInfo info = new TestInfo(filterType);
                                    info.FileName = xmlFile.InnerText;
                                    info.Verbose = bVerbose;
                                    info.Exception = bDisplayException;
                                    info.Interactive = bInteractive;

                                    collection.Add(info);
                                }
                            }
                        }

                        XmlNodeList elemDirs = element.SelectNodes("dirs/dir");
                        if (elemDirs != null)
                        {
                            for (Int32 k = 0; k < elemDirs.Count; k++)
                            {
                                XmlNode xmlDir = elemDirs[k];
                                if (xmlDir != null)
                                {
                                    TestInfo info = new TestInfo(filterType);
                                    info.Directory = xmlDir.InnerText;
                                    info.Verbose = bVerbose;
                                    info.Exception = bDisplayException;
                                    info.Interactive = bInteractive;

                                    collection.Add(info);
                                }
                            }
                        }
                    }
                }

                return collection;
            }
            catch (Exception ex)
            {
                XmlTestExceptionManager.Publish(ex);

                return null;
            }
        }

        public TestInfoCollection Parse(String[] args)
        {
            TestInfoCollection collection = new TestInfoCollection();

            // Command line parsing
            Arguments commandLine = new Arguments(args);

            // Check if default total test is requested...
            _bIsDefault = false;
            if (commandLine["default"] != null)
            {
                _bIsDefault = true;
            }

            // 1. Handle the "proj" option
            if (commandLine["proj"] != null)
            {
                return ParseProject(commandLine["proj"]);
            }

            // 2. Handle the display filter option
            XmlTestType testType = XmlTestType.None;
            if (commandLine["filter"] != null)
            {
                testType = ParseXmlTestType(commandLine["filter"]);
            }

            // 3. Handle the display of the exception messages option
            Boolean bDisplayException = true;
            if (commandLine["exception"] != null)
            {
                String strException = commandLine["exception"];
                strException = strException.ToLower();

                bDisplayException = (strException == "true");
            }

            // 4. Handle the verbose display option 
            Boolean bVerbose = true;
            if (commandLine["verbose"] != null)
            {
                String strVerbose = commandLine["verbose"];
                strVerbose = strVerbose.ToLower();

                bVerbose = (strVerbose == "true");
            }

            // 4. Handle the interactivity option 
            Boolean bInteractive = false;
            if (commandLine["interactive"] != null)
            {
                String strInteractive = commandLine["interactive"];
                strInteractive = strInteractive.ToLower();

                bInteractive = (strInteractive == "true");
                if (bInteractive)
                {
                    TestInfo info = new TestInfo(testType);
                    info.Verbose = bVerbose;
                    info.Exception = bDisplayException;
                    info.Interactive = bInteractive;

                    collection.Add(info);

                    return collection;
                }
            }

            // 5. Handle the files option, if any
            if (commandLine["files"] != null)
            {
                String strFiles = commandLine["files"];
                SysConsole.WriteLine(strFiles);

                String[] strSplits = strFiles.Split(',');

                for (Int32 i = 0; i < strSplits.Length; i++)
                {
                    TestInfo info = new TestInfo(testType);
                    info.FileName = strSplits[i].Trim();
                    info.Verbose = bVerbose;
                    info.Exception = bDisplayException;
                    info.Interactive = false;

                    collection.Add(info);
                }
            }

            // 6. Handle the dirs option, if any
            if (commandLine["dirs"] != null)
            {
                String strDirs = commandLine["dirs"];

                String[] strSplits = strDirs.Split(',');

                for (Int32 i = 0; i < strSplits.Length; i++)
                {
                    TestInfo info = new TestInfo(testType);
                    info.Directory = strSplits[i].Trim();
                    info.Verbose = bVerbose;
                    info.Exception = bDisplayException;
                    info.Interactive = false;

                    collection.Add(info);
                }
            }

            return collection;
        }

        public XmlTestType ParseXmlTestType(String testType)
        {
            String strTemp = testType.ToLower();

            if (strTemp == "area")
            {
                return XmlTestType.Area;
            }
            else if (strTemp == "boundary")
            {
                return XmlTestType.Boundary;
            }
            else if (strTemp == "boundarydimension")
            {
                return XmlTestType.BoundaryDimension;
            }
            else if (strTemp == "buffer")
            {
                return XmlTestType.Buffer;
            }
            else if (strTemp == "centroid")
            {
                return XmlTestType.Centroid;
            }
            else if (strTemp == "contains")
            {
                return XmlTestType.Contains;
            }
            else if (strTemp == "convexhull")
            {
                return XmlTestType.ConvexHull;
            }
            else if (strTemp == "crosses")
            {
                return XmlTestType.Crosses;
            }
            else if (strTemp == "difference")
            {
                return XmlTestType.Difference;
            }
            else if (strTemp == "dimension")
            {
                return XmlTestType.Dimension;
            }
            else if (strTemp == "disjoint")
            {
                return XmlTestType.Disjoint;
            }
            else if (strTemp == "distance")
            {
                return XmlTestType.Distance;
            }
            else if (strTemp == "envelope")
            {
                return XmlTestType.Envelope;
            }
            else if (strTemp == "equals")
            {
                return XmlTestType.Equals;
            }
            else if (strTemp == "interiorpoint")
            {
                return XmlTestType.InteriorPoint;
            }
            else if (strTemp == "intersection")
            {
                return XmlTestType.Intersection;
            }
            else if (strTemp == "intersects")
            {
                return XmlTestType.Intersects;
            }
            else if (strTemp == "isempty")
            {
                return XmlTestType.IsEmpty;
            }
            else if (strTemp == "issimple")
            {
                return XmlTestType.IsSimple;
            }
            else if (strTemp == "isvalid")
            {
                return XmlTestType.IsValid;
            }
            else if (strTemp == "iswithindistance")
            {
                return XmlTestType.IsWithinDistance;
            }
            else if (strTemp == "length")
            {
                return XmlTestType.Length;
            }
            else if (strTemp == "numpoints")
            {
                return XmlTestType.NumPoints;
            }
            else if (strTemp == "overlaps")
            {
                return XmlTestType.Overlaps;
            }
            else if (strTemp == "relate")
            {
                return XmlTestType.Relate;
            }
            else if (strTemp == "srid")
            {
                return XmlTestType.SRID;
            }
            else if (strTemp == "symmetricdifference")
            {
                return XmlTestType.SymmetricDifference;
            }
            else if (strTemp == "symdifference")
            {
                return XmlTestType.SymmetricDifference;
            }
            else if (strTemp == "touches")
            {
                return XmlTestType.Touches;
            }
            else if (strTemp == "union")
            {
                return XmlTestType.Union;
            }
            else if (strTemp == "within")
            {
                return XmlTestType.Within;
            }

            return XmlTestType.None;
        }
    }
}