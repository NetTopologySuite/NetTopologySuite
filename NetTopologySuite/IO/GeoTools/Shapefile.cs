using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO.Handlers;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// This class is used to read and write ESRI Shapefiles.
	/// </summary>
	public class Shapefile
	{
		internal const int ShapefileId = 9994;
        internal const int Version = 1000;
        
        /// <summary>
		/// Given a geomtery object, returns the equilivent shape file type.
		/// </summary>
		/// <param name="geom">A Geometry object.</param>
		/// <returns>The equilivent for the geometry object.</returns>
        public static ShapeGeometryType GetShapeType(IGeometry geom) 
		{
			if (geom is IPoint) 
                return ShapeGeometryType.Point;
			if (geom is IPolygon) 
                return ShapeGeometryType.Polygon;
			if (geom is IMultiPolygon) 			
                return ShapeGeometryType.Polygon;
			if (geom is ILineString) 
                return ShapeGeometryType.LineString;
			if (geom is IMultiLineString) 			
                return ShapeGeometryType.LineString;
            if (geom is IMultiPoint)
                return ShapeGeometryType.MultiPoint;
            return ShapeGeometryType.NullShape;
		}

		/// <summary>
		/// Returns the appropriate class to convert a shaperecord to an OGIS geometry given the type of shape.
		/// </summary>
		/// <param name="type">The shapefile type.</param>
		/// <returns>An instance of the appropriate handler to convert the shape record to a Geometry object.</returns>
        public static ShapeHandler GetShapeHandler(ShapeGeometryType type) 
		{
			switch (type) 
			{
				case ShapeGeometryType.Point:
                case ShapeGeometryType.PointM:
                case ShapeGeometryType.PointZ:
                case ShapeGeometryType.PointZM:
					return new PointHandler();

                case ShapeGeometryType.Polygon:
                case ShapeGeometryType.PolygonM:
                case ShapeGeometryType.PolygonZ:
                case ShapeGeometryType.PolygonZM:
					return new PolygonHandler();

                case ShapeGeometryType.LineString:
                case ShapeGeometryType.LineStringM:
                case ShapeGeometryType.LineStringZ:
                case ShapeGeometryType.LineStringZM:
					return new MultiLineHandler();

                case ShapeGeometryType.MultiPoint:
                case ShapeGeometryType.MultiPointM:
                case ShapeGeometryType.MultiPointZ:
                case ShapeGeometryType.MultiPointZM:
                    return new MultiPointHandler();

                default:
                    return null;
			}			
		}

		/// <summary>
		/// Returns an ShapefileDataReader representing the data in a shapefile.
		/// </summary>
		/// <param name="filename">The filename (minus the . and extension) to read.</param>
		/// <param name="geometryFactory">The geometry factory to use when creating the objects.</param>
		/// <returns>An ShapefileDataReader representing the data in the shape file.</returns>
		public static ShapefileDataReader CreateDataReader(string filename, GeometryFactory geometryFactory)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");
			if (geometryFactory == null)
				throw new ArgumentNullException("geometryFactory");
			ShapefileDataReader shpDataReader= new ShapefileDataReader(filename,geometryFactory);
			return shpDataReader;
		}

		/// <summary>
		/// Creates a DataTable representing the information in a shape file.
		/// </summary>
		/// <param name="filename">The filename (minus the . and extension) to read.</param>
		/// <param name="tableName">The name to give to the table.</param>
		/// <param name="geometryFactory">The geometry factory to use when creating the objects.</param>
		/// <returns>DataTable representing the data </returns>
		public static DataTable CreateDataTable(string filename, string tableName, GeometryFactory geometryFactory)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");
			if (tableName == null)
				throw new ArgumentNullException("tableName");
			if (geometryFactory == null)
				throw new ArgumentNullException("geometryFactory");

			ShapefileDataReader shpfileDataReader= new ShapefileDataReader(filename, geometryFactory);
			DataTable table = new DataTable(tableName);
		
			// use ICustomTypeDescriptor to get the properies/ fields. This way we can get the 
			// length of the dbase char fields. Because the dbase char field is translated into a string
			// property, we lost the length of the field. We need to know the length of the
			// field when creating the table in the database.

			IEnumerator enumerator = shpfileDataReader.GetEnumerator();
			bool moreRecords = enumerator.MoveNext();
			ICustomTypeDescriptor typeDescriptor  = (ICustomTypeDescriptor) enumerator.Current;
			foreach (PropertyDescriptor property in typeDescriptor.GetProperties())
			{
				ColumnStructure column = (ColumnStructure) property;
				Type fieldType = column.PropertyType;
				DataColumn datacolumn = new DataColumn(column.Name, fieldType);
				if (fieldType== typeof(string))
					// use MaxLength to pass the length of the field in the dbase file
					datacolumn.MaxLength=column.Length;
				table.Columns.Add( datacolumn );
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
				sb.AppendFormat("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\n",table.TableName);
				sb.AppendFormat("drop table [dbo].[{0}]\n",table.TableName);
			}

			sb.AppendFormat("CREATE TABLE [dbo].[{0}] ( \n",table.TableName);
			for (int i=0; i < table.Columns.Count; i++)
			{
				string type = GetDbType(table.Columns[i].DataType, table.Columns[i].MaxLength );
				string columnName = table.Columns[i].ColumnName;
				if (columnName=="PRIMARY")
				{
					columnName="DBF_PRIMARY";
					Debug.Assert(false, "Shp2Db: Column PRIMARY renamed to PRIMARY.");
					Trace.WriteLine("Shp2Db: Column PRIMARY renamed to PRIMARY.");
				}
				sb.AppendFormat("[{0}] {1} ", columnName, type );
				
				// the unique id column cannot be null
				if (i==1)
					sb.Append(" NOT NULL ");
				if (i+1 != table.Columns.Count)
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
			throw new NotSupportedException("Need to add the SQL type for "+type.Name);
		}
	}
}