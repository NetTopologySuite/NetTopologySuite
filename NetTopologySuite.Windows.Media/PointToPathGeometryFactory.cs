using System.Collections.Generic;
using System.Windows;
using GeoAPI.Geometries;
using WpfArcSegment = System.Windows.Media.ArcSegment;
using WpfGeometry = System.Windows.Media.Geometry;
using WpfPathFigure = System.Windows.Media.PathFigure;
using WpfPathGeometry = System.Windows.Media.PathGeometry;
using WpfPoint = System.Windows.Point;
using WpfPolyLineSegment = System.Windows.Media.PolyLineSegment;
using WpfSweepDirection = System.Windows.Media.SweepDirection;

namespace NetTopologySuite.Windows.Media
{
    public interface IPointToPathGeometryFactory
    {
        ///<summary>
        /// Creates a shape representing an <see cref="IPoint"/>.
        ///</summary>
        /// <param name="point">The location of the point</param>
        /// <returns>A <see cref="WpfGeometry"/></returns>
        WpfGeometry CreatePoint(WpfPoint point);

        void AddShape(WpfPoint viewPoint, WpfPathGeometry pathGeometry);
    }

    public abstract class BasePointToPathGeometryFactory : IPointToPathGeometryFactory
    {
        ///<summary>
        /// The default size of the shape
        ///</summary>
        public static double DefaultSize = 3.0;

        protected readonly double Size = DefaultSize;

        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        protected BasePointToPathGeometryFactory() { }

        ///<summary>
        /// Creates a factory for points of given size.
        ///</summary>
        /// <param name="size">The size of the points</param>
        protected BasePointToPathGeometryFactory(double size)
        {
            Size = (float)size;
        }

        ///<summary>
        /// Creates a shape representing an <see cref="IPoint"/>.
        ///</summary>
        public WpfGeometry CreatePoint(WpfPoint point)
        {
            var pointMarker = new WpfPathGeometry();
            AddShape(point, pointMarker);
            return pointMarker;
        }

        public abstract void AddShape(WpfPoint point, WpfPathGeometry sgc);
    }

