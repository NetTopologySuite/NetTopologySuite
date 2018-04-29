using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Overlay.Validate;
using Open.Topology.TestRunner.Result;

namespace Open.Topology.TestRunner.Operations
{
    /// <summary>
    /// A <see cref="IGeometryOperation"/> which validates the result of overlay operations.
    /// If an invalid result is found, an exception is thrown (this is the most
    /// convenient and noticeable way of flagging the problem when using the TestRunner).
    /// All other Geometry methods are executed normally.
    /// <para/>
    /// In order to eliminate the need to specify the precise result of an overlay, 
    /// this class forces the final return value to be <tt>GEOMETRYCOLLECTION EMPTY</tt>.
    /// <para/>
    /// This class can be used via the <tt>-geomop</tt> command-line option
    /// or by the <tt>&lt;geometryOperation&gt;</tt> XML test file setting.
    /// </summary>
    /// <author>Martin Davis</author>
    public class OverlayValidatedGeometryOperation : IGeometryOperation
    {
        public static SpatialFunction OverlayOpCode(String methodName)
        {
            if (methodName.Equals("intersection", StringComparison.InvariantCultureIgnoreCase) ) return SpatialFunction.Intersection;
            if (methodName.Equals("union", StringComparison.InvariantCultureIgnoreCase)) return SpatialFunction.Union;
            if (methodName.Equals("difference", StringComparison.InvariantCultureIgnoreCase)) return SpatialFunction.Difference;
            if (methodName.Equals("symDifference", StringComparison.InvariantCultureIgnoreCase)) return SpatialFunction.SymDifference;
            throw new ArgumentOutOfRangeException("methodName");
        }

        private const bool ReturnEmptyGeometryCollection = true;

        private readonly GeometryMethodOperation _chainOp = new GeometryMethodOperation();

        public OverlayValidatedGeometryOperation()
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
        /// Creates a new operation which chains to the given {@link GeometryMethodOperation}
        /// for non-intercepted methods.
        /// </summary>
        /// <param name="chainOp">the operation to chain to</param>
        public OverlayValidatedGeometryOperation(GeometryMethodOperation chainOp)
        {
            _chainOp = chainOp;
        }

        /// <summary>
        /// Invokes the named operation.
        /// </summary>
        /// <param name="opName">The name of the operation</param>
        /// <param name="geometry">The geometry to process</param>
        /// <param name="args">The arguments to the operation (which may be typed as Strings)</param>
        /// <returns>The result</returns>
        /// <exception cref="Exception">If some error was encountered trying to find or process the operation</exception>
        public IResult Invoke(XmlTestType opName, IGeometry geometry, Object[] args)
        {
            return Invoke(opName.ToString(), geometry, args);
        }

        public IResult Invoke(String opName, IGeometry geometry, Object[] args)
        {
            var opCode = OverlayOpCode(opName);

            // if not an overlay op, do the default
            if (opCode < 0)
            {
                return _chainOp.Invoke(opName, geometry, args);
            }
            return InvokeValidatedOverlayOp(opCode, geometry, args);
        }

        /**
         * 
         * and optionally validating the result.
         * 
         * @param opCode
         * @param g0
         * @param args
         * @return
         * @throws Exception
         */
        /// <summary>
        /// Invokes an overlay op, optionally using snapping,
        /// and optionally validating the result.
        /// </summary>
        public IResult InvokeValidatedOverlayOp(SpatialFunction opCode, IGeometry g0, Object[] args)
        {
            var g1 = (IGeometry) args[0];

            var result = InvokeGeometryOverlayMethod(opCode, g0, g1);

            // validate
            Validate(opCode, g0, g1, result);
            AreaValidate(g0, g1);

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

        private static void Validate(SpatialFunction opCode, IGeometry g0, IGeometry g1, IGeometry result)
        {
            var validator = new OverlayResultValidator(g0, g1, result);
            // check if computed result is valid
            if (!validator.IsValid(opCode))
            {
                var invalidLoc = validator.InvalidLocation;
                String msg = "Operation result is invalid [OverlayResultValidator] ( " + WKTWriter.ToPoint(invalidLoc) + " )";
                ReportError(msg);
            }
        }

        private const double AreaDiffTol = 5.0;

        private static void AreaValidate(IGeometry g0, IGeometry g1)
        {
            double areaDiff = AreaDiff(g0, g1);
            //  	System.out.println("Area diff = " + areaDiff);
            if (Math.Abs(areaDiff) > AreaDiffTol)
            {
                String msg = "Operation result is invalid [AreaTest] (" + areaDiff + ")";
                ReportError(msg);
            }
        }

        public static double AreaDiff(IGeometry g0, IGeometry g1)
        {
            double areaA = g0.Area;
            double areaAdiffB = g0.Difference(g1).Area;
            double areaAintB = g0.Intersection(g1).Area;
            return areaA - areaAdiffB - areaAintB;
        }

        private static void ReportError(String msg)
        {
            //Console.WriteLine(msg);
            throw new Exception(msg);
        }

        public static IGeometry InvokeGeometryOverlayMethod(SpatialFunction opCode, IGeometry g0, IGeometry g1)
        {
            switch (opCode)
            {
                case SpatialFunction.Intersection:
                    return g0.Intersection(g1);
                case SpatialFunction.Union:
                    return g0.Union(g1);
                case SpatialFunction.Difference:
                    return g0.Difference(g1);
                case SpatialFunction.SymDifference:
                    return g0.SymmetricDifference(g1);
            }
            throw new ArgumentException(@"Unknown overlay op code");
        }
    }
}