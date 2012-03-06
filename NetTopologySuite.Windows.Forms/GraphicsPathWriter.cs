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
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;

namespace NetTopologySuite.Windows.Forms
{
    ///<summary>
    /// Writes <see cref="IGeometry"/>s int DotNet <see cref="GraphicsPath"/>s.
    ///</summary>
    public class GraphicsPathWriter
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

        /**
         * Cache a PointF object to use to transfer coordinates into shape
         */
        private PointF _transPoint;

        ///<summary>
        /// Creates a new GraphicsPathWriter with a specified point transformation and point shape factory.
        ///</summary>
        /// <param name="pointTransformer">A transformation from model to view space to use </param>
        /// <param name="pointFactory">The PointShapeFactory to use</param>
        public GraphicsPathWriter(IPointTransformation pointTransformer, IPointShapeFactory pointFactory)
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
        public GraphicsPathWriter(IPointTransformation pointTransformer)
            : this(pointTransformer, null)
        {
        }

        /**
         *
         *
         */

        ///<summary>
        /// Creates a new GraphicsPathWriter with the default (identity) point transformation.
        ///</summary>
        public GraphicsPathWriter()
        {
        }

        ///<summary>
        /// Creates a <see cref="GraphicsPath"/> representing a <see cref="IGeometry"/>, according to the specified PointTransformation and PointShapeFactory (if relevant).
        ///</summary>
        ///<remarks>
        /// Note that GraphicsPaths do not preserve information about which elements in heterogeneous collections are 1D and which are 2D.
        /// For example, a GeometryCollection containing a ring and a disk will render as two disks if Graphics.FillPath is used,
        /// or as two rings if Graphics.DrawPath is used. To avoid this issue use separate shapes for the components.
        ///</remarks>
        public GraphicsPath ToShape(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return new GraphicsPath();
            if (geometry is IPolygon)
                return ToShape((IPolygon)geometry);
            if (geometry is ILineString)
                return ToShape((ILineString)geometry);
            if (geometry is IMultiLineString)
                return ToShape((IMultiLineString)geometry);
            if (geometry is IPoint)
                return ToShape((IPoint)geometry);
            if (geometry is IGeometryCollection)
                return ToShape((IGeometryCollection)geometry);

            throw new ArgumentException(
                "Unrecognized Geometry class: " + geometry.GetType());
        }

        private GraphicsPath ToShape(IPolygon p)
        {
            var poly = new PolygonGraphicsPath();

            Append(poly, p.ExteriorRing.Coordinates);
            for (int j = 0; j < p.NumInteriorRings; j++)
            {
                Append(poly, p.GetInteriorRingN(j).Coordinates);
            }

            return poly.Path;
        }

        private void Append(PolygonGraphicsPath poly, Coordinate[] coords)
        {
            GraphicsPath ring = null;
            for (var i = 0; i < coords.Length; i++)
            {
                _transPoint = TransformPoint(coords[i], _transPoint);
                poly.AddToRing(_transPoint, ref ring);
            }
            poly.EndRing(ring);
        }

        /*
         // Obsolete (slower code)
        private Shape OLDtoShape(Polygon p)
        {
            ArrayList holeVertexCollection = new ArrayList();

            for (int j = 0; j < p.getNumInteriorRing(); j++) {
                holeVertexCollection.add(
                    toViewCoordinates(p.getInteriorRingN(j).getCoordinates()));
            }

            return new PolygonShape(
                toViewCoordinates(p.getExteriorRing().getCoordinates()),
                holeVertexCollection);
        }

        private Coordinate[] toViewCoordinates(Coordinate[] modelCoordinates)
        {
            Coordinate[] viewCoordinates = new Coordinate[modelCoordinates.length];

            for (int i = 0; i < modelCoordinates.length; i++) {
                Point2D point2D = toPoint(modelCoordinates[i]);
                viewCoordinates[i] = new Coordinate(point2D.getX(), point2D.getY());
            }

            return viewCoordinates;
        }
    */

        private GraphicsPath ToShape(IGeometryCollection gc)
        {
            var shape = new GeometryCollectionGraphicsPath();
            // add components to GC shape
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                shape.Add(ToShape(g));
            }
            return shape.Path;
        }

        private GraphicsPath ToShape(IMultiLineString mls)
        {
            var path = new GraphicsPath();

            foreach (ILineString ml in mls)
                path.AddPath(ToShape(ml), false);

            return path;
        }

        private GraphicsPath ToShape(ILineString lineString)
        {
            var shape = new GraphicsPath();

            var points = _pointTransformer.Transform(lineString.Coordinates);
            shape.AddLines(points);
            return shape;
        }

        private GraphicsPath ToShape(IPoint point)
        {
            var viewPoint = TransformPoint(point.Coordinate);
            return _pointFactory.CreatePoint(viewPoint);
        }

        private PointF TransformPoint(Coordinate model)
        {
            return TransformPoint(model, new PointF());
        }

        private PointF TransformPoint(Coordinate model, PointF view)
        {
            _pointTransformer.Transform(model, ref view);
            /**
             * Do the rounding now instead of relying on Java 2D rounding. Java2D seems
             * to do rounding differently for drawing and filling, resulting in the draw
             * being a pixel off from the fill sometimes.
             */
            return new PointF((float)Math.Round(view.X, MidpointRounding.AwayFromZero), (float)Math.Round(view.Y, MidpointRounding.AwayFromZero));
        }
    }
}