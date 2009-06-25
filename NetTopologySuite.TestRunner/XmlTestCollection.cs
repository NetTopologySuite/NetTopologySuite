using System;
using System.Collections;
using GeoAPI.Diagnostics;
using GisSharpBlog.NetTopologySuite;

namespace GisSharpBlog.NetTopologySuite
{
    /// <summary>
    /// Summary description for XmlTestCollection.
    /// </summary>
    public class XmlTestCollection : CollectionBase
    {
        #region Private Members

        private String m_strCollectionName;

        #endregion

        #region Events

        public event XmlTextEventHandler TestEvent;

        #endregion

        #region Constructors and Destructors

        public XmlTestCollection()
        {
            m_strCollectionName = String.Empty;
        }

        #endregion

        #region Public Properties

        public XmlTest this[Int32 index]
        {
            get { return (XmlTest)List[index]; }

            set { List[index] = value; }
        }

        public String Name
        {
            get { return m_strCollectionName; }

            set { m_strCollectionName = value; }
        }

        #endregion

        #region Public Methods

        public Int32 Add(XmlTest value)
        {
            return List.Add(value);
        }

        public Int32 IndexOf(XmlTest value)
        {
            return List.IndexOf(value);
        }

        public void Insert(Int32 index, XmlTest value)
        {
            List.Insert(index, value);
        }

        public void Remove(XmlTest value)
        {
            List.Remove(value);
        }

        public Boolean RunTests()
        {
            if (List.Count > 0)
            {
                for (Int32 i = 0; i < List.Count; i++)
                {
                    XmlTest testItem = (XmlTest)List[i];

                    if (testItem != null)
                    {
                        Boolean succeeded = testItem.Run();


                        XmlTestEventArgs args =
                            new XmlTestEventArgs(i, succeeded, testItem);

                        if (TestEvent != null)
                        {
                            TestEvent(this, args);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}