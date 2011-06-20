using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate.Quadedge
{
    ///<summary>
    /// An interface for classes which locate an edge in a <see cref="QuadEdgeSubdivision{TCoordinate}"/>
    /// which either contains a given <see cref="Vertex{TCoordinate}"/> 
    /// or is an edge of a triangle which contains V. 
    /// Implementors may utilized different strategies for
    /// optimizing locating containing edges/triangles.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public interface IQuadEdgeLocator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        ///<summary>
        /// Finds a quadedge of a triangle containing a location specified by a <see cref="Vertex{TCoordinate}"/>, if one exists.
        ///</summary>
        ///<param name="v">the vertex to locate</param>
        ///<returns>a quadedge on the edge of a triangle which touches or contains the location </returns>
        ///<returns>null if no such triangle exists</returns>
        QuadEdge<TCoordinate> Locate(Vertex<TCoordinate> v);
    }
}
