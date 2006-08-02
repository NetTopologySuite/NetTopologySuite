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
         *  HACK: for SQLCLR integration i does avoid to use static members,
         *        i try to use readonly members and singleton implementations...
         */

        private NumberFormatInfo nfi = null;            

        private Global() 
        {
            nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";            
        }

        private readonly static Global global = new Global();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IFormatProvider GetNfi()
        {            
            return global.nfi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DataSet GetEPSGDataSet()
        {
            DataSet dataSet = new DataSet("EPGS");
            dataSet.ReadXml(Directory.GetCurrentDirectory() + @"\Database\EPSG_v61.xml");
            return dataSet;
        }
    }
}

