using System;
using System.IO;
using System.Xml;
using Open.Topology.TestRunner;

namespace ConsoleTestRunner
{
	/// <summary>
	/// Summary description for TestOptionsParser.
	/// </summary>
	public class TestOptionsParser
	{
        private bool m_bIsDefault;

		public TestOptionsParser()
		{
            m_bIsDefault = false;
		}

        public bool IsDefault
        {
            get
            {
                return m_bIsDefault;
            }
        }

        public TestInfoCollection ParseProject(string projectFile)
        {
            if (!File.Exists(projectFile))
            {
                throw new ArgumentException(projectFile, 
                    "The file does not exits or the 'projectFile' is not valid.");
            }

            try
            {
                TestInfoCollection collection = new TestInfoCollection();

                XmlDocument xmldoc = new XmlDocument();

                xmldoc.Load(projectFile);

                XmlElement root = xmldoc.DocumentElement; 

                // Now, handle the "case" nodes
                XmlNodeList elemList = xmldoc.GetElementsByTagName("test");
                for (int i = 0; i < elemList.Count; i++)
                {   
                    XmlNode element = elemList[i];
                    if (element != null)
                    {
                        XmlTestType filterType = XmlTestType.None;
                        bool bDisplayException = true;
                        bool bVerbose          = true;
                        bool bInteractive      = false;

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
                            for (int j = 0; j < elemFiles.Count; j++)
                            { 
                                XmlNode xmlFile = elemFiles[j];
                                if (xmlFile != null)
                                {
                                    TestInfo info    = new TestInfo(filterType);
                                    info.FileName    = xmlFile.InnerText;
                                    info.Verbose     = bVerbose;
                                    info.Exception   = bDisplayException;
                                    info.Interactive = bInteractive;

                                    collection.Add(info);
                                }
                            } 
                        }
 
                        XmlNodeList elemDirs = element.SelectNodes("dirs/dir");
                        if (elemDirs != null)
                        {
                            for (int k = 0; k < elemDirs.Count; k++)
                            { 
                                XmlNode xmlDir = elemDirs[k];
                                if (xmlDir != null)
                                {
                                    TestInfo info    = new TestInfo(filterType);
                                    info.Directory   = xmlDir.InnerText;
                                    info.Verbose     = bVerbose;
                                    info.Exception   = bDisplayException;
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

        public TestInfoCollection Parse(string[] args)
        {
            TestInfoCollection collection = new TestInfoCollection();

            // Command line parsing
            Arguments commandLine = new Arguments(args);

            // Check if default total test is requested...
            m_bIsDefault = false;
            if (commandLine["default"] != null)
            {
                m_bIsDefault = true;
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
            bool bDisplayException = true;
            if (commandLine["exception"] != null)
            {
                string strException = commandLine["exception"];
                strException        = strException.ToLower();

                bDisplayException   = (strException == "true");
            }

            // 4. Handle the verbose display option 
            bool bVerbose = true;
            if (commandLine["verbose"] != null)
            {
                string strVerbose = commandLine["verbose"];
                strVerbose        = strVerbose.ToLower();

                bVerbose   = (strVerbose == "true");
            }

            // 4. Handle the interactivity option 
            bool bInteractive = false;
            if (commandLine["interactive"] != null)
            {
                string strInteractive = commandLine["interactive"];
                strInteractive        = strInteractive.ToLower();

                bInteractive          = (strInteractive == "true");
                if (bInteractive)
                {
                    TestInfo info = new TestInfo(testType);
                    info.Verbose     = bVerbose;
                    info.Exception   = bDisplayException;
                    info.Interactive = bInteractive;

                    collection.Add(info);

                    return collection;
                }
            }

            // 5. Handle the files option, if any
            if (commandLine["files"] != null)
            {
                string strFiles = commandLine["files"];
                Console.WriteLine(strFiles);

                string[] strSplits = strFiles.Split(',');
    
                for (int i = 0; i < strSplits.Length; i++)
                {
                    TestInfo info    = new TestInfo(testType);
                    info.FileName    = strSplits[i].Trim();
                    info.Verbose     = bVerbose;
                    info.Exception   = bDisplayException;
                    info.Interactive = false;

                    collection.Add(info);
                }
            }

            // 6. Handle the dirs option, if any
            if (commandLine["dirs"] != null)
            {
                string strDirs = commandLine["dirs"];

                string[] strSplits = strDirs.Split(',');
    
                for (int i = 0; i < strSplits.Length; i++)
                {
                    TestInfo info    = new TestInfo(testType);
                    info.Directory   = strSplits[i].Trim();
                    info.Verbose     = bVerbose;
                    info.Exception   = bDisplayException;
                    info.Interactive = false;

                    collection.Add(info);
                }
            }

            return collection;
        }

        public XmlTestType ParseXmlTestType(string testType)
        {
            string strTemp = testType.ToLower();

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
