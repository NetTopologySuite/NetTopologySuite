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
                Parameters.Add("desc",   string.Empty);
                Parameters.Add("a",      string.Empty);
                Parameters.Add("b",      string.Empty);
                Parameters.Add("name",   string.Empty);
                Parameters.Add("result", string.Empty);
                Parameters.Add("arg1",   string.Empty);
                Parameters.Add("arg2",   string.Empty);
                Parameters.Add("arg3",   string.Empty);
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
            return string.Empty;
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
            var arg1 = GetValue("arg1");
            if (!string.IsNullOrEmpty(arg1))
            {
                return (arg1 == "a") || (arg1 == "A");
            }
            return true;
        }
	}
}
