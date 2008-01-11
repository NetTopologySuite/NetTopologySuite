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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private LineIntersector<TCoordinate> _lineIntersector = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector();
        private IPrecisionModel<TCoordinate> _resultPrecisionModel;
        private readonly GeometryGraph<TCoordinate> _arg1;
        private readonly GeometryGraph<TCoordinate> _arg2;

        public GeometryGraphOperation(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
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

            _arg1 = new GeometryGraph<TCoordinate>(0, g0);
            _arg2 = new GeometryGraph<TCoordinate>(1, g1);
        }

        public GeometryGraphOperation(IGeometry<TCoordinate> g0)
        {
            ComputationPrecision = g0.PrecisionModel;

            _arg1 = new GeometryGraph<TCoordinate>(0, g0);
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