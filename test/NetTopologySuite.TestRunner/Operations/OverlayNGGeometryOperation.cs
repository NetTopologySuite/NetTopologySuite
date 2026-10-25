using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;
using Open.Topology.TestRunner.Result;
using SpatialFunction = NetTopologySuite.Operation.Overlay.SpatialFunction;

namespace Open.Topology.TestRunner.Operations
{
    /// <summary>
    /// Runs the overlay operations of a case through <see cref="OverlayNG"/>.
    /// <para/>
    /// The <c>*NG</c> operations cannot be reached by handing the geometries a factory whose
    /// overlay engine is <see cref="GeometryOverlay.NG"/>: <see cref="Geometry.Union(Geometry)"/>,
    /// <see cref="Geometry.Difference"/> and <see cref="Geometry.SymmetricDifference"/> reject a
    /// <c>GeometryCollection</c> argument before dispatching to the engine, so the cases the
    /// suffix exists to cover would never get there. The upstream runner calls OverlayNG
    /// directly for the same reason.
    /// </summary>
    public class OverlayNGGeometryOperation : IGeometryOperation
    {
        private readonly IGeometryOperation _other;

        /// <summary>
        /// Creates an operation that runs the overlay operations through OverlayNG.
        /// </summary>
        /// <param name="other">
        /// Handles the operations this one does not, if a case brings its own. A case that
        /// declares no operation never needs it: the name carrying the <c>NG</c> suffix is the
        /// only one this instance is asked about.
        /// </param>
        public OverlayNGGeometryOperation(IGeometryOperation other = null)
        {
            _other = other;
        }

        /// <summary>
        /// The type an operation returns, which for the overlay ones is always a geometry.
        /// </summary>
        public Type GetReturnType(XmlTestType opName)
        {
            return IsOverlay(opName) ? typeof(Geometry) : Other(opName).GetReturnType(opName);
        }

        /// <summary>
        /// Runs an overlay operation through <see cref="OverlayNG"/>, and hands anything else to
        /// the operation this one was given.
        /// </summary>
        public IResult Invoke(XmlTestType opName, Geometry geometry, object[] args)
        {
            switch (opName)
            {
                case XmlTestType.Intersection:
                    return Overlay(geometry, args, SpatialFunction.Intersection);
                case XmlTestType.Difference:
                    return Overlay(geometry, args, SpatialFunction.Difference);
                case XmlTestType.SymmetricDifference:
                    return Overlay(geometry, args, SpatialFunction.SymDifference);
                case XmlTestType.Union:
                    return Overlay(geometry, args, SpatialFunction.Union);
                default:
                    return Other(opName).Invoke(opName, geometry, args);
            }
        }

        private static bool IsOverlay(XmlTestType opName)
        {
            return opName == XmlTestType.Intersection
                || opName == XmlTestType.Difference
                || opName == XmlTestType.SymmetricDifference
                || opName == XmlTestType.Union;
        }

        private IGeometryOperation Other(XmlTestType opName)
        {
            return _other ?? throw new NotSupportedException(
                $"'{opName}' does not carry the NG suffix, so it should not have reached {nameof(OverlayNGGeometryOperation)}.");
        }

        private static IResult Overlay(Geometry geometry, object[] args, SpatialFunction op)
        {
            // Left alone, a missing or non-geometry operand would become a null one, and the
            // overlay would answer for a different pair of inputs than the case describes.
            object operand = args.Length > 0 ? args[0] : null;
            if (!(operand is Geometry other))
            {
                throw new ArgumentException(
                    $"An overlay case needs a geometry as its second operand, not '{operand ?? "nothing"}'.",
                    nameof(args));
            }

            return new GeometryResult(OverlayNG.Overlay(geometry, other, op));
        }
    }
}
