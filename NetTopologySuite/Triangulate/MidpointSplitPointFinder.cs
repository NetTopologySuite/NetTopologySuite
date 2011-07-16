using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.Triangulate
{

///<summary>
///A simple split point finder which returns the midpoint of the split segment. This is a default
///strategy only. Usually a more sophisticated strategy is required to prevent repeated splitting.
///Other points which could be used are:
///<list>
///<item>The projection of the encroaching point on the segment</item>
///<item>A point on the segment which will produce two segments which will not be further encroached</item>
///<item>The point on the segment which is the same distance from an endpoint as the encroaching</item>
///point
///</ul>
///</summary>
public class MidpointSplitPointFinder<TCoordinate> : IConstraintSplitPointFinder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
{
    private readonly ICoordinateFactory<TCoordinate> _coordinateFactory;

    ///<summary>
    /// Creates an instance of this class
    ///</summary>
    ///<param name="coordinateFactory">the factory to create new MidpointSplitPoints</param>
    public MidpointSplitPointFinder(ICoordinateFactory<TCoordinate> coordinateFactory)
    {
        _coordinateFactory = coordinateFactory;
    }

    ///<summary>Gets the midpoint of the split segment
    ///</summary>
    ///<param name="seg">the encroached segment</param>
    ///<param name="encroachPt">the encroaching point</param>
    ///<returns>the point at which to split the encroached segment</returns>
    TCoordinate IConstraintSplitPointFinder<TCoordinate>.FindSplitPoint(Segment<TCoordinate> seg, TCoordinate encroachPt) {
        TCoordinate p0 = seg.Start;
        TCoordinate p1 = seg.End;
        return _coordinateFactory.Create((p0[Ordinates.X] + p1[Ordinates.X]) / 2, (p0[Ordinates.Y] + p1[Ordinates.Y]) / 2);
    }

}
}
