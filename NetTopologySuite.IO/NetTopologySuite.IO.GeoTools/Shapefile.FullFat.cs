﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    public partial class Shapefile
    {
        /// <summary>
        /// Creates a DataTable representing the information in a shape file.
        /// </summary>
        /// <param name="filename">The filename (minus the . and extension) to read.</param>
        /// <param name="tableName">The name to give to the table.</param>
        /// <param name="geometryFactory">The geometry factory to use when creating the objects.</param>
        /// <returns>DataTable representing the data </returns>
        public static DataTable CreateDataTable(string filename, string tableName, GeometryFactory geometryFactory, Encoding encoding = null)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (geometryFactory == null)
                throw new ArgumentNullException("geometryFactory");

            ShapefileDataReader shpfileDataReader = new ShapefileDataReader(filename, geometryFactory, encoding);
            DataTable table = new DataTable(tableName);

            // use ICustomTypeDescriptor to get the properies/ fields. This way we can get the 
            // length of the dbase char fields. Because the dbase char field is translated into a string
            // property, we lost the length of the field. We need to know the length of the
            // field when creating the table in the database.

            IEnumerator enumerator = shpfileDataReader.GetEnumerator();
            bool moreRecords = enumerator.MoveNext();
            ICustomTypeDescriptor typeDescriptor = (ICustomTypeDescriptor)enumerator.Current;
            foreach (PropertyDescriptor property in typeDescriptor.GetProperties())
            {
                ColumnStructure column = (ColumnStructure)property;
                Type fieldType = column.PropertyType;
                DataColumn datacolumn = new DataColumn(column.Name, fieldType);
                if (fieldType == typeof(string))
                    // use MaxLength to pass the length of the field in the dbase file
                    datacolumn.MaxLength = column.Length;
                table.Columns.Add(datacolumn);
            }

            // add the rows - need a do-while loop because we read one row in order to determine the fields
            int iRecordCount = 0;
            object[] values = new object[shpfileDataReader.FieldCount];
            do
            {
                iRecordCount++;
                shpfileDataReader.GetValues(values);
                table.Rows.Add(values);
                moreRecords = enumerator.MoveNext();
            }
            while (moreRecords);
            return table;
        }

        /// <summary>
        /// Imports a shapefile into a database table.
        /// </summary>
        /// <remarks>
        /// This method assumes a table has already been crated in the database.
        /// Calling this method does not close the connection that is passed in.
        /// </remarks>
        /// <param name="filename"></param>
        /// <param name="connectionstring"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static int ImportShapefile(string filename, string connectionstring, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                int rowsAdded = -1;
                PrecisionModel pm = new PrecisionModel();
                GeometryFactory geometryFactory = new GeometryFactory(pm, -1);

                DataTable shpDataTable = CreateDataTable(filename, tableName, geometryFactory);
                string createTableSql = CreateDbTable(shpDataTable, true);

                SqlCommand createTableCommand = new SqlCommand(createTableSql, connection);
                connection.Open();
                createTableCommand.ExecuteNonQuery();

                string sqlSelect = String.Format("select * from {0}", tableName);
                SqlDataAdapter selectCommand = new SqlDataAdapter(sqlSelect, connection);

                // use a data adaptor - saves donig the inserts ourselves
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand(sqlSelect, connection);
                SqlCommandBuilder custCB = new SqlCommandBuilder(dataAdapter);
                DataSet ds = new DataSet();

                // fill dataset
                dataAdapter.Fill(ds, shpDataTable.TableName);

                // copy rows from our datatable to the empty table in the DataSet
                int i = 0;
                foreach (DataRow row in shpDataTable.Rows)
                {
                    DataRow newRow = ds.Tables[0].NewRow();
                    newRow.ItemArray = row.ItemArray;
                    //gotcha! - new row still needs to be added to the table.
                    //NewRow() just creates a new row with the same schema as the table. It does
                    //not add it to the table.
                    ds.Tables[0].Rows.Add(newRow);
                    i++;
                }

                // update all the rows in batch
                rowsAdded = dataAdapter.Update(ds, shpDataTable.TableName);
                int iRows = shpDataTable.Rows.Count;
                Debug.Assert(rowsAdded != iRows, String.Format("{0} of {1] rows were added to the database.", rowsAdded, shpDataTable.Rows.Count));
                return rowsAdded;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="deleteExisting"></param>
        /// <returns></returns>
        private static string CreateDbTable(DataTable table, bool deleteExisting)
        {
            StringBuilder sb = new StringBuilder();
            if (deleteExisting)
            {
                sb.AppendFormat("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\n", table.TableName);
                sb.AppendFormat("drop table [dbo].[{0}]\n", table.TableName);
            }

            sb.AppendFormat("CREATE TABLE [dbo].[{0}] ( \n", table.TableName);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                string type = GetDbType(table.Columns[i].DataType, table.Columns[i].MaxLength);
                string columnName = table.Columns[i].ColumnName;
                if (columnName == "PRIMARY")
                {
                    columnName = "DBF_PRIMARY";
                    Debug.Assert(false, "Shp2Db: Column PRIMARY renamed to PRIMARY.");
                    Trace.WriteLine("Shp2Db: Column PRIMARY renamed to PRIMARY.");
                }
                sb.AppendFormat("[{0}] {1} ", columnName, type);

                // the unique id column cannot be null
                if (i == 1)
                    sb.Append(" NOT NULL ");
                if (i + 1 != table.Columns.Count)
                    sb.Append(",\n");
            }
            sb.Append(")\n");

            // the DataSet update stuff requires a unique column - so give it the row colum that we added
            //sb.AppendFormat("ALTER TABLE [dbo].[{0}] WITH NOCHECK ADD CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([{1}])  ON [PRIMARY]\n",table.TableName, table.Columns[1].ColumnName);
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="length"></param>
        /// <returns></returns>

        private static string GetDbType(Type type, int length)
        {
            if (type == typeof(double))
                return "real";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(string))
                return String.Format("nvarchar({0}) ", length);
            else if (type == typeof(byte[]))
                return "image";
            else if (type == typeof(int))
                return "int";
            else if (type == typeof(char[]))
                return String.Format("nvarchar({0}) ", length);
            throw new NotSupportedException("Need to add the SQL type for " + type.Name);
        }
    }
}
