using System;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Buffer.Validate;
using NetTopologySuite.Utilities;
using Open.Topology.TestRunner.Result;

namespace Open.Topology.TestRunner.Operations
{

    /// <summary>
    /// A <see cref="IGeometryOperation"/> which validates the results of the 
    /// <see cref="IGeometry"/> <tt>buffer()</tt> method.
    /// If an invalid result is found, an exception is thrown (this is the most
    /// convenient and noticeable way of flagging the problem when using the TestRunner).
    /// All other Geometry methods are executed normally.
    /// <para>
    /// This class can be used via the <tt>-geomop</tt> command-line option
    /// or by the <tt>&lt;geometryOperation&gt;</tt> XML test file setting.
    /// </para>
    /// </summary>
    /// <author>
    /// mbdavis
    /// </author>
    public class BufferValidatedGeometryOperation : IGeometryOperation
    {

        private const bool ReturnEmptyGeometryCollection = false;

        private readonly GeometryMethodOperation _chainOp = new GeometryMethodOperation();

        private int _argCount;
        private double _distance;
        private int _quadSegments;
        private EndCapStyle _endCapStyle;

        public BufferValidatedGeometryOperation()
        {

        }

        public Type GetReturnType(XmlTestType op)
        {
            return GetReturnType(op.ToString());
        }

        public Type GetReturnType(String opName)
        {
            return _chainOp.GetReturnType(opName);
        }

        /// <summary>
        /// Creates a new operation which chains to the given <see cref="IGeometryOperation"/>
        /// for non-intercepted methods.
        /// </summary>
        /// <param name="chainOp">The operation to chain to</param>
        public BufferValidatedGeometryOperation(GeometryMethodOperation chainOp)
        {
            _chainOp = chainOp;
        }

        /// <summary>
        /// Invokes the named operation
        /// </summary>
        /// <param name="op">The name of the operation</param>
        /// <param name="geometry">The geometry to process</param>
        /// <param name="args">The arguments to the operation (which may be typed as Strings)</param>
        /// <returns>The result</returns>
        /// <exception cref="Exception">If some error was encountered trying to find or process the operation</exception>
        public IResult Invoke(XmlTestType op, IGeometry geometry, Object[] args)
        {
            string opName = op.ToString();
            bool isBufferOp = opName.Equals("buffer", StringComparison.InvariantCultureIgnoreCase);
            // if not a buffer op, do the default
            if (! isBufferOp)
            {
                return _chainOp.Invoke(opName, geometry, args);
            }
            ParseArgs(args);
            return InvokeBufferOpValidated(geometry /*, args */);
        }

        private void ParseArgs(Object[] args)
        {
            _argCount = args.Length;
            _distance = Double.Parse((String) args[0]);
            if (_argCount >= 2)
                _quadSegments = Int32.Parse((String) args[1]);
            if (_argCount >= 3)
                _endCapStyle = (EndCapStyle)Int32.Parse((String)args[2]);
        }

        private IResult InvokeBufferOpValidated(IGeometry geometry /*, Object[] args*/)
        {
            IGeometry result = InvokeBuffer(geometry);

            // validate
            Validate(geometry, result);

            /**
             * Return an empty GeometryCollection as the result.  
             * This allows the test case to avoid specifying an exact result
             */
            if (ReturnEmptyGeometryCollection)
            {
                result = result.Factory.CreateGeometryCollection(null);
            }
            return new GeometryResult(result);
        }

        private IGeometry InvokeBuffer(IGeometry geom)
        {
            if (_argCount == 1)
            {
                return geom.Buffer(_distance);
            }
            if (_argCount == 2)
            {
                return geom.Buffer(_distance, _quadSegments);
            }
            Assert.ShouldNeverReachHere("Unknown or unhandled buffer method");
            return null;
        }

        private void Validate(IGeometry geom, IGeometry buffer)
        {
            if (IsEmptyBufferExpected(geom))
            {
                CheckEmpty(buffer);
                return;
            }
            // simple containment check
            CheckContainment(geom, buffer);

            // could also check distances of boundaries
            CheckDistance(geom, _distance, buffer);
            // need special check for negative buffers which disappear.  Somehow need to find maximum inner circle - via skeleton?
        }

        private bool IsEmptyBufferExpected(IGeometry geom)
        {
            var isNegativeBufferOfNonAreal = (int) geom.Dimension < 2 && _distance <= 0.0;
            return isNegativeBufferOfNonAreal;
        }

        private static void CheckEmpty(IGeometry geom)
        {
            if (geom.IsEmpty)
            {
                return;
            }
            ReportError("Expected empty buffer result", null);
        }

        private void CheckContainment(IGeometry geom, IGeometry buffer)
        {
            var isCovered = true;
            var errMsg = "";
            if (_distance > 0)
            {
                isCovered = buffer.Covers(geom);
                errMsg = "Geometry is not contained in (positive) buffer";
            }
            else if (_distance < 0)
            {
                errMsg = "Geometry does not contain (negative) buffer";
                // covers is always false for empty geometries, so don't bother testing them
                if (buffer.IsEmpty)
                {
                    isCovered = true;
                }
                else
                {
                    isCovered = geom.Covers(buffer);
                }

            }
            if (! isCovered)
            {
                ReportError(errMsg, null);
            }
        }

        private static void CheckDistance(IGeometry geom, double distance, IGeometry buffer)
        {
            var bufValidator = new BufferResultValidator(geom, distance, buffer);
            if (! bufValidator.IsValid())
            {
                var errorMsg = bufValidator.ErrorMessage;
                var errorLoc = bufValidator.ErrorLocation;
                ReportError(errorMsg, errorLoc);
            }
        }

        private static void ReportError(String msg, Coordinate loc)
        {
            String locStr = "";
            if (loc != null)
            {
                locStr = " at " + WKTWriter.ToPoint(loc);
            }
//  	System.out.println(msg);
            throw new Exception(msg + locStr);
        }


    }
}