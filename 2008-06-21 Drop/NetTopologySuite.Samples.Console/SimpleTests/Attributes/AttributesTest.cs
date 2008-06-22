using System;
using System.Collections;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Features;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Attributes
{
    public class AttributesTest : BaseSamples
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            // Set current dir to shapefiles dir
			Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../NetTopologySuite.Samples.Shapefiles");

            // ReadFromShapeFile();
            // TestSharcDbf();
            TestShapeCreation();
        }

        private void TestShapeCreation()
        {
            ICoordinate[] points = new ICoordinate[3];
            points[0] = new Coordinate(0, 0);
            points[1] = new Coordinate(1, 0);
            points[2] = new Coordinate(1, 1);

            LineString line_string = new LineString(points);

            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("FOO", "FOO");

            Feature feature = new Feature(Factory.CreateMultiLineString(new ILineString[] { line_string }), attributes);
            Feature[] features = new Feature[1];
            features[0] = feature;

            ShapefileDataWriter shp_writer = new ShapefileDataWriter("C:\\line_string");
            shp_writer.Header = ShapefileDataWriter.GetHeader(features[0], features.Length);
            shp_writer.Write(features);             
        }

        private void TestSharcDbf()
        {
			string filename = @"\Strade.dbf";
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename + " not found at " + Environment.CurrentDirectory);

            DbaseFileReader reader = new DbaseFileReader(filename);
            DbaseFileHeader header = reader.GetHeader();
            Console.WriteLine("HeaderLength: " + header.HeaderLength);
            Console.WriteLine("RecordLength: " + header.RecordLength);
            Console.WriteLine("NumFields: " + header.NumFields);
            Console.WriteLine("NumRecords: " + header.NumRecords);            
            Console.WriteLine("LastUpdateDate: " + header.LastUpdateDate);
            foreach (DbaseFieldDescriptor descr in header.Fields)
            {
                Console.WriteLine("FieldName: " + descr.Name);
                Console.WriteLine("DBF Type: " + descr.DbaseType);
                Console.WriteLine("CLR Type: " + descr.Type);
                Console.WriteLine("Length: " + descr.Length);
                Console.WriteLine("DecimalCount: " + descr.DecimalCount);                
                Console.WriteLine("DataAddress: " + descr.DataAddress);                
            }

            IEnumerator ienum = reader.GetEnumerator();
            while (ienum.MoveNext())
            {
                ArrayList objs = (ArrayList)ienum.Current;
                foreach (object obj in objs)
                    Console.WriteLine(obj);                
            }
            Console.WriteLine();            
        }

        private void ReadFromShapeFile()
        {
            ArrayList featureCollection = new ArrayList();
            string filename = @"country";
            if (!File.Exists(filename + ".dbf"))
                throw new FileNotFoundException(filename + " not found at " + Environment.CurrentDirectory);
            ShapefileDataReader dataReader = new ShapefileDataReader(filename, new GeometryFactory());                        
            while (dataReader.Read())
            {
                Feature feature = new Feature();                
                feature.Geometry = dataReader.Geometry;                
                                
                int length = dataReader.DbaseHeader.NumFields;
                string[] keys = new string[length];
                for (int i = 0; i < length; i++)                
                    keys[i] = dataReader.DbaseHeader.Fields[i].Name;                

                feature.Attributes = new AttributesTable();
                for (int i = 0; i < length; i++)
                {                                        
                    object val = dataReader.GetValue(i);
                    feature.Attributes.AddAttribute(keys[i], val);
                }
               
                featureCollection.Add(feature);
            }

            int index = 0;
            Console.WriteLine("Elements = " + featureCollection.Count);
            foreach (Feature feature in featureCollection)
            {
                Console.WriteLine("Feature " + index++);                
                AttributesTable table = feature.Attributes as AttributesTable;
                foreach (string name in table.GetNames())
                    Console.WriteLine(name + ": " + table[name]);
            }
            
            // Test write with stub header            
			string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../NetTopologySuite.Samples.Shapefiles/testWriteStubHeader");
			if (File.Exists(file + ".shp")) File.Delete(file + ".shp");
            if (File.Exists(file + ".shx")) File.Delete(file + ".shx");
            if (File.Exists(file + ".dbf")) File.Delete(file + ".dbf");

            ShapefileDataWriter dataWriter = new ShapefileDataWriter(file);
            dataWriter.Header = ShapefileDataWriter.GetHeader(featureCollection[0] as Feature, featureCollection.Count);
            dataWriter.Write(featureCollection);

            // Test write with header from a existing shapefile
			file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../NetTopologySuite.Samples.Shapefiles/testWriteShapefileHeader");
			if (File.Exists(file + ".shp")) File.Delete(file + ".shp");
            if (File.Exists(file + ".shx")) File.Delete(file + ".shx");
            if (File.Exists(file + ".dbf")) File.Delete(file + ".dbf");

            dataWriter = new ShapefileDataWriter(file);
            dataWriter.Header = ShapefileDataWriter.GetHeader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../NetTopologySuite.Samples.Shapefiles/country.dbf"));
            dataWriter.Write(featureCollection);
        }
    }
}
