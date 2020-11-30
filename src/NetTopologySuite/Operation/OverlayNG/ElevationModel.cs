using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>A simple elevation model used to populate missing Z values
    /// in overlay results.
    /// <para/>
    /// The model divides the extent of the input geometry(s)
    /// into an NxM grid.
    /// The default grid size is 3x3.
    /// If the input has no extent in the X or Y dimension,
    /// that dimension is given grid size 1.
    /// The elevation of each grid cell is computed as the average of the Z values
    /// of the input vertices in that cell (if any).
    /// If a cell has no input vertices within it, it is assigned
    /// the average elevation over all cells.
    /// <para/>
    /// If no input vertices have Z values, the model does not assign a Z value.
    /// <para/>
    /// The elevation of an arbitrary location is determined as the
    /// Z value of the nearest grid cell.
    /// <para/>
    /// An elevation model can be used to populate missing Z values
    /// in an overlay result geometry.
    /// </summary>
    /// <author>Martin Davis</author>
    class ElevationModel
    {

        private const int DEFAULT_CELL_NUM = 3;

        /// <summary>
        /// Creates an elevation model from two geometries (which may be null).
        /// </summary>
        /// <param name="geom1">An input geometry</param>
        /// <param name="geom2">An input geometry</param>
        /// <returns>The elevation model computed from the geometries</returns>
        public static ElevationModel Create(Geometry geom1, Geometry geom2)
        {
            var extent = new Envelope();
            if (geom1 != null)
                extent.ExpandToInclude(geom1.EnvelopeInternal);
            if (geom2 != null)
                extent.ExpandToInclude(geom2.EnvelopeInternal);

            if (extent.IsNull)
                throw new ArgumentException("Arguments don't have an extent!");

            var model = new ElevationModel(extent, DEFAULT_CELL_NUM, DEFAULT_CELL_NUM);
            if (geom1 != null) model.Add(geom1);
            if (geom2 != null) model.Add(geom2);

            return model;
        }

        private readonly Envelope _extent;
        private readonly int _numCellX;
        private readonly int _numCellY;
        private readonly double _cellSizeX;
        private readonly double _cellSizeY;
        private readonly ElevationCell[][] _cells;
        private bool _isInitialized;
        private bool _hasZValue;
        private double _averageZ = double.NaN;

        /// <summary>
        /// Creates a new elevation model covering an extent by a grid of given dimensions.
        /// </summary>
        /// <param name="extent">The XY extent to cover</param>
        /// <param name="numCellX">The number of grid cells in the X dimension</param>
        /// <param name="numCellY">The number of grid cells in the Y dimension</param>
        public ElevationModel(Envelope extent, int numCellX, int numCellY)
        {
            _extent = extent;
            _numCellX = numCellX;
            _numCellY = numCellY;

            _cellSizeX = extent.Width / numCellX;
            _cellSizeY = extent.Height / numCellY;
            if (_cellSizeX <= 0.0)
            {
                _numCellX = 1;
            }

            if (_cellSizeY <= 0.0)
            {
                _numCellY = 1;
            }

            _cells = new ElevationCell[numCellY][];
            for (int i = 0; i < numCellY; i++)
                _cells[i] = new ElevationCell[numCellX];
        }

        /// <summary>
        /// Updates the model using the Z values of a given geometry.
        /// </summary>
        /// <param name="geom">The geometry to scan for Z values.</param>
        public void Add(Geometry geom)
        {
            var filter = new HasZFilter(this);
            geom.Apply(filter);
        }

        private void Add(double x, double y, double z)
        {
            if (double.IsNaN(z))
                return;
            _hasZValue = true;
            var cell = GetCell(x, y, true);
            cell.Add(z);
        }

        private void Init()
        {
            _isInitialized = true;
            int numCells = 0;
            double sumZ = 0.0;

            for (int i = 0; i < _cells.Length; i++)
            {
                for (int j = 0; j < _cells[0].Length; j++)
                {
                    var cell = _cells[i][j];
                    if (cell != null)
                    {
                        cell.Compute();
                        numCells++;
                        sumZ += cell.Z;
                    }
                }
            }

            _averageZ = double.NaN;
            if (numCells > 0)
            {
                _averageZ = sumZ / numCells;
            }
        }

        /// <summary>
        /// Gets the model Z value at a given location.
        /// If the location lies outside the model grid extent,
        /// this returns the Z value of the nearest grid cell.
        /// If the model has no elevation computed (i.e. due
        /// to empty input), the value is returned as <see cref="double.NaN"/>
        /// </summary>
        /// <param name="x">x-ordinate of the location</param>
        /// <param name="y">y-ordinate of the location</param>
        /// <returns>The computed Z value</returns>
        public double GetZ(double x, double y)
        {
            if (!_isInitialized)
                Init();
            var cell = GetCell(x, y, false);
            if (cell == null)
                return _averageZ;
            return cell.Z;
        }

        /// <summary>
        /// Computes Z values for any missing Z values in a geometry,
        /// using the computed model.
        /// If the model has no Z value, or the geometry coordinate dimension
        /// does not include Z, the geometry is not updated.
        /// </summary>
        /// <param name="geom">The geometry to populate Z values for</param>
        public void PopulateZ(Geometry geom)
        {
            // short-circuit if no Zs are present in model
            if (!_hasZValue)
                return;

            if (!_isInitialized)
                Init();

            var filter = new PopulateZFilter(this);
            geom.Apply(filter);
        }

        private ElevationCell GetCell(double x, double y, bool isCreateIfMissing)
        {
            int ix = 0;
            if (_numCellX > 1)
            {
                ix = (int) ((x - _extent.MinX) / _cellSizeX);
                ix = MathUtil.Clamp(ix, 0, _numCellX - 1);
            }

            int iy = 0;
            if (_numCellY > 1)
            {
                iy = (int) ((y - _extent.MinY) / _cellSizeY);
                iy = MathUtil.Clamp(iy, 0, _numCellY - 1);
            }

            var cell = _cells[iy][ix];
            if (isCreateIfMissing && cell == null)
            {
                cell = new ElevationCell();
                _cells[iy][ix] = cell;
            }

            return cell;
        }

        private class PopulateZFilter : IEntireCoordinateSequenceFilter
        {
            private readonly ElevationModel _em;

            public PopulateZFilter(ElevationModel em)
            {
                _em = em;
            }

            public bool Done { get; private set; }

            public bool GeometryChanged
            {
                get => false;
            }

            public void Filter(CoordinateSequence seq)
            {
                if (!seq.HasZ)
                {
                    // if no Z then short-circuit evaluation
                    Done = true;
                    return;
                }

                for (int i = 0; i < seq.Count; i++)
                {
                    // if Z not populated then assign using model
                    if (double.IsNaN(seq.GetZ(i)))
                    {
                        double z = _em.GetZ(seq.GetOrdinate(i, Ordinate.X), seq.GetOrdinate(i, Ordinate.Y));
                        seq.SetOrdinate(i, Ordinate.Z, z);
                    }
                }
            }
        }

        private class HasZFilter : IEntireCoordinateSequenceFilter
        {
            private bool _hasZ;
            private readonly ElevationModel _em;

            public HasZFilter(ElevationModel em)
            {
                _em = em;
                _hasZ = true;
            }

            public bool Done
            {
                get => !_hasZ;
            }

            public bool GeometryChanged { get; } = false;

            public void Filter(CoordinateSequence seq)
            {
                if (!seq.HasZ)
                {
                    _hasZ = false;
                    return;
                }

                for (int i = 0; i < seq.Count; i++)
                {
                    double z = seq.GetOrdinate(i, Ordinate.Z);
                    _em.Add(seq.GetOrdinate(i, Ordinate.X),
                        seq.GetOrdinate(i, Ordinate.Y),
                        z);
                }
            }
        }

        private class ElevationCell
        {

            private int _numZ;
            private double _sumZ;
            private double _avgZ;

            public void Add(double z)
            {
                _numZ++;
                _sumZ += z;
            }

            public void Compute()
            {
                _avgZ = double.NaN;
                if (_numZ > 0)
                    _avgZ = _sumZ / _numZ;
            }

            public double Z => _avgZ;
        }
    }

}
