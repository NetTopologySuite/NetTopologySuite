using System;
using System.Collections;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Samples.SimpleTests.Attributes
{
    public class AttributesTest : BaseSamples
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            // Set current dir to shapefiles dir
			Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar));

            // ReadFromShapeFile();
            // TestSharcDbf();
            TestShapeCreation();
        }

        private void TestShapeCreation()
        {
            var points = new Coordinate[3];
            points[0] = new Coordinate(0, 0);
            points[1] = new Coordinate(1, 0);
            points[2] = new Coordinate(1, 1);

            var line_string = new LineString(points);

            var attributes = new AttributesTable();
            attributes.AddAttribute("FOO", "FOO");

            var feature = new Feature(Factory.CreateMultiLineString(new ILineString[] { line_string }), attributes);
            var features = new Feature[1];
            features[0] = feature;

            var shp_writer = new ShapefileDataWriter("line_string")
            {
                Header = ShapefileDataWriter.GetHeader(features[0], features.Length)
            };
            shp_writer.Write(features);             
        }

        private void TestSharcDbf()
        {
			const string filename = @"Strade.dbf";
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename + " not found at " + Environment.CurrentDirectory);

            var reader = new DbaseFileReader(filename);
            var header = reader.GetHeader();
            Console.WriteLine("HeaderLength: " + header.HeaderLength);
            Console.WriteLine("RecordLength: " + header.RecordLength);
            Console.WriteLine("NumFields: " + header.NumFields);
            Console.WriteLine("NumRecords: " + header.NumRecords);            
            Console.WriteLine("LastUpdateDate: " + header.LastUpdateDate);
            foreach (var descr in header.Fields)
            {
                Console.WriteLine("FieldName: " + descr.Name);
                Console.WriteLine("DBF Type: " + descr.DbaseType);
                Console.WriteLine("CLR Type: " + descr.Type);
                Console.WriteLine("Length: " + descr.Length);
                Console.WriteLine("DecimalCount: " + descr.DecimalCount);                
                Console.WriteLine("DataAddress: " + descr.DataAddress);                
            }

            var ienum = reader.GetEnumerator();
            while (ienum.MoveNext())
            {
                var objs = (ArrayList)ienum.Current;
                foreach (var obj in objs)
                    Console.WriteLine(obj);                
            }
            Console.WriteLine();            
        }

        private void ReadFromShapeFile()
        {
            var featureCollection = new ArrayList();
            const string filename = @"country";
            if (!File.Exists(filename + ".dbf"))
                throw new FileNotFoundException(filename + " not found at " + Environment.CurrentDirectory);
            var dataReader = new ShapefileDataReader(filename, new GeometryFactory());                        
            while (dataReader.Read())
            {
                var feature = new Feature {Geometry = dataReader.Geometry};

                var length = dataReader.DbaseHeader.NumFields;
                var keys = new string[length];
                for (var i = 0; i < length; i++)                
                    keys[i] = dataReader.DbaseHeader.Fields[i].Name;                

                feature.Attributes = new AttributesTable();
                for (var i = 0; i < length; i++)
                {                                        
                    var val = dataReader.GetValue(i);
                    feature.Attributes.AddAttribute(keys[i], val);
                }
               
                featureCollection.Add(feature);
            }

            var index = 0;
            Console.WriteLine("Elements = " + featureCollection.Count);
            foreach (IFeature feature in featureCollection)
            {
                Console.WriteLine("Feature " + index++);                
                var table = feature.Attributes as AttributesTable;
                foreach (var name in table.GetNames())
                    Console.WriteLine(name + ": " + table[name]);
            }
            
            //Directory
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                   string.Format(@"..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles{0}", 
                                                 Path.DirectorySeparatorChar));
            // Test write with stub header            
            var file = dir + "testWriteStubHeader";
			if (File.Exists(file + ".shp")) File.Delete(file + ".shp");
            if (File.Exists(file + ".shx")) File.Delete(file + ".shx");
            if (File.Exists(file + ".dbf")) File.Delete(file + ".dbf");

            var dataWriter = new ShapefileDataWriter(file);
            dataWriter.Header = ShapefileDataWriter.GetHeader(featureCollection[0] as IFeature, featureCollection.Count);
            dataWriter.Write(featureCollection);

            // Test write with header from a existing shapefile
			file = dir + "testWriteShapefileHeader";
			if (File.Exists(file + ".shp")) File.Delete(file + ".shp");
            if (File.Exists(file + ".shx")) File.Delete(file + ".shx");
            if (File.Exists(file + ".dbf")) File.Delete(file + ".dbf");

            dataWriter = new ShapefileDataWriter(file)
            {
                Header =
                    ShapefileDataWriter.GetHeader(dir + "country.dbf")
            };
            dataWriter.Write(featureCollection);
        }
    }
}
