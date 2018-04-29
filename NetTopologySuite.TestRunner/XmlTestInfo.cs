using System;
using System.Collections.Specialized;

namespace Open.Topology.TestRunner
{
	/// <summary>
	/// Summary description for XmlTestInfo.
	/// </summary>
	public struct XmlTestInfo
	{
	    public XmlTestInfo(bool bReset)
		{
            Parameters = new StringDictionary();

            if (bReset)
                Reset();
		}

        public StringDictionary Parameters { get; }

	    public void Reset()
        {
            if (Parameters != null)
            {
                Parameters.Clear();

                Parameters.Add("desc",   String.Empty);
                Parameters.Add("a",      String.Empty);
                Parameters.Add("b",      String.Empty);
                Parameters.Add("name",   String.Empty);
                Parameters.Add("result", String.Empty);
                Parameters.Add("arg1",   String.Empty);
                Parameters.Add("arg2",   String.Empty);
                Parameters.Add("arg3",   String.Empty);
            }
        }

        public void SetValue(string key, string value)
        {
            if (Parameters != null)
            {
                Parameters[key] = value;
            }
        }

        public string GetValue(string key)
        {
            if (Parameters != null)
            {
                return Parameters[key];
            }

            return String.Empty;
        }

        public void Clear()
        {
            if (Parameters != null)
            {
                Parameters.Clear();
            }
        }

        public bool IsDefaultTarget()
        {
            string arg1 = GetValue("arg1");

            if (!string.IsNullOrEmpty(arg1))
            {
                return (arg1 == "a") || (arg1 == "A");
            }

            return true;
        }
	}
}
