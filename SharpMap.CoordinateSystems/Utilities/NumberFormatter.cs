using System;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace SharpMap.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NumberFormatter
    {
        /*
         *  HACK: for SQLCLR integration i does avoid to use public static members,
         *        i try to use readonly members and singleton implementations...
         */

        private NumberFormatInfo nfi = null;            

        /// <summary>
        /// 
        /// </summary>
        private NumberFormatter() 
        {
            nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";            
        }

        /// <summary>
        /// 
        /// </summary>
        private readonly static NumberFormatter formatter = new NumberFormatter();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static NumberFormatInfo GetNfi()
        {
            return formatter.nfi;
        }        
    }
}

