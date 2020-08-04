using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Extracts atomic elements from
    /// input geometries or collections,
    /// recording the dimension found.
    /// Empty geometries are discarded since they
    /// do not contribute to the result of <see cref="UnaryUnionOp"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    class InputExtracter : IGeometryFilter
    {
        /// <summary>
        /// Extracts elements from an enumeration of geometries.
        /// </summary>
        /// <param name="geoms">An enumeration of geometries</param>
        /// <returns>An extracter over the geometries.</returns>
        public static InputExtracter Extract(IEnumerable<Geometry> geoms)
        {
            var extracter = new InputExtracter();
            extracter.Add(geoms);
            return extracter;
        }

        /// <summary>
        /// Extracts elements from a geometry.
        /// </summary>
        /// <param name="geom">An geometry to extract from</param>
        /// <returns>An extracter over the geometry.</returns>
        public static InputExtracter Extract(Geometry geom)
        {
            var extracter = new InputExtracter();
            extracter.Add(geom);
            return extracter;
        }

        private GeometryFactory _geomFactory;
        private readonly IList<Geometry> _polygons = new List<Geometry>();
        private readonly IList<Geometry> _lines = new List<Geometry>();
        private readonly IList<Geometry> _points = new List<Geometry>();

        /// <summary>The default dimension for an empty GeometryCollection</summary>
        private Dimension _dimension = Dimension.False;

        /// <summary>
        /// Gets a value indicating if there were any non-empty geometries extracted
        /// </summary>
        public bool IsEmpty
        {
            get => _polygons.Count == 0 && _lines.Count == 0 && _points.Count == 0;
        }

        /// <summary>
        /// Gets a value indicating the maximum <see cref="Geometries.Dimension"/> extracted.
        /// </summary>
        public Dimension Dimension
        {
            get => _dimension;
        }

        /// <summary>
        /// Gets a value indicating the geometry factory from the extracted geometry,
        /// if there is one. <para/>
        /// If an empty collection was extracted, will return <c>null</c>.
        /// </summary>
        public GeometryFactory Factory
        {
            get => _geomFactory;
        }

        /// <summary>
        /// Gets the extracted atomic geometries of the given dimension <c>dim</c>.
        /// </summary>
        /// <param name="dim">The dimension of geometry to return</param>
        /// <returns>A list of the extracted geometries of dimension dim.</returns>
        public IList<Geometry> GetExtract(Dimension dim)
        {
            switch (dim)
            {
                case Dimension.Point: return _points;
                case Dimension.Curve: return _lines;
                case Dimension.Surface: return _polygons;
            }

            Assert.ShouldNeverReachHere($"Invalid dimension: {dim}");
            return null;
        }

        private void Add(IEnumerable<Geometry> geoms)
        {
            foreach (var geom in geoms)
            {
                Add(geom);
            }
        }

        private void Add(Geometry geom)
        {
            if (_geomFactory == null)
                _geomFactory = geom.Factory;

            geom.Apply(this);
        }

        public void Filter(Geometry geom)
        {
            RecordDimension(geom.Dimension);

            if (geom is GeometryCollection)
            {
                return;
            }

            /*
             * Don't keep empty geometries
             */
            if (geom.IsEmpty)
                return;

            if (geom is Polygon)
            {
                _polygons.Add(geom);
                return;
            }

            if (geom is LineString)
            {
                _lines.Add(geom);
                return;
            }

            if (geom is Point)
            {
                _points.Add(geom);
                return;
            }

            Assert.ShouldNeverReachHere($"Unhandled geometry type: {geom.GeometryType}");
        }

        private void RecordDimension(Dimension dim)
        {
            if (dim > _dimension)
                _dimension = dim;
        }
    }

}
