using System;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Global
    {
        /*
         *  HACK: for SQLCLR integration i does avoid to use public static members,
         *        i try to use readonly members and singleton implementations...
         */

        private NumberFormatInfo nfi = null;            

        /// <summary>
        /// 
        /// </summary>
        private Global() 
        {
            nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";            
        }

        /// <summary>
        /// 
        /// </summary>
		private static readonly Global global = new Global();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static NumberFormatInfo GetNfi()
        {            
            return global.nfi;
        }        
    }
}

