using System;
using System.Collections.Specialized;

namespace Open.Topology.TestRunner
{
	/// <summary>
	/// Summary description for XmlTestInfo.
	/// </summary>
	public struct XmlTestInfo
	{
        private readonly StringDictionary _listParameters;

		public XmlTestInfo(bool bReset)
		{
            _listParameters = new StringDictionary();

            if (bReset)
                Reset();
		}

        public StringDictionary Parameters
        {
            get
            {
                return _listParameters;
            }
        }

        public void Reset()
        {
            if (_listParameters != null)
            {
                _listParameters.Clear();

                _listParameters.Add("desc",   String.Empty);
                _listParameters.Add("a",      String.Empty);
                _listParameters.Add("b",      String.Empty);
                _listParameters.Add("name",   String.Empty);
                _listParameters.Add("result", String.Empty);
                _listParameters.Add("arg1",   String.Empty);
                _listParameters.Add("arg2",   String.Empty);
                _listParameters.Add("arg3",   String.Empty);
            }
        }

        public void SetValue(string key, string value)
        {
            if (_listParameters != null)
            {
                _listParameters[key] = value;
            }
        }

        public string GetValue(string key)
        {
            if (_listParameters != null)
            {
                return _listParameters[key];
            }

            return String.Empty;
        }

        public void Clear()
        {
            if (_listParameters != null)
            {
                _listParameters.Clear();
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
