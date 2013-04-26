using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;

namespace NetTopologySuite.Windows.Forms
{
public class PolygonGraphicsPath 
{
    // use a GraphicsPath with a winding rule, since it supports floating point coordinates
    //private GraphicsPath _ringPath;
    
    ///<summary>
    /// Creates a new polygon shape.
    ///</summary>
    /// <param name="shellVertices">The vertices of the shell</param>
    /// <param name="holeVerticesCollection">A collection of ICoordinate[] for each hole</param>
    public PolygonGraphicsPath(Coordinate[] shellVertices,
        IEnumerable<Coordinate[]> holeVerticesCollection) 
    {
        Path = ToPath(shellVertices);

        foreach (var coordinates in holeVerticesCollection)
            Path.AddPath(ToPath(coordinates), false);
    }

    public PolygonGraphicsPath() 
    {
    }

    public GraphicsPath Path { get; private set; }


    private PointF _lastPoint;
    internal void AddToRing(PointF p, ref GraphicsPath ringPath)
    {
    	if (ringPath == null)
        {
    		ringPath = new GraphicsPath(FillMode.Alternate);
            ringPath.StartFigure();
    	}
    	else
        {
    		ringPath.AddLine(_lastPoint, p);
    	}
        _lastPoint = p;
    }
    
    internal void EndRing(GraphicsPath ringPath)
    {
        ringPath.CloseFigure();
        if (Path == null)
            Path = ringPath;
    	else
    		Path.AddPath(ringPath, false);
    }
    
    ///<summary>
    /// Creates a <see cref="GraphicsPath"/> representing a polygon ring
    /// having the given coordinate sequence.
    ///</summary>
    /// <remarks>
    /// Uses <see cref="FillMode.Alternate"/> winding rule
    /// </remarks>
    /// <param name="coordinates">A coordinate sequence</param>
    /// <returns>The path for the coordinate sequence</returns>
    private static GraphicsPath ToPath(Coordinate[] coordinates)
    {
        var path = new GraphicsPath(FillMode.Alternate);

        path.StartFigure();
        if (coordinates.Length > 0)
            path.AddLines(ToPointF(coordinates));
        path.CloseFigure();

        return path;
    }

    private static PointF[] ToPointF(Coordinate[] coordinates)
    {
        var ret = new PointF[coordinates.Length];
        for( var i = 0; i < coordinates.Length; i++)
            ret[i] = new PointF((float)coordinates[i].X, (float)coordinates[i].Y);
        return ret;
    }

    public Rectangle GetBounds()
    {
        var bounds = GetBoundsF();
        return new Rectangle((int) bounds.Left, (int) bounds.Right, (int) bounds.Width, (int) bounds.Height);
    }

    public RectangleF GetBoundsF() {
        return Path.GetBounds();
    }

    /*
    public Boolean Contains(double x, double y) {
      return polygonPath.Contains(x, y);
    }

    public Boolean Contains(PointF p) {
      return polygonPath.contains(p);
    }

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
    public GraphicsPathIterator GetPathIterator(Matrix at)
    {
        return GetPathIterator(at, 0.25);
    }

    public GraphicsPathIterator GetPathIterator(Matrix at, double flatness)
    {
        var p = (GraphicsPath) Path.Clone();
        p.Flatten(at, (float)flatness);
        return new GraphicsPathIterator(p);
    }
}
}