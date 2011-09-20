using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Converts a Shapefile point to a OGIS Polygon.
    /// </summary>
    public class PolygonHandler : ShapeHandler
    {
        //Thanks to Bruno.Labrecque
        private static readonly ProbeLinearRing ProbeLinearRing = new ProbeLinearRing();
        /// <summary>
        /// The ShapeType this handler handles.
        /// </summary>
        public override ShapeGeometryType ShapeType
        {
            get { return ShapeGeometryType.Polygon; }
        }

        /// <summary>
        /// Reads a stream and converts the shapefile record to an equilivent geometry object.
        /// </summary>
        /// <param name="file">The stream to read.</param>
        /// <param name="geometryFactory">The geometry factory to use when making the object.</param>
        /// <returns>The Geometry object that represents the shape file record.</returns>
        public override IGeometry Read(BigEndianBinaryReader file, IGeometryFactory geometryFactory)
        {
            int shapeTypeNum = file.ReadInt32();
            type = (ShapeGeometryType) EnumUtility.Parse(typeof(ShapeGeometryType), shapeTypeNum.ToString());
            if (type == ShapeGeometryType.NullShape)
                return geometryFactory.CreatePolygon(null, null);

            if (!(type == ShapeGeometryType.Polygon  || type == ShapeGeometryType.PolygonM ||
                  type == ShapeGeometryType.PolygonZ || type == ShapeGeometryType.PolygonZM))	
                throw new ShapefileException("Attempting to load a non-polygon as polygon.");

            // Read and for now ignore bounds.
            int bblength = GetBoundingBoxLength();
            bbox = new double[bblength];
            for (; bbindex < 4; bbindex++)
            {
                double d = file.ReadDouble();
                bbox[bbindex] = d;
            }

            int numParts = file.ReadInt32();
            int numPoints = file.ReadInt32();
            int[] partOffsets = new int[numParts];
            for (int i = 0; i < numParts; i++)
                partOffsets[i] = file.ReadInt32();

            var shells = new List<ILinearRing>();
            var holes = new List<ILinearRing>();

            for (int part = 0; part < numParts; part++)
            {
                int start = partOffsets[part];
                int finish;
                if (part == numParts - 1)
                    finish = numPoints;
                else finish = partOffsets[part + 1];
                int length = finish - start;
                CoordinateList points = new CoordinateList();
                points.Capacity = length;
                for (int i = 0; i < length; i++)
                {
                    Coordinate external = new Coordinate(file.ReadDouble(), file.ReadDouble() );					
                    geometryFactory.PrecisionModel.MakePrecise( external);
                    Coordinate internalCoord = external;

                    // Thanks to Abhay Menon!
                    if (!Double.IsNaN(internalCoord.Y) && !Double.IsNaN(internalCoord.X))
                        points.Add(internalCoord, false);
                }

                if (points.Count > 2) // Thanks to Abhay Menon!
                {
                    if (points[0].Distance(points[points.Count - 1]) > .00001)
                        points.Add(new Coordinate(points[0]));
                    else if (points[0].Distance(points[points.Count - 1]) > 0.0)
                        points[points.Count - 1].CoordinateValue = points[0];

                    ILinearRing ring = geometryFactory.CreateLinearRing(points.ToArray());

                    // If shape have only a part, jump orientation check and add to shells
                    if (numParts == 1)
                        shells.Add(ring);
                    else
                    {
                        // Orientation check
                        if (CGAlgorithms.IsCCW(points.ToArray()))
                             holes.Add(ring);
                        else shells.Add(ring);
                    }                    
                }
            }

            // Now we have a list of all shells and all holes
            var holesForShells = new List<List<ILinearRing>>(shells.Count);
            for (int i = 0; i < shells.Count; i++)
                holesForShells.Add(new List<ILinearRing>());

            //Thanks to Bruno.Labrecque
            //Sort shells by area, rings should only be added to the smallest shell, that contains the ring
            shells.Sort(ProbeLinearRing);

            // Find holes
            for (int i = 0; i < holes.Count; i++)
            {
                ILinearRing testRing = holes[i];
                IEnvelope testEnv = testRing.EnvelopeInternal;
                Coordinate testPt = testRing.GetCoordinateN(0);
                ILinearRing tryRing;
                for (int j = 0; j < shells.Count; j++)
                {
                    tryRing = shells[j];
                    IEnvelope tryEnv = tryRing.EnvelopeInternal;
                    bool isContained = false;
                    CoordinateList coordList = new CoordinateList(tryRing.Coordinates);
                    if (tryEnv.Contains(testEnv) && 
                       (CGAlgorithms.IsPointInRing(testPt, coordList.ToArray()) || (PointInList(testPt, coordList)))) 				
                        isContained = true;

                    // Check if this new containing ring is smaller than the current minimum ring
                    if (isContained)
                    {
                        // Suggested by Brian Macomber and added 3/28/2006:
                        // holes were being found but never added to the holesForShells array
                        // so when converted to geometry by the factory, the inner rings were never created.
                        var holesForThisShell = holesForShells[j];
                        holesForThisShell.Add(testRing);
                        
                        //Suggested by Bruno.Labrecque
                        //A LinearRing should only be added to one outer shell
                        break;
                    }
                }
            }

            IPolygon[] polygons = new IPolygon[shells.Count];
            for (int i = 0; i < shells.Count; i++)
                polygons[i] = (geometryFactory.CreatePolygon(shells[i], 
                    holesForShells[i].ToArray()));

            if (polygons.Length == 1)
                 geom = polygons[0];
            else geom = geometryFactory.CreateMultiPolygon(polygons);
            GrabZMValues(file);
            return geom;
        }

        /// <summary>
        /// Writes a Geometry to the given binary wirter.
        /// </summary>
        /// <param name="geometry">The geometry to write.</param>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public override void Write(IGeometry geometry, BinaryWriter file, IGeometryFactory geometryFactory)
        {
            // This check seems to be not useful and slow the operations...
            // if (!geometry.IsValid)    
            // Trace.WriteLine("Invalid polygon being written.");

            IGeometryCollection multi;
            if (geometry is IGeometryCollection)
                multi = (IGeometryCollection) geometry;
            else 
            {
                GeometryFactory gf = new GeometryFactory(geometry.PrecisionModel);				
                multi = gf.CreateMultiPolygon(new[] { (IPolygon) geometry, } );
            }

            file.Write(int.Parse(EnumUtility.Format(typeof(ShapeGeometryType), ShapeType, "d")));

            IEnvelope box = multi.EnvelopeInternal;
            IEnvelope bounds = GetEnvelopeExternal(geometryFactory.PrecisionModel,  box);
            file.Write(bounds.MinX);
            file.Write(bounds.MinY);
            file.Write(bounds.MaxX);
            file.Write(bounds.MaxY);
        
            int numParts = GetNumParts(multi);
            int numPoints = multi.NumPoints;
            file.Write(numParts);
            file.Write(numPoints);
        			
            // write the offsets to the points
            int offset = 0;
            for (int part = 0; part < multi.NumGeometries; part++)
            {
                // offset to the shell points
                IPolygon polygon = (IPolygon) multi.Geometries[part];
                file.Write(offset);
                offset = offset + polygon.ExteriorRing.NumPoints;

                // offstes to the holes
                foreach (ILinearRing ring in polygon.InteriorRings)
                {
                    file.Write(offset);
                    offset = offset + ring.NumPoints;
                }	
            }

            // write the points 
            for (int part = 0; part < multi.NumGeometries; part++)
            {
                IPolygon poly = (IPolygon) multi.Geometries[part];
                var points = poly.ExteriorRing.Coordinates;
                WriteCoords(points, file, geometryFactory);
                foreach(ILinearRing ring in poly.InteriorRings)
                {
                    var points2 = ring.Coordinates;					
                    WriteCoords(points2, file, geometryFactory);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="file"></param>
        /// <param name="geometryFactory"></param>
        private static void WriteCoords(IEnumerable<Coordinate> points, BinaryWriter file, IGeometryFactory geometryFactory)
        {
            Coordinate external;
            foreach (Coordinate point in points)
            {
                // external = geometryFactory.PrecisionModel.ToExternal(point);
                external = point;
                file.Write(external.X);
                file.Write(external.Y);
            }
        }

        /// <summary>
        /// Gets the length of the shapefile record using the geometry passed in.
        /// </summary>
        /// <param name="geometry">The geometry to get the length for.</param>
        /// <returns>The length in bytes this geometry is going to use when written out as a shapefile record.</returns>
        public override int GetLength(IGeometry geometry)
        {
            int numParts = GetNumParts(geometry);
            return (22 + (2 * numParts) + geometry.NumPoints * 8); // 22 => shapetype(2) + bbox(4*4) + numparts(2) + numpoints(2)
        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private int GetNumParts(IGeometry geometry)
        {
            int numParts = 0;
            if (geometry is IMultiPolygon)
            {
                IMultiPolygon mpoly = geometry as IMultiPolygon;
                foreach (IPolygon poly in mpoly.Geometries)
                    numParts = numParts + poly.InteriorRings.Length + 1;
            }
            else if (geometry is IPolygon)
                numParts = ((IPolygon) geometry).InteriorRings.Length + 1;
            else throw new InvalidOperationException("Should not get here.");
            return numParts;
        }

        /// <summary>
        /// Test if a point is in a list of coordinates.
        /// </summary>
        /// <param name="testPoint">TestPoint the point to test for.</param>
        /// <param name="pointList">PointList the list of points to look through.</param>
        /// <returns>true if testPoint is a point in the pointList list.</returns>
        private bool PointInList(Coordinate testPoint, CoordinateList pointList) 
        {
            foreach(Coordinate p in pointList)
                if (p.Equals2D(testPoint))
                    return true;
            return false;
        }
    }
}