using System;
using System.Collections;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// Contains methods for reading a single <c>Geometry</c> in binary ESRI shapefile format.
    /// </summary>
    public class ShapeReader
    {
        /// <summary>
        /// Geometry creator.
        /// </summary>
        private GeometryFactory factory = null;

        /// <summary>
        /// 
        /// </summary>
        public GeometryFactory Factory
        {
            get { return factory; }            
        }

        /// <summary>
        /// Initialize reader with a standard <c>GeometryFactory</c>.
        /// </summary>
        public ShapeReader() : this(new GeometryFactory()) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
        public ShapeReader(GeometryFactory factory)
        {
            this.factory = factory;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IGeometry ReadPoint(BinaryReader reader)
        {
            ICoordinate coordinate = ReadCoordinate(reader);
            IGeometry point = CreatePoint(coordinate);
            return point;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IGeometry ReadLineString(BinaryReader reader)
        {
            ReadBoundingBox(reader);  // Jump boundingbox

            int numParts = ReadNumParts(reader);
            int numPoints = ReadNumPoints(reader);

            int[] indexParts = ReadIndexParts(reader, numParts);

            ICoordinate[] coords = ReadCoordinates(reader, numPoints);

            if (numParts == 1)
                 return CreateLineString(coords);
            else return CreateMultiLineString(numPoints, indexParts, coords);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IGeometry ReadPolygon(BinaryReader reader)
        {
            ReadBoundingBox(reader);  // Jump boundingbox

            int numParts = ReadNumParts(reader);
            int numPoints = ReadNumPoints(reader);

            int[] indexParts = ReadIndexParts(reader, numParts);

            ICoordinate[] coords = ReadCoordinates(reader, numPoints);

            if (numParts == 1)
                 return CreateSimpleSinglePolygon(coords);
            else return CreateSingleOrMultiPolygon(numPoints, indexParts, coords);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IGeometry ReadMultiPoint(BinaryReader reader)
        {
            ReadBoundingBox(reader);  // Jump boundingbox

            int numPoints = ReadNumPoints(reader);
            ICoordinate[] coords = new ICoordinate[numPoints];
            for (int i = 0; i < numPoints; i++)
                coords[i] = ReadCoordinate(reader);
            return CreateMultiPoint(coords);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public IPoint CreatePoint(ICoordinate coordinate)
        {
            return Factory.CreatePoint(coordinate);
        }

        /// <summary>
        /// Creates a single LineString.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public ILineString CreateLineString(ICoordinate[] coords)
        {
            return Factory.CreateLineString(coords);
        }

        /// <summary>
        /// Creates a MultiLineString.
        /// </summary>
        /// <param name="numPoints"></param>
        /// <param name="indexParts"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        public IGeometry CreateMultiLineString(int numPoints, int[] indexParts, ICoordinate[] coords)
        {
            // Support vars
            ILineString[] strings = new ILineString[indexParts.Length];
            ICoordinate[] destCoords = null;
            int index = 0;
            int length = 0;
            int partIndex = 1;

            // Create parts
            for (int i = 0; i < coords.Length - 1; i++)
            {
                if (partIndex == indexParts.Length)
                    break;  // Exit and add manually last part
                if (i == indexParts[partIndex])
                {
                    length = indexParts[partIndex] - indexParts[partIndex - 1];
                    destCoords = new ICoordinate[length];
                    Array.Copy(coords, indexParts[partIndex - 1], destCoords, 0, length);
                    partIndex++;
                    strings[index++] = Factory.CreateLineString(destCoords);
                }
            }

            // Create last part
            int lastIndex = indexParts.Length - 1;
            length = numPoints - indexParts[lastIndex];
            destCoords = new ICoordinate[length];
            Array.Copy(coords, indexParts[lastIndex], destCoords, 0, length);
            strings[index] = Factory.CreateLineString(destCoords);

            // Create geometryString
            return Factory.CreateMultiLineString(strings);
        }

        /// <summary>
        /// Creates a single Polygon with holes.
        /// </summary>
        /// <param name="numPoints"></param>
        /// <param name="indexParts"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        public IGeometry CreateSingleOrMultiPolygon(int numPoints, int[] indexParts, ICoordinate[] coords)
        {
            // Support vars
            int i = 0;
            int index = 0;
            int shellLength = 0;
            ICoordinate[] shellCoords = null;
            ILinearRing[] shells = new ILinearRing[indexParts.Length];
            ArrayList polygonIndex = new ArrayList();

            // Reading shells
            for (i = 0; i < indexParts.Length - 1; i++)
            {
                // Init vars
                shellLength = indexParts[i + 1] - indexParts[i];
                shellCoords = new ICoordinate[shellLength];
                Array.Copy(coords, indexParts[i], shellCoords, 0, shellLength);

                // Verify polygon area
                if (!CGAlgorithms.IsCCW(shellCoords))
                    polygonIndex.Add(i);

                // Adding shell to array
                shells[index++] = Factory.CreateLinearRing(shellCoords);
            }

            // Adding last shell            
            int lastIndex = indexParts.Length - 1;
            shellLength = numPoints - indexParts[lastIndex];
            shellCoords = new ICoordinate[shellLength];
            Array.Copy(coords, indexParts[lastIndex], shellCoords, 0, shellLength);
            if (!CGAlgorithms.IsCCW(shellCoords))
                polygonIndex.Add(lastIndex);
            shells[index] = Factory.CreateLinearRing(shellCoords);
            // Create geometryString
            if (polygonIndex.Count == 1)
            {
                // Single Polygon building
                ILinearRing shell = shells[(int) polygonIndex[0]];
                ILinearRing[] holes = new ILinearRing[shells.Length - 1];
                Array.Copy(shells, 1, holes, 0, shells.Length - 1);

                // Create Polygon point
                return Factory.CreatePolygon(shell, holes);
            }
            else
            {
                // MultiPolygon building:   At this time i have all Linear Rings (shells and holes) undifferenceds into shells[] array,
                //                          and in polygonIndex ArrayList i have all index for all shells (not holes!).                

                // Support vars
                index = 0;
                int start = 0;
                int end = 0;
                int length = 0;
                ILinearRing shell = null;    // Contains Polygon Shell
                ILinearRing[] holes = null;  // Contains Polygon Holes
                IPolygon[] polygons = new IPolygon[polygonIndex.Count];   // Array containing all Polygons                                              

                // Building procedure
                for (i = 0; i < polygonIndex.Count - 1; i++)
                {
                    start = (int )polygonIndex[i];                       // First element of polygon (Shell)
                    end = ((int )polygonIndex[i + 1] - 1);               // Index of last Hole                                            
                    length = end - start;                                // Holes
                    shell = shells[start];                               // Shell
                    holes = new ILinearRing[length];                     // Holes
                    Array.Copy(shells, start + 1, holes, 0, length);     // (start + 1) because i jump the Shell and keep only the Holes!
                    polygons[index++] = Factory.CreatePolygon(shell, holes);
                }

                // Add manually last polygon
                start = (int) polygonIndex[polygonIndex.Count - 1];      // First element of polygon (Shell)
                end = shells.Length - 1;                                 // Index of last Hole                                            
                length = end - start;                                    // Holes
                shell = shells[start];                                   // Shell
                holes = new ILinearRing[length];                         // Holes
                Array.Copy(shells, start + 1, holes, 0, length);         // (start + 1) because i jump the Shell and keep only the Holes!                
                polygons[index++] = Factory.CreatePolygon(shell, holes);

                // Create MultiPolygon point
                return Factory.CreateMultiPolygon(polygons);
            }
        }

        /// <summary>
        /// Creates a single Polygon without holes.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public IPolygon CreateSimpleSinglePolygon(ICoordinate[] coords)
        {
            return Factory.CreatePolygon(Factory.CreateLinearRing(coords), null);
        }

        /// <summary>
        /// Creates a MultiPoint.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public IMultiPoint CreateMultiPoint(ICoordinate[] coords)
        {
            return Factory.CreateMultiPoint(coords);
        }        

        /// <summary>
        /// Jump values for VeDEx BoundingBox
        /// </summary>
        /// <param name="reader"></param>
        public void ReadBoundingBox(BinaryReader reader)
        {
            for (int i = 0; i < 4; i++)
                reader.ReadDouble();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public int ReadNumParts(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public int ReadNumPoints(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="numParts"></param>
        /// <returns></returns>
        public int[] ReadIndexParts(BinaryReader reader, int numParts)
        {
            int[] indexParts = new int[numParts];
            for (int i = 0; i < numParts; i++)
                indexParts[i] = reader.ReadInt32();
            return indexParts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="numPoints"></param>
        /// <returns></returns>
        public ICoordinate[] ReadCoordinates(BinaryReader reader, int numPoints)
        {
            ICoordinate[] coords = new ICoordinate[numPoints];
            for (int i = 0; i < numPoints; i++)
                coords[i] = ReadCoordinate(reader);
            return coords;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public ICoordinate ReadCoordinate(BinaryReader reader)
        {
            return new Coordinate(reader.ReadDouble(), reader.ReadDouble());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IGeometryCollection CreateGeometryCollection(IList list)
        {            
            IGeometry[] geometries = (IGeometry[]) (new ArrayList(list).ToArray(typeof(IGeometry)));
            return Factory.CreateGeometryCollection(geometries);
        }
    }
}
