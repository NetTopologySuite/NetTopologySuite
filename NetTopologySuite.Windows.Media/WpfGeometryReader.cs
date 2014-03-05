using System;
using System.Collections.Generic;
using System.Windows.Media;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using WpfGeometry = System.Windows.Media.Geometry;
using WpfLineSegment = System.Windows.Media.LineSegment;
using WpfPoint = System.Windows.Point;

namespace NetTopologySuite.Windows.Media
{
    ///<summary>
    /// Converts a WPF <see cref="WpfGeometry"/> into a <see cref="IGeometry"/>.
    ///</summary>
    ///<remarks>
    /// <para>
    /// The coordinate system for <see cref="WpfGeometry"/> is typically screen coordinates,
    /// which has the Y axis inverted  relative to the usual JTS coordinate system.
    /// This is rectified during conversion. </para>
    /// <para>
    /// GraphicsPaths to be converted are expected to be linear or flat.
    /// That is, <see cref="System.Windows.Media.Geometry.MayHaveCurves"/> must always return false.
    /// Otherwise an exception will be thrown.
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class WpfGeometryReader
    {
        private static readonly Matrix InvertY = new Matrix(1, 0, 0, -1, 0, 0);

        ///<summary>
        /// Converts a flat path to a <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="pathIt">The path to convert</param>
        /// <param name="geomFact">The GeometryFactory to use</param>
        /// <returns>A Geometry representing the path</returns>
        public static IGeometry Read(WpfGeometry pathIt, IGeometryFactory geomFact)
        {
            var pc = new WpfGeometryReader(geomFact);
            return pc.Read(pathIt);
        }

        ///<summary>
        /// Converts a <see cref="WpfGeometry"/> to a Geometry, flattening it first.
        ///</summary>
        /// <param name="shp">The <see cref="WpfGeometry"/></param>
        /// <param name="tolerance">The tolerance parameter to use</param>
        /// <param name="geomFact">The GeometryFactory to use</param>
        /// <returns>A Geometry representing the shape</returns>
        public static IGeometry Read(WpfGeometry shp, double tolerance, IGeometryFactory geomFact)
        {
            var path = shp.GetFlattenedPathGeometry(tolerance, ToleranceType.Relative);
            return Read(path, geomFact);
        }

        private readonly IGeometryFactory _geometryFactory;

        public WpfGeometryReader(IGeometryFactory geometryFactory)
        {
            _geometryFactory = geometryFactory;
        }

        ///<summary>
        /// Converts a flat path to a <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="pathIt">The path iterator of the path to convert</param>
        /// <returns>A Geometry representing the path</returns>
        public IGeometry Read(WpfGeometry pathIt)
        {
            var pathPtSeq = ToCoordinates(pathIt);

            var polys = new List<IGeometry>();
            var seqIndex = 0;
            while (seqIndex < pathPtSeq.Count)
            {
                // assume next seq is shell
                // TODO: test this
                var pts = pathPtSeq[seqIndex];
                var shell = _geometryFactory.CreateLinearRing(pts);
                seqIndex++;

                var holes = new List<ILinearRing>();
                Coordinate[] holePts;
                // add holes as long as rings are CCW
                while (seqIndex < pathPtSeq.Count && IsHole(holePts = pathPtSeq[seqIndex]))
                {
                    var hole = _geometryFactory.CreateLinearRing(holePts);
                    holes.Add(hole);
                    seqIndex++;
                }
                var holeArray = holes.ToArray();//GeometryFactory.ToLinearRingArray(holes);
                polys.Add(_geometryFactory.CreatePolygon(shell, holeArray));
            }
            return _geometryFactory.BuildGeometry(polys);
        }

        private static bool IsHole(Coordinate[] pts)
        {
            return CGAlgorithms.IsCCW(pts);
        }

        ///<summary>
        /// Extracts the points of the paths in a flat {@link PathIterator} into
        /// a list of Coordinate arrays.
        ///</summary>
        /// <param name="pathGeometry">A path figure collection</param>
        /// <returns>A list of coordinate arrays</returns>
        /// <exception cref="ArgumentException">If a non-linear segment type is encountered</exception>
        private static IList<Coordinate[]> ToCoordinates(PathGeometry pathGeometry)
        {
            if (pathGeometry.MayHaveCurves())
                throw new ArgumentException("WPF geometry must not have non-linear segments");

            var coordArrays = new List<Coordinate[]>();

            var pathFigures = pathGeometry.Figures;

            foreach (PathFigure pathFigure in pathFigures)
            {
                var pts = NextCoordinateArray(pathFigure);
                coordArrays.Add(pts);
            }
            return coordArrays;
        }

        private static IList<Coordinate[]> ToCoordinates(WpfGeometry wpfGeometry)
        {
            return ToCoordinates(PathGeometry.CreateFromGeometry(wpfGeometry));
        }

        private static Coordinate[] NextCoordinateArray(PathFigure pathFigure)
        {
            var coordinateList = new List<Coordinate>(pathFigure.Segments.Count + 1);

            coordinateList.Add(ToCoordinate(pathFigure.StartPoint));
            foreach (var segment in pathFigure.Segments)
            {
                if (segment is PolyLineSegment)
                {
                    var pseg = segment as PolyLineSegment;
                    foreach (var point in pseg.Points)
                        coordinateList.Add(ToCoordinate(point));
                }
                else if (segment is WpfLineSegment)
                    coordinateList.Add(ToCoordinate(((WpfLineSegment)segment).Point));
                else
                {
                    throw new NotSupportedException(string.Format("'{0}' is not supported", segment.GetType()));
                }
            }
            return coordinateList.ToArray();
        }

        private static Coordinate ToCoordinate(WpfPoint point)
        {
            return new Coordinate(point.X, point.Y);
        }
    }
}