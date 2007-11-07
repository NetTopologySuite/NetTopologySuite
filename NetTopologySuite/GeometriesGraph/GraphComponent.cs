using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
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
        protected Label label;

        // isInResult indicates if this component has already been included in the result
        private Boolean isInResult = false;

        private Boolean isCovered = false;
        private Boolean isCoveredSet = false;
        private Boolean isVisited = false;

        /// <summary>
        /// 
        /// </summary>
        public GraphComponent() {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        public GraphComponent(Label label)
        {
            this.label = label;
        }

        /// <summary>
        /// 
        /// </summary>
        public Label Label
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean InResult
        {
            get { return isInResult; }
            set { isInResult = value; }
        }

        /// <summary> 
        /// IsInResult indicates if this component has already been included in the result.
        /// </summary>
        public Boolean IsInResult
        {
            get { return InResult; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean Covered
        {
            get { return isCovered; }
            set
            {
                isCovered = value;
                isCoveredSet = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsCovered
        {
            get { return Covered; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsCoveredSet
        {
            get { return isCoveredSet; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean Visited
        {
            get { return isVisited; }
            set { isVisited = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsVisited
        {
            get { return isVisited; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// A coordinate in this component (or null, if there are none).
        /// </returns>
        public abstract ICoordinate Coordinate { get; }

        /// <summary>
        /// Compute the contribution to an IM for this component.
        /// </summary>
        public abstract void ComputeIM(IntersectionMatrix im);

        /// <summary>
        /// An isolated component is one that does not intersect or touch any other
        /// component.  This is the case if the label has valid locations for
        /// only a single Geometry.
        /// </summary>
        /// <returns><c>true</c> if this component is isolated.</returns>
        public abstract Boolean IsIsolated { get; }

        /// <summary>
        /// Update the IM with the contribution for this component.
        /// A component only contributes if it has a labelling for both parent geometries.
        /// </summary>
        /// <param name="im"></param>
        public void UpdateIM(IntersectionMatrix im)
        {
            Assert.IsTrue(label.GeometryCount >= 2, "found partial label");
            ComputeIM(im);
        }
    }
}