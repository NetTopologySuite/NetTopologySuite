using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Operation
{
    /// <summary>
    /// The base class for operations that require <c>GeometryGraph</c>s.
    /// </summary>
    public class GeometryGraphOperation
    {
        private LineIntersector li = new RobustLineIntersector();

        protected LineIntersector lineIntersector
        {
            get { return li; }
            set { li = value; }
        }

        protected IPrecisionModel resultPrecisionModel;

        /// <summary>
        /// The operation args into an array so they can be accessed by index.
        /// </summary>
        protected GeometryGraph[] arg;

        public GeometryGraphOperation(IGeometry g0, IGeometry g1)
        {
            // use the most precise model for the result
            if (g0.PrecisionModel.CompareTo(g1.PrecisionModel) >= 0)
            {
                ComputationPrecision = g0.PrecisionModel;
            }
            else
            {
                ComputationPrecision = g1.PrecisionModel;
            }

            arg = new GeometryGraph[2];
            arg[0] = new GeometryGraph(0, g0);
            arg[1] = new GeometryGraph(1, g1);
        }

        public GeometryGraphOperation(IGeometry g0)
        {
            ComputationPrecision = g0.PrecisionModel;

            arg = new GeometryGraph[1];
            arg[0] = new GeometryGraph(0, g0);
        }

        public IGeometry GetArgGeometry(Int32 i)
        {
            return arg[i].Geometry;
        }

        protected IPrecisionModel ComputationPrecision
        {
            get { return resultPrecisionModel; }
            set
            {
                resultPrecisionModel = value;
                lineIntersector.PrecisionModel = resultPrecisionModel;
            }
        }
    }
}