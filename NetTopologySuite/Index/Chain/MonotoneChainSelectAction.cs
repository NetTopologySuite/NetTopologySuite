using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Chain
{
    /// <summary>
    /// The action for the internal iterator for performing
    /// envelope select queries on a MonotoneChain.
    /// </summary>
    public class MonotoneChainSelectAction<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private Extents<TCoordinate> _searchExtents1 = new Extents<TCoordinate>();
        private LineSegment<TCoordinate> _selectedSegment = new LineSegment<TCoordinate>();

        /// <summary> 
        /// This function can be overridden if the original chain is needed.
        /// </summary>
        public virtual void Select(MonotoneChain<TCoordinate> mc, Int32 start)
        {
            mc.GetLineSegment(start, ref _selectedSegment);
            Select(SelectedSegment);
        }

        /// <summary>
        /// This is a convenience function which can be overridden to obtain the actual
        /// line segment which is selected.
        /// </summary>
        public virtual void Select(LineSegment<TCoordinate> seg) { }

        public LineSegment<TCoordinate> SelectedSegment
        {
            get { return _selectedSegment; }
        }

        /// <summary>
        /// Gets the <see cref="Extents{TCoordinate}"/> which is
        /// used during the <see cref="MonotoneChain{TCoordinate}"/> search process.
        /// </summary>
        public IExtents<TCoordinate> SearchExtents
        {
            get
            {
                return _searchExtents1;
            }
        }
    }
}