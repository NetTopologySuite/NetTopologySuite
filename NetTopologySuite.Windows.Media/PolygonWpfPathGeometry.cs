using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GeoAPI.Geometries;

namespace NetTopologySuite.Windows.Media
{
    public class PolygonWpfPathGeometry
    {
        ///<summary>
        /// Creates a new polygon shape.
        ///</summary>
        /// <param name="shellVertices">The vertices of the shell</param>
        /// <param name="holeVerticesCollection">A collection of Coordinate[] for each hole</param>
        public PolygonWpfPathGeometry(Coordinate[] shellVertices,
                                      IEnumerable<Coordinate[]> holeVerticesCollection)
        {
            var path = new PathGeometry();
            AddRing(path, shellVertices);
            if (holeVerticesCollection != null)
            {
                foreach (var hole in holeVerticesCollection)
                    AddRing(path, hole);
            }
            Path = path;
        }

        public PolygonWpfPathGeometry()
        {
        }

        public PathGeometry Path { get; private set; }

        //private WpfPoint _lastPoint;
        //internal void AddToRing(WpfPoint p, ref GraphicsPath ringPath)
        //{
        //    if (ringPath == null)
        //    {
        //        ringPath = new GraphicsPath(FillMode.Alternate);
        //        ringPath.StartFigure();
        //    }
        //    else
        //    {
        //        ringPath.AddLine(_lastPoint, p);
        //    }
        //    _lastPoint = p;
        //}

        //internal void EndRing(GraphicsPath ringPath)
        //{
        //    ringPath.CloseFigure();
        //    if (Path == null)
        //        Path = ringPath;
        //    else
        //        Path.AddPath(ringPath, false);
        //}

        ///<summary>
        /// Adds a <see cref="PathFigure"/> representing a polygon ring
        /// having the given coordinate sequence to the supplied <see cref="pathGeometry"/>
        ///</summary>
        ///<param name="pathGeometry">The path geometry.</param>
        ///<param name="coordinates">A coordinate sequence</param>
        ///<returns>The path for the coordinate sequence</returns>
        private static void AddRing(PathGeometry pathGeometry, Coordinate[] coordinates)
        {
            if (coordinates.Length <= 0)
                return;
            var figure = new PathFigure(ToPoint(coordinates[0]), ToPathSegments(coordinates), true);
            pathGeometry.Figures.Add(figure);
        }

        private static IEnumerable<PathSegment> ToPathSegments(Coordinate[] coordinates)
        {
            for (var i = 1; i < coordinates.Length; i++)
                yield return new LineSegment(ToPoint(coordinates[i]), true);
        }

        private static Point ToPoint(Coordinate coordinate)
        {
            return new Point(coordinate.X, coordinate.Y);
        }

        private static readonly Pen DefaultPen = new Pen(Brushes.Black, 1d);

        public Rect GetBounds()
        {
            return GetBounds(DefaultPen);
        }

        public Rect GetBounds(Pen pen)
        {
            return Path.GetRenderBounds(pen);
        }

        public bool Contains(double x, double y)
        {
            return Path.FillContains(new Point(x, y));
        }

        public bool Contains(Point p)
        {
            return Path.FillContains(p);
        }       
    }
}