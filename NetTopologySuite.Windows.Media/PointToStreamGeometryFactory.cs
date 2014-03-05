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
using System.Windows;
using GeoAPI.Geometries;
using WpfGeometry = System.Windows.Media.Geometry;
using WpfPoint = System.Windows.Point;
using WpfStreamGeometry = System.Windows.Media.StreamGeometry;
using WpfStreamGeometryContext = System.Windows.Media.StreamGeometryContext;
using WpfSweepDirection = System.Windows.Media.SweepDirection;

namespace NetTopologySuite.Windows.Media
{
    ///<summary>
    /// An interface for classes which create <see cref="WpfGeometry"/>s to represent
    /// <see cref="IPoint"/> geometries. Windows.Forms does not provide an actual
    /// point shape, so some other shape must be used to render points (e.g. such
    /// as a Rectangle or Ellipse)
    ///</summary>
    /// <author>Martin Davis</author>
    [Obsolete("Use IPointStreamGeometryFactory or IPointPathGeometryFactory")]
    public interface IPointShapeFactory : IPointToStreamGeometryFactory
    {
    }

    public interface IPointToStreamGeometryFactory
    {
        ///<summary>
        /// Creates a shape representing an <see cref="IPoint"/>.
        ///</summary>
        /// <param name="point">The location of the point</param>
        /// <returns>A <see cref="WpfGeometry"/></returns>
        WpfGeometry CreatePoint(WpfPoint point);

        void AddShape(WpfPoint viewPoint, WpfStreamGeometryContext sgc);
    }

    public abstract class BasePointShapeFactory : IPointToStreamGeometryFactory
    {
        ///<summary>
        /// The default size of the shape
        ///</summary>
        public static double DefaultSize = 3.0;

        protected readonly double Size = DefaultSize;

        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        protected BasePointShapeFactory() { }

        ///<summary>
        /// Creates a factory for points of given size.
        ///</summary>
        /// <param name="size">The size of the points</param>
        protected BasePointShapeFactory(double size)
        {
            Size = (float)size;
        }

        ///<summary>
        /// Creates a shape representing an <see cref="IPoint"/>.
        ///</summary>
        public WpfGeometry CreatePoint(WpfPoint point)
        {
            var pointMarker = new WpfStreamGeometry();
            using (var sgc = pointMarker.Open())
                AddShape(point, sgc);
            return pointMarker;
        }

        public abstract void AddShape(WpfPoint point, WpfStreamGeometryContext sgc);
    }

