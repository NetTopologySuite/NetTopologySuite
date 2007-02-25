using System;
using System.Collections;
using System.Text;

using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Algorithm;

namespace GisSharpBlog.NetTopologySuite.Operation
{
    /// <summary>
    /// The base class for operations that require <c>GeometryGraph</c>s.
    /// </summary>
    public class GeometryGraphOperation
    {        
  
        private LineIntersector li = new RobustLineIntersector();

        /// <summary>
        /// 
        /// </summary>
        protected LineIntersector lineIntersector
        {
            get
            {
                return li;
            }
            set
            {
                li = value;
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        protected PrecisionModel resultPrecisionModel;

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
        {
            Geometry geom0 = (Geometry) g0;
            Geometry geom1 = (Geometry) g1;

            // use the most precise model for the result
            if (geom0.PrecisionModel.CompareTo(geom1.PrecisionModel) >= 0)
                ComputationPrecision = geom0.PrecisionModel;
            else ComputationPrecision = geom1.PrecisionModel;

            arg = new GeometryGraph[2];
            arg[0] = new GeometryGraph(0, g0);
            arg[1] = new GeometryGraph(1, g1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        public GeometryGraphOperation(IGeometry g0) 
        {
            Geometry geom0 = (Geometry) g0;
            ComputationPrecision = geom0.PrecisionModel;

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
        protected PrecisionModel ComputationPrecision
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
