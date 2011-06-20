using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate.Quadedge
{
    ///<summary>
    /// Locates <see cref="QuadEdge{TCoordinate, TData}"/>s in a <see cref="QuadEdgeSubdivision{TCoordinate}"/>,
    /// optimizing the search by starting in the locality of the last edge found.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class LastFoundQuadEdgeLocator<TCoordinate> : IQuadEdgeLocator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly QuadEdgeSubdivision<TCoordinate> _subdiv;
        private QuadEdge<TCoordinate> _lastEdge;

        ///<summary>
        /// Creates a new <see cref="LastFoundQuadEdgeLocator{TCoordinate, TData}"/>.
        ///</summary>
        ///<param name="subdiv"></param>
        public LastFoundQuadEdgeLocator(QuadEdgeSubdivision<TCoordinate> subdiv)
        {
            _subdiv = subdiv;
            Init();
        }

        private void Init()
        {
            _lastEdge = FindEdge();
        }

        private QuadEdge<TCoordinate> FindEdge()
        {
            // assume there is an edge - otherwise will get an exception
            return Slice.GetFirst(_subdiv.Edges);
        }


        ///<summary>
        /// Locates an edge e, such that either v is on e, or e is an edge of a triangle containing v.
        /// The search starts from the last located edge amd proceeds on the general direction of v.
        ///</summary>
        ///<param name="v"></param>
        ///<returns></returns>
        public QuadEdge<TCoordinate> Locate(Vertex<TCoordinate> v)
        {
            if (!_lastEdge.IsLive)
                Init();

            QuadEdge<TCoordinate> e = _subdiv.LocateFromEdge(v, _lastEdge);
            _lastEdge = e;
            return e;
        }
    }
}
