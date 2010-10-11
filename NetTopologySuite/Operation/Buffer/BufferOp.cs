#define one_ten
using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NPack;
using NPack.Interfaces;

//using NetTopologySuite.Precision;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Computes the buffer of a geometry, for both positive and negative 
    /// buffer distances.  
    /// </summary>
    /// <remarks>  
    /// <para>
    /// In GIS, the buffer of a point is defined as
    /// the Minkowski sum or difference of the point
    /// with a circle with radius equal to the absolute value of the buffer distance.
    /// </para>
    /// <para>
    /// In the CAD/CAM world buffers are known as offset curves.
    /// Since true buffer curves may contain circular arcs,
    /// computed buffer polygons can only be approximations to the true point.
    /// The user can control the accuracy of the curve approximation by specifying
    /// the number of linear segments with which to approximate a curve.
    /// </para>
    /// <para>
    /// The end cap endCapStyle of a linear buffer may be specified. The
    /// following end cap styles are supported:
    /// <list type="table">
    /// <item>
    /// <term><see cref="BufferStyle.Round"/></term>
    /// <description>the usual round end caps</description>
    /// </item>
    /// <item>
    /// <term><see cref="BufferStyle.Butt"/></term>
    /// <description>
    /// end caps are truncated flat at the line ends
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="BufferStyle.Square"/></term>
    /// <description>
    /// end caps are squared off at the buffer distance beyond the line ends
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The computation uses an algorithm involving iterated noding 
    /// and precision reduction to provide a high degree of robustness.
    /// </para>
    /// </remarks>
    public class BufferOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        // NOTE: modified for "safe" assembly in Sql 2005
        // Const added!
        private const Int32 MaxPrecisionDigits = 12;

        private readonly IGeometry<TCoordinate> _argGeom;
        private readonly ICoordinateFactory<TCoordinate> _coordFactory;
        private readonly ICoordinateSequenceFactory<TCoordinate> _coordSequenceFactory;
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private Double _distance;
        private BufferStyle _endCapStyle = BufferStyle.Round;
        private Int32 _quadrantSegments = OffsetCurveBuilder<TCoordinate>.DefaultQuadrantSegments;
        private IGeometry<TCoordinate> _resultGeometry;
        private TopologyException _saveException; // debugging only

        /// <summary>
        /// Initializes a buffer computation for the given point.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        public BufferOp(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            _argGeom = g;
            _geoFactory = g.Factory;
            _coordSequenceFactory = _geoFactory.CoordinateSequenceFactory;
            _coordFactory = _geoFactory.CoordinateFactory;
        }

        /// <summary> 
        /// Gets or sets the end cap <see cref="BufferStyle"/> of the 
        /// generated buffer geometry.
        /// </summary>
        public BufferStyle EndCapStyle
        {
            get { return _endCapStyle; }
            set { _endCapStyle = value; }
        }

        public Int32 QuadrantSegments
        {
            get { return _quadrantSegments; }
            set { _quadrantSegments = value; }
        }

        /// <summary>
        /// Compute a reasonable scale factor to limit the precision of
        /// a given combination of geometry and buffer distance.
        /// The scale factor is based on a heuristic.
        /// </summary>
        /// <param name="g">
        /// The <see cref="IGeometry{TCoordinate}"/> being buffered.
        /// </param>
        /// <param name="distance">The buffer distance.</param>
        /// <param name="maxPrecisionDigits">
        /// The max # of digits that should be allowed by
        /// the precision determined by the computed scale factor.
        /// </param>
        /// <returns>
        /// A scale factor that allows a reasonable amount of precision 
        /// for the buffer computation.
        /// </returns>
        public static Double PrecisionScaleFactor(IGeometry<TCoordinate> g,
                                                  Double distance,
                                                  Int32 maxPrecisionDigits)
        {
            IExtents<TCoordinate> extents = g.Extents;
            Double envSize = Math.Max(extents.GetSize(Ordinates.Y),
                                      extents.GetSize(Ordinates.X));
            Double expandByDistance = distance > 0.0 ? distance : 0.0;
            Double bufEnvSize = envSize + 2*expandByDistance;

            // the smallest power of 10 greater than the buffer envelope
            Int32 bufEnvLog10 = (Int32) (Math.Log(bufEnvSize)/Math.Log(10) + 1.0);
            Int32 minUnitLog10 = bufEnvLog10 - maxPrecisionDigits;

            // scale factor is inverse of min Unit size, so flip sign of exponent
            Double scaleFactor = Math.Pow(10.0, -minUnitLog10);
            return scaleFactor;
        }

        /// <summary>
        /// Computes the buffer of a geometry for a given buffer distance.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <returns> The buffer of the input point.</returns>
        public static IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g,
                                                    Double distance)
        {
            BufferOp<TCoordinate> gBuf = new BufferOp<TCoordinate>(g);
            IGeometry<TCoordinate> geomBuf = gBuf.GetResultGeometry(distance);
            return geomBuf;
        }

        /// <summary>
        /// Computes the buffer of a point for a given buffer distance,
        /// using the given Cap Style for borders of the point.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        /// <param name="distance">The buffer distance.</param>        
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns> The buffer of the input point.</returns>
        public static IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g,
                                                    Double distance,
                                                    BufferStyle endCapStyle)
        {
            BufferOp<TCoordinate> gBuf = new BufferOp<TCoordinate>(g);
            gBuf.EndCapStyle = endCapStyle;
            IGeometry<TCoordinate> geomBuf = gBuf.GetResultGeometry(distance);
            return geomBuf;
        }

        /// <summary>
        /// Computes the buffer for a point for a given buffer distance
        /// and accuracy of approximation.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <param name="quadrantSegments">The number of segments used to approximate a quarter circle.</param>
        /// <returns>The buffer of the input point.</returns>
        public static IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g,
                                                    Double distance,
                                                    Int32 quadrantSegments)
        {
            BufferOp<TCoordinate> bufOp = new BufferOp<TCoordinate>(g);
            bufOp.QuadrantSegments = quadrantSegments;
            IGeometry<TCoordinate> geomBuf = bufOp.GetResultGeometry(distance);
            return geomBuf;
        }

        /// <summary>
        /// Computes the buffer for a point for a given buffer distance
        /// and accuracy of approximation.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <param name="quadrantSegments">The number of segments used to approximate a quarter circle.</param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>The buffer of the input point.</returns>
        public static IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g,
                                                    Double distance,
                                                    Int32 quadrantSegments,
                                                    BufferStyle endCapStyle)
        {
            BufferOp<TCoordinate> bufOp = new BufferOp<TCoordinate>(g);
            bufOp.EndCapStyle = endCapStyle;
            bufOp.QuadrantSegments = quadrantSegments;
            IGeometry<TCoordinate> geomBuf = bufOp.GetResultGeometry(distance);
            return geomBuf;
        }

        public IGeometry<TCoordinate> GetResultGeometry(Double distance)
        {
            _distance = distance;
            computeGeometry();
            return _resultGeometry;
        }

        public IGeometry<TCoordinate> GetResultGeometry(Double distance,
                                                        Int32 quadrantSegments)
        {
            _distance = distance;
            QuadrantSegments = quadrantSegments;
            computeGeometry();
            return _resultGeometry;
        }

        private void computeGeometry()
        {
            bufferOriginalPrecision();

            if (_resultGeometry != null)
            {
                return;
            }

            IPrecisionModel<TCoordinate> argPM = _argGeom.Factory.PrecisionModel;

            if (argPM.PrecisionModelType == PrecisionModelType.Fixed)
            {
                bufferFixedPrecision(argPM);
            }
            else
            {
                bufferReducedPrecision();
            }
        }

        private void bufferOriginalPrecision()
        {
            try
            {
                BufferBuilder<TCoordinate> bufBuilder
                    = new BufferBuilder<TCoordinate>(_geoFactory);
                bufBuilder.QuadrantSegments = _quadrantSegments;
                bufBuilder.EndCapStyle = _endCapStyle;
                _resultGeometry = bufBuilder.Buffer(_argGeom, _distance);
            }
            catch (TopologyException ex)
            {
                _saveException = ex;
                // don't propagate the exception - it will be detected by fact 
                // that resultGeometry is null
            }
        }

        private void bufferFixedPrecision(IPrecisionModel<TCoordinate> fixedPM)
        {
            MonotoneChainIndexSnapRounder<TCoordinate> snapRounder =
                new MonotoneChainIndexSnapRounder<TCoordinate>(_geoFactory,
                                                               _coordFactory.CreatePrecisionModel(1.0));

            INoder<TCoordinate> noder = new ScaledNoder<TCoordinate>(
                _coordSequenceFactory,snapRounder,
                fixedPM.Scale
                );

            BufferBuilder<TCoordinate> bufBuilder
                = new BufferBuilder<TCoordinate>(_geoFactory);
            bufBuilder.WorkingPrecisionModel = fixedPM;
            bufBuilder.Noder = noder;
            bufBuilder.QuadrantSegments = _quadrantSegments;
            bufBuilder.EndCapStyle = _endCapStyle;

            // this may throw an exception, if robustness errors are encountered
            _resultGeometry = bufBuilder.Buffer(_argGeom, _distance);
        }

        private void bufferReducedPrecision()
        {
            // try and compute with decreasing precision
            for (Int32 precDigits = MaxPrecisionDigits; precDigits >= 0; precDigits--)
            {
                try
                {
                    bufferReducedPrecision(precDigits);
                }
                catch (TopologyException ex)
                {
                    _saveException = ex;
                    // don't propagate the exception - it will be detected by fact that resultGeometry is null
                }

                if (_resultGeometry != null)
                {
                    return;
                }
            }

            // tried everything - have to bail
            throw _saveException;
        }

        private void bufferReducedPrecision(Int32 precisionDigits)
        {
            Double sizeBasedScaleFactor = PrecisionScaleFactor(_argGeom, _distance, precisionDigits);
            // Debug.WriteLine(String.Format("recomputing with precision scale factor = {0}", sizeBasedScaleFactor));

            IPrecisionModel<TCoordinate> fixedPM =
                _coordFactory.CreatePrecisionModel(sizeBasedScaleFactor);

            //// TODO: Fix scaled noder.
            //throw new NotImplementedException("Fix scaled noder.");
            bufferFixedPrecision(fixedPM);
        }
    }
}