using System;
using GeoAPI.Geometries;
using Open.Topology.TestRunner.Functions;
using Open.Topology.TestRunner.Result;

namespace Open.Topology.TestRunner.Operations
{/// <summary>
    /// Invokes a function from registry 
    ///  or a Geometry method determined by a named operation with a list of arguments,
    ///  the first of which is a <see cref="IGeometry"/>.
    ///  This class allows overriding Geometry methods
    ///  or augmenting them
    ///  with functions defined in a <see cref="GeometryFunctionRegistry"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryFunctionOperation : IGeometryOperation
    {
        private readonly GeometryFunctionRegistry registry;
        private readonly IGeometryOperation defaultOp = new GeometryMethodOperation();
        private readonly ArgumentConverter argConverter = new ArgumentConverter();

        public GeometryFunctionOperation() { }

        public GeometryFunctionOperation(GeometryFunctionRegistry registry)
        {
            this.registry = registry;
        }

        /// <summary>
        /// Gets the return type for the operation
        /// </summary>
        /// <param name="op">The name of the operation</param>
        /// <returns>The return type of the operation</returns>
        public Type GetReturnType(XmlTestType op)
        {
            string opName = op.ToString();
            IGeometryFunction func = registry.Find(opName);
            if (func == null)
                return defaultOp.GetReturnType(op);
            return func.ReturnType;
        }

        public IResult Invoke(XmlTestType opName, IGeometry geometry, Object[] args)
        {
            IGeometryFunction func = registry.Find(opName.ToString(), args.Length);
            if (func == null)
                return defaultOp.Invoke(opName, geometry, args);

            return Invoke(func, geometry, args);
        }

        private IResult Invoke(IGeometryFunction func, IGeometry geometry, Object[] args)
        {
            Object[] actualArgs = argConverter.Convert(func.ParameterTypes, args);
            if (func.ReturnType == typeof(bool))
                return new BooleanResult((Boolean)func.Invoke(geometry, actualArgs));
            if (typeof(IGeometry).IsAssignableFrom(func.ReturnType))
                return new GeometryResult((IGeometry)func.Invoke(geometry, actualArgs));
            if (func.ReturnType == typeof(double))
                return new DoubleResult((double)func.Invoke(geometry, actualArgs));
            if (func.ReturnType == typeof(int))
                return new IntegerResult((int)func.Invoke(geometry, actualArgs));
            string opName = String.Format("Unsupported result type: {0}", func.ReturnType);
            throw new NTSTestReflectionException(opName);
        }
    }
}