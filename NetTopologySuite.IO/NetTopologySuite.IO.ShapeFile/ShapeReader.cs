using System.Collections.Generic;
using System.IO;
using GeoAPI;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Contains methods for reading a single <c>Geometry</c> in binary ESRI shapefile format.
    /// </summary>
    public abstract class ShapeReader
    {
        /// <summary>
        /// Geometry creator.
        /// </summary>
        private IGeometryFactory _factory;

        /// <summary>
        /// 
        /// </summary>
        public IGeometryFactory Factory
        {
            get { return _factory ?? (_factory = GeometryServiceProvider.Instance.CreateGeometryFactory()); }
            set
            {
                if (value != null)
                    _factory = value;
            }
        }

        /// <summary>
        /// Initialize reader with a standard <c>GeometryFactory</c>.
        /// </summary>
        protected ShapeReader() 
            : this(GeometryServiceProvider.Instance.CreateGeometryFactory()) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
        protected ShapeReader(IGeometryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Function to read a <see cref="IPoint"/> from a ShapeFile stream using the specified <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader to use</param>
        /// <param name="ordinates">The ordinates to read</param>
        /// <returns>The read point geometry</returns>
        protected IGeometry ReadPoint(BinaryReader reader, Ordinates ordinates)
        {
            var buffer = new CoordinateBuffer(1, ShapeFileConstants.NoDataBorder, true);
            ReadCoordinates(reader, 1, new[] {0}, ordinates, buffer);
            IGeometry point = _factory.CreatePoint(buffer.ToSequence());
            return point;
        }

        /// <summary>
        /// Function to read a <see cref="ILineString"/> or <see cref="IMultiLineString"/> from a ShapeFile stream using the specified <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader to use</param>
        /// <param name="ordinates">The ordinates to read</param>
        /// <returns>The read lineal geometry</returns>
        protected IGeometry ReadLineString(BinaryReader reader, Ordinates ordinates)
        {
            /*var bbox = */ReadBoundingBox(reader);

            var numParts = ReadNumParts(reader);
            var numPoints = ReadNumPoints(reader);
            var indexParts = ReadIndexParts(reader, numParts, numPoints);

            var buffer = new CoordinateBuffer(numPoints, ShapeFileConstants.NoDataBorder, true);
            ReadCoordinates(reader, numPoints, indexParts, ordinates, buffer);

            if (numParts == 1)
                 return _factory.CreateLineString(buffer.ToSequence());
            return CreateMultiLineString(buffer.ToSequences());
        }

        /// <summary>
        /// Function to read a either a <see cref="IPolygon"/> or an <see cref="IMultiPolygon"/> from a ShapeFile stream using the specified <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader to use</param>
        /// <param name="ordinates">The ordinates to read</param>
        /// <returns>The read polygonal geometry</returns>
        protected IGeometry ReadPolygon(BinaryReader reader, Ordinates ordinates)
        {
            /*var bbox = */ReadBoundingBox(reader);  // Jump boundingbox

            var numParts = ReadNumParts(reader);
            var numPoints = ReadNumPoints(reader);
            var indexParts = ReadIndexParts(reader, numParts, numPoints);

            var buffer = new CoordinateBuffer(numPoints, ShapeFileConstants.NoDataBorder, true);
            ReadCoordinates(reader, numPoints, indexParts, ordinates, buffer);

            return numParts == 1 
                ? _factory.CreatePolygon(_factory.CreateLinearRing(buffer.ToSequence()), null) 
                : CreateSingleOrMultiPolygon(buffer);
        }

        /// <summary>
        /// Function to read a <see cref="IMultiPoint"/> from a ShapeFile stream using the specified <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader to use</param>
        /// <param name="ordinates">The ordinates to read</param>
        /// <returns>The read polygonal geometry</returns>
        public IGeometry ReadMultiPoint(BinaryReader reader, Ordinates ordinates)
        {
            /*var bbox = */ReadBoundingBox(reader);  // Jump boundingbox

            var numPoints = ReadNumPoints(reader);
            var buffer = new CoordinateBuffer(numPoints, ShapeFileConstants.NoDataBorder, true);
            ReadCoordinates(reader, numPoints, new [] {numPoints-1}, ordinates, buffer);
            return _factory.CreateMultiPoint(buffer.ToSequence());
        }        

        /// <summary>
        /// Creates a MultiLineString.
        /// </summary>
        /// <returns></returns>
        private IGeometry CreateMultiLineString(ICoordinateSequence[] sequences)
        {
            var ls = new ILineString[sequences.Length];
            for (var i = 0; i < sequences.Length; i++)
                ls[i] = _factory.CreateLineString(sequences[i]);
            return _factory.CreateMultiLineString(ls);
        }

        /*
        /// <summary>
        /// Creates a MultiLineString.
        /// </summary>
        /// <param name="numPoints"></param>
        /// <param name="indexParts"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        public IGeometry CreateMultiLineString(int numPoints, int[] indexParts, Coordinate[] coords)
        {
            // Support vars
            ILineString[] strings = new ILineString[indexParts.Length];
            Coordinate[] destCoords = null;
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
                    destCoords = new Coordinate[length];
                    Array.Copy(coords, indexParts[partIndex - 1], destCoords, 0, length);
                    partIndex++;
                    strings[index++] = Factory.CreateLineString(destCoords);
                }
            }

            // Create last part
            int lastIndex = indexParts.Length - 1;
            length = numPoints - indexParts[lastIndex];
            destCoords = new Coordinate[length];
            Array.Copy(coords, indexParts[lastIndex], destCoords, 0, length);
            strings[index] = Factory.CreateLineString(destCoords);

            // Create geometryString
            return Factory.CreateMultiLineString(strings);
        }
        */
        
        /// <summary>
        /// Creates a single Polygon with holes.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private IGeometry CreateSingleOrMultiPolygon(CoordinateBuffer buffer)
        {
            // Support vars
            var cses = buffer.ToSequences();
            var shellRings = new List<ILinearRing>();
            var holeRings = new List<ILinearRing>();
            var numHoleRings = new Queue<int>();

            //Sort for shells and holes
            foreach (var cs in cses)
            {
                var ring = _factory.CreateLinearRing(cs);
                if (!ring.IsCCW)
                {
                    shellRings.Add(ring);
                    numHoleRings.Enqueue(holeRings.Count);
                }
                else
                    holeRings.Add(ring);
            }
            numHoleRings.Enqueue(holeRings.Count);

            if (shellRings.Count == 1)
                return _factory.CreatePolygon(shellRings[0], holeRings.ToArray());

            var polygons = new IPolygon[shellRings.Count];
            var offset = numHoleRings.Dequeue();
            var i = 0;
            foreach (var shellRing in shellRings)
            {
                var numRings = numHoleRings.Dequeue();
                var holes = holeRings.GetRange(offset, numRings - offset).ToArray();
                polygons[i] = _factory.CreatePolygon(shellRing, holes);
                
            }
            return _factory.CreateMultiPolygon(polygons);

            /*

            int i = 0;
            int index = 0;
            int shellLength = 0;
            Coordinate[] shellCoords = null;
            ILinearRing[] shells = new ILinearRing[indexParts.Length];
            ArrayList polygonIndex = new ArrayList();

            // Reading shells
            for (i = 0; i < indexParts.Length - 1; i++)
            {
                // Init vars
                shellLength = indexParts[i + 1] - indexParts[i];
                shellCoords = new Coordinate[shellLength];
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
            shellCoords = new Coordinate[shellLength];
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
             */
        }

        /// <summary>
        /// Creates a single Polygon without holes.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public IPolygon CreateSimpleSinglePolygon(Coordinate[] coords)
        {
            return Factory.CreatePolygon(Factory.CreateLinearRing(coords), null);
        }

        /// <summary>
        /// Read the x-y Envelope
        /// </summary>
        /// <param name="reader">The reader to use</param>
        protected static Envelope ReadBoundingBox(BinaryReader reader)
        {
            return new Envelope(
                new Coordinate(reader.ReadDouble(), reader.ReadDouble()),
                new Coordinate(reader.ReadDouble(), reader.ReadDouble()));
        }

        /// <summary>
        /// Read the ordinate range Envelope
        /// </summary>
        /// <param name="reader">The reader to use</param>
        protected static Interval ReadInterval(BinaryReader reader)
        {
            return Interval.Create(reader.ReadDouble(), reader.ReadDouble());
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
        protected int ReadNumPoints(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        /// <summary>
        /// Read the index parts of the shape header
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="numParts">The number of parts</param>
        /// <param name="numPoints">The total number of points</param>
        /// <returns>An array of integer values</returns>
        protected int[] ReadIndexParts(BinaryReader reader, int numParts, int numPoints)
        {
            var indexParts = new int[numParts];
            //The first one is 0, we already know that
            reader.ReadInt32();
            for (var i = 1; i < numParts; i++)
                indexParts[i-1] = reader.ReadInt32() - 1;
            
            //The last one is numPoints
            indexParts[numParts - 1] = numPoints - 1;
            
            return indexParts;
        }

        /// <summary>
        /// Method to read the coordinates block
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="numPoints">The total number of points to read</param>
        /// <param name="markers">The markers</param>
        /// <param name="ordinates">The ordinates to read</param>
        /// <param name="buffer">The buffer to add the coordinates to.</param>
        private static void ReadCoordinates(BinaryReader reader, int numPoints, int[] markers, Ordinates ordinates, CoordinateBuffer buffer)
        {
            var offset = buffer.Count;
            var j = 0;

            // Add x- and y-ordinates
            for (var i = 0; i < numPoints; i++)
            {
                //Read x- and y- ordinates
                buffer.AddCoordinate(reader.ReadDouble(), reader.ReadDouble());
                
                //Check if we have reached a marker
                if (i != markers[j]) continue;
                
                //Add a marker
                buffer.AddMarker();
                j++;
            }
            
            // are there any z-ordinates
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
            {
                //Read zInterval
                /*var zInterval = */ReadInterval(reader);
                //Set the z-values
                for (var i = 0; i < numPoints; i++)
                    buffer.SetZ(offset + i, reader.ReadDouble());
            }
            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                //Read m-interval
                /*var mInterval = */ReadInterval(reader);
                //Set the m-values
                for (var i = 0; i < numPoints; i++)
                    buffer.SetZ(offset + i, reader.ReadDouble());
            }
        }
    }
}
