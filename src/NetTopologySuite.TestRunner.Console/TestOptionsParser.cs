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

        public bool IsDefault => m_bIsDefault;

        public TestInfoCollection ParseProject(string projectFile)
        {
            if (!File.Exists(projectFile))
            {
                throw new ArgumentException($"The file {projectFile} does not exist or is not valid.", nameof(projectFile));
            }

            try
            {
                var projectFileInfo = new FileInfo(projectFile);
                var collection = new TestInfoCollection();

                var xmldoc = new XmlDocument();

                xmldoc.Load(projectFile);

                var root = xmldoc.DocumentElement;

                // Now, handle the "case" nodes
                var elemList = xmldoc.GetElementsByTagName("test");
                for (int i = 0; i < elemList.Count; i++)
                {
                    var element = elemList[i];
                    if (element != null)
                    {
                        string filterType = null;
                        bool bDisplayException = true;
                        bool bVerbose          = true;
                        bool bInteractive      = false;

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
                                bDisplayException = bool.Parse(attException.InnerText);
                            }

                            var attVerbose = attributes["verbose"];
                            if (attVerbose != null)
                            {
                                bVerbose = bool.Parse(attVerbose.InnerText);
                            }

                            var attInteractive = attributes["interactive"];
                            if (attInteractive != null)
                            {
                                bInteractive = bool.Parse(attInteractive.InnerText);
                            }
                        }

                        var elemFiles = element.SelectNodes("files/file");
                        if (elemFiles != null)
                        {
                            for (int j = 0; j < elemFiles.Count; j++)
                            {
                                var xmlFile = elemFiles[j];
                                if (xmlFile != null)
                                {
                                    var info    = new TestInfo(filterType);
                                    info.FileName    = Path.Combine(projectFileInfo.DirectoryName, xmlFile.InnerText);
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
                            for (int k = 0; k < elemDirs.Count; k++)
                            {
                                var xmlDir = elemDirs[k];
                                if (xmlDir != null)
                                {
                                    var info    = new TestInfo(filterType);
                                    info.Directory   = Path.Combine(projectFileInfo.DirectoryName, xmlDir.InnerText);
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
            string testType = null;
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
                string strFiles = commandLine["files"];
                Console.WriteLine(strFiles);

                string[] strSplits = strFiles.Split(',');

                for (int i = 0; i < strSplits.Length; i++)
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
                string strDirs = commandLine["dirs"];

                string[] strSplits = strDirs.Split(',');

                for (int i = 0; i < strSplits.Length; i++)
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

        /// <summary>
        /// The operation named on the command line, carried through as written.
        /// </summary>
        /// <remarks>
        /// The runner resolves an operation by name, so there is no fixed set to map onto here:
        /// a name nothing answers to simply matches no case.
        /// </remarks>
        public string ParseXmlTestType(string testType)
        {
            string trimmed = testType?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }
    }
}
