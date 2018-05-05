using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation
{
    /// <summary>
    /// The base class for operations that require <see cref="GeometryGraph"/>s.
    /// </summary>
    public class GeometryGraphOperation
    {        
  
        private LineIntersector _li = new RobustLineIntersector();

        /// <summary>
        /// 
        /// </summary>
        protected LineIntersector lineIntersector
        {
            get
            {
                return _li;
            }
            set
            {
                _li = value;
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        protected IPrecisionModel resultPrecisionModel;

        /// <summary>
        /// The operation args into an array so they can be accessed by index.
        /// </summary>
        protected GeometryGraph[] arg;  

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        public GeometryGraphOperation(IGeometry g0, IGeometry g1)
            :this(g0, g1, BoundaryNodeRules.OgcSfsBoundaryRule /*BoundaryNodeRules.EndpointBoundaryRule*/)
        {}

        public GeometryGraphOperation(IGeometry g0, IGeometry g1, IBoundaryNodeRule boundaryNodeRule)
        {
            // use the most precise model for the result
            if (g0.PrecisionModel.CompareTo(g1.PrecisionModel) >= 0)
                 ComputationPrecision = g0.PrecisionModel;
            else ComputationPrecision = g1.PrecisionModel;

            arg = new GeometryGraph[2];
            arg[0] = new GeometryGraph(0, g0, boundaryNodeRule);
            arg[1] = new GeometryGraph(1, g1, boundaryNodeRule);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        public GeometryGraphOperation(IGeometry g0) 
        {
            ComputationPrecision = g0.PrecisionModel;

            arg = new GeometryGraph[1];
            arg[0] = new GeometryGraph(0, g0);;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IGeometry GetArgGeometry(int i)
        {
            return arg[i].Geometry; 
        }

        /// <summary>
        /// 
        /// </summary>
        protected IPrecisionModel ComputationPrecision
        {
            get
            {
                return resultPrecisionModel;
            }
            set
            {
                resultPrecisionModel = value;
                lineIntersector.PrecisionModel = resultPrecisionModel;
            }
        }
    }
}