    public class Dot : Square
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public Dot()
            : base(1)
        { }
    }

    public class Square : BasePointShapeFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public Square() { }

        public Square(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfStreamGeometryContext sgc)
        {
            var start = new WpfPoint(point.X - 0.5 * Size, point.Y - 0.5 * Size);
            var polylineTo = new List<WpfPoint>(new[]
                                 {
                                     new WpfPoint(point.X + 0.5*Size, point.Y - 0.5*Size),
                                     new WpfPoint(point.X + 0.5*Size, point.Y + 0.5*Size),
                                     new WpfPoint(point.X - 0.5*Size, point.Y + 0.5*Size),
                                     start
                                 });
            sgc.BeginFigure(start, true, true);
            sgc.PolyLineTo(polylineTo, true, true);
        }
    }

    public class Star : BasePointShapeFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public Star() { }

        public Star(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfStreamGeometryContext sgc)
        {
            var start = new WpfPoint(point.X, (point.Y - Size / 2));
            var linesTo = new List<WpfPoint>(
                new[]
                    {
                        new WpfPoint((point.X + Size*1/8), (point.Y - Size*1/8)),
                        new WpfPoint((point.X + Size/2), (point.Y - Size*1/8)),
                        new WpfPoint((point.X + Size*2/8), (point.Y + Size*1/8)),
                        new WpfPoint((point.X + Size*3/8), (point.Y + Size/2)),
                        new WpfPoint((point.X), (point.Y + Size*2/8)),
                        new WpfPoint((point.X - Size*3/8), (point.Y + Size/2)),
                        new WpfPoint((point.X - Size*2/8), (point.Y + Size*1/8)),
                        new WpfPoint((point.X - Size/2), (point.Y - Size*1/8)),
                        new WpfPoint((point.X - Size*1/8), (point.Y - Size*1/8))
                    }
                );

            sgc.BeginFigure(start, true, true);
            sgc.PolyLineTo(linesTo, true, true);
        }
    }

    public class Triangle : BasePointShapeFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public Triangle() { }

        public Triangle(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfStreamGeometryContext sgc)
        {
            var start = new WpfPoint(point.X, (point.Y - Size / 2));
            var linesTo = new List<WpfPoint>(
                new[]
                    {
                        new WpfPoint((point.X + Size/2), (point.Y + Size/2)),
                        new WpfPoint((point.X - Size/2), (point.Y + Size/2)),
                        new WpfPoint((point.X), (point.Y - Size/2))
                    });
            sgc.BeginFigure(start, true, true);
            sgc.PolyLineTo(linesTo, true, true);
        }
    }

    public class Circle : BasePointShapeFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public Circle() { }

        public Circle(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfStreamGeometryContext sgc)
        {
            var start = new WpfPoint(point.X, point.Y - Size * 0.5);
            sgc.BeginFigure(start, true, true);
            sgc.ArcTo(start, new Size(Size, Size), 360, true, WpfSweepDirection.Clockwise, true, true);
        }
    }

    public class Cross : BasePointShapeFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public Cross() { }

        public Cross(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfStreamGeometryContext sgc)
        {
            var x1 = point.X - Size / 2f;
            var x2 = point.X - Size / 4f;
            var x3 = point.X + Size / 4f;
            var x4 = point.X + Size / 2f;

            var y1 = point.Y - Size / 2f;
            var y2 = point.Y - Size / 4f;
            var y3 = point.Y + Size / 4f;
            var y4 = point.Y + Size / 2f;

            sgc.BeginFigure(new WpfPoint(x2, y1), true, true);
            sgc.PolyLineTo(new List<WpfPoint>(new[]
                {
                    new WpfPoint(x3, y1),
                    new WpfPoint(x3, y2),
                    new WpfPoint(x4, y2),
                    new WpfPoint(x4, y3),
                    new WpfPoint(x3, y3),
                    new WpfPoint(x3, y4),
                    new WpfPoint(x2, y4),
                    new WpfPoint(x2, y3),
                    new WpfPoint(x1, y3),
                    new WpfPoint(x1, y2),
                    new WpfPoint(x2, y2),
                    new WpfPoint(x2, y1)
                }), true, true);
        }
    }

    public class X : BasePointShapeFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public X() { }

        public X(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfStreamGeometryContext sgc)
        {
            sgc.BeginFigure(new WpfPoint((point.X), (point.Y - Size * 1 / 8)), true, true);
            sgc.PolyLineTo(new List<WpfPoint>(new[]
                {
                    new WpfPoint((point.X + Size*2/8), (point.Y - Size/2)),
                    new WpfPoint((point.X + Size/2), (point.Y - Size/2)),
                    new WpfPoint((point.X + Size*1/8), (point.Y)),
                    new WpfPoint((point.X + Size/2), (point.Y + Size/2)),
                    new WpfPoint((point.X + Size*2/8), (point.Y + Size/2)),
                    new WpfPoint((point.X), (point.Y + Size*1/8)),
                    new WpfPoint((point.X - Size*2/8), (point.Y + Size/2)),
                    new WpfPoint((point.X - Size/2), (point.Y + Size/2)),
                    new WpfPoint((point.X - Size*1/8), (point.Y)),
                    new WpfPoint((point.X - Size/2), (point.Y - Size/2)),
                    new WpfPoint((point.X - Size*2/8), (point.Y - Size/2))
                }), true, true);
        }
    }
}
#endif