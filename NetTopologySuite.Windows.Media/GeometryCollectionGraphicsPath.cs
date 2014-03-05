#if STREAM_GEOMETRY
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
using WpfStreamGeometry = System.Windows.Media.StreamGeometry;
using WpfStreamGeometryContext = System.Windows.Media.StreamGeometryContext;
using WpfRectangle = System.Windows.Rect;
using WpfPen = System.Windows.Media.Pen;
using WpfTransform = System.Windows.Media.Transform;

namespace NetTopologySuite.Windows.Media
{
    /////<summary>
    ///// A list of <see cref="WpfGeometry"/> which contains a heterogeneous collection of other shapes
    ///// representing NTS <see cref="IGeometry"/>s.
    /////</summary>
    //public class GeometryCollectionGraphicsPath
    //{
    //    private readonly IList<WpfGeometry> _shapes = new List<WpfGeometry>();
    //    public WpfGeometry Path
    //    {
    //        get
    //        {
    //            var p = new WpfStreamGeometry();
    //            using (var sgc = p.Open())
    //            {
    //                foreach (var graphicsPath in _shapes)
    //                    sgc.
    //                    p.AddPath(graphicsPath, false);
    //            }
    //            return p;
    //        }
    //    }

    //    public void Add(GraphicsPath shape)
    //    {
    //        _shapes.Add(shape);
    //    }

    //    public WpfRectangle GetBounds()
    //    {
    //        /**@todo Implement this java.awt.Shape method*/
    //        throw new NotSupportedException(
    //            "Method getBounds() not yet implemented.");
    //    }

    //    public WpfRectangle GetBounds(WpfPen pen)
    //    {
    //        var rectangle = new WpfRectangle();

    //        foreach (var shape in _shapes)
    //        {
    //            rectangle = rectangle.IsEmpty
    //                ? shape.GetRenderBounds(pen)
    //                : WpfRectangle.Union(rectangle, shape.GetRenderBounds(pen));
    //        }

    //        return rectangle;
    //    }

    //    //public bool Contains(double x, double y)
    //    //{
    //    //    /**@todo Implement this java.awt.Shape method*/
    //    //    throw new NotSupportedException(
    //    //        "Method contains() not yet implemented.");
    //    //}

    //    //public bool Contains(PointF p)
    //    //{
    //    //    /**@todo Implement this java.awt.Shape method*/
    //    //    throw new NotSupportedException(
    //    //        "Method contains() not yet implemented.");
    //    //}

    //    //public bool Intersects(double x, double y, double w, double h)
    //    //{
    //    //    /**@todo Implement this java.awt.Shape method*/
    //    //    throw new NotSupportedException(
    //    //        "Method intersects() not yet implemented.");
    //    //}

    //    //public bool Intersects(RectangleF r)
    //    //{
    //    //    /**@todo Implement this java.awt.Shape method*/
    //    //    throw new NotSupportedException(
    //    //        "Method intersects() not yet implemented.");
    //    //}

    //    //public bool Contains(double x, double y, double w, double h)
    //    //{
    //    //    /**@todo Implement this java.awt.Shape method*/
    //    //    throw new NotSupportedException(
    //    //        "Method contains() not yet implemented.");
    //    //}

    //    //public bool Contains(RectangleF r)
    //    //{
    //    //    /**@todo Implement this java.awt.Shape method*/
    //    //    throw new NotSupportedException(
    //    //        "Method contains() not yet implemented.");
    //    //}

    //    public WpfGeometry GetPathIterator(WpfTransform at)
    //    {
    //        var p = (GraphicsPath) Path.Clone();
    //        p.Flatten(at);
    //        return new GraphicsPathIterator(p);
    //    }

    //    public WpfGeometry GetPathIterator(WpfTransform at, double flatness)
    //    {
    //        // since Geometry is linear, can simply delegate to the simple method
    //        return GetPathIterator(at);
    //    }
    //}
}
#endif