using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A GraphComponent is the parent class for the objects'
    /// that form a graph.  Each GraphComponent can carry a
    /// Label.
    /// </summary>
    public abstract class GraphComponent<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private Label? _label;

        // isInResult indicates if this component has already been included in the result
        private Boolean _isInResult = false;

        private Boolean _isCovered = false;
        private Boolean _isCoveredSet = false;
        private Boolean _isVisited = false;

        public GraphComponent() {}

        public GraphComponent(Label label)
        {
            _label = label;
        }

        public Label? Label
        {
            get { return _label; }
            set { _label = value; }
        }

        public Boolean InResult
        {
            get { return _isInResult; }
            set { _isInResult = value; }
        }

        /// <summary> 
        /// Indicates if this component has already been included 
        /// in the result.
        /// </summary>
        public Boolean IsInResult
        {
            get { return InResult; }
        }

        public Boolean Covered
        {
            get { return _isCovered; }
            set
            {
                _isCovered = value;
                _isCoveredSet = true;
            }
        }

        public Boolean IsCovered
        {
            get { return Covered; }
        }

        public Boolean IsCoveredSet
        {
            get { return _isCoveredSet; }
        }

        public Boolean Visited
        {
            get { return _isVisited; }
            set { _isVisited = value; }
        }

        public Boolean IsVisited
        {
            get { return _isVisited; }
        }

        /// <returns>
        /// A coordinate in this component (or null, if there are none).
        /// </returns>
        public abstract TCoordinate Coordinate { get; }

        /// <summary>
        /// Compute the contribution to an IM for this component.
        /// </summary>
        public abstract void ComputeIntersectionMatrix(IntersectionMatrix im);

        /// <summary>
        /// An isolated component is one that does not intersect or touch any other
        /// component.  This is the case if the label has valid locations for
        /// only a single Geometry.
        /// </summary>
        /// <returns><see langword="true"/> if this component is isolated.</returns>
        public abstract Boolean IsIsolated { get; }

        /// <summary>
        /// Update the IM with the contribution for this component.
        /// A component only contributes if it has a labeling for both 
        /// parent geometries.
        /// </summary>
        public void UpdateIntersectionMatrix(IntersectionMatrix im)
        {
            Debug.Assert(_label.HasValue);
            Assert.IsTrue(_label.Value.GeometryCount >= 2, "found partial label");
            ComputeIntersectionMatrix(im);
        }
    }
}