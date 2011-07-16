using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.Triangulate
{
    ///<summary>
    /// An interface for factories which create a <see cref="ConstraintVertex{TCoordinate}"/>
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public interface IConstraintVertexFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        ConstraintVertex<TCoordinate> CreateVertex(TCoordinate p, Segment<TCoordinate> constraintSeg);
    }
}
