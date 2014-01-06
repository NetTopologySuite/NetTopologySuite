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
        private IGeometryFactory _factory;

        /// <summary>
        /// <see cref="IGeometry"/> creator.
        /// </summary>        
        public IGeometryFactory Factory
        {
            get
            {
                if (_factory == null)
                    _factory = GeometryServiceProvider.Instance.CreateGeometryFactory();
                return _factory;
            }
            set
            {
                if (value != null)
                    _factory = value;
            }
        }

        /// <summary>
        /// Initialize reader with a standard <c>GeometryFactory</c>.
        /// </summary>
        protected ShapeReader() :
            this(GeometryServiceProvider.Instance.CreateGeometryFactory()) { }

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
            ReadCoordinates(reader, 1, new[] { 0 }, ordinates, buffer);
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
            /*var bbox = */ ReadBoundingBox(reader); // Jump boundingbox

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
            /*var bbox = */ ReadBoundingBox(reader); // jump boundingbox

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
            /*var bbox = */ ReadBoundingBox(reader); // jump boundingbox

            var numPoints = ReadNumPoints(reader);
            var buffer = new CoordinateBuffer(numPoints, ShapeFileConstants.NoDataBorder, true);
            ReadCoordinates(reader, numPoints, new[] { numPoints - 1 }, ordinates, buffer);
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
            for (int i = 0; i < shellRings.Count; i++)
            {
                var shellRing = shellRings[i];
                var numHoles = numHoleRings.Dequeue();
                var holes = holeRings.GetRange(offset, numHoles - offset).ToArray();
                polygons[i] = _factory.CreatePolygon(shellRing, holes);
            }
            return _factory.CreateMultiPolygon(polygons);            
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
                indexParts[i - 1] = reader.ReadInt32() - 1;

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
                /*var zInterval = */ ReadInterval(reader);
                //Set the z-values
                for (var i = 0; i < numPoints; i++)
                    buffer.SetZ(offset + i, reader.ReadDouble());
            }
            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                //Read m-interval
                /*var mInterval = */ ReadInterval(reader);
                //Set the m-values
                for (var i = 0; i < numPoints; i++)
                    buffer.SetZ(offset + i, reader.ReadDouble());
            }
        }
    }
}
