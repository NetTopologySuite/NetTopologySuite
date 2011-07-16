using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Buffer
{
    public class BufferOp_110<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /**
         * A number of digits of precision which leaves some computational "headroom"
         * for floating point operations.
         * 
         * This value should be less than the decimal precision of double-precision values (16).
         */
        private static int MaxPrecisionDigits = 12;

        /**
         * Compute a scale factor to limit the precision of
         * a given combination of Geometry and buffer distance.
         * The scale factor is determined by a combination of
         * the number of digits of precision in the (geometry + buffer distance),
         * limited by the supplied <code>maxPrecisionDigits</code> value.
         *
         * @param g the Geometry being buffered
         * @param distance the buffer distance
         * @param maxPrecisionDigits the max # of digits that should be allowed by
         *          the precision determined by the computed scale factor
         *
         * @return a scale factor for the buffer computation
         */
        private static Double PrecisionScaleFactor(IGeometry<TCoordinate> g,
            Double distance,
          int maxPrecisionDigits)
        {
            IExtents<TCoordinate> extents = g.Extents;
            double envSize = Math.Max(extents.GetSize(Ordinates.Y), extents.GetSize(Ordinates.X));
            double expandByDistance = distance > 0.0 ? distance : 0.0;
            double bufEnvSize = envSize + 2 * expandByDistance;

            // the smallest power of 10 greater than the buffer envelope
            int bufEnvLog10 = (int)(Math.Log(bufEnvSize) / Math.Log(10) + 1.0);
            int minUnitLog10 = bufEnvLog10 - maxPrecisionDigits;
            // scale factor is inverse of min Unit size, so flip sign of exponent
            double scaleFactor = Math.Pow(10.0, -minUnitLog10);
            return scaleFactor;
        }

        /**
         * Computes the buffer of a geometry for a given buffer distance.
         *
         * @param g the geometry to buffer
         * @param distance the buffer distance
         * @return the buffer of the input geometry
         */
        public static IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g, Double distance)
        {
            BufferOp_110<TCoordinate> gBuf = new BufferOp_110<TCoordinate>(g);
            IGeometry<TCoordinate> geomBuf = gBuf.GetResultGeometry(distance);
            //BufferDebug.saveBuffer(geomBuf);
            //BufferDebug.runCount++;
            return geomBuf;
        }

        /**
         * Comutes the buffer for a geometry for a given buffer distance
         * and accuracy of approximation.
         *
         * @param g the geometry to buffer
         * @param distance the buffer distance
         * @param params the buffer parameters to use
         * @return the buffer of the input geometry
         *
         */
        public static IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g, double distance, BufferParameters parameters)
        {
            BufferOp_110<TCoordinate> bufOp = new BufferOp_110<TCoordinate>(g, parameters);
            IGeometry<TCoordinate> geomBuf = bufOp.GetResultGeometry(distance);
            return geomBuf;
        }

        /**
         * Comutes the buffer for a geometry for a given buffer distance
         * and accuracy of approximation.
         *
         * @param g the geometry to buffer
         * @param distance the buffer distance
         * @param quadrantSegments the number of segments used to approximate a quarter circle
         * @return the buffer of the input geometry
         *
         */
        public static IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g, double distance, int quadrantSegments)
        {
            BufferOp_110<TCoordinate> bufOp = new BufferOp_110<TCoordinate>(g);
            bufOp.Parameters.QuadrantSegments = quadrantSegments;
            IGeometry<TCoordinate> geomBuf = bufOp.GetResultGeometry(distance);
            return geomBuf;
        }

        /**
         * Comutes the buffer for a geometry for a given buffer distance
         * and accuracy of approximation.
         *
         * @param g the geometry to buffer
         * @param distance the buffer distance
         * @param quadrantSegments the number of segments used to approximate a quarter circle
         * @param endCapStyle the end cap style to use
         * @return the buffer of the input geometry
         *
         */
        public static IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g,
                                        Double distance,
          int quadrantSegments,
          BufferStyle endCapStyle)
        {
            BufferOp_110<TCoordinate> bufOp = new BufferOp_110<TCoordinate>(g);
            bufOp.Parameters.QuadrantSegments = quadrantSegments;
            bufOp.Parameters.EndCapStyle = (BufferParameters.BufferEndCapStyle)endCapStyle;
            IGeometry<TCoordinate> geomBuf = bufOp.GetResultGeometry(distance);
            return geomBuf;
        }

        private IGeometry<TCoordinate> _argGeom;
        private readonly ICoordinateFactory<TCoordinate> _coordFactory;
        private readonly ICoordinateSequenceFactory<TCoordinate> _coordSequenceFactory;
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private double _distance;

        private BufferParameters _bufParams = new BufferParameters(BufferParameters.DefaultQuadrantSegments);

        private IGeometry<TCoordinate> _resultGeometry = null;
        private TopologyException _saveException; // debugging only

        /**
         * Initializes a buffer computation for the given geometry
         *
         * @param g the geometry to buffer
         */
        public BufferOp_110(IGeometry<TCoordinate> g)
            : this(g, new BufferParameters(BufferParameters.DefaultQuadrantSegments))
        {
        }

        /**
         * Initializes a buffer computation for the given geometry
         * with the given set of parameters
         *
         * @param g the geometry to buffer
         * @param bufParams the buffer parameters to use
         */
        public BufferOp_110(IGeometry<TCoordinate> g, BufferParameters bufParams)
        {
            _argGeom = g;
            _geoFactory = g.Factory;
            _coordFactory = _geoFactory.CoordinateFactory;
            _coordSequenceFactory = _geoFactory.CoordinateSequenceFactory;
            _bufParams = bufParams;
        }

        BufferParameters Parameters
        {
            get { return _bufParams; }
            set
            {
                if (value != null)
                    _bufParams = value;
            }
        }
        ///**
        // * Specifies the end cap style of the generated buffer.
        // * The styles supported are {@link #CAP_ROUND}, {@link #CAP_BUTT}, and {@link #CAP_SQUARE}.
        // * The default is CAP_ROUND.
        // *
        // * @param endCapStyle the end cap style to specify
        // */
        //public void setEndCapStyle(int endCapStyle)
        //{
        //  bufParams.setEndCapStyle(endCapStyle);
        //}

        ///**
        // * Sets the number of segments used to approximate a angle fillet
        // *
        // * @param quadrantSegments the number of segments in a fillet for a quadrant
        // */
        //public void setQuadrantSegments(int quadrantSegments)
        //{
        //  bufParams.setQuadrantSegments(quadrantSegments);
        //}

        /**
         * Returns the buffer computed for a geometry for a given buffer distance.
         *
         * @param distance the buffer distance
         * @return the buffer of the input geometry
         */
        public IGeometry<TCoordinate> GetResultGeometry(Double distance)
        {
            _distance = distance;
            ComputeGeometry();
            return _resultGeometry;
        }

        private void ComputeGeometry()
        {
            BufferOriginalPrecision();
            if (_resultGeometry != null) return;

            IPrecisionModel<TCoordinate> argPM = _argGeom.Factory.PrecisionModel;
            if (argPM.PrecisionModelType == PrecisionModelType.Fixed)
                //throw new NotImplementedException("Fix scaled noder");
                BufferFixedPrecision(argPM);
            else
                BufferReducedPrecision();
        }

        private void BufferReducedPrecision()
        {
            // try and compute with decreasing precision
            for (int precDigits = MaxPrecisionDigits; precDigits >= 0; precDigits--)
            {
                try
                {
                    BufferReducedPrecision(precDigits);
                }
                catch (TopologyException ex)
                {
                    _saveException = ex;
                    // don't propagate the exception - it will be detected by fact that resultGeometry is null
                }
                if (_resultGeometry != null) return;
            }

            // tried everything - have to bail
            throw _saveException;
        }

        private void BufferOriginalPrecision()
        {
            try
            {
                // use fast noding by default
                BufferBuilder_110<TCoordinate> bufBuilder = new BufferBuilder_110<TCoordinate>(_geoFactory, _bufParams);
                _resultGeometry = bufBuilder.Buffer(_argGeom, _distance);
            }
            catch (TopologyException ex)
            {
                _saveException = ex;
                // don't propagate the exception - it will be detected by fact that resultGeometry is null

                // testing - propagate exception
                //throw ex;
            }
        }

        private void BufferReducedPrecision(int precisionDigits)
        {
            double sizeBasedScaleFactor = PrecisionScaleFactor(_argGeom, _distance, precisionDigits);
            //    System.out.println("recomputing with precision scale factor = " + sizeBasedScaleFactor);

            IPrecisionModel<TCoordinate> fixedPM =
                _coordFactory.CreatePrecisionModel(sizeBasedScaleFactor);
            BufferFixedPrecision(fixedPM);
        }

        private void BufferFixedPrecision(IPrecisionModel<TCoordinate> fixedPM)
        {
            MonotoneChainIndexSnapRounder<TCoordinate> snapRounder =
                new MonotoneChainIndexSnapRounder<TCoordinate>(_geoFactory,
                                                               _coordFactory.CreatePrecisionModel(1.0));

            INoder<TCoordinate> noder = new ScaledNoder<TCoordinate>(
                _coordSequenceFactory,
                snapRounder,
                fixedPM.Scale);

            BufferBuilder_110<TCoordinate> bufBuilder = new BufferBuilder_110<TCoordinate>(_geoFactory, _bufParams);
            bufBuilder.WorkingPrecisionModel = fixedPM;
            bufBuilder.WorkingNoder = noder;
            // this may throw an exception, if robustness errors are encountered
            _resultGeometry = bufBuilder.Buffer(_argGeom, _distance);
        }
    }
}
