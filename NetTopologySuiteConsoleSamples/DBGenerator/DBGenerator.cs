using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;

using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.DBGenerator
{
    /// <summary>
    /// Generate XML exchange file for MDB Coordinate Systems Database.
    /// </summary>
    public class DBGenerator
    {        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void main(string[] args)
        {
            try
            {
                DataSet dataSet = new DataSet("EPSG_v61");

                string mdbpath = @"..\..\..\Database\EPSG_v61.mdb";
                string xmlpath = @"..\..\..\Database\EPSG_v61.xml";
                if (!File.Exists(mdbpath))
                {
                    Console.WriteLine("Database EPSG_v61.mdb not found at " + Path.GetDirectoryName(mdbpath));
                    return;
                }
                if(File.Exists(xmlpath))
                {
                    Console.WriteLine("Deleting xml database file at " + Path.GetDirectoryName(xmlpath));
                    File.Delete(xmlpath);
                }

                
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbpath;                
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    DataTable table = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });                    
                    DataRow[] rows = table.Select(null, null, DataViewRowState.CurrentRows);
                    string[] tableNames = new string[rows.Length];
                    int i = 0;
                    foreach (DataRow row in rows)
                    {
                        string name = row["TABLE_NAME"] as string;                        
                        tableNames[i++] = name;
                    }

                    foreach(string tableName in tableNames)
                    {
                        OleDbDataAdapter adapter = new OleDbDataAdapter();                                                       
                        OleDbCommand command = new OleDbCommand("SELECT * FROM [" + tableName + "]", connection);
                        Console.WriteLine(command.CommandText);
                        adapter.SelectCommand = command;
                        adapter.Fill(dataSet, tableName);
                    }

                    dataSet.WriteXml(xmlpath);
                    Console.WriteLine("XML Database file wtitten at " + Path.GetDirectoryName(xmlpath));
                    
                    connection.Close();                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
