using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    ///     A GraphComponent is the parent class for the objects'
    ///     that form a graph.  Each GraphComponent can carry a
    ///     Label.
    /// </summary>
    public abstract class GraphComponent
    {
        // isInResult indicates if this component has already been included in the result

        private bool _isCovered;

        /// <summary>
        /// </summary>
        private Label _label;

        /// <summary>
        /// </summary>
        protected GraphComponent()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="label"></param>
        protected GraphComponent(Label label)
        {
            _label = label;
        }

        /// <summary>
        /// </summary>
        public Label Label
        {
            get { return _label; }
            protected internal set { _label = value; }
        }

        /// <summary>
        /// </summary>
        public bool InResult { get; set; }

        /// <summary>
        ///     IsInResult indicates if this component has already been included in the result.
        /// </summary>
        public bool IsInResult => InResult;

        /// <summary>
        /// </summary>
        public bool Covered
        {
            get { return _isCovered; }
            set
            {
                _isCovered = value;
                IsCoveredSet = true;
            }
        }

        /// <summary>
        /// </summary>
        public bool IsCovered => Covered;

        /// <summary>
        /// </summary>
        public bool IsCoveredSet { get; private set; }

        /// <summary>
        /// </summary>
        public bool Visited { get; set; }

        /// <summary>
        /// </summary>
        public bool IsVisited => Visited;

        /// <summary>
        /// </summary>
        /// <returns>
        ///     A coordinate in this component (or null, if there are none).
        /// </returns>
        public abstract Coordinate Coordinate { get; protected set; }

        /// <summary>
        ///     An isolated component is one that does not intersect or touch any other
        ///     component.  This is the case if the label has valid locations for
        ///     only a single Geometry.
        /// </summary>
        /// <returns><c>true</c> if this component is isolated.</returns>
        public abstract bool IsIsolated { get; }

        /// <summary>
        ///     Compute the contribution to an IM for this component.
        /// </summary>
        public abstract void ComputeIM(IntersectionMatrix im);

        /// <summary>
        ///     Update the IM with the contribution for this component.
        ///     A component only contributes if it has a labelling for both parent geometries.
        /// </summary>
        /// <param name="im"></param>
        public void UpdateIM(IntersectionMatrix im)
        {
            Assert.IsTrue(_label.GeometryCount >= 2, "found partial label");
            ComputeIM(im);
        }
    }
}