    public class DotPath : SquarePath
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public DotPath()
            : base(1)
        { }
    }

    public class SquarePath : BasePointToPathGeometryFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public SquarePath() { }

        public SquarePath(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfPathGeometry pathGeometry)
        {
            var figure = new WpfPathFigure {StartPoint = new WpfPoint(point.X - 0.5*Size, point.Y - 0.5*Size)};
            var segment = new WpfPolyLineSegment();
            var points = segment.Points;
            points.Add(new WpfPoint(point.X + 0.5*Size, point.Y - 0.5*Size));
            points.Add(new WpfPoint(point.X + 0.5*Size, point.Y + 0.5*Size));
            points.Add(new WpfPoint(point.X - 0.5*Size, point.Y + 0.5*Size));
            points.Add(figure.StartPoint);

            figure.Segments.Add(segment);
            figure.IsClosed = true;
            figure.IsFilled = true;

            pathGeometry.Figures.Add(figure);
        }
    }

    public class StarPath : BasePointToPathGeometryFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public StarPath() { }

        public StarPath(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfPathGeometry pathGeometry)
        {
            var start = new WpfPoint(point.X, (point.Y - Size / 2));
            var figure = new WpfPathFigure {StartPoint = start};
            var segment = new WpfPolyLineSegment();
            var points = segment.Points;
            points.Add(new WpfPoint((point.X + Size*1/8), (point.Y - Size*1/8)));
            points.Add(new WpfPoint((point.X + Size/2), (point.Y - Size*1/8)));
            points.Add(new WpfPoint((point.X + Size*2/8), (point.Y + Size*1/8)));
            points.Add(new WpfPoint((point.X + Size*3/8), (point.Y + Size/2)));
            points.Add(new WpfPoint((point.X), (point.Y + Size*2/8)));
            points.Add(new WpfPoint((point.X - Size*3/8), (point.Y + Size/2)));
            points.Add(new WpfPoint((point.X - Size*2/8), (point.Y + Size*1/8)));
            points.Add(new WpfPoint((point.X - Size/2), (point.Y - Size*1/8)));
            points.Add(new WpfPoint((point.X - Size*1/8), (point.Y - Size*1/8)));
            points.Add(start);

            figure.IsClosed = true;
            figure.IsFilled = true;

            pathGeometry.Figures.Add(figure);
        }
    }

    public class TrianglePath : BasePointToPathGeometryFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public TrianglePath() { }

        public TrianglePath(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfPathGeometry pathGeometry)
        {
            var start = new WpfPoint(point.X, (point.Y - Size / 2));
            var figure = new WpfPathFigure {StartPoint = start};
            var segment = new WpfPolyLineSegment();
            var points = segment.Points;
            points.Add(new WpfPoint((point.X + Size/2), (point.Y + Size/2)));
            points.Add(new WpfPoint((point.X - Size/2), (point.Y + Size/2)));
            points.Add(new WpfPoint((point.X), (point.Y - Size/2)));
            points.Add(start);

            figure.IsFilled = true;
            figure.IsClosed = true;

            pathGeometry.Figures.Add(figure);
        }
    }

    public class CirclePath : BasePointToPathGeometryFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public CirclePath() { }

        public CirclePath(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfPathGeometry pathGeometry)
        {
            var start = new WpfPoint(point.X, point.Y - Size * 0.5);
            var figure = new WpfPathFigure {StartPoint = start};
            var segment = new WpfArcSegment { Size = new Size(Size, Size), RotationAngle = 360, IsLargeArc = true, SweepDirection = WpfSweepDirection.Clockwise, Point = start };
            figure.Segments.Add(segment);
            figure.IsClosed = true;
            figure.IsFilled = true;
            pathGeometry.Figures.Add(figure);
        }
    }

    public class CrossPath : BasePointToPathGeometryFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public CrossPath() { }

        public CrossPath(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfPathGeometry pathGeometry)
        {
            var x1 = point.X - Size/2f;
            var x2 = point.X - Size/4f;
            var x3 = point.X + Size/4f;
            var x4 = point.X + Size/2f;

            var y1 = point.Y - Size/2f;
            var y2 = point.Y - Size/4f;
            var y3 = point.Y + Size/4f;
            var y4 = point.Y + Size/2f;

            var figure = new WpfPathFigure {StartPoint = new WpfPoint(x2, y1)};
            var segment = new WpfPolyLineSegment();
            var points = segment.Points;
            points.Add(new WpfPoint(x3, y1));
            points.Add(new WpfPoint(x3, y2));
            points.Add(new WpfPoint(x4, y2));
            points.Add(new WpfPoint(x4, y3));
            points.Add(new WpfPoint(x3, y3));
            points.Add(new WpfPoint(x3, y4));
            points.Add(new WpfPoint(x2, y4));
            points.Add(new WpfPoint(x2, y3));
            points.Add(new WpfPoint(x1, y3));
            points.Add(new WpfPoint(x1, y2));
            points.Add(new WpfPoint(x2, y2));
            points.Add(new WpfPoint(x2, y1));

            figure.Segments.Add( segment);
            figure.IsClosed = true;
            figure.IsFilled = true;

            pathGeometry.Figures.Add(figure);
        }
    }

    public class XPath : BasePointToPathGeometryFactory
    {
        ///<summary>
        /// Creates a new factory for points with default size.
        ///</summary>
        public XPath() { }

        public XPath(double size) : base(size) { }

        public override void AddShape(WpfPoint point, WpfPathGeometry pathGeometry)
        {
            var figure = new WpfPathFigure {StartPoint = new WpfPoint((point.X), (point.Y - Size*1/8))};
            var segment = new WpfPolyLineSegment();
            var pts = segment.Points;
            pts.Add(new WpfPoint((point.X + Size*2/8), (point.Y - Size/2)));
            pts.Add(new WpfPoint((point.X + Size/2), (point.Y - Size/2)));
            pts.Add(new WpfPoint((point.X + Size*1/8), (point.Y)));
            pts.Add(new WpfPoint((point.X + Size/2), (point.Y + Size/2)));
            pts.Add(new WpfPoint((point.X + Size*2/8), (point.Y + Size/2)));
            pts.Add(new WpfPoint((point.X), (point.Y + Size*1/8)));
            pts.Add(new WpfPoint((point.X - Size*2/8), (point.Y + Size/2)));
            pts.Add(new WpfPoint((point.X - Size/2), (point.Y + Size/2)));
            pts.Add(new WpfPoint((point.X - Size*1/8), (point.Y)));
            pts.Add(new WpfPoint((point.X - Size/2), (point.Y - Size/2)));
            pts.Add(new WpfPoint((point.X - Size*2/8),(point.Y - Size/2)));
            figure.Segments.Add(segment);
            figure.IsClosed = true;
            figure.IsFilled = true;
            pathGeometry.Figures.Add(figure);
        }
    }
}