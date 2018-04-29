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
	    public TestOptionsParser()
		{
            IsDefault = false;
		}
        public bool IsDefault { get; private set; }
	    public TestInfoCollection ParseProject(string projectFile)
        {
            if (!File.Exists(projectFile))
            {
                throw new ArgumentException(projectFile,
                    "The file does not exits or the 'projectFile' is not valid.");
            }
            try
            {
                var collection = new TestInfoCollection();
                var xmldoc = new XmlDocument();
                xmldoc.Load(projectFile);
                var root = xmldoc.DocumentElement;
                // Now, handle the "case" nodes
                var elemList = xmldoc.GetElementsByTagName("test");
                for (var i = 0; i < elemList.Count; i++)
                {
                    var element = elemList[i];
                    if (element != null)
                    {
                        var filterType = XmlTestType.None;
                        var bDisplayException = true;
                        var bVerbose          = true;
                        var bInteractive      = false;
                        var attributes = element.Attributes;
                        if (attributes != null && attributes.Count > 0)
                        {
                            var attFilter = attributes["filter"];
                            if (attFilter != null)
                            {
                                filterType = ParseXmlTestType(attFilter.InnerText);
                            }
                            var attException = attributes["exception"];
                            if (attException != null)
                            {
                                bDisplayException = Boolean.Parse(attException.InnerText);
                            }
                            var attVerbose = attributes["verbose"];
                            if (attVerbose != null)
                            {
                                bVerbose = Boolean.Parse(attVerbose.InnerText);
                            }
                            var attInteractive = attributes["interactive"];
                            if (attInteractive != null)
                            {
                                bInteractive = Boolean.Parse(attInteractive.InnerText);
                            }
                        }
                        var elemFiles = element.SelectNodes("files/file");
                        if (elemFiles != null)
                        {
                            for (var j = 0; j < elemFiles.Count; j++)
                            {
                                var xmlFile = elemFiles[j];
                                if (xmlFile != null)
                                {
                                    var info    = new TestInfo(filterType);
                                    info.FileName    = xmlFile.InnerText;
                                    info.Verbose     = bVerbose;
                                    info.Exception   = bDisplayException;
                                    info.Interactive = bInteractive;
                                    collection.Add(info);
                                }
                            }
                        }
                        var elemDirs = element.SelectNodes("dirs/dir");
                        if (elemDirs != null)
                        {
                            for (var k = 0; k < elemDirs.Count; k++)
                            {
                                var xmlDir = elemDirs[k];
                                if (xmlDir != null)
                                {
                                    var info    = new TestInfo(filterType);
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
            var collection = new TestInfoCollection();
            // Command line parsing
            var commandLine = new Arguments(args);
            // Check if default total test is requested...
            IsDefault = false;
            if (commandLine["default"] != null)
            {
                IsDefault = true;
            }
            // 1. Handle the "proj" option
            if (commandLine["proj"] != null)
            {
                return ParseProject(commandLine["proj"]);
            }
            // 2. Handle the display filter option
            var testType = XmlTestType.None;
            if (commandLine["filter"] != null)
            {
                testType = ParseXmlTestType(commandLine["filter"]);
            }
            // 3. Handle the display of the exception messages option
            var bDisplayException = true;
            if (commandLine["exception"] != null)
            {
                var strException = commandLine["exception"];
                strException        = strException.ToLower();
                bDisplayException   = (strException == "true");
            }
            // 4. Handle the verbose display option
            var bVerbose = true;
            if (commandLine["verbose"] != null)
            {
                var strVerbose = commandLine["verbose"];
                strVerbose        = strVerbose.ToLower();
                bVerbose   = (strVerbose == "true");
            }
            // 4. Handle the interactivity option
            var bInteractive = false;
            if (commandLine["interactive"] != null)
            {
                var strInteractive = commandLine["interactive"];
                strInteractive        = strInteractive.ToLower();
                bInteractive          = (strInteractive == "true");
                if (bInteractive)
                {
                    var info = new TestInfo(testType);
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
                var strFiles = commandLine["files"];
                Console.WriteLine(strFiles);
                var strSplits = strFiles.Split(',');
                for (var i = 0; i < strSplits.Length; i++)
                {
                    var info    = new TestInfo(testType);
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
                var strDirs = commandLine["dirs"];
                var strSplits = strDirs.Split(',');
                for (var i = 0; i < strSplits.Length; i++)
                {
                    var info    = new TestInfo(testType);
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
            var strTemp = testType.ToLower();
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
