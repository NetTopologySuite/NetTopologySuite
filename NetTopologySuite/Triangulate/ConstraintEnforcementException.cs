using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate
{
///<summary>
/// Indicates a failure during constraint enforcement.
///</summary>
///<typeparam name="TCoordinate"></typeparam>
public class ConstraintEnforcementException<TCoordinate>    : Exception
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
 {
    private static String MsgWithCoord(String msg, TCoordinate pt) {
        if (Equals(pt, null))
            return msg + " [ " + pt + " ]";
        return msg;
    }

    /// <summary>
    /// The approximate location of this error.
    /// </summary>
    public readonly TCoordinate Coordinate;

    ///<summary>
    /// Creates a new instance with a given message.
    ///</summary>
    ///<param name="msg"> a string</param>
    public ConstraintEnforcementException(String msg)
        :base(msg)
    {
    }

    ///<summary>
    /// Creates a new instance with a given message and approximate location.
    ///</summary>
    ///<param name="msg">a string</param>
    ///<param name="pt">the location of the error</param>
    public ConstraintEnforcementException(String msg, TCoordinate pt)
        :base(MsgWithCoord(msg, pt))
    {
        Coordinate = pt.Clone();
    }

    }
}
