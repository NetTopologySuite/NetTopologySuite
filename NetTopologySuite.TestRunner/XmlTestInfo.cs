using System;
using System.Collections.Specialized;

namespace Open.Topology.TestRunner
{
	/// <summary>
	/// Summary description for XmlTestInfo.
	/// </summary>
	public struct XmlTestInfo
	{
        private StringDictionary m_listParameters;

		public XmlTestInfo(bool bReset)
		{
            m_listParameters = new StringDictionary();

            if (bReset)
                Reset();
		}

        public StringDictionary Parameters
        {
            get
            {
                return m_listParameters;
            }
        }

        public void Reset()
        {
            if (m_listParameters != null)
            {
                m_listParameters.Clear();

                m_listParameters.Add("desc",   String.Empty);
                m_listParameters.Add("a",      String.Empty);
                m_listParameters.Add("b",      String.Empty);
                m_listParameters.Add("name",   String.Empty);
                m_listParameters.Add("result", String.Empty);
                m_listParameters.Add("arg1",   String.Empty);
                m_listParameters.Add("arg2",   String.Empty);
                m_listParameters.Add("arg3",   String.Empty);
            }
        }

        public void SetValue(string key, string value)
        {
            if (m_listParameters != null)
            {
                m_listParameters[key] = value;
            }
        }

        public string GetValue(string key)
        {
            if (m_listParameters != null)
            {
                return m_listParameters[key];
            }

            return String.Empty;
        }

        public void Clear()
        {
            if (m_listParameters != null)
            {
                m_listParameters.Clear();
            }
        }

        public bool IsDefaultTarget()
        {
            string arg1 = GetValue("arg1");

            if (arg1 != null && arg1.Length > 0)
            {
                return (arg1 == "a") || (arg1 == "A");
            }

            return true;
        }
	}
}
