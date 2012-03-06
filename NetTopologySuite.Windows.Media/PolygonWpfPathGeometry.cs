using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GeoAPI.Geometries;

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
#if SILVERLIGHT
        var figure = new PathFigure();
        figure.StartPoint = new Point(coordinates[0].X, coordinates[0].Y);
        figure.Segments.Add(ToPolyLineSegment(coordinates));
#else
        var figure = new PathFigure(ToPoint(coordinates[0]), ToPathSegments(coordinates), true);
#endif
        pathGeometry.Figures.Add(figure);
    }

#if !SILVERLIGHT
    private static IEnumerable<PathSegment> ToPathSegments(Coordinate[] coordinates)
    {
        for (var i = 1; i < coordinates.Length; i++)
            yield return new LineSegment(ToPoint(coordinates[i]), true);
    }
#else
    private static PolyLineSegment ToPolyLineSegment(IEnumerable<Coordinate> coordinates)
    {
        var res = new PolyLineSegment();
        var pts = res.Points;
        foreach (var coordinate in coordinates.Skip(1))
        {
            pts.Add(new Point(coordinate.X, coordinate.Y));
        }
        return res;
    }
#endif
    private static Point ToPoint(Coordinate coordinate)
    {
        return new Point(coordinate.X, coordinate.Y);
    }

#if !SILVERLIGHT
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

    /*
        public Boolean Intersects(double x, double y, double w, double h) {
          return polygonPath.intersects(x, y, w, h);
        }

        public Boolean Intersects(RectangleF r) {
          return polygonPath.intersects(r);
        }

        public Boolean Contains(double x, double y, double w, double h) {
          return polygonPath.contains(x, y, w, h);
        }

        public Boolean Contains(Rectangle2D r) {
          return polygonPath.Contains(r);
        }
        */
    //public GraphicsPathIterator GetPathIterator(Matrix at)
    //{
    //    return GetPathIterator(at, 0.25);
    //}

    //public GraphicsPathIterator GetPathIterator(Matrix at, double flatness)
    //{
    //    var p = (GraphicsPath) Path.Clone();
    //    p.Flatten(at, (float)flatness);
    //    return new GraphicsPathIterator(p);
    //}
#endif
}