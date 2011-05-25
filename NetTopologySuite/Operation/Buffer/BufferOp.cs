using System;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;

namespace NetTopologySuite.Operation.Buffer
{
    /**
     * Computes the buffer of a geometry, for both positive and negative buffer distances.
     * <p>
     * In GIS, the positive buffer of a geometry is defined as
     * the Minkowski sum or difference of the geometry
     * with a circle of radius equal to the absolute value of the buffer distance.
     * In the CAD/CAM world buffers are known as </i>offset curves</i>.
     * In morphological analysis they are known as <i>erosion</i> and <i>dilation</i>
     * <p>
     * The buffer operation always returns a polygonal result.
     * The negative or zero-distance buffer of lines and points is always an empty {@link Polygon}.
     * <p>
     * Since true buffer curves may contain circular arcs,
     * computed buffer polygons can only be approximations to the true geometry.
     * The user can control the accuracy of the curve approximation by specifying
     * the number of linear segments used to approximate curves.
     * <p>
     * The <b>end cap style</b> of a linear buffer may be specified. The
     * following end cap styles are supported:
     * <ul
     * <li>{@link #CAP_ROUND} - the usual round end caps
     * <li>{@link #CAP_BUTT} - end caps are truncated flat at the line ends
     * <li>{@link #CAP_SQUARE} - end caps are squared off at the buffer distance beyond the line ends
     * </ul>
     * <p>
     *
     * @version 1.7
     */
    public class BufferOp
    {
        ///**
        // * Specifies a round line buffer end cap style.
        // * @deprecated use BufferParameters
        // */
        //public static final int CAP_ROUND = BufferParameters.CAP_ROUND;
        ///**
        // * Specifies a butt (or flat) line buffer end cap style.
        // * @deprecated use BufferParameters
        // */
        //public static final int CAP_BUTT = BufferParameters.CAP_FLAT;

        ///**
        // * Specifies a butt (or flat) line buffer end cap style.
        // * @deprecated use BufferParameters
        // */
        //public static final int CAP_FLAT = BufferParameters.CAP_FLAT;
        ///**
        // * Specifies a square line buffer end cap style.
        // * @deprecated use BufferParameters
        // */
        //public static final int CAP_SQUARE = BufferParameters.CAP_SQUARE;

        /**
         * A number of digits of precision which leaves some computational "headroom"
         * for floating point operations.
         * 
         * This value should be less than the decimal precision of double-precision values (16).
         */
        private const int MaxPrecisionDigits = 12;

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
        private static double PrecisionScaleFactor(IGeometry g,
            double distance,
          int maxPrecisionDigits)
        {
            IEnvelope env = g.EnvelopeInternal;
            double envSize = Math.Max(env.Height, env.Width);
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
        public static IGeometry Buffer(IGeometry g, double distance)
        {
            BufferOp gBuf = new BufferOp(g);
            IGeometry geomBuf = gBuf.GetResultGeometry(distance);
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
        public static IGeometry Buffer(IGeometry g, double distance, IBufferParameters parameters)
        {
            BufferOp bufOp = new BufferOp(g, parameters);
            IGeometry geomBuf = bufOp.GetResultGeometry(distance);
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
        public static IGeometry Buffer(IGeometry g, double distance, int quadrantSegments)
        {
            BufferOp bufOp = new BufferOp(g);
            bufOp.QuadrantSegments = quadrantSegments;
            IGeometry geomBuf = bufOp.GetResultGeometry(distance);
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
        public static IGeometry Buffer(IGeometry g, double distance,
          int quadrantSegments,
          BufferStyle endCapStyle)
        {
            BufferOp bufOp = new BufferOp(g);
            bufOp.QuadrantSegments = quadrantSegments;
            bufOp.BufferStyle = endCapStyle;
            IGeometry geomBuf = bufOp.GetResultGeometry(distance);
            return geomBuf;
        }

        private readonly IGeometry _argGeom;
        private double _distance;

        private readonly IBufferParameters _bufParams = new BufferParameters();

        private IGeometry _resultGeometry;
        private Exception _saveException;   // debugging only

        /**
         * Initializes a buffer computation for the given geometry
         *
         * @param g the geometry to buffer
         */
        public BufferOp(IGeometry g)
        {
            _argGeom = g;
        }

        /**
         * Initializes a buffer computation for the given geometry
         * with the given set of parameters
         *
         * @param g the geometry to buffer
         * @param bufParams the buffer parameters to use
         */
        public BufferOp(IGeometry g, IBufferParameters bufParams)
        {
            _argGeom = g;
            _bufParams = bufParams;
        }

        /**
         * Specifies the end cap style of the generated buffer.
         * The styles supported are {@link #CAP_ROUND}, {@link #CAP_BUTT}, and {@link #CAP_SQUARE}.
         * The default is CAP_ROUND.
         *
         * @param endCapStyle the end cap style to specify
         */
        public BufferStyle BufferStyle
        {
            get { return (BufferStyle)_bufParams.EndCapStyle; }
            set { _bufParams.EndCapStyle = (EndCapStyle)value; }
        }

        /**
         * Sets the number of segments used to approximate a angle fillet
         *
         * @param quadrantSegments the number of segments in a fillet for a quadrant
         */
        public int QuadrantSegments
        {
            get { return _bufParams.QuadrantSegments; }
            set { _bufParams.QuadrantSegments = value; }
        }

        /**
         * Returns the buffer computed for a geometry for a given buffer distance.
         *
         * @param distance the buffer distance
         * @return the buffer of the input geometry
         */
        public IGeometry GetResultGeometry(double distance)
        {
            _distance = distance;
            ComputeGeometry();
            return _resultGeometry;
        }

        private void ComputeGeometry()
        {
            BufferOriginalPrecision();
            if (_resultGeometry != null) return;

            IPrecisionModel argPM = _argGeom.Factory.PrecisionModel;
            if (argPM.PrecisionModelType == PrecisionModels.Fixed)
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
                BufferBuilder bufBuilder = new BufferBuilder(_bufParams);
                _resultGeometry = bufBuilder.Buffer(_argGeom, _distance);
            }
            catch (Exception ex)
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

            PrecisionModel fixedPM = new PrecisionModel(sizeBasedScaleFactor);
            BufferFixedPrecision(fixedPM);
        }

        private void BufferFixedPrecision(IPrecisionModel fixedPM)
        {
            INoder noder = new ScaledNoder(new MCIndexSnapRounder(new PrecisionModel(1.0)),
                                          fixedPM.Scale);

            BufferBuilder bufBuilder = new BufferBuilder(_bufParams);
            bufBuilder.WorkingPrecisionModel = fixedPM;
            bufBuilder.Noder = noder;
            // this may throw an exception, if robustness errors are encountered
            _resultGeometry = bufBuilder.Buffer(_argGeom, _distance);
        }
    }
}
