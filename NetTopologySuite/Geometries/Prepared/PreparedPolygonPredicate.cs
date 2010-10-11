using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Locate;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    ///</summary>
    public abstract class PreparedPolygonPredicate<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly IPointOnGeometryLocator<TCoordinate> _targetPointLocator;
        protected PreparedPolygon<TCoordinate> _prepPoly;

        ///<summary>
        /// Creates an instance of this operation.
        ///</summary>
        /// <param name="prepPoly">the prepared polygon</param>
        public PreparedPolygonPredicate(PreparedPolygon<TCoordinate> prepPoly)
        {
            _prepPoly = prepPoly;
            _targetPointLocator = prepPoly.PointLocator;
        }

        ///<summary>
        /// Tests whether all components of the test Geometry  are contained in the target geometry.
        /// Handles both linear and point components.
        ///</summary>
        /// <param name="testGeom">a geometry to test</param>
        /// <returns>true if all componenta of the argument are contained in the target geometry</returns>
        protected Boolean IsAllTestComponentsInTarget(IGeometry<TCoordinate> testGeom)
        {
            foreach (TCoordinate p in testGeom.Coordinates)
            {
                Locations loc = _targetPointLocator.Locate(p);
                if (loc == Locations.Exterior)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Tests whether all components of the test Geometry are contained in the interior of the target geometry.
        /// Handles both linear and point components.
        /// </summary>
        /// <param name="testGeom">a geometry to test</param>
        /// <returns>true if all componenta of the argument are contained in the target geometry interior</returns>
        protected Boolean IsAllTestComponentsInTargetInterior(IGeometry<TCoordinate> testGeom)
        {
            foreach (TCoordinate p in testGeom.Coordinates)
            {
                Locations loc = _targetPointLocator.Locate(p);
                if (loc == Locations.Interior)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Tests whether any component of the test Geometry intersects 
        /// Handles test geometries with both linear and point components.
        /// </summary>
        /// <param name="testGeom">a geometry to test</param>
        /// <returns>true if any component of the argument intersects the prepared area geometry the area of the target geometry.</returns>
        protected Boolean IsAnyTestComponentInTarget(IGeometry<TCoordinate> testGeom)
        {
            foreach (TCoordinate p in testGeom.Coordinates)
            {
                Locations loc = _targetPointLocator.Locate(p);
                if (loc != Locations.Exterior)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether any component of the test Geometry intersects the interior of the target geometry.
        /// Handles test geometries with both linear and point components.
        /// </summary>
        /// <param name="testGeom">a geometry to test</param>
        /// <returns>true if any component of the argument intersects the prepared area geometry interior</returns>
        protected Boolean IsAnyTestComponentInTargetInterior(IGeometry<TCoordinate> testGeom)
        {
            foreach (TCoordinate p in testGeom.Coordinates)
            {
                Locations loc = _targetPointLocator.Locate(p);
                if (loc != Locations.Interior)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether any component of the target geometry intersects the test geometry (which must be an areal geometry) 
        /// </summary>
        /// <param name="testGeom">the test geometry</param>
        /// <param name="targetRepPts">the representative points of the target geometry</param>
        /// <returns>true if any component intersects the areal test geometry</returns>
        protected Boolean IsAnyTargetComponentInAreaTest(IGeometry<TCoordinate> testGeom,
                                                         IEnumerable<TCoordinate> targetRepPts)
        {
            IPointOnGeometryLocator<TCoordinate> piaLoc = new SimplePointInAreaLocator<TCoordinate>(testGeom);
            foreach (TCoordinate p in targetRepPts)
            {
                Locations loc = piaLoc.Locate(p);
                if (loc != Locations.Exterior)
                    return true;
            }
            return false;
        }
    }
}