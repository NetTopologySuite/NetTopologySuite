using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using GeoAPI.Coordinates;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite
{
    public class XmlTestDocument<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private static NumberFormatInfo _numberFormatInfo;

        protected static IFormatProvider GetNumberFormatInfo()
        {
            if (_numberFormatInfo == null)
            {
                _numberFormatInfo = new NumberFormatInfo();
                _numberFormatInfo.NumberDecimalSeparator = ".";
            }

            return _numberFormatInfo;
        }

        #region Private Members

        private readonly ArrayList _listarrTests;

        private XmlTestCollection<TCoordinate> _listCurTests;

        private XmlTestFactory<TCoordinate> _xmlTestFactory;

        #endregion

        #region Constructors and Destructor

        public XmlTestDocument()
        {
            _listarrTests = new ArrayList();
        }

        #endregion

        public void ResetTests()
        {
            if (_listarrTests != null)
            {
                _listarrTests.Clear();
            }

            if (_listCurTests != null)
            {
                _listCurTests.Clear();
                _listCurTests = null;
            }

            _xmlTestFactory = null;
        }

        public XmlTestCollection<TCoordinate> CurrentTests
        {
            get { return _listCurTests; }
        }

        public ArrayList Tests
        {
            get { return _listarrTests; }
        }

        public delegate ICoordinateFactory<TCoordinate> CreateCoordinateFactory(PrecisionModelType type,  Double scale);

        public delegate ICoordinateSequenceFactory<TCoordinate> CreateCoordinateSequenceFactory(
            ICoordinateFactory<TCoordinate> coordinateFactory);

        public Boolean LoadFile(String fileName, CreateCoordinateFactory createCoordinateFactory, CreateCoordinateSequenceFactory createCoordinateSequenceFactory )
        {
            if (!File.Exists(fileName))
            {
                String message = String.Format("The file does not exits or is not valid: {0}.",
                                               fileName);
                throw new ArgumentException(message, "fileName");
            }

            try
            {
                XmlDocument xmldoc = new XmlDocument();

                xmldoc.Load(fileName);

                XmlElement root = xmldoc.DocumentElement;

                // Retrieve the "desc" tag, if any.
                XmlNode desc = root["desc"];
                String testDescription;

                if (desc != null && desc.InnerText.Length > 0)
                {
                    testDescription = desc.InnerText;
                }
                else
                {
                    testDescription = Path.GetFileNameWithoutExtension(fileName);
                }

                // Retrieve the "workspace", if any.
                //XmlNode workspace = root["workspace"];

                //if (workspace != null)
                //{
                //    XmlAttributeCollection workspaceAttributes = workspace.Attributes;

                //    if (workspaceAttributes != null && workspaceAttributes.TotalItemCount > 0)
                //    {
                //        m_strTestWorkspace = workspaceAttributes["dir"].InnerText;
                //    }
                //}

                // Retrieve the "tolerance" attribute, if any.
                XmlNode toleranceNode = root["tolerance"];

                Double tolerance = 0.0;

                if (toleranceNode != null)
                {
                    String toleranceText = toleranceNode.InnerText;

                    try
                    {
                        tolerance = Double.Parse(toleranceText, GetNumberFormatInfo());
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                    }
                }

                // Retrieve the precisionName" tag, if any.
                Double scale = Double.NaN;
                PrecisionModelType type = PrecisionModelType.DoubleFloating;

                XmlNode precision = root["precisionModel"];

                if (precision != null)
                {
                    XmlAttributeCollection precisionAttributes = precision.Attributes;

                    if (precisionAttributes != null && precisionAttributes.Count > 0)
                    {
                        XmlAttribute attribute = precisionAttributes["type"];

                        if (attribute != null)
                        {
                            String strPrecision = attribute.InnerText;

                            if (strPrecision == "FIXED" && precisionAttributes.Count == 4)
                            {
                                try
                                {
                                    scale = Double.Parse(precisionAttributes["scale"].InnerText,
                                                         GetNumberFormatInfo());
                                    Double offsetx
                                        = Double.Parse(precisionAttributes["offsetx"].InnerText,
                                                       GetNumberFormatInfo());
                                    Double offsety
                                        = Double.Parse(precisionAttributes["offsety"].InnerText,
                                                       GetNumberFormatInfo());
                                }
                                catch (Exception ex)
                                {
                                    XmlTestExceptionManager.Publish(ex);
                                }
                            }
                            else if (strPrecision == "FLOATING_SINGLE")
                            {
                                type = PrecisionModelType.SingleFloating;
                            }
                        }
                        else
                        {
                            if (precisionAttributes.Count == 3)
                            {
                                scale = Double.Parse(precisionAttributes["scale"].InnerText,
                                                     GetNumberFormatInfo());
                                Double offsetx =
                                    Double.Parse(precisionAttributes["offsetx"].InnerText,
                                                 GetNumberFormatInfo());
                                Double offsety =
                                    Double.Parse(precisionAttributes["offsety"].InnerText,
                                                 GetNumberFormatInfo());
                            }
                        }
                    }
                }

                ICoordinateFactory<TCoordinate> coordFactory = createCoordinateFactory(type, scale);;

                //if (!Double.IsNaN(scale))
                //{
                //    coordFactory = 
                //}
                //else
                //{
                //    coordFactory = new BufferedCoordinateFactory(type);
                //}

                ICoordinateSequenceFactory<TCoordinate> seqFactory =
                    createCoordinateSequenceFactory(coordFactory);
                    //new BufferedCoordinateSequenceFactory(coordFactory);

                _xmlTestFactory = new XmlTestFactory<TCoordinate>(seqFactory);
                _listCurTests = new XmlTestCollection<TCoordinate>();

                _listCurTests.Name = testDescription;

                // Now, handle the "case" nodes
                XmlNodeList elemList = xmldoc.GetElementsByTagName("case");

                for (Int32 i = 0; i < elemList.Count; i++)
                {
                    ParseCaseNode(elemList[i], tolerance);
                }

                _listarrTests.Add(_listCurTests);

                return true;
            }
            catch (Exception ex)
            {
                XmlTestExceptionManager.Publish(ex);

                return false;
            }
        }

        private void ParseCaseNode(XmlNode caseNode, Double tolerance)
        {
            if (caseNode != null && _xmlTestFactory != null)
            {
                XmlTestInfo testInfo = new XmlTestInfo(true);

                XmlNode desc = caseNode["desc"];

                if (desc != null)
                {
                    testInfo.SetValue("desc", desc.InnerText);
                }

                XmlElement a = caseNode["a"];

                if (a != null)
                {
                    if (!a.HasAttribute("file"))
                    {
                        testInfo.SetValue("a", a.InnerText);
                    }
                }

                XmlElement b = caseNode["b"];

                if (b != null)
                {
                    if (!b.HasAttribute("file"))
                    {
                        testInfo.SetValue("b", b.InnerText);
                    }
                }

                // Now, handle the "test" nodes
                XmlNodeList elemList = caseNode.SelectNodes("test");

                if (elemList == null)
                {
                    return;
                }

                if (elemList.Count <= 0)
                {
                    return;
                }
                
                if (elemList.Count == 1)
                {
                    XmlElement testElement = (elemList[0])["op"];
                    Debug.Assert(testElement != null);
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

                    XmlTest<TCoordinate> xmlTest = _xmlTestFactory.Create(_listCurTests.Count, testInfo, tolerance);

                    if (xmlTest != null && _listCurTests != null)
                    {
                        _listCurTests.Add(xmlTest);
                    }
                }
                else
                {
                    String baseDesc = testInfo.GetValue("desc");

                    for (Int32 i = 0; i < elemList.Count; i++)
                    {
                        String strDescNew = baseDesc + " - " + (i + 1);

                        testInfo.SetValue("desc", strDescNew);

                        XmlElement testElement = (elemList[i])["op"];
                        Debug.Assert(testElement != null);
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

                        XmlTest<TCoordinate> xmlTest = _xmlTestFactory.Create(_listCurTests.Count, testInfo, tolerance);

                        if (xmlTest != null && _listCurTests != null)
                        {
                            _listCurTests.Add(xmlTest);
                        }
                    }
                }

                testInfo.Clear();
            }
        }
    }
}