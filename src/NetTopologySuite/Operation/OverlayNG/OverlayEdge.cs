using System;
using System.Collections.Generic;
using NetTopologySuite.EdgeGraph;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNg
{
    internal class OverlayEdge : HalfEdge
    {
  /// <summary>
  /// Creates a single OverlayEdge.
  /// </summary>
  /// <param name="pts"></param>
  /// <param name="lbl"></param>
  /// <param name="direction"></param>
  /// <returns>A new edge based on the given coordinates and direction.</returns>
  public static OverlayEdge createEdge(Coordinate[] pts, OverlayLabel lbl, bool direction)
    {
        Coordinate origin;
        Coordinate dirPt;
        if (direction)
        {
            origin = pts[0];
            dirPt = pts[1];
        }
        else
        {
            int ilast = pts.Length - 1;
            origin = pts[ilast];
            dirPt = pts[ilast - 1];
        }
        return new OverlayEdge(origin, dirPt, direction, lbl, pts);
    }

  /**
     * Gets a {@link Comparator} which sorts by the origin Coordinates.
     * 
     * @return a Comparator sorting by origin coordinate
     */
  public static IComparer<OverlayEdge> nodeComparator => Comparer<OverlayEdge>.Create((e1, e2) => e1.Orig.CompareTo(e2.Orig));

private readonly Coordinate[] _pts;

/**
 * <code>true</code> indicates direction is forward along segString
 * <code>false</code> is reverse direction
 * The label must be interpreted accordingly.
 */
private bool direction;
private Coordinate dirPt;
private OverlayLabel label;

private bool isInResultArea = false;
private bool isInResultLine = false;
private bool isVisited = false;

/**
 * Link to next edge in the result ring.
 * The origin of the edge is the dest of this edge.
 */
private OverlayEdge _nextResultEdge;

private OverlayEdgeRing _edgeRing;

private MaximalEdgeRing _maxEdgeRing;

private OverlayEdge _nextResultMaxEdge;


public OverlayEdge(Coordinate orig, Coordinate dirPt, bool direction, OverlayLabel label, Coordinate[] pts) : base(orig)
{
    this.dirPt = dirPt;
    this.direction = direction;
    this._pts = pts;
    this.label = label;
}

public bool IsForward
{
    get => direction;
}
public Coordinate DirectionPt
{
    get => dirPt;
}

public OverlayLabel Label
{
    get => label;
}

public Location GetLocation(int index, Positions position)
{
    return label.getLocation(index, position, direction);
}

public Coordinate Coordinate
{
    get => Orig;
}

public Coordinate[] Coordinates
{
    get => _pts;
}

public Coordinate[] CoordinatesOriented
{
    get
    {
        if (direction)
        {
            return _pts;
        }

        var copy = (Coordinate[])_pts.Clone();
        CoordinateArrays.Reverse(copy);
        return copy;
    }
}

/**
 * Adds the coordinates of this edge to the given list,
 * in the direction of the edge.
 * Duplicate coordinates are removed
 * (which means that this is safe to use for a path 
 * of connected edges in the topology graph).
 * 
 * @param coords the coordinate list to add to
 */
public void AddCoordinates(CoordinateList coords)
{
    bool isFirstEdge = coords.Count > 0;
    if (direction)
    {
        int startIndex = 1;
        if (isFirstEdge) startIndex = 0;
        for (int i = startIndex; i < _pts.Length; i++)
        {
            coords.Add(_pts[i], false);
        }
    }
    else
    { // is backward
        int startIndex = _pts.Length - 2;
        if (isFirstEdge) startIndex = _pts.Length - 1;
        for (int i = startIndex; i >= 0; i--)
        {
            coords.Add(_pts[i], false);
        }
    }
}

public OverlayEdge SymOE
{
    get => (OverlayEdge)Sym;
}

public OverlayEdge ONextOE
{
    get => (OverlayEdge)ONext;
}

public bool IsInResultArea
{
    get => isInResultArea;
    private set => isInResultArea = value;
}

public bool IsInResultAreaBoth
{
    get => IsInResultArea && SymOE.IsInResultArea;
}

public void unmarkFromResultAreaBoth()
{
    IsInResultArea = false;
    SymOE.IsInResultArea = false;
}

public void markInResultArea()
{
    IsInResultArea = true;
}

public void markInResultAreaBoth()
{
    isInResultArea = true;
    SymOE.isInResultArea = true;
}

public bool IsInResultLine
{
    get => isInResultLine;
    private set => isInResultLine = value;
}

public void markInResultLine()
{
    isInResultLine = true;
    SymOE.IsInResultLine = true;
}

public bool IsInResult
{
    get => IsInResultArea || IsInResultLine;
}

[Obsolete("Use NextResult property")]
public OverlayEdge ResultNext
{
    get => NextResult;
    set => NextResult = value;
}
[Obsolete("Use NextResult property")]
public void setResultNext(OverlayEdge e)
{
    // Assert: e.orig() == this.dest();
    _nextResultEdge = e;
}
[Obsolete("Use NextResult property")]
public OverlayEdge nextResult()
{
    return _nextResultEdge;
}


        public OverlayEdge NextResult
{
    get => _nextResultEdge;
    set => _nextResultEdge = value;
}

        [Obsolete("Use NextResult property")]

public bool IsResultLinked
{
    get => _nextResultEdge != null;
}

[Obsolete("Use NextResultMax property")]
internal void setResultNextMax(OverlayEdge e)
{
    // Assert: e.orig() == this.dest();
    _nextResultMaxEdge = e;
}

public OverlayEdge NextResultMax
{
    get => _nextResultMaxEdge;
    set => _nextResultMaxEdge = value;
}

public bool IsResultMaxLinked
{
    get => _nextResultMaxEdge != null;
}

public bool IsVisited
{
    get => isVisited;
    set => isVisited = value;
}

[Obsolete("Use IsVisited property")]
private void MarkVisited()
{
    IsVisited = true;
}

public void markVisitedBoth()
{
    MarkVisited();
    SymOE.MarkVisited();
    IsVisited = true;
    SymOE.isVisited = true;
}

public OverlayEdgeRing EdgeRing
{
    get => _edgeRing;
    set => _edgeRing = value;
}

[Obsolete("Use EdgeRing property")]
public void setEdgeRing(OverlayEdgeRing edgeRing)
{
    EdgeRing = edgeRing;
}

[Obsolete("Use EdgeRing property")]
public OverlayEdgeRing getEdgeRing()
{
    return EdgeRing;
}

public MaximalEdgeRing getEdgeRingMax()
{
    return _maxEdgeRing;
}

public void setEdgeRingMax(MaximalEdgeRing maximalEdgeRing)
{
    _maxEdgeRing = maximalEdgeRing;
}

public override string ToString()
{
    var orig = Orig;
    var dest = Dest;
    string dirPtStr = (_pts.Length > 2)
        ? ", " + WKTWriter.Format(DirectionPt)
            : "";

    return "OE( " + WKTWriter.Format(orig)
        + dirPtStr
        + " .. " + WKTWriter.Format(dest)
        + " ) "
        + label.ToString(direction)
        + ResultSymbol
        + " / Sym: " + SymOE.Label.ToString(SymOE.direction)
        + SymOE.ResultSymbol
        ;
}

private string ResultSymbol
{
    get
    {
        if (isInResultArea) return " resA";
        if (isInResultLine) return " resL";
        return "";
    }
}




}
}
