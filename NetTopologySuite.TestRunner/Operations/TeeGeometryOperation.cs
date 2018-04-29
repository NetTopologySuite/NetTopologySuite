using System;
using GeoAPI.Geometries;
using Open.Topology.TestRunner.Result;

namespace Open.Topology.TestRunner.Operations
{
    /// <summary>
    /// A <see cref="IGeometryOperation"/> which executes the original operation 
    /// and returns that result,
    /// but also executes a separate operation (which could be multiple operations).
    /// The side operations can throw exceptions if they do not compute 
    /// correct results.  This relies on the availability of 
    /// another reliable implementation to provide the expected result.
    /// <para>
    /// This class can be used via the <tt>-geomop</tt> command-line option
    /// or by the <tt>&lt;geometryOperation&gt;</tt> XML test file setting.</para>
    /// </summary>
    /// <author>mbdavis</author>
    public abstract class TeeGeometryOperation : IGeometryOperation
    {
        private readonly GeometryMethodOperation _chainOp = new GeometryMethodOperation();

        protected TeeGeometryOperation()
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
        /// Creates a new operation which chains to the given <see cref="IGeometryOperation"/> for non-intercepted methods.
        /// </summary>
        /// <param name="chainOp">The operation to chain to</param>
        protected TeeGeometryOperation(GeometryMethodOperation chainOp)
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
            RunTeeOp(opName, geometry, args);

            return _chainOp.Invoke(opName, geometry, args);
        }

        protected abstract void RunTeeOp(String opName, IGeometry geometry, Object[] args);


    }
}