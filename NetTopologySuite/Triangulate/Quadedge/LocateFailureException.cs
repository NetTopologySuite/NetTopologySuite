using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate.Quadedge
{
///<summary>
///</summary>
///<typeparam name="TCoordinate"></typeparam>
public class LocateFailureException<TCoordinate> : Exception
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
{
	private static String MsgWithSpatial(String msg, LineSegment<TCoordinate> seg)
    {
		return msg + " [ " + seg + " ]";
	}

	private readonly LineSegment<TCoordinate> _seg;

	public LocateFailureException(String msg)
        :base(msg)
    {
	}

	public LocateFailureException(String msg, LineSegment<TCoordinate> seg)
        : base(MsgWithSpatial(msg, seg))
    {
		_seg = new LineSegment<TCoordinate>(seg);
	}

	public LocateFailureException(LineSegment<TCoordinate> seg)
        :base(string.Format("Locate failed to converge (at edge: {0}).\nPossible causes include invalid Subdivision topology or very close sites", seg))
    {
		_seg = new LineSegment<TCoordinate>(seg);
	}

	public LineSegment<TCoordinate> Segment
    {
        get { return _seg; }
    }

}
}
