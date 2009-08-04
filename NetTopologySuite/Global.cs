using System.Globalization;

namespace GisSharpBlog.NetTopologySuite
{
    public sealed class Global
    {
        /*
         *  HACK: for SQLCLR integration i does avoid to use public static members,
         *        i try to use readonly members and singleton implementations...
         */

        private static readonly Global global = new Global();
        private readonly NumberFormatInfo _nfi;

        private Global()
        {
            _nfi = new NumberFormatInfo();
            _nfi.NumberDecimalSeparator = ".";
        }

        public static NumberFormatInfo NumberFormatInfo
        {
            get { return global._nfi; }
        }
    }
}