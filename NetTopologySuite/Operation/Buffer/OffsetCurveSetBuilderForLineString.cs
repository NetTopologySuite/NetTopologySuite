using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Creates all the raw offset curves for a buffer of a <see cref="Geometry{TCoordinate}"/>.
    /// Raw curves need to be noded together and polygonized to form the final buffer area.
    /// </summary>
    public class OffsetCurveSetBuilderForLineString<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _inputGeometry;
        private readonly Double _distanceForth, _distanceBack;
        private readonly OffsetCurveBuilderForLineString<TCoordinate> _curveBuilder;

        private readonly List<NodedSegmentString<TCoordinate>> _curveList
            = new List<NodedSegmentString<TCoordinate>>();

        public OffsetCurveSetBuilderForLineString(IGeometry<TCoordinate> inputGeom, Double distanceForth, Double distanceBack, OffsetCurveBuilderForLineString<TCoordinate> curveBuilder)
        {
            _inputGeometry = inputGeom;
            _distanceForth = distanceForth;
            _distanceBack = distanceBack;
            _curveBuilder = curveBuilder;
        }

        /// <summary>
        /// Computes the set of raw offset curves for the buffer.
        /// Each offset curve has an attached {Label} indicating
        /// its left and right location.
        /// </summary>
        /// <returns>
        /// A set of <see cref="NodedSegmentString{TCoordinate}"/>s 
        /// representing the raw buffer curves.
        /// </returns>
        public IEnumerable<NodedSegmentString<TCoordinate>> GetCurves()
        {
            
            add(_inputGeometry as ILineString<TCoordinate>);
            return _curveList;
        }

        private void addCurves(IEnumerable<ICoordinateSequence<TCoordinate>> lineList, Locations leftLoc, Locations rightLoc)
        {
            foreach (ICoordinateSequence<TCoordinate> line in lineList)
            {
                addCurve(line, leftLoc, rightLoc);
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
        private void addCurve(ICoordinateSequence<TCoordinate> coord, Locations leftLoc, Locations rightLoc)
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

        private void add(ILineString g)
        {
            if (g.IsEmpty)
            {
                return;
            }

            addLineString(g as ILineString<TCoordinate>);
        }

        private void add(IMultiLineString<TCoordinate> multiLine)
        {
            IEnumerable<ILineString<TCoordinate>> enumLineString = multiLine as IEnumerable<ILineString<TCoordinate>>;
            foreach (IGeometry<TCoordinate> line in enumLineString)
            {
                add(line as ILineString<TCoordinate>);
            }
        }

        private void addLineString(ILineString<TCoordinate> line)
        {
            if (_distanceForth <= 0.0d && _distanceBack <= 0.0d)
            {
                return;
            }

            IEnumerable<TCoordinate> coord = line.Coordinates.WithoutRepeatedPoints();
            IEnumerable<ICoordinateSequence<TCoordinate>> lineList = _curveBuilder.GetLineCurve(coord, _distanceForth, _distanceBack);
            addCurves(lineList, Locations.Exterior, Locations.Interior);
        }

        /// <summary>
        /// The ringCoord is assumed to contain no repeated points.
        /// It may be degenerate (i.e. contain only 1, 2, or 3 points).
        /// In this case it has no area, and hence has a minimum diameter of 0.
        /// </summary>
        private Boolean isErodedCompletely(IEnumerable<TCoordinate> ringCoord, Double bufferDistance)
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
                    = _inputGeometry.Factory.CoordinateFactory;
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
            ILinearRing<TCoordinate> ring = _inputGeometry.Factory.CreateLinearRing(ringCoord);
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
