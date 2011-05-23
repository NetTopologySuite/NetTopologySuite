using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Locate;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// A base class for predicate operations on <see cref="PreparedPolygon"/>s.
    ///</summary>
    /// <author>mbdavis</author>
    public abstract class PreparedPolygonPredicate
    {
        protected PreparedPolygon prepPoly;
        private readonly IPointOnGeometryLocator _targetPointLocator;

        ///<summary>
        /// Creates an instance of this operation.
        ///</summary>
        /// <param name="prepPoly">the PreparedPolygon to evaluate</param>
        protected PreparedPolygonPredicate(PreparedPolygon prepPoly)
        {
            this.prepPoly = prepPoly;
            _targetPointLocator = prepPoly.PointLocator;
        }

        ///<summary>
        /// Tests whether all components of the test Geometry are contained in the target geometry.
        ///</summary>
        /// <remarks>Handles both linear and point components.</remarks>
        /// <param name="testGeom">A geometry to test</param>
        /// <returns>
        /// true if all componenta of the argument are contained in the target geometry
        /// </returns>
        protected bool IsAllTestComponentsInTarget(IGeometry testGeom)
        {
            IList<ICoordinate> coords = ComponentCoordinateExtracter.GetCoordinates(testGeom);
            foreach (ICoordinate p in coords)
            {
                Locations loc = _targetPointLocator.Locate(p);
                if (loc == Locations.Exterior)
                    return false;
            }
            return true;
        }

        /**
         * Tests whether all components of the test Geometry 
           * are contained in the interior of the target geometry.
         * Handles both linear and point components.
         * 
         * @param geom a geometry to test
         * @return true if all componenta of the argument are contained in the target geometry interior
         */
        protected bool IsAllTestComponentsInTargetInterior(IGeometry testGeom)
        {
            IList<ICoordinate> coords = ComponentCoordinateExtracter.GetCoordinates(testGeom);
            foreach (ICoordinate p in coords)
            {
                Locations loc = _targetPointLocator.Locate(p);
                if (loc != Locations.Interior)
                    return false;
            }
            return true;
        }

        /**
         * Tests whether any component of the test Geometry intersects
         * the area of the target geometry.
         * Handles test geometries with both linear and point components.
         * 
         * @param geom a geometry to test
         * @return true if any component of the argument intersects the prepared area geometry
         */
        protected bool IsAnyTestComponentInTarget(IGeometry testGeom)
        {
            IList<ICoordinate> coords = ComponentCoordinateExtracter.GetCoordinates(testGeom);
            foreach (ICoordinate p in coords)
            {
                Locations loc = _targetPointLocator.Locate(p);
                if (loc != Locations.Exterior)
                    return true;
            }
            return false;
        }

        /**
         * Tests whether any component of the test Geometry intersects
         * the interior of the target geometry.
         * Handles test geometries with both linear and point components.
         * 
         * @param geom a geometry to test
         * @return true if any component of the argument intersects the prepared area geometry interior
         */
        protected bool IsAnyTestComponentInTargetInterior(IGeometry testGeom)
        {
            IList<ICoordinate> coords = ComponentCoordinateExtracter.GetCoordinates(testGeom);
            foreach (ICoordinate p in coords)
            {
                Locations loc = _targetPointLocator.Locate(p);
                if (loc == Locations.Interior)
                    return true;
            }
            return false;
        }


        /**
         * Tests whether any component of the target geometry 
         * intersects the test geometry (which must be an areal geometry) 
         * 
         * @param geom the test geometry
         * @param repPts the representative points of the target geometry
         * @return true if any component intersects the areal test geometry
         */
        protected bool IsAnyTargetComponentInAreaTest(IGeometry testGeom, IList<ICoordinate> targetRepPts)
        {
            IPointOnGeometryLocator piaLoc = new SimplePointInAreaLocator(testGeom);
            foreach (ICoordinate p in targetRepPts)
            {
                Locations loc = piaLoc.Locate(p);
                if (loc != Locations.Exterior)
                    return true;
            }
            return false;
        }

    }
}