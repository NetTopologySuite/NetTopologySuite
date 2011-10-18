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

using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using WpfGeometry = System.Windows.Media.Geometry;
using WpfPoint = System.Windows.Point;
using WpfStreamGeometry = System.Windows.Media.StreamGeometry;
using WpfStreamGeometryContext = System.Windows.Media.StreamGeometryContext;

namespace NetTopologySuite.Windows.Media
{

    ///<summary>
    /// Writes <see cref="IGeometry"/>s into  <see cref="WpfGeometry"/>s.
    ///</summary>
    public class WpfGeometryWriter
    {
        /**
         * The point transformation used by default.
         */
        public static readonly IPointTransformation DefaultPointTransformation = new IdentityPointTransformation();

        /**
         * The point shape factory used by default.
         */
        public static readonly IPointShapeFactory DefaultPointFactory = new Square(3.0);

        private readonly IPointTransformation _pointTransformer = DefaultPointTransformation;
        private readonly IPointShapeFactory _pointFactory = DefaultPointFactory;

        ///**
        // * Cache a PointF object to use to transfer coordinates into shape
        // */
        //private WpfPoint _transPoint;

        ///<summary>
        /// Creates a new GraphicsPathWriter with a specified point transformation and point shape factory.
        ///</summary>
        /// <param name="pointTransformer">A transformation from model to view space to use </param>
        /// <param name="pointFactory">The PointShapeFactory to use</param>
        public WpfGeometryWriter(IPointTransformation pointTransformer, IPointShapeFactory pointFactory)
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
        public WpfGeometryWriter(IPointTransformation pointTransformer)
            : this(pointTransformer, null)
        {
        }

        ///<summary>
        /// Creates a new GraphicsPathWriter with the default (identity) point transformation.
        ///</summary>
        public WpfGeometryWriter()
        {
        }

        ///<summary>
        /// Creates a <see cref="WpfGeometry"/> representing a <see cref="IGeometry"/>, according to the specified PointTransformation and PointShapeFactory (if relevant).
        ///</summary>
        public WpfGeometry ToShape(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return new WpfStreamGeometry();

            var p = new WpfStreamGeometry();
            using (var sgc = p.Open())
                AddShape(sgc, geometry);

            p.Freeze();
            return p;
        }

        private void AddShape(WpfStreamGeometryContext sgc, IPolygon p)
        {
            AddShape(sgc, p.Shell, true);
            var holes = p.Holes;
            if (holes != null)
            {
                foreach (ILinearRing hole in holes)
                    AddShape(sgc, hole, true);
            }
        }

        private void AddShape(WpfStreamGeometryContext sgc, IGeometry geometry)
        {
            if (geometry is IPolygon)
                AddShape(sgc, (IPolygon)geometry);
            else if (geometry is ILineString)
                AddShape(sgc, (ILineString)geometry);
            //else if (geometry is IMultiLineString)
            //    AddShape(sgc, (IMultiLineString)geometry);
            else if (geometry is IPoint)
                AddShape(sgc, (IPoint) geometry);
            else if (geometry is IGeometryCollection)
                AddShape(sgc, (IGeometryCollection)geometry);
            else
            {
                throw new ArgumentException(
                    "Unrecognized Geometry class: " + geometry.GetType());
            }
            
        }


        private void AddShape(WpfStreamGeometryContext sgc, IGeometryCollection gc)
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

        private void AddShape(WpfStreamGeometryContext sgc, ILineString lineString)
        {
            var coords = lineString.Coordinates;
            sgc.BeginFigure(TransformPoint(coords[0]), false, false);
            sgc.PolyLineTo(TransformPoints(coords, 1), true, true);
        }

        private void AddShape(WpfStreamGeometryContext sgc, ILinearRing linearRing, bool filled)
        {
            var coords = linearRing.Coordinates;
            sgc.BeginFigure(TransformPoint(coords[0]), filled, true);
            sgc.PolyLineTo(TransformPoints(coords, 1), true, true);
        }

        private void AddShape(WpfStreamGeometryContext sgc, IPoint point)
        {
            var viewPoint = TransformPoint(point.Coordinate);
            _pointFactory.AddShape(viewPoint, sgc);
        }

        private IList<WpfPoint> TransformPoints(ICoordinate[] model, int start)
        {
            var ret = new List<WpfPoint>(model.Length - start);
            for( int i = 1; i < model.Length; i++)
                ret.Add(TransformPoint(model[i], new WpfPoint()));
            return ret;
        }

        private WpfPoint TransformPoint(ICoordinate model)
        {
            return TransformPoint(model, new WpfPoint());
        }

        private WpfPoint TransformPoint(ICoordinate model, WpfPoint view)
        {
            _pointTransformer.Transform(model, ref view);
            /**
             * Do the rounding now instead of relying on Java 2D rounding. Java2D seems
             * to do rounding differently for drawing and filling, resulting in the draw
             * being a pixel off from the fill sometimes.
             */
            return new WpfPoint(Math.Round(view.X), Math.Round(view.Y));
        }
    }
}