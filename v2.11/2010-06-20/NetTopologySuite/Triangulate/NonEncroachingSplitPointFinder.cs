using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate
{
///<summary>
/// A strategy for finding constraint split points which attempts to maximise the length of the split
/// segments while preventing further encroachment. (This is not always possible for narrow angles).
///</summary>
///<typeparam name="TCoordinate"></typeparam>
public class NonEncroachingSplitPointFinder<TCoordinate> : IConstraintSplitPointFinder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
{

    private readonly ICoordinateFactory<TCoordinate> _coordinateFactory;

    ///<summary>
    /// creates an instance of this class
    ///</summary>
    ///<param name="coordinateFactory"></param>
    public NonEncroachingSplitPointFinder(ICoordinateFactory<TCoordinate> coordinateFactory)
    {
        _coordinateFactory = coordinateFactory;
    }

    ///<summary>
    /// A basic strategy for finding split points when nothing extra is known about the geometry of the situation.
    ///</summary>
    ///<param name="seg">the encroached segment</param>
    ///<param name="encroachPt">the encroaching point</param>
    ///<returns>the point at which to split the encroached segment</returns>
    public TCoordinate FindSplitPoint(Segment<TCoordinate> seg, TCoordinate encroachPt)
    {
        LineSegment<TCoordinate> lineSeg = seg.LineSegment;
        Double segLen = lineSeg.Length;
        Double midPtLen = segLen * 0.5d;
        SplitSegment<TCoordinate> splitSeg = new SplitSegment<TCoordinate>(lineSeg);

        TCoordinate projPt = ProjectedSplitPoint(seg, encroachPt, _coordinateFactory);
        /**
         * Compute the largest diameter (length) that will produce a split segment which is not
         * still encroached upon by the encroaching point (The length is reduced slightly by a
         * safety factor)
         */
        double nonEncroachDiam = projPt.Distance(encroachPt) * 2 * 0.8; // .99;
        double maxSplitLen = nonEncroachDiam;
        if (maxSplitLen > midPtLen)
        {
            maxSplitLen = midPtLen;
        }
        splitSeg.MinimumLength = maxSplitLen;

        splitSeg.SplitAt(_coordinateFactory, projPt);

        return splitSeg.SplitPoint;
    }

    ///<summary>
    /// Computes a split point which is the projection of the encroaching point on the segment
    ///</summary>
    ///<param name="seg"></param>
    ///<param name="encroachPt"></param>
    ///<param name="factory"></param>
    ///<returns></returns>
    static public TCoordinate ProjectedSplitPoint(Segment<TCoordinate> seg, TCoordinate encroachPt, ICoordinateFactory<TCoordinate> factory) 
    {
        LineSegment<TCoordinate> lineSeg = seg.LineSegment;
        TCoordinate projPt = lineSeg.Project(encroachPt, factory);
        return projPt;
    }
}
}
