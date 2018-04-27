using System;
using GeoAPI.Geometries;
using Open.Topology.TestRunner.Result;

namespace Open.Topology.TestRunner.Operations
{

    /// <summary>
    /// Interface for classes which execute operations on {@link Geometry}s.
    /// The arguments may be presented as Strings, even if they
    /// should be calling a method with non-String arguments.
    /// Geometry will always be supplied as Geometry objects, however.
    /// This interface abstracts out the invocation of a method
    /// on a Geometry during a Test.  Subclasses can provide substitute
    /// or additional methods during runs of the same test file.
    /// </summary>
    /// <author>Martin Davis</author>
    public interface IGeometryOperation
    {
        /// <summary>
        /// Gets the type of the return type of the given operation.
        /// </summary>
        /// <param name="opName">The name of the operation</param>
        /// <returns>The return type of the specified operation</returns>
        Type GetReturnType(XmlTestType opName);

        /// <summary>
        /// Invokes an operation on a <see cref="IGeometry"/>.
        /// </summary>
        /// <param name="opName">The name of the operation</param>
        /// <param name="geometry">The geometry to process</param>
        /// <param name="args">The arguments to the operation (which may be typed as Strings)</param>
        /// <exception cref="Exception">If some error was encountered trying to find or process the operation</exception>
        IResult Invoke(XmlTestType opName, IGeometry geometry, Object[] args);
    }
}