using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Encodings
{
    public class EncodingRegistry : IEncodingRegistry
    {
        private static readonly Dictionary<int, Encoding> _mapCodePageToEncoding
            = new Dictionary<int, Encoding>
                  {
                      {1250, new CP1250()},
                      {1251, new CP1251()},
                      {1252, new CP1252()},
                      {1253, new CP1253()},
                      {1254, new CP1254()},
                      {1257, new CP1257()},
                      {437, new CP437()},
                      {737, new CP737()},
                      {850, new CP850()},
                      {852, new CP852()},
                      {857, new CP857()},
                      {860, new CP860()},
                      {861, new CP861()},
                      {863, new CP863()},
                      {865, new CP865()},
                      {866, new CP866()},
                      {874, new CP874()},
                      {10029, new CP10029()}
                  };

        private static readonly Dictionary<Encoding, int> _mapEncodingToCodePage =
            _mapCodePageToEncoding.ToDictionary(a => a.Value, a => a.Key);

        #region IEncodingRegistry Members

        private static readonly Encoding _ascii = new ASCIIEncoding();
        public Encoding ASCII
        {
            get { return _ascii; }
        }

        public Encoding UTF8
        {
            get { return Encoding.UTF8; }
        }


        public Encoding Unicode
        {
            get { return Encoding.Unicode; }
        }

        public Encoding GetEncoding(int codePage)
        {
            Encoding enc;

            if (!_mapCodePageToEncoding.TryGetValue(codePage, out enc))
                enc = UTF8;

            return enc;
        }

        #endregion


        public int GetCodePage(Encoding encoding)
        {
            int codePage;
            if (!_mapEncodingToCodePage.TryGetValue(encoding, out codePage))
                codePage = encoding.CodePage();

            return codePage;
        }
    }
}