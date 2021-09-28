using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A GraphComponent is the parent class for the objects'
    /// that form a graph.  Each GraphComponent can carry a
    /// Label.
    /// </summary>
    public abstract class GraphComponent
    {
        /// <summary>
        ///
        /// </summary>
        private Label _label;

        // isInResult indicates if this component has already been included in the result

        private bool _isCovered;
        private bool _isCoveredSet;
        private bool _isVisited;

        /// <summary>
        ///
        /// </summary>
        protected GraphComponent() { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="label"></param>
        protected GraphComponent(Label label)
        {
            _label = label;
        }

        /// <summary>
        ///
        /// </summary>
        public Label Label
        {
            get => _label;
            protected internal set => _label = value;
        }

        /// <summary>
        ///
        /// </summary>
        public bool InResult { get; set; }

        /// <summary>
        /// IsInResult indicates if this component has already been included in the result.
        /// </summary>
        public bool IsInResult => InResult;

        /// <summary>
        ///
        /// </summary>
        public bool Covered
        {
            get => _isCovered;
            set
            {
                _isCovered = value;
                _isCoveredSet = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsCovered => Covered;

        /// <summary>
        ///
        /// </summary>
        public bool IsCoveredSet => _isCoveredSet;

        /// <summary>
        ///
        /// </summary>
        public bool Visited
        {
            get => _isVisited;
            set => _isVisited = value;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsVisited => _isVisited;

        /// <summary>
        ///
        /// </summary>
        /// <returns>
        /// A coordinate in this component (or null, if there are none).
        /// </returns>
        public abstract Coordinate Coordinate { get; protected set; }

        /// <summary>
        /// Compute the contribution to an IM for this component.
        /// </summary>
        /// <param name="im">An <c>IntersectionMatrix</c></param>
        public abstract void ComputeIM(IntersectionMatrix im);

        /// <summary>
        /// An isolated component is one that does not intersect or touch any other
        /// component.  This is the case if the label has valid locations for
        /// only a single Geometry.
        /// </summary>
        /// <returns><c>true</c> if this component is isolated.</returns>
        public abstract bool IsIsolated { get; }

        /// <summary>
        /// Update the IM with the contribution for this component.
        /// A component only contributes if it has a labelling for both parent geometries.
        /// </summary>
        /// <param name="im">An <c>IntersectionMatrix</c></param>
        public void UpdateIM(IntersectionMatrix im)
        {
            Assert.IsTrue(_label.GeometryCount >= 2, "found partial label");
            ComputeIM(im);
        }
    }
}
