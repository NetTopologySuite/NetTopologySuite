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
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;

namespace NetTopologySuite.Windows.Forms
{
    ///<summary>
    /// A list of <see cref="GraphicsPath"/> which contains a heterogeneous collection of other shapes
    /// representing NTS <see cref="IGeometry"/>s.
    ///</summary>
    public class GeometryCollectionGraphicsPath
    {
        private readonly IList<GraphicsPath> _shapes = new List<GraphicsPath>();

        public GraphicsPath Path
        {
            get
            {
                var p = new GraphicsPath();
                foreach (var graphicsPath in _shapes)
                    p.AddPath(graphicsPath, false);
                return p;
            }
        }

        public void Add(GraphicsPath shape)
        {
            _shapes.Add(shape);
        }

        public Rectangle GetBounds()
        {
            var res = GetBoundsF();
            return Rectangle.Truncate(res);
        }

        public RectangleF GetBoundsF()
        {
            var rectangle = new RectangleF();

            foreach (var shape in _shapes)
            {
                rectangle = rectangle.IsEmpty
                    ? shape.GetBounds()
                    : RectangleF.Union(rectangle, shape.GetBounds());
            }

            return rectangle;
        }

        public bool Contains(double x, double y)
        {
            /**@todo Implement this java.awt.Shape method*/
            throw new NotSupportedException(
                "Method contains() not yet implemented.");
        }

        public bool Contains(PointF p)
        {
            /**@todo Implement this java.awt.Shape method*/
            throw new NotSupportedException(
                "Method contains() not yet implemented.");
        }

        public bool Intersects(double x, double y, double w, double h)
        {
            /**@todo Implement this java.awt.Shape method*/
            throw new NotSupportedException(
                "Method intersects() not yet implemented.");
        }

        public bool Intersects(RectangleF r)
        {
            /**@todo Implement this java.awt.Shape method*/
            throw new NotSupportedException(
                "Method intersects() not yet implemented.");
        }

        public bool Contains(double x, double y, double w, double h)
        {
            /**@todo Implement this java.awt.Shape method*/
            throw new NotSupportedException(
                "Method contains() not yet implemented.");
        }

        public bool Contains(RectangleF r)
        {
            /**@todo Implement this java.awt.Shape method*/
            throw new NotSupportedException(
                "Method contains() not yet implemented.");
        }

        public GraphicsPathIterator GetPathIterator(Matrix at)
        {
            var p = (GraphicsPath)Path.Clone();
            p.Flatten(at);
            return new GraphicsPathIterator(p);
        }

        public GraphicsPathIterator GetPathIterator(Matrix at, double flatness)
        {
            var p = (GraphicsPath)Path.Clone();
            p.Flatten(at, (float)flatness);
            return new GraphicsPathIterator(p);
        }
    }
}