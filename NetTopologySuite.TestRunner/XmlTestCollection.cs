using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Diagnostics;
using NetTopologySuite;
using NPack.Interfaces;

namespace NetTopologySuite
{
    /// <summary>
    /// Summary description for XmlTestCollection.
    /// </summary>
    public class XmlTestCollection<TCoordinate> : List<XmlTest<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        #region Private Members

        private String m_strCollectionName;

        #endregion

        #region Events

        public event XmlTextEventHandler<TCoordinate> TestEvent;

        #endregion

        #region Constructors and Destructors

        public XmlTestCollection()
        {
            m_strCollectionName = String.Empty;
        }

        #endregion

        #region Public Properties

        //public XmlTest<TCoordinate> this[Int32 index]
        //{
        //    get { return (XmlTest<TCoordinate>)List[index]; }

        //    set { List[index] = value; }
        //}

        public String Name
        {
            get { return m_strCollectionName; }

            set { m_strCollectionName = value; }
        }

        #endregion

        #region Public Methods

        //public Int32 Add(XmlTest<TCoordinate> value)
        //{
        //    return List.Add(value);
        //}

        //public Int32 IndexOf(XmlTest<TCoordinate> value)
        //{
        //    return List.IndexOf(value);
        //}

        //public void Insert(Int32 index, XmlTest<TCoordinate> value)
        //{
        //    List.Insert(index, value);
        //}

        //public void Remove(XmlTest<TCoordinate> value)
        //{
        //    List.Remove(value);
        //}

        public Boolean RunTests()
        {
            if (Count > 0)
            {
                for (Int32 i = 0; i < Count; i++)
                {
                    XmlTest<TCoordinate> testItem = this[i];

                    if (testItem != null)
                    {
                        RunTest(i);
                    }
                }

                return true;
            }

            return false;
        }

        public bool RunTest(int index)
        {
            if (Count > index)
            {
                XmlTest<TCoordinate> testItem = this[index];

                if (testItem != null)
                {
                    Boolean succeeded = testItem.Run();


                    XmlTestEventArgs<TCoordinate> args = new XmlTestEventArgs<TCoordinate>(index, succeeded, testItem);

                    if (TestEvent != null)
                    {
                        TestEvent(this, args);
                    }

                    return succeeded;
                }
            }
            return false;
        }

        #endregion
    }
}