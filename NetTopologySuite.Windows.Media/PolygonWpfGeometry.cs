using System.Collections.Generic;
using System.Windows.Media;
using GeoAPI.Geometries;
using WpfGeometry = System.Windows.Media.Geometry;
using WpfStreamGeometry = System.Windows.Media.StreamGeometry;
using WpfPoint = System.Windows.Point;
using WpfRectangle = System.Windows.Rect;
using WpfPen = System.Windows.Media.Pen;
namespace NetTopologySuite.Windows.Media
{
public class PolygonWpfGeometry 
{
    // use a GraphicsPath with a winding rule, since it supports floating point coordinates
    //private GraphicsPath _ringPath;
    
    ///<summary>
    /// Creates a new polygon shape.
    ///</summary>
    /// <param name="shellVertices">The vertices of the shell</param>
    /// <param name="holeVerticesCollection">A collection of ICoordinate[] for each hole</param>
    public PolygonWpfGeometry(ICoordinate[] shellVertices,
        IEnumerable<ICoordinate[]> holeVerticesCollection)
    {
        var path = new WpfStreamGeometry();
        using (var sgc = path.Open())
        {
            AddRing(sgc, shellVertices, true);
            if (holeVerticesCollection != null)
                foreach ( var hole in holeVerticesCollection)
                {
                    AddRing(sgc, hole, false);
                }
        }

        Path = path;
    }

    public PolygonWpfGeometry() 
    {
    }

    public WpfGeometry Path { get; private set; }


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
    /// having the given coordinate sequence to the supplied <see cref="StreamGeometryContext"/>
    ///</summary>
    ///<param name="sgc">The stream geometry context.</param>
    ///<param name="coordinates">A coordinate sequence</param>
    ///<param name="filled">Starting paramter for </param>
    ///<returns>The path for the coordinate sequence</returns>
    private static void AddRing(StreamGeometryContext sgc, ICoordinate[] coordinates, bool filled)
    {
        if (coordinates.Length <= 0) 
            return;
        
        sgc.BeginFigure(ToPoint(coordinates[0]), filled, true);
        if (coordinates.Length > 0)
            sgc.PolyLineTo(ToPoint(coordinates, 1), true, true);
    }

    private static WpfPoint ToPoint(ICoordinate coordinate)
    {
        return new WpfPoint(coordinate.X, coordinate.Y);
    }

    private static IList<WpfPoint> ToPoint(ICoordinate[] coordinates, int start)
    {
        var ret = new List<WpfPoint>(coordinates.Length - start);
        for( var i = start; i < coordinates.Length; i++)
            ret.Add(ToPoint(coordinates[i]));
        return ret;
    }

    private static readonly WpfPen DefaultPen = new WpfPen(Brushes.Black, 1d);

    public WpfRectangle GetBounds()
    {
        return GetBounds(DefaultPen);
    }

    public WpfRectangle GetBounds(Pen pen) {
        return Path.GetRenderBounds(pen);
    }

    public bool Contains(double x, double y) {
      return Path.FillContains(new WpfPoint(x, y));
    }

    public bool Contains(WpfPoint p) {
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
}
}