using System;
using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    /// <summary>
    /// A reification of a function which can be executed on a
    /// <see cref="IGeometry"/>, possibly with other arguments.
    /// The function may return a Geometry or a scalar value.
    /// </summary>
    /// <author>Martin Davis</author>
    public interface IGeometryFunction
    {
        /// <summary>
        /// Gets the category name of this function
        /// </summary>
        String Category { get; }

        /// <summary>
        /// Gets the name of this function
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the parameter names for this function
        /// </summary>
        String[] ParameterNames { get; }

        /// <summary>
        /// Gets the types of the other function arguments, if any.
        /// </summary>
        Type[] ParameterTypes { get; }

        /// <summary>
        /// Gets the return type of this function
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Gets a string representing the signature of this function.
        /// </summary>
        String Signature { get; }

        /// <summary>
        /// Invokes this function.
        /// </summary>
        /// <remarks>Note that any exceptions returned must be <see cref="Exception"/></remarks>
        /// <param name="geom">The target geometry</param>
        /// <param name="args">The other arguments to the function</param>
        /// <returns>The value computed by the function</returns>
        Object Invoke(IGeometry geom, Object[] args);        
    }
}