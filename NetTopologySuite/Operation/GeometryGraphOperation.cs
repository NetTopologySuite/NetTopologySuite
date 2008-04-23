using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation
{
    /// <summary>
    /// The base class for operations that require <see cref="GeometryGraph{TCoordinate}"/>s.
    /// </summary>
    public class GeometryGraphOperation<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private LineIntersector<TCoordinate> _lineIntersector;
        private IPrecisionModel<TCoordinate> _resultPrecisionModel;
        private readonly GeometryGraph<TCoordinate> _arg1;
        private readonly GeometryGraph<TCoordinate> _arg2;

        public GeometryGraphOperation(IGeometry<TCoordinate> g0, 
                                      IGeometry<TCoordinate> g1)
            : this(g0, g1, new Mod2BoundaryNodeRule()) { }

        public GeometryGraphOperation(IGeometry<TCoordinate> g0, 
                                      IGeometry<TCoordinate> g1, 
                                      IBoundaryNodeRule boundaryNodeRule)
        {
            if (g0 == null) throw new ArgumentNullException("g0");
            if (g1 == null) throw new ArgumentNullException("g1");

            _lineIntersector = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(
                                    g0.Factory);

            // use the most precise model for the result
            if (g0.PrecisionModel.CompareTo(g1.PrecisionModel) >= 0)
            {
                ComputationPrecision = g0.PrecisionModel;
            }
            else
            {
                ComputationPrecision = g1.PrecisionModel;
            }

            _arg1 = new GeometryGraph<TCoordinate>(0, g0, boundaryNodeRule);
            _arg2 = new GeometryGraph<TCoordinate>(1, g1, boundaryNodeRule);
        }
        
        public GeometryGraphOperation(IGeometry<TCoordinate> g0)
        {
            if (g0 == null) throw new ArgumentNullException("g0");

            ComputationPrecision = g0.PrecisionModel;

            _arg1 = new GeometryGraph<TCoordinate>(0, g0);
            /*
            * Use factory of primary point.
            * Note that this does NOT handle mixed-precision arguments
            * where the second arg has greater precision than the first.
            */
            _lineIntersector = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(
                                    g0.Factory);
        }

        public IGeometry<TCoordinate> GetArgumentGeometry(Int32 i)
        {
            if (i == 0)
            {
                return _arg1.Geometry;
            }
            else if (i == 1)
            {
                return _arg2.Geometry;
            }

            return null;
        }

        protected LineIntersector<TCoordinate> LineIntersector
        {
            get { return _lineIntersector; }
            set { _lineIntersector = value; }
        }

        protected GeometryGraph<TCoordinate> Argument1
        {
            get { return _arg1; }
        }

        protected GeometryGraph<TCoordinate> Argument2
        {
            get { return _arg2; }
        }

        protected IPrecisionModel<TCoordinate> ComputationPrecision
        {
            get { return _resultPrecisionModel; }
            set
            {
                _resultPrecisionModel = value;
                LineIntersector.PrecisionModel = _resultPrecisionModel;
            }
        }
    }
}