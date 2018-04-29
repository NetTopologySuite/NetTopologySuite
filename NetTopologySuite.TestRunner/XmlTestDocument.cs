using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using NetTopologySuite.Geometries;
using Open.Topology.TestRunner.Operations;
using Open.Topology.TestRunner.Result;
namespace Open.Topology.TestRunner
{
	/// <summary>
	/// Summary description for XmlTestDocument.
	/// </summary>
	public class XmlTestDocument
	{
        private static NumberFormatInfo nfi = null;
        protected static IFormatProvider GetNumberFormatInfo()
        {
            if (nfi == null)
            {
                nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
            }
            return nfi;
        }
        #region Private Members
	    private XmlTestFactory    m_objFactory   = null;
        private string m_strTestWorkspace        = null;
        #endregion
        #region Constructors and Destructor
        public XmlTestDocument()
		{
            Tests = new ArrayList();
        }
        #endregion
        public void ResetTests()
        {
            if (Tests != null)
            {
                Tests.Clear();
            }
            if (CurrentTests != null)
            {
                CurrentTests.Clear();
                CurrentTests = null;
            }
            m_strTestWorkspace   = null;
            m_objFactory         = null;
        }
        public XmlTestCollection CurrentTests { get; private set; } = null;
	    public ArrayList Tests { get; } = null;
	    public bool LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new ArgumentException(fileName,
                    "The file does not exits or the 'fileName' is not valid.");
            }
            try
            {
                var xmldoc = new XmlDocument();
                xmldoc.Load(fileName);
                var root = xmldoc.DocumentElement;
                // Retrieve the "desc" tag, if any.
                XmlNode desc = root["desc"];
                var strTestDescription = string.Empty;
                if (desc != null && desc.InnerText.Length > 0)
                {
                    strTestDescription = desc.InnerText;
                }
                else
                {
                    strTestDescription = Path.GetFileNameWithoutExtension(fileName);
                }
                // Retrieve the "workspace", if any.
                XmlNode workspace = root["workspace"];
                if (workspace != null)
                {
                    var workspaceAttributes = workspace.Attributes;
                    if (workspaceAttributes != null && workspaceAttributes.Count > 0)
                    {
                        m_strTestWorkspace = workspaceAttributes["dir"].InnerText;
                    }
                }
                // Retrieve the "tolerance" attribute, if any.
                XmlNode tolerance = root["tolerance"];
                var dTolerance = 0.0;
                if (tolerance != null)
                {
                    var strTolerance = tolerance.InnerText;
                    try
                    {
                        dTolerance = double.Parse(strTolerance, GetNumberFormatInfo());
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                    }
                }
                // Retrieve the precisionName" tag, if any.
                PrecisionModel pm = null;
                XmlNode precision = root["precisionModel"];
                if (precision != null)
                {
                    var precisionAttributes = precision.Attributes;
                    if (precisionAttributes != null && precisionAttributes.Count > 0)
                    {
                        var attribute = precisionAttributes["type"];
                        if (attribute != null)
                        {
                            var strPrecision = attribute.InnerText;
                            if (strPrecision == "FIXED" && precisionAttributes.Count == 4)
                            {
                                try
                                {
                                    var scale   =
                                        double.Parse(precisionAttributes["scale"].InnerText, GetNumberFormatInfo());
                                    var offsetx =
                                        double.Parse(precisionAttributes["offsetx"].InnerText, GetNumberFormatInfo());
                                    var offsety =
                                        double.Parse(precisionAttributes["offsety"].InnerText, GetNumberFormatInfo());
                                    pm = new PrecisionModel(scale);
                                }
                                catch (Exception ex)
                                {
                                    XmlTestExceptionManager.Publish(ex);
                                }
                            }
                            else
                            {
                                pm = new PrecisionModel();
                            }
                        }
                        else
                        {
                            if (precisionAttributes.Count == 3)
                            {
                                var scale   =
                                    double.Parse(precisionAttributes["scale"].InnerText, GetNumberFormatInfo());
                                var offsetx =
                                    double.Parse(precisionAttributes["offsetx"].InnerText, GetNumberFormatInfo());
                                var offsety =
                                    double.Parse(precisionAttributes["offsety"].InnerText, GetNumberFormatInfo());
                                pm = new PrecisionModel(scale);
                            }
                        }
                   }
                }
                if (pm == null)
                {
                    pm = new PrecisionModel();
                }
                IGeometryOperation geometryOperation = null;
                XmlNode go = root["geometryOperation"];
                if (go != null)
                {
                    go = go.FirstChild;
                    switch (go.Value)
                    {
                        case "com.vividsolutions.jtstest.geomop.PreparedGeometryOperation":
                            geometryOperation = new PreparedGeometryOperation();
                            break;
                        case "com.vividsolutions.jtstest.geomop.BufferValidatedGeometryOperation":
                            geometryOperation = new BufferValidatedGeometryOperation();
                            break;
                        case "com.vividsolutions.jtstest.geomop.OverlayValidatedGeometryOperation":
                            geometryOperation = new OverlayValidatedGeometryOperation();
                            break;
                        default:
                            Console.WriteLine(string.Format("\n *** {0} *** \n", go.Value));
                            Console.ReadKey(true);
                            geometryOperation = new GeometryMethodOperation();
                            break;
                    }
                }
                IResultMatcher resultMatcher = null;
                XmlNode rm = root["resultMatcher"];
                if (rm != null)
                {
                    rm = rm.FirstChild;
                    if (rm.Value.EndsWith("BufferResultMatcher", StringComparison.InvariantCultureIgnoreCase))
                        resultMatcher = new BufferResultMatcher();
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                m_objFactory   = new XmlTestFactory(pm, geometryOperation, resultMatcher);
                CurrentTests = new XmlTestCollection();
                CurrentTests.Name = strTestDescription;
                // Now, handle the "case" nodes
                var elemList = xmldoc.GetElementsByTagName("case");
                for (var i = 0; i < elemList.Count; i++)
                {
                    ParseCaseNode(elemList[i], dTolerance);
                }
                Tests.Add(CurrentTests);
                return true;
            }
            catch (Exception ex)
            {
                XmlTestExceptionManager.Publish(ex);
                return false;
            }
        }
        private void ParseCaseNode(XmlNode caseNode, double tolerance)
        {
            if (caseNode != null && m_objFactory != null)
            {
                var testInfo = new XmlTestInfo(true);
                XmlNode desc = caseNode["desc"];
                if (desc != null)
                {
                    testInfo.SetValue("desc", desc.InnerText);
                }
                var a = (XmlElement)caseNode["a"];
                if (a != null)
                {
                    if (a.HasAttribute("file"))
                    {
                    }
                    else
                    {
                        testInfo.SetValue("a", a.InnerText);
                    }
                }
                var b = (XmlElement)caseNode["b"];
                if (b != null)
                {
                    if (b.HasAttribute("file"))
                    {
                    }
                    else
                    {
                        testInfo.SetValue("b", b.InnerText);
                    }
                }
                // Now, handle the "test" nodes
                var elemList = caseNode.SelectNodes("test");
                if (elemList == null)
                    return;
                if (elemList.Count <= 0)
                {
                    return;
                }
                else if (elemList.Count == 1)
                {
                    var testElement = ((XmlElement)elemList[0])["op"];
                    testInfo.SetValue("result", testElement.InnerText);
                    if (testElement.HasAttribute("name"))
                    {
                        testInfo.SetValue("name", testElement.GetAttribute("name"));
                    }
                    if (testElement.HasAttribute("arg1"))
                    {
                        testInfo.SetValue("arg1", testElement.GetAttribute("arg1").ToLower());
                    }
                    if (testElement.HasAttribute("arg2"))
                    {
                        testInfo.SetValue("arg2", testElement.GetAttribute("arg2").ToLower());
                    }
                    if (testElement.HasAttribute("arg3"))
                    {
                        testInfo.SetValue("arg3", testElement.GetAttribute("arg3"));
                    }
                    var xmlTest = m_objFactory.Create(testInfo, tolerance);
                    if (xmlTest != null && CurrentTests != null)
                    {
                        CurrentTests.Add(xmlTest);
                    }
                }
                else
                {
                    var baseDesc = testInfo.GetValue("desc");
                    for (var i = 0; i < elemList.Count; i++)
                    {
                        var strDescNew = baseDesc + " - " + (i + 1).ToString();
                        testInfo.SetValue("desc", strDescNew);
                        var testElement = ((XmlElement)elemList[i])["op"];
                        testInfo.SetValue("result", testElement.InnerText);
                        if (testElement.HasAttribute("name"))
                        {
                            testInfo.SetValue("name", testElement.GetAttribute("name"));
                        }
                        if (testElement.HasAttribute("arg1"))
                        {
                            testInfo.SetValue("arg1", testElement.GetAttribute("arg1"));
                        }
                        if (testElement.HasAttribute("arg2"))
                        {
                            testInfo.SetValue("arg2", testElement.GetAttribute("arg2"));
                        }
                        if (testElement.HasAttribute("arg3"))
                        {
                            testInfo.SetValue("arg3", testElement.GetAttribute("arg3"));
                        }
                        var xmlTest = m_objFactory.Create(testInfo, tolerance);
                        if (xmlTest != null && CurrentTests != null)
                        {
                            CurrentTests.Add(xmlTest);
                        }
                    }
                }
                testInfo.Clear();
            }
        }
    }
}
