using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Triangulate.Quadedge;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate
{
///<summary>
/// A vertex in a Constrained Delaunay Triangulation. The vertex may or may not lie on a constraint.
/// If it does it may carry extra information about the original constraint.
///</summary>
///<typeparam name="TCoordinate"></typeparam>
public class ConstraintVertex<TCoordinate> : Vertex<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
{
    private Boolean _isOnConstraint;
    private Object  _constraint;

    ///<summary>
    /// Creates a new constraint vertex
    ///</summary>
    ///<param name="p"></param>
    public ConstraintVertex(TCoordinate p) 
        :base(p)
    {
    }

    ///<summary>
    /// Gets/Sets whether this vertex lies on a constraint.
    ///</summary>
    public Boolean IsOnConstraint
    {
        get { return _isOnConstraint; }
        set { _isOnConstraint = value; }
    }

    ///<summary>
    /// Gets/Sets the external constraint information
    ///</summary>
    public Object Constraint
    {
        get { return _constraint; }
        set
        {
            IsOnConstraint = true;
            _constraint = value;
        }
    }

    ///<summary>
    /// Merges the constraint data in the vertex <tt>other</tt> into this vertex.
    /// This method is called when an inserted vertex is very close to an existing vertex in the triangulation.
    ///</summary>"/>
    /// <param name="other">the constraint vertex to merge</param>
    internal void Merge(ConstraintVertex<TCoordinate> other) 
    {
        if (other.IsOnConstraint)
        {
            Constraint = other.Constraint;
        }
    }
}}
