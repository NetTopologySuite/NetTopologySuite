using System;
using System.Collections.Specialized;

namespace NetTopologySuite
{
    public class XmlTestInfo
    {
        private readonly StringDictionary _listParameters;

        public XmlTestInfo(Boolean bReset)
        {
            _listParameters = new StringDictionary();

            if (bReset)
            {
                Reset();
            }
        }

        public StringDictionary Parameters
        {
            get { return _listParameters; }
        }

        public void Reset()
        {
            if (_listParameters != null)
            {
                _listParameters.Clear();

                _listParameters.Add("desc", String.Empty);
                _listParameters.Add("a", String.Empty);
                _listParameters.Add("b", String.Empty);
                _listParameters.Add("name", String.Empty);
                _listParameters.Add("result", String.Empty);
                _listParameters.Add("arg1", String.Empty);
                _listParameters.Add("arg2", String.Empty);
                _listParameters.Add("arg3", String.Empty);
            }
        }

        public void SetValue(String key, String value)
        {
            if (_listParameters != null)
            {
                _listParameters[key] = value;
            }
        }

        public String GetValue(String key)
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

        public Boolean IsDefaultTarget()
        {
            String arg1 = GetValue("arg1");

            return String.IsNullOrEmpty(arg1) || ((arg1[0] == 'a') || (arg1[0] == 'A'));
        }
    }
}