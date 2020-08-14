using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// A base class for predicate operations on <see cref="PreparedPolygon"/>s.
    /// </summary>
    /// <author>mbdavis</author>
    internal abstract class PreparedPolygonPredicate
    {
        protected PreparedPolygon prepPoly;
        private readonly IPointOnGeometryLocator _targetPointLocator;

        /// <summary>
        /// Creates an instance of this operation.
        /// </summary>
        /// <param name="prepPoly">the PreparedPolygon to evaluate</param>
        protected PreparedPolygonPredicate(PreparedPolygon prepPoly)
        {
            this.prepPoly = prepPoly;
            _targetPointLocator = prepPoly.PointLocator;
        }

        /// <summary>
        /// Tests whether all components of the test Geometry are contained in the target geometry.
        /// </summary>
        /// <remarks>Handles both linear and point components.</remarks>
        /// <param name="testGeom">A geometry to test</param>
        /// <returns>
        /// true if all components of the argument are contained in the target geometry
        /// </returns>
        protected bool IsAllTestComponentsInTarget(Geometry testGeom)
        {
            var coords = ComponentCoordinateExtracter.GetCoordinates(testGeom);
            foreach (var p in coords)
            {
                var loc = _targetPointLocator.Locate(p);
                if (loc == Location.Exterior)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Tests whether all components of the test Geometry are contained in the interior of the target geometry.
        /// </summary>
        /// <remarks>Handles both linear and point components.</remarks>
        /// <param name="testGeom">A geometry to test</param>
        /// <returns>true if all components of the argument are contained in the target geometry interior</returns>
        protected bool IsAllTestComponentsInTargetInterior(Geometry testGeom)
        {
            var coords = ComponentCoordinateExtracter.GetCoordinates(testGeom);
            foreach (var p in coords)
            {
                var loc = _targetPointLocator.Locate(p);
                if (loc != Location.Interior)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Tests whether any component of the test Geometry intersects
        /// the area of the target geometry.
        /// Handles test geometries with both linear and point components.
        /// </summary>
        /// <param name="testGeom">A geometry to test</param>
        /// <returns>true if any component of the argument intersects the prepared area geometry</returns>
        protected bool IsAnyTestComponentInTarget(Geometry testGeom)
        {
            var coords = ComponentCoordinateExtracter.GetCoordinates(testGeom);
            foreach (var p in coords)
            {
                var loc = _targetPointLocator.Locate(p);
                if (loc != Location.Exterior)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether any point of the test Geometry intersects the interior of the target geometry.
        /// </summary>
        /// <remarks>Handles test geometries with both linear and point components.</remarks>
        /// <param name="testGeom">A geometry to test</param>
        /// <returns>true if any point of the argument intersects the prepared area geometry</returns>
        protected bool IsAllTestPointsInTarget(Geometry testGeom)
        {
            for (int i = 0; i < testGeom.NumGeometries; i++)
            {
                var pt = (Point)testGeom.GetGeometryN(i);
                var p = pt.Coordinate;
                var loc = _targetPointLocator.Locate(p);
                if (loc != Location.Exterior)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether any point of the test Geometry intersects the interior of the target geometry.
        /// </summary>
        /// <param name="testGeom">A geometry to test</param>
        /// <returns>true if any point of the argument intersects the prepared area geometry interior</returns>
        protected bool IsAnyTestPointInTargetInterior(Geometry testGeom)
        {
            for (int i = 0; i < testGeom.NumGeometries; i++)
            {
                var pt = (Point)testGeom.GetGeometryN(i);
                var p = pt.Coordinate;
                var loc = _targetPointLocator.Locate(p);
                if (loc == Location.Interior)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether any component of the test Geometry intersects the interior of the target geometry.
        /// </summary>
        /// <remarks>Handles test geometries with both linear and point components.</remarks>
        /// <param name="testGeom">A geometry to test</param>
        /// <returns>true if any component of the argument intersects the prepared area geometry interior</returns>
        [Obsolete]
        protected bool IsAnyTestComponentInTargetInterior(Geometry testGeom)
        {
            var coords = ComponentCoordinateExtracter.GetCoordinates(testGeom);
            foreach (var p in coords)
            {
                var loc = _targetPointLocator.Locate(p);
                if (loc == Location.Interior)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether all points of the test Pointal geometry
        /// are contained in the target geometry.
        /// </summary>
        /// <param name="testGeom">A geometry to test</param>
        /// <returns>true if all points of the argument are contained in the target geometry</returns>
        protected bool IsAllTestPointsInTargetInterior(Geometry testGeom)
        {
            for (int i = 0; i < testGeom.NumGeometries; i++)
            {
                var pt = (Point)testGeom.GetGeometryN(i);
                var p = pt.Coordinate;
                var loc = _targetPointLocator.Locate(p);
                if (loc == Location.Exterior)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Tests whether any component of the target geometry intersects the test geometry (which must be an areal geometry)
        /// </summary>
        /// <param name="testGeom">The test geometry</param>
        /// <param name="targetRepPts">The representative points of the target geometry</param>
        /// <returns>true if any component intersects the areal test geometry</returns>
        protected bool IsAnyTargetComponentInAreaTest(Geometry testGeom, IList<Coordinate> targetRepPts)
        {
            var piaLoc = new SimplePointInAreaLocator(testGeom);
            foreach (var p in targetRepPts)
            {
                var loc = piaLoc.Locate(p);
                if (loc != Location.Exterior)
                    return true;
            }
            return false;
        }
    }
}
