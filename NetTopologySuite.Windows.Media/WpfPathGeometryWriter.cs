/*
 * The JTS Topology Suite is a collection of Java classes that
 * implement the fundamental operations required to validate a given
 * geo-spatial data set to a known topological specification.
 *
 * Copyright (C) 2001 Vivid Solutions
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * For more information, contact:
 *
 *     Vivid Solutions
 *     Suite #1A
 *     2328 Government Street
 *     Victoria BC  V8T 5G5
 *     Canada
 *
 *     (250)385-6040
 *     www.vividsolutions.com
 */

#if PATH_GEOMETRY

using System;
using System.Collections.Generic;
using System.Windows.Media;
using GeoAPI.Geometries;
using WpfGeometry = System.Windows.Media.Geometry;
using WpfPathGeometry = System.Windows.Media.PathGeometry;
using WpfPoint = System.Windows.Point;

namespace NetTopologySuite.Windows.Media
{
    ///<summary>
    /// Writes <see cref="IGeometry"/>s into <see cref="WpfGeometry"/>s
    /// of the appropriate type.
    /// This supports rendering geometries using System.Windows.Media.
    /// The GeometryWriter allows supplying a <see cref="IPointTransformation"/>
    /// class, to transform coordinates from model space into view space.
    /// This is useful if a client is providing its own transformation
    /// logic, rather than relying on <see cref="Transform"/>.
    /// <para/>
    /// The writer supports removing duplicate consecutive points
    /// (via the <see cref="RemoveDuplicatePoints"/> property) 
    /// as well as true <b>decimation</b>
    /// (via the <see cref="Decimation"/> property. 
    /// Enabling one of these strategies can substantially improve 
    /// rendering speed for large geometries.
    /// It is only necessary to enable one strategy.
    /// Using decimation is preferred, but this requires 
    /// determining a distance below which input geometry vertices
    /// can be considered unique (which may not always be feasible).
    /// If neither strategy is enabled, all vertices
    /// of the input <tt>Geometry</tt>
    /// will be represented in the output <tt>WpfGeometry</tt>.
    ///</summary>
    public class WpfPathGeometryWriter
    {
        /**
         * The point transformation used by default.
         */
        public static readonly IPointTransformation DefaultPointTransformation = new IdentityPointTransformation();

        /**
         * The point shape factory used by default.
         */
        public static readonly IPointToPathGeometryFactory DefaultPointFactory = new SquarePath(3.0);

        private readonly IPointTransformation _pointTransformer = DefaultPointTransformation;
        private readonly IPointToPathGeometryFactory _pointFactory = DefaultPointFactory;

        ///**
        // * Cache a PointF object to use to transfer coordinates into shape
        // */
        //private WpfPoint _transPoint;

        ///<summary>
        /// Creates a new GraphicsPathWriter with a specified point transformation and point shape factory.
        ///</summary>
        /// <param name="pointTransformer">A transformation from model to view space to use </param>
        /// <param name="pointFactory">The PointShapeFactory to use</param>
        public WpfPathGeometryWriter(IPointTransformation pointTransformer, IPointToPathGeometryFactory pointFactory)
        {
            if (pointTransformer != null)
                _pointTransformer = pointTransformer;
            if (pointFactory != null)
                _pointFactory = pointFactory;
        }

        ///<summary>
        /// Creates a new GraphicsPathWriter with a specified point transformation and the default point shape factory.
        ///</summary>
        /// <param name="pointTransformer">A transformation from model to view space to use </param>
        public WpfPathGeometryWriter(IPointTransformation pointTransformer)
            : this(pointTransformer, null)
        {
        }

        ///<summary>
        /// Creates a new GraphicsPathWriter with the default (identity) point transformation.
        ///</summary>
        public WpfPathGeometryWriter()
        {
        }

        /// <summary>
        /// Gets or sets whether duplicate consecutive points should be eliminated.
        /// This can reduce the size of the generated Shapes
        /// and improve rendering speed, especially in situations
        /// where a transform reduces the extent of the geometry.
        /// <para/>
        /// The default is <tt>false</tt>.
        /// </summary>
        public bool RemoveDuplicatePoints { get; set; }

        /// <summary>
        /// Gets or sets the decimation distance used to determine
        /// whether vertices of the input geometry are 
        /// considered to be duplicate and thus removed.
        /// The distance is axis distance, not Euclidean distance.
        /// The distance is specified in the input geometry coordinate system
        /// (NOT the transformed output coordinate system).
        /// <para/>
        /// When rendering to a screen image, a suitably small distance should be used
        /// to avoid obvious rendering defects.  
        /// A distance equivalent to 2 pixels or less is recommended
        /// (and perhaps even smaller to avoid any chance of visible artifacts).
        /// <para/>
        /// The default distance is 0.0, which disables decimation.
        /// </summary>
        public double Decimation { get; set; }

