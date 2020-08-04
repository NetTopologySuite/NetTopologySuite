using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;

namespace NetTopologySuite.Operation.Overlay.Validate
{
    /// <summary>
    /// Validates that the result of an overlay operation is geometrically correct within a determined tolerance.
    /// Uses fuzzy point location to find points which are
    /// definitely in either the interior or exterior of the result
    /// geometry, and compares these results with the expected ones.
    /// </summary>
    /// <remarks>
    /// This algorithm is only useful where the inputs are polygonal.
    /// This is a heuristic test, and may return false positive results
    /// (I.e. it may fail to detect an invalid result.)
    /// It should never return a false negative result, however
    /// (I.e. it should never report a valid result as invalid.)
    /// </remarks>
    /// <author>Martin Davis</author>
    /// <see cref="OverlayOp"/>
    public class OverlayResultValidator
    {
        public static bool IsValid(Geometry a, Geometry b, SpatialFunction overlayOp, Geometry result)
        {
            var validator = new OverlayResultValidator(a, b, result);
            return validator.IsValid(overlayOp);
        }

        private static double ComputeBoundaryDistanceTolerance(Geometry g0, Geometry g1)
        {
            return Math.Min(GeometrySnapper.ComputeSizeBasedSnapTolerance(g0),
                    GeometrySnapper.ComputeSizeBasedSnapTolerance(g1));
        }

        private const double Tolerance = 0.000001;

        private readonly Geometry[] _geom;
        private readonly FuzzyPointLocator[] _locFinder;
        private readonly Location[] _location = new Location[3];
        private readonly double _boundaryDistanceTolerance = Tolerance;
        private readonly List<Coordinate> _testCoords = new List<Coordinate>();

        private Coordinate _invalidLocation;

        public OverlayResultValidator(Geometry a, Geometry b, Geometry result)
        {
            /*
             * The tolerance to use needs to depend on the size of the geometries.
             * It should not be more precise than double-precision can support.
             */
            _boundaryDistanceTolerance = ComputeBoundaryDistanceTolerance(a, b);
            _geom = new[] {a, b, result};
            _locFinder = new[]
                             {
                                 new FuzzyPointLocator(_geom[0], _boundaryDistanceTolerance),
                                 new FuzzyPointLocator(_geom[1], _boundaryDistanceTolerance),
                                 new FuzzyPointLocator(_geom[2], _boundaryDistanceTolerance)
                             };
        }

        public bool IsValid(SpatialFunction overlayOp)
        {
            AddTestPts(_geom[0]);
            AddTestPts(_geom[1]);
            bool isValid = CheckValid(overlayOp);

            /*
            System.out.println("OverlayResultValidator: " + isValid);
            System.out.println("G0");
            System.out.println(geom[0]);
            System.out.println("G1");
            System.out.println(geom[1]);
            System.out.println("Result");
            System.out.println(geom[2]);
            */

            return isValid;
        }

        public Coordinate InvalidLocation => _invalidLocation;

        private void AddTestPts(Geometry g)
        {
            var ptGen = new OffsetPointGenerator(g);
            _testCoords.AddRange(ptGen.GetPoints(5 * _boundaryDistanceTolerance));
        }

        private bool CheckValid(SpatialFunction overlayOp)
        {
            for (int i = 0; i < _testCoords.Count; i++)
            {
                var pt = _testCoords[i];
                if (!CheckValid(overlayOp, pt))
                {
                    _invalidLocation = pt;
                    return false;
                }
            }
            return true;
        }

        private bool CheckValid(SpatialFunction overlayOp, Coordinate pt)
        {
            _location[0] = _locFinder[0].GetLocation(pt);
            _location[1] = _locFinder[1].GetLocation(pt);
            _location[2] = _locFinder[2].GetLocation(pt);

            /*
             * If any location is on the Boundary, can't deduce anything, so just return true
             */
            if (HasLocation(_location, Location.Boundary))
                return true;

            return IsValidResult(overlayOp, _location);
        }

        private static bool HasLocation(Location[] location, Location loc)
        {
            for (int i = 0; i < 3; i++)
            {
                if (location[i] == loc)
                    return true;
            }
            return false;
        }

        private static bool IsValidResult(SpatialFunction overlayOp, Location[] location)
        {
            bool expectedInterior = OverlayOp.IsResultOfOp(location[0], location[1], overlayOp);

            bool resultInInterior = (location[2] == Location.Interior);
            // MD use simpler: boolean isValid = (expectedInterior == resultInInterior);
            bool isValid = !(expectedInterior ^ resultInInterior);

            if (!isValid) ReportResult(overlayOp, location, expectedInterior);

            return isValid;
        }

        private static void ReportResult(SpatialFunction overlayOp, Location[] location, bool expectedInterior)
        {
// ReSharper disable RedundantStringFormatCall
            // String.Format needed to build 2.0 release!
            Debug.WriteLine(string.Format("{0}:" + " A:{1} B:{2} expected:{3} actual:{4}",
                overlayOp,
                LocationUtility.ToLocationSymbol(location[0]),
                LocationUtility.ToLocationSymbol(location[1]), expectedInterior ? 'i' : 'e',
                LocationUtility.ToLocationSymbol(location[2])));
// ReSharper restore RedundantStringFormatCall
        }
    }
}
