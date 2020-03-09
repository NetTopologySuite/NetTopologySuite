#nullable disable
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

        public StringDictionary Parameters => _listParameters;

        public void Reset()
        {
            if (_listParameters != null)
            {
                _listParameters.Clear();

                _listParameters.Add("desc",   string.Empty);
                _listParameters.Add("a",      string.Empty);
                _listParameters.Add("b",      string.Empty);
                _listParameters.Add("name",   string.Empty);
                _listParameters.Add("result", string.Empty);
                _listParameters.Add("arg1",   string.Empty);
                _listParameters.Add("arg2",   string.Empty);
                _listParameters.Add("arg3",   string.Empty);
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

            return string.Empty;
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