        ///<summary>
        /// Creates a <see cref="WpfGeometry"/> representing a <see cref="IGeometry"/>, according to the specified PointTransformation and PointShapeFactory (if relevant).
        ///</summary>
        public WpfGeometry ToShape(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return new WpfPathGeometry();

            var p = new WpfPathGeometry();
            AddShape(p, geometry);

            p.Freeze();
            return p;
        }

        private void AddShape(WpfPathGeometry pathGeometry, IPolygon p)
        {
            AddShape(pathGeometry, p.Shell, true);
            var holes = p.Holes;
            if (holes == null) return;

            foreach (var hole in holes)
                AddShape(pathGeometry, hole, true);
        }

        private void AddShape(WpfPathGeometry pathGeometry, IGeometry geometry)
        {
            if (geometry is IPolygon)
                AddShape(pathGeometry, (IPolygon)geometry);
            else if (geometry is ILinearRing)
                AddShape(pathGeometry, (ILinearRing)geometry);
            else if (geometry is ILineString)
                AddShape(pathGeometry, (ILineString)geometry);
            else if (geometry is IPoint)
                AddShape(pathGeometry, (IPoint)geometry);
            else if (geometry is IGeometryCollection)
                AddShape(pathGeometry, (IGeometryCollection)geometry);
            else
            {
                throw new ArgumentException(
                    "Unrecognized Geometry class: " + geometry.GetType());
            }
        }

        private void AddShape(WpfPathGeometry sgc, IGeometryCollection gc)
        {
            foreach (IGeometry geometry in gc.Geometries)
            {
                AddShape(sgc, geometry);
            }
        }

        //private void AddShape(WpfStreamGeometryContext sgc, IMultiLineString mls)
        //{
        //    var path = new WpfStreamGeometry();
        //    using
        //    foreach (ILineString ml in mls)
        //        path.AddPath(ToShape(ml), false);

        //    return path;
        //}

        private void AddShape(WpfPathGeometry pathGeometry, ILineString lineString, bool closed = false, bool filled = false)
        {
            var coords = lineString.Coordinates;

            WpfPoint[] wpfPoints;
            var startPoint = TransformSequence(coords, out wpfPoints);
            
            var polyLineSegment = new PolyLineSegment();

            foreach (var coordinate in wpfPoints)
                polyLineSegment.Points.Add(coordinate);

            var figure = new PathFigure
                             {
                                 StartPoint = startPoint, 
                                 IsClosed = closed, 
                                 IsFilled = filled
                             };
            figure.Segments.Add(polyLineSegment);
            pathGeometry.Figures.Add(figure);
        }

        private void AddShape(WpfPathGeometry pathGeometry, ILinearRing linearRing, bool filled)
        {
            AddShape(pathGeometry, linearRing, true, filled);
        }

        private void AddShape(WpfPathGeometry pathGeometry, IPoint point)
        {
            var viewPoint = TransformPoint(point.Coordinate);
            _pointFactory.AddShape(viewPoint, pathGeometry);
        }

        //private IEnumerable<WpfPoint> TransformPoints(Coordinate[] model, int start)
        //{
        //    var ret = new List<WpfPoint>(model.Length - start);
        //    for (var i = start; i < model.Length; i++)
        //        ret.Add(TransformPoint(model[i], new WpfPoint()));
        //    return ret;
        //}

        private WpfPoint TransformSequence(Coordinate[] coords, out WpfPoint[] points)
        {
            var resPoint = TransformPoint(coords[0]);
            var prev = coords[0];

            var n = coords.Length - 1;
            var pointList = new List<WpfPoint>(n);

            var prevX = resPoint.X;
            var prevY = resPoint.Y;

            //int count = 0;
            for (var i = 1; i <= n; i++)
            {
                if (Decimation > 0)
                {
                    var isDecimated = Math.Abs(coords[i].X - prev.X) < Decimation &&
                                      Math.Abs(coords[i].Y - prev.Y) < Decimation;
                    if (isDecimated)
                        continue;
                    prev = coords[i];
                }

                var tmpPoint = TransformPoint(coords[i]);
                if (RemoveDuplicatePoints)
                {
                    // skip duplicate points (except the last point)
                    var isDup = tmpPoint.X == prevX && 
                                tmpPoint.Y == prevY;
                    if (/*i < n && */isDup)
                        continue;
                    prevX = tmpPoint.X;
                    prevY = tmpPoint.Y;
                    //count++;
                }
                pointList.Add(tmpPoint);
            }

            points = pointList.ToArray();
            return resPoint;
        }

        private WpfPoint TransformPoint(Coordinate model)
        {
            return _pointTransformer.Transform(model);
        }
    }
}

#endif