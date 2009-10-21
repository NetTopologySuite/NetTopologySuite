using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate.Quadedge
{
    ///<summary>
    /// An interface for algorithms which process the triangles in a <see cref="QuadEdgeSubdivision{TCoordinate}"/>
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    ///<typeparam name="TData"></typeparam>
    public interface ITriangleVisitor<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        ///<summary>
        /// Visits the <see cref="QuadEdge{TCoordinate, TData}"/>s of a triangle.
        ///</summary>
        ///<param name="coordFactory">the factory to create coordinates</param>
        ///<param name="triEdges">an array of the 3 quad edges in a triangle (in CCW order)</param>
        void Visit(ICoordinateFactory<TCoordinate> coordFactory, QuadEdge<TCoordinate>[] triEdges);
    }
}
