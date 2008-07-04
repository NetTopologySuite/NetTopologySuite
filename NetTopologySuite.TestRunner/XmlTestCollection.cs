using System;
using System.Collections;

namespace Open.Topology.TestRunner
{
	/// <summary>
	/// Summary description for XmlTestCollection.
	/// </summary>
	public class XmlTestCollection : CollectionBase
	{
        #region Private Members

        private string m_strCollectionName;
        
        #endregion

        #region Events

        public event XmlTextEventHandler TestEvent;
       
        #endregion

        #region Constructors and Destructors
		
        public XmlTestCollection() : base()
		{
            m_strCollectionName = String.Empty;
		}

        #endregion

        #region Public Properties
        
        public XmlTest this[int index]  
        {
            get  
            {
                return (XmlTest)List[index];
            }

            set  
            {
                List[index] = value;
            }
        }

        public string Name
        {
            get
            {
                return m_strCollectionName;
            }

            set
            {
                m_strCollectionName = value;
            }
        }

        #endregion

        #region Public Methods

        public int Add(XmlTest value)  
        {
            return List.Add(value);
        }

        public int IndexOf(XmlTest value)  
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, XmlTest value)  
        {
            List.Insert(index, value);
        }

        public void Remove(XmlTest value)  
        {
            List.Remove(value);
        }

        public bool RunTests()
        {
            if (List.Count > 0)
            {
                for (int i = 0; i < List.Count; i++)
                {
                    XmlTest testItem = (XmlTest) List[i];

                    if (testItem != null)
                    {
                        XmlTestEventArgs args = 
                            new XmlTestEventArgs(i, testItem.Run(), testItem);

                        if (TestEvent != null)
                            TestEvent(this, args);
                    }
                }

                return true;
            }

            return false;
        }

        #endregion 
	}
}
