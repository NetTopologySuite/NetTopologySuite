using System.Globalization;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    public sealed class Global
    {
        /*
         *  HACK: for SQLCLR integration i does avoid to use public static members,
         *        i try to use readonly members and singleton implementations...
         */

        private readonly NumberFormatInfo _nfi = null;

        private Global()
        {
            _nfi = new NumberFormatInfo();
            _nfi.NumberDecimalSeparator = ".";
        }

        private static readonly Global global = new Global();

        public static NumberFormatInfo NumberFormatInfo
        {
            get
            {
                return global._nfi;
            }
        }
    }
}