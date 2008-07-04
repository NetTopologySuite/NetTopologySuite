using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml;
using GisSharpBlog.NetTopologySuite.Geometries;

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
        
        private ArrayList m_listarrTests         = null;

        private XmlTestCollection m_listCurTests = null;

        private XmlTestFactory    m_objFactory   = null;

        private string m_strTestWorkspace        = null;

        #endregion

        #region Constructors and Destructor
        
        public XmlTestDocument()
		{
            m_listarrTests = new ArrayList();
        }

        #endregion

        public void ResetTests()
        {
            if (m_listarrTests != null)
            {
                m_listarrTests.Clear();
            }

            if (m_listCurTests != null)
            {
                m_listCurTests.Clear();
                m_listCurTests = null;
            }

            m_strTestWorkspace   = null;
            m_objFactory         = null;
        }

        public XmlTestCollection CurrentTests
        {
            get
            {
                return m_listCurTests;
            }
        }

        public ArrayList Tests
        {
            get
            {
                return m_listarrTests;
            }
        }

        public bool LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new ArgumentException(fileName, 
                    "The file does not exits or the 'fileName' is not valid.");
            }

            try
            {
                XmlDocument xmldoc = new XmlDocument();

                xmldoc.Load(fileName);

                XmlElement root = xmldoc.DocumentElement;

                // Retrieve the "desc" tag, if any.
                XmlNode desc = root["desc"];
                string strTestDescription = String.Empty;
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
                    XmlAttributeCollection workspaceAttributes = workspace.Attributes;
                    if (workspaceAttributes != null && workspaceAttributes.Count > 0)
                    {
                        m_strTestWorkspace = workspaceAttributes["dir"].InnerText;
                    }
                }

                // Retrieve the "tolerance" attribute, if any.
                XmlNode tolerance = root["tolerance"];

                double dTolerance = 0.0;
                if (tolerance != null)
                {
                    string strTolerance = tolerance.InnerText;
                    try
                    {
                        dTolerance = Double.Parse(strTolerance, GetNumberFormatInfo());
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
                    XmlAttributeCollection precisionAttributes = precision.Attributes;
                    if (precisionAttributes != null && precisionAttributes.Count > 0)
                    {

                        XmlAttribute attribute = precisionAttributes["type"];
                        if (attribute != null)
                        {
                            string strPrecision = attribute.InnerText;

                            if (strPrecision == "FIXED" && precisionAttributes.Count == 4)
                            {
                                try
                                {
                                    double scale   =
                                        Double.Parse(precisionAttributes["scale"].InnerText, GetNumberFormatInfo());
                                    double offsetx =
                                        Double.Parse(precisionAttributes["offsetx"].InnerText, GetNumberFormatInfo());
                                    double offsety =
                                        Double.Parse(precisionAttributes["offsety"].InnerText, GetNumberFormatInfo());

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
                                double scale   =
                                    Double.Parse(precisionAttributes["scale"].InnerText, GetNumberFormatInfo());
                                double offsetx =
                                    Double.Parse(precisionAttributes["offsetx"].InnerText, GetNumberFormatInfo());
                                double offsety =
                                    Double.Parse(precisionAttributes["offsety"].InnerText, GetNumberFormatInfo());
 
                                pm = new PrecisionModel(scale);
                            }
                        }
                   }
                }

                if (pm == null)
                {
                    pm = new PrecisionModel();
                }
                m_objFactory   = new XmlTestFactory(pm);
                m_listCurTests = new XmlTestCollection();

                m_listCurTests.Name = strTestDescription;

                // Now, handle the "case" nodes
                XmlNodeList elemList = xmldoc.GetElementsByTagName("case");
                for (int i = 0; i < elemList.Count; i++)
                {   
                    ParseCaseNode(elemList[i], dTolerance);
                }

                m_listarrTests.Add(m_listCurTests);

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
                XmlTestInfo testInfo = new XmlTestInfo(true);

                XmlNode desc = caseNode["desc"];
                if (desc != null)
                {
                    testInfo.SetValue("desc", desc.InnerText);
                }

                XmlElement a = (XmlElement)caseNode["a"];
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

                XmlElement b = (XmlElement)caseNode["b"];
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
                XmlNodeList elemList = caseNode.SelectNodes("test");
                if (elemList == null)
                    return;
                if (elemList.Count <= 0)
                {
                    return;
                }
                else if (elemList.Count == 1)
                {
                    XmlElement testElement = ((XmlElement)elemList[0])["op"];
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

                    XmlTest xmlTest = m_objFactory.Create(testInfo, tolerance);
                    if (xmlTest != null && m_listCurTests != null)
                    {
                        m_listCurTests.Add(xmlTest);
                    }
                }
                else
                {
                    string baseDesc = testInfo.GetValue("desc");

                    for (int i = 0; i < elemList.Count; i++)
                    {   
                        string strDescNew = baseDesc + " - " + (i + 1).ToString();

                        testInfo.SetValue("desc", strDescNew);

                        XmlElement testElement = ((XmlElement)elemList[i])["op"];
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

                        XmlTest xmlTest = m_objFactory.Create(testInfo, tolerance);
                        if (xmlTest != null && m_listCurTests != null)
                        {
                            m_listCurTests.Add(xmlTest);
                        }
                    }
                }

                testInfo.Clear();
            }
        }

    }
}
