using System;
using System.Collections;

namespace ConsoleTestRunner
{
	/// <summary>
	/// Summary description for TestInfoCollection.
	/// </summary>
    public class TestInfoCollection : CollectionBase  
    {
        #region Constructors and Destructor
        
        public TestInfoCollection() : base()
        {
        }

        #endregion

        #region Public Properties
        
        public TestInfo this[int index]  
        {
            get  
            {
                return ((TestInfo)List[index]);
            }
            set  
            {
                List[index] = value;
            }
        }

        #endregion

        #region Public Methods
        
        public int Add(TestInfo value)  
        {
            return (List.Add(value));
        }

        public int IndexOf(TestInfo value)  
        {
            return (List.IndexOf(value));
        }

        public void Insert(int index, TestInfo value)  
        {
            List.Insert(index, value);
        }

        public void Remove(TestInfo value)  
        {
            List.Remove(value);
        }

        public bool Contains(TestInfo value)  
        {
            // If value is not of type TestInfo, this will return false.
            return (List.Contains(value));
        }

        #endregion

        #region Protected Overridable Methods

        protected override void OnInsert(int index, Object value)  
        {
            if (value.GetType() != Type.GetType("ConsoleTestRunner.TestInfo"))
                throw new ArgumentException("value must be of type TestInfo.", "value");
        }

        protected override void OnRemove(int index, Object value)  
        {
            if (value.GetType() != Type.GetType("ConsoleTestRunner.TestInfo"))
                throw new ArgumentException("value must be of type TestInfo.", "value");
        }

        protected override void OnSet( int index, Object oldValue, Object newValue )  
        {
            if (newValue.GetType() != Type.GetType("ConsoleTestRunner.TestInfo"))
                throw new ArgumentException("newValue must be of type TestInfo.", "newValue");
        }

        protected override void OnValidate(Object value)  
        {
            if (value.GetType() != Type.GetType("ConsoleTestRunner.TestInfo"))
                throw new ArgumentException("value must be of type TestInfo.");
        }

        #endregion
   }

}
