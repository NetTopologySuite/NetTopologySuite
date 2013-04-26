using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Windows.Forms
{
    ///<summary>
    /// Converts a DotNet <see cref="GraphicsPath"/>
    /// or the more general <see cref="GraphicsPathIterator"/> into a <see cref="IGeometry"/>.
    ///</summary>
    ///<remarks>
    /// <para>
    /// The coordinate system for <see cref="System.Drawing.Graphics"/> is typically screen coordinates, 
    /// which has the Y axis inverted  relative to the usual JTS coordinate system.
    /// This is rectified during conversion. </para>
    /// <para>
    /// GraphicsPaths to be converted are expected to be linear or flat.
    /// That is, <see cref="GraphicsPathIterator.HasCurve"/> must always return false.
    /// Otherwise an exception will be thrown.
    /// </para>
    /// </remarks>  
    /// <author>Martin Davis</author>
    public class GraphicsPathReader
    {
        private static readonly Matrix InvertY = new Matrix(1, 0, 0, -1, 0, 0);

        ///<summary>
        /// Converts a flat path to a <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="pathIt">The path to convert</param>
        /// <param name="geomFact">The GeometryFactory to use</param>
        /// <returns>A Geometry representing the path</returns>
        public static IGeometry Read(GraphicsPathIterator pathIt, IGeometryFactory geomFact)
        {
            var pc = new GraphicsPathReader(geomFact);
            return pc.Read(pathIt);
        }
        ///<summary>
        /// Converts a <see cref="GraphicsPath"/> to a Geometry, flattening it first.
        ///</summary>
        /// <param name="shp">The <see cref="GraphicsPath"/></param>
        /// <param name="flatness">The flatness parameter to use</param>
        /// <param name="geomFact">The GeometryFactory to use</param>
        /// <returns>A Geometry representing the shape</returns>
        public static IGeometry Read(GraphicsPath shp, double flatness, IGeometryFactory geomFact)
        {
            var path = (GraphicsPath)shp.Clone();
            path.Flatten(InvertY, (float)flatness);
            var pathIt = new GraphicsPathIterator(path);
            return Read(pathIt, geomFact);
        }

        private readonly IGeometryFactory _geometryFactory;

        public GraphicsPathReader(IGeometryFactory geometryFactory)
        {
            _geometryFactory = geometryFactory;
        }

        ///<summary>
        /// Converts a flat path to a <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="pathIt">The path iterator of the path to convert</param>
        /// <returns>A Geometry representing the path</returns>
        public IGeometry Read(GraphicsPathIterator pathIt)
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
        /// <param name="pathIt">A path iterator</param>
        /// <returns>A list of coordinate arrays</returns>
        /// <exception cref="ArgumentException">If a non-linear segment type is encountered</exception>
        public static IList<Coordinate[]> ToCoordinates(GraphicsPathIterator pathIt)
        {
            if (pathIt.HasCurve())
                throw new ArgumentException("Path must not have non-linear segments");

            var coordArrays = new List<Coordinate[]>();
            int startIndex, endIndex;
            bool isClosed;

            while (pathIt.NextSubpath(out startIndex, out endIndex, out isClosed) > 0)
            {
                Coordinate[] pts = NextCoordinateArray(pathIt, startIndex, endIndex, isClosed);
                coordArrays.Add(pts);
                if (endIndex == pathIt.Count - 1) break;

            }
            return coordArrays;
        }

        private static Coordinate[] NextCoordinateArray(GraphicsPathIterator pathIt, int start, int end, bool isClosed)
        {
            var num = end - start + 1;
            var points = new PointF[num];
            var types = new byte[num];
            pathIt.CopyData(ref points, ref types, start, end);

            var ret = new Coordinate[num + (isClosed ? 1 : 0)];
            for (var i = 0; i < num; i++)
                ret[i] = new Coordinate(points[i].X, points[i].Y);
            if (isClosed)
                ret[ret.Length - 1] = new Coordinate(points[0].X, points[0].Y);

            return ret;
        }

    }
}