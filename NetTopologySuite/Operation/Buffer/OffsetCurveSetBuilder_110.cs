using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /**
     * 
     * 
     *
     * @version 1.7
     */
    ///<summary>
    /// Creates all the raw offset curves for a buffer of a <see cref="IGeometry{TCoordinate}"/>.
    /// Raw curves need to be noded together and polygonized to form the final buffer area.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class OffsetCurveSetBuilder_110<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
        private IGeometry<TCoordinate> _inputGeom;
        private Double _distance;
        private OffsetCurveBuilder_110<TCoordinate> _curveBuilder;

        private List<NodedSegmentString<TCoordinate>> _curveList = new List<NodedSegmentString<TCoordinate>>();

        ///<summary>
        /// Creates an instance of this class
        ///</summary>
        ///<param name="inputGeom"></param>
        ///<param name="distance"></param>
        ///<param name="curveBuilder"></param>
        public OffsetCurveSetBuilder_110(
            IGeometry<TCoordinate> inputGeom,
                Double distance,
                OffsetCurveBuilder_110<TCoordinate> curveBuilder)
        {
            _inputGeom = inputGeom;
            _distance = distance;
            _curveBuilder = curveBuilder;
        }

        /**
         * Computes the set of raw offset curves for the buffer.
         * Each offset curve has an attached {@link Label} indicating
         * its left and right location.
         *
         * @return a Collection of SegmentStrings representing the raw buffer curves
         */
        public IEnumerable<NodedSegmentString<TCoordinate>> GetCurves()
        {
            Add(_inputGeom);
            return _curveList;
        }

        private void AddCurves(IEnumerable<ICoordinateSequence<TCoordinate>> lineList, Locations leftLoc,
                           Locations rightLoc)
        {
            foreach (ICoordinateSequence<TCoordinate> line in lineList)
            {
                AddCurve(line, leftLoc, rightLoc);
            }
        }

        /// <summary>
        /// Creates a <see cref="NodedSegmentString{TCoordinate}"/> for a coordinate list
        /// which is a raw offset curve, and adds it to the list of buffer curves.
        /// </summary>
        /// <remarks>
        /// The SegmentString is tagged with a Label giving the topology of the curve.
        /// The curve may be oriented in either direction.
        /// If the curve is oriented CW, the locations will be:
        /// Left: Locations.Exterior.
        /// Right: Locations.Interior.
        /// </remarks>
        private void AddCurve(ICoordinateSequence<TCoordinate> coord, Locations leftLoc, Locations rightLoc)
        {
            // don't add null curves!
            if (!Slice.CountGreaterThan(coord, 2))
            {
                return;
            }

            // add the edge for a coordinate list which is a raw offset curve
            TopologyLocation location = new TopologyLocation(Locations.Boundary, leftLoc, rightLoc);
            Label label = new Label(location, TopologyLocation.None);
            NodedSegmentString<TCoordinate> e = new NodedSegmentString<TCoordinate>(coord, label);
            _curveList.Add(e);
        }


        private void Add(IGeometry<TCoordinate> g)
        {
            if (g.IsEmpty)
                return;

            if (g is IPolygon<TCoordinate>)
            {
                AddPolygon(g as IPolygon<TCoordinate>);
            }
            // LineString also handles LinearRings
            else if (g is ILineString<TCoordinate>)
            {
                AddLineString(g as ILineString<TCoordinate>);
            }
            else if (g is IPoint<TCoordinate>)
            {
                AddPoint(g as IPoint<TCoordinate>);
            }
            else if (g is IMultiPoint<TCoordinate>)
            {
                AddCollection(g as IMultiPoint<TCoordinate>);
            }
            else if (g is IMultiLineString<TCoordinate>)
            {
                AddCollection(g as IMultiLineString<TCoordinate>);
            }
            else if (g is IMultiPolygon<TCoordinate>)
            {
                AddCollection(g as IMultiPolygon<TCoordinate>);
            }
            else if (g is IGeometryCollection<TCoordinate>)
            {
                AddCollection(g as IGeometryCollection<TCoordinate>);
            }
            else
            {
                throw new NotSupportedException(g.GetType().FullName);
            }
        }

        private void AddCollection(IGeometryCollection<TCoordinate> gc)
        {
            foreach (IGeometry<TCoordinate> geometry in gc)
            {
                Add(geometry);
            }
        }
        /// <summary>
        /// Add a Point to the graph.
        /// </summary>
        private void AddPoint(IPoint<TCoordinate> p)
        {
            if ( _distance <= 0d )
                return;

            IEnumerable<TCoordinate> coord = p.Coordinates;
            IEnumerable<ICoordinateSequence<TCoordinate>> lineList = _curveBuilder.GetLineCurve(coord, _distance);
            AddCurves(lineList, Locations.Exterior, Locations.Interior);
        }

        private void AddLineString(ILineString<TCoordinate> line)
        {
            if (_distance <= 0.0)
                return;

            IEnumerable<TCoordinate> coord = line.Coordinates.WithoutRepeatedPoints();
            IEnumerable<ICoordinateSequence<TCoordinate>> lineList = _curveBuilder.GetLineCurve(coord, _distance);
            AddCurves(lineList, Locations.Exterior, Locations.Interior);
        }

        private void AddPolygon(IPolygon<TCoordinate> p)
        {
            Double offsetDistance = _distance;
            Positions offsetSide = Positions.Left;
            if (_distance < 0.0)
            {
                offsetDistance = -_distance;
                offsetSide = Positions.Right;
            }

            ILinearRing<TCoordinate> shell = p.ExteriorRing as ILinearRing<TCoordinate>;
            Debug.Assert(shell != null);
            IEnumerable<TCoordinate> shellCoord = shell.Coordinates.WithoutRepeatedPoints();

            // optimization - don't bother computing buffer
            // if the polygon would be completely eroded
            if (_distance < 0.0 && IsErodedCompletely(shellCoord, _distance))
            {
                return;
            }

            AddPolygonRing(shellCoord, offsetDistance, offsetSide,
                           Locations.Exterior, Locations.Interior);

            foreach (ILinearRing<TCoordinate> hole in p.InteriorRings)
            {
                IEnumerable<TCoordinate> holeCoord = hole.Coordinates.WithoutRepeatedPoints();

                // optimization - don't bother computing buffer for this hole
                // if the hole would be completely covered
                if (_distance > 0.0 && IsErodedCompletely(holeCoord, -_distance))
                {
                    continue;
                }

                // Holes are topologically labeled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CCW)
                AddPolygonRing(holeCoord, offsetDistance, Position.Opposite(offsetSide),
                               Locations.Interior, Locations.Exterior);
            }
        }
        /// <summary>
        /// Add an offset curve for a ring.
        /// The side and left and right topological location arguments
        /// assume that the ring is oriented CW.
        /// If the ring is in the opposite orientation,
        /// the left and right locations must be interchanged and the side flipped.
        /// </summary>
        /// <param name="coord">The coordinates of the ring (must not contain repeated points).</param>
        /// <param name="offsetDistance">The distance at which to create the buffer.</param>
        /// <param name="side">The side of the ring on which to construct the buffer line.</param>
        /// <param name="cwLeftLoc">The location on the L side of the ring (if it is CW).</param>
        /// <param name="cwRightLoc">The location on the R side of the ring (if it is CW).</param>
        private void AddPolygonRing(IEnumerable<TCoordinate> coord, Double offsetDistance,
                                    Positions side, Locations cwLeftLoc, Locations cwRightLoc)
        {
            Locations leftLoc = cwLeftLoc;
            Locations rightLoc = cwRightLoc;

            if (CGAlgorithms<TCoordinate>.IsCCW(coord))
            {
                leftLoc = cwRightLoc;
                rightLoc = cwLeftLoc;
                side = Position.Opposite(side);
            }

            IEnumerable<ICoordinateSequence<TCoordinate>> lineList
                = _curveBuilder.GetRingCurve(coord, side, offsetDistance);

            AddCurves(lineList, leftLoc, rightLoc);
        }

        /// <summary>
        /// The ringCoord is assumed to contain no repeated points.
        /// It may be degenerate (i.e. contain only 1, 2, or 3 points).
        /// In this case it has no area, and hence has a minimum diameter of 0.
        /// </summary>
        private Boolean IsErodedCompletely(IEnumerable<TCoordinate> ringCoord, Double bufferDistance)
        {
            Double minDiam;

            Int32 count = Slice.GetLength(ringCoord);

            // degenerate ring has no area
            if (count < 3)
            {
                return bufferDistance < 0;
            }

            // important test to eliminate inverted triangle bug
            // also optimizes erosion test for triangles
            if (count == 4)
            {
                ICoordinateFactory<TCoordinate> coordinateFactory
                    = _inputGeom.Factory.CoordinateFactory;
                return isTriangleErodedCompletely(coordinateFactory,
                                                  ringCoord,
                                                  bufferDistance);
            }

            /*
             * The following is a heuristic test to determine whether an
             * inside buffer will be eroded completely.
             * It is based on the fact that the minimum diameter of the ring pointset
             * provides an upper bound on the buffer distance which would erode the
             * ring.
             * If the buffer distance is less than the minimum diameter, the ring
             * may still be eroded, but this will be determined by
             * a full topological computation.
             *
             */
            ILinearRing<TCoordinate> ring = _inputGeom.Factory.CreateLinearRing(ringCoord);
            MinimumDiameter<TCoordinate> md = new MinimumDiameter<TCoordinate>(ring);
            minDiam = md.Length;
            return minDiam < 2 * Math.Abs(bufferDistance);
        }

        /// <summary>
        /// Tests whether a triangular ring would be eroded completely by the given
        /// buffer distance.
        /// This is a precise test.  It uses the fact that the inner buffer of a
        /// triangle converges on the inCentre of the triangle (the point
        /// equidistant from all sides).  If the buffer distance is greater than the
        /// distance of the inCentre from a side, the triangle will be eroded completely.
        /// This test is important, since it removes a problematic case where
        /// the buffer distance is slightly larger than the inCentre distance.
        /// In this case the triangle buffer curve "inverts" with incorrect topology,
        /// producing an incorrect hole in the buffer.       
        /// </summary>
        private static Boolean isTriangleErodedCompletely(ICoordinateFactory<TCoordinate> coordinateFactory,
                                                          IEnumerable<TCoordinate> triangleCoord,
                                                          Double bufferDistance)
        {
            Triple<TCoordinate> points = Slice.GetTriple(triangleCoord).Value;
            Triangle<TCoordinate> tri = new Triangle<TCoordinate>(coordinateFactory,
                                                                  points.First,
                                                                  points.Second,
                                                                  points.Third);
            TCoordinate inCenter = tri.InCenter;
            Double distToCentre = CGAlgorithms<TCoordinate>.DistancePointLine(inCenter, tri.P0, tri.P1);
            return distToCentre < Math.Abs(bufferDistance);
        }
    }
}
