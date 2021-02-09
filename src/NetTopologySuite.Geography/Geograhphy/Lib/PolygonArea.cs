using System;

namespace NetTopologySuite.Geography.Lib
{
    /**
     * Polygon areas.
     * <p>
     * This computes the area of a geodesic polygon using the method given
     * Section 6 of
     * <ul>
     * <li>
     *   C. F. F. Karney,
     *   <a href="https://doi.org/10.1007/s00190-012-0578-z">
     *   Algorithms for geodesics</a>,
     *   J. Geodesy <b>87</b>, 43&ndash;55 (2013)
     *   (<a href="https://geographiclib.sourceforge.io/geod-addenda.html">
     *   addenda</a>).
     * </ul>
     * <p>
     * Arbitrarily complex polygons are allowed.  In the case self-intersecting of
     * polygons the area is accumulated "algebraically", e.g., the areas of the 2
     * loops in a figure-8 polygon will partially cancel.
     * <p>
     * This class lets you add vertices one at a time to the polygon.  The area
     * and perimeter are accumulated at two times the standard floating point
     * precision to guard against the loss of accuracy with many-sided polygons.
     * At any point you can ask for the perimeter and area so far.  There's an
     * option to treat the points as defining a polyline instead of a polygon; in
     * that case, only the perimeter is computed.
     * <p>
     * Example of use:
     * <pre>
     * {@code
     * // Compute the area of a geodesic polygon.
     *
     * // This program reads lines with lat, lon for each vertex of a polygon.
     * // At the end of input, the program prints the number of vertices,
     * // the perimeter of the polygon and its area (for the WGS84 ellipsoid).
     *
     * import java.util.*;
     * import net.sf.geographiclib.*;
     *
     * public class Planimeter {
     *   public static void main(String[] args) {
     *     PolygonArea p = new PolygonArea(Geodesic.WGS84, false);
     *     try {
     *       Scanner in = new Scanner(System.in);
     *       while (true) {
     *         double lat = in.nextDouble(), lon = in.nextDouble();
     *         p.AddPoint(lat, lon);
     *       }
     *     }
     *     catch (Exception e) {}
     *     PolygonResult r = p.Compute();
     *     System.out.println(r.num + " " + r.perimeter + " " + r.area);
     *   }
     * }}</pre>
     **********************************************************************/
    internal class PolygonArea
    {

        private Geodesic _earth;
        private double _area0;        // Full ellipsoid area
        private bool _polyline;    // Assume polyline (don't close and skip area)
        private int _mask;
        private int _num;
        private int _crossings;
        private Accumulator _areasum, _perimetersum;
        private double _lat0, _lon0, _lat1, _lon1;
        private static int transit(double lon1, double lon2)
        {
            // Return 1 or -1 if crossing prime meridian in east or west direction.
            // Otherwise return zero.
            // Compute lon12 the same way as Geodesic.Inverse.
            lon1 = GeoMath.AngNormalize(lon1);
            lon2 = GeoMath.AngNormalize(lon2);
            (double first, double second) p = (0d, 0d);
            GeoMath.AngDiff(ref p, lon1, lon2);
            double lon12 = p.first;
            int cross =
              lon1 <= 0 && lon2 > 0 && lon12 > 0 ? 1 :
              (lon2 <= 0 && lon1 > 0 && lon12 < 0 ? -1 : 0);
            return cross;
        }
        // an alternate version of transit to deal with longitudes in the direct
        // problem.
        private static int transitdirect(double lon1, double lon2)
        {
            // We want to compute exactly
            //   int(ceil(lon2 / 360)) - int(ceil(lon1 / 360))
            // Since we only need the parity of the result we can use std::remquo but
            // this is buggy with g++ 4.8.3 and requires C++11.  So instead we do
            lon1 = lon1 % 720.0; lon2 = lon2 % 720.0;
            return (((lon2 <= 0 && lon2 > -360) || lon2 > 360 ? 1 : 0) -
                     ((lon1 <= 0 && lon1 > -360) || lon1 > 360 ? 1 : 0));
        }
        // reduce Accumulator area to allowed range
        private static double AreaReduceA(Accumulator area, double area0,
                                          int crossings,
                                          bool reverse, bool sign)
        {
            area.Remainder(area0);
            if ((crossings & 1) != 0)
                area.Add((area.Sum() < 0 ? 1 : -1) * area0 / 2);
            // area is with the clockwise sense.  If !reverse convert to
            // counter-clockwise convention.
            if (!reverse)
                area.Negate();
            // If sign put area in (-area0/2, area0/2], else put area in [0, area0)
            if (sign)
            {
                if (area.Sum() > area0 / 2)
                    area.Add(-area0);
                else if (area.Sum() <= -area0 / 2)
                    area.Add(+area0);
            }
            else
            {
                if (area.Sum() >= area0)
                    area.Add(-area0);
                else if (area.Sum() < 0)
                    area.Add(+area0);
            }
            return 0 + area.Sum();
        }
        // reduce double area to allowed range
        private static double AreaReduceB(double area, double area0,
                                          int crossings,
                                          bool reverse, bool sign)
        {
            area = GeoMath.remainder(area, area0);
            if ((crossings & 1) != 0)
                area += (area < 0 ? 1 : -1) * area0 / 2;
            // area is with the clockwise sense.  If !reverse convert to
            // counter-clockwise convention.
            if (!reverse)
                area *= -1;
            // If sign put area in (-area0/2, area0/2], else put area in [0, area0)
            if (sign)
            {
                if (area > area0 / 2)
                    area -= area0;
                else if (area <= -area0 / 2)
                    area += area0;
            }
            else
            {
                if (area >= area0)
                    area -= area0;
                else if (area < 0)
                    area += area0;
            }
            return 0 + area;
        }

        /**
         * Constructor for PolygonArea.
         * <p>
         * @param earth the Geodesic object to use for geodesic calculations.
         * @param polyline if true that treat the points as defining a polyline
         *   instead of a polygon.
         **********************************************************************/
        public PolygonArea(Geodesic earth, bool polyline)
        {
            _earth = earth;
            _area0 = _earth.EllipsoidArea();
            _polyline = polyline;
            _mask = GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE |
              GeodesicMask.DISTANCE |
              (_polyline ? GeodesicMask.NONE :
               GeodesicMask.AREA | GeodesicMask.LONG_UNROLL);
            _perimetersum = new Accumulator(0);
            if (!_polyline)
                _areasum = new Accumulator(0);
            Clear();
        }

        /**
         * Clear PolygonArea, allowing a new polygon to be started.
         **********************************************************************/
        public void Clear()
        {
            _num = 0;
            _crossings = 0;
            _perimetersum.Set(0);
            if (!_polyline) _areasum.Set(0);
            _lat0 = _lon0 = _lat1 = _lon1 = double.NaN;
        }

        /**
         * Add a point to the polygon or polyline.
         * <p>
         * @param lat the latitude of the point (degrees).
         * @param lon the latitude of the point (degrees).
         * <p>
         * <i>lat</i> should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public void AddPoint(double lat, double lon)
        {
            lon = GeoMath.AngNormalize(lon);
            if (_num == 0)
            {
                _lat0 = _lat1 = lat;
                _lon0 = _lon1 = lon;
            }
            else
            {
                var g = _earth.Inverse(_lat1, _lon1, lat, lon, _mask);
                _perimetersum.Add(g.s12);
                if (!_polyline)
                {
                    _areasum.Add(g.S12);
                    _crossings += transit(_lon1, lon);
                }
                _lat1 = lat; _lon1 = lon;
            }
            ++_num;
        }

        /**
         * Add an edge to the polygon or polyline.
         * <p>
         * @param azi azimuth at current point (degrees).
         * @param s distance from current point to next point (meters).
         * <p>
         * This does nothing if no points have been added yet.  Use
         * PolygonArea.CurrentPoint to determine the position of the new vertex.
         **********************************************************************/
        public void AddEdge(double azi, double s)
        {
            if (_num > 0)
            {             // Do nothing if _num is zero
                var g = _earth.Direct(_lat1, _lon1, azi, s, _mask);
                _perimetersum.Add(g.s12);
                if (!_polyline)
                {
                    _areasum.Add(g.S12);
                    _crossings += transitdirect(_lon1, g.lon2);
                }
                _lat1 = g.lat2; _lon1 = g.lon2;
                ++_num;
            }
        }

        /**
         * Return the results so far.
         * <p>
         * @return PolygonResult(<i>num</i>, <i>perimeter</i>, <i>area</i>) where
         *   <i>num</i> is the number of vertices, <i>perimeter</i> is the perimeter
         *   of the polygon or the length of the polyline (meters), and <i>area</i>
         *   is the area of the polygon (meters<sup>2</sup>) or Double.NaN of
         *   <i>polyline</i> is true in the constructor.
         * <p>
         * Counter-clockwise traversal counts as a positive area.
         **********************************************************************/
        public PolygonResult Compute() { return Compute(false, true); }
        /**
         * Return the results so far.
         * <p>
         * @param reverse if true then clockwise (instead of counter-clockwise)
         *   traversal counts as a positive area.
         * @param sign if true then return a signed result for the area if
         *   the polygon is traversed in the "wrong" direction instead of returning
         *   the area for the rest of the earth.
         * @return PolygonResult(<i>num</i>, <i>perimeter</i>, <i>area</i>) where
         *   <i>num</i> is the number of vertices, <i>perimeter</i> is the perimeter
         *   of the polygon or the length of the polyline (meters), and <i>area</i>
         *   is the area of the polygon (meters<sup>2</sup>) or Double.NaN of
         *   <i>polyline</i> is true in the constructor.
         * <p>
         * More points can be added to the polygon after this call.
         **********************************************************************/
        public PolygonResult Compute(bool reverse, bool sign)
        {
            if (_num < 2)
                return new PolygonResult(_num, 0, _polyline ? double.NaN : 0);
            if (_polyline)
                return new PolygonResult(_num, _perimetersum.Sum(), double.NaN);

            var g = _earth.Inverse(_lat1, _lon1, _lat0, _lon0, _mask);
            var tempsum = new Accumulator(_areasum);
            tempsum.Add(g.S12);

            return new PolygonResult(_num, _perimetersum.Sum(g.s12),
                                AreaReduceA(tempsum, _area0,
                                            _crossings + transit(_lon1, _lon0),
                                            reverse, sign));
        }

        /**
         * Return the results assuming a tentative final test point is added;
         * however, the data for the test point is not saved.  This lets you report
         * a running result for the perimeter and area as the user moves the mouse
         * cursor.  Ordinary floating point arithmetic is used to accumulate the
         * data for the test point; thus the area and perimeter returned are less
         * accurate than if AddPoint and Compute are used.
         * <p>
         * @param lat the latitude of the test point (degrees).
         * @param lon the longitude of the test point (degrees).
         * @param reverse if true then clockwise (instead of counter-clockwise)
         *   traversal counts as a positive area.
         * @param sign if true then return a signed result for the area if
         *   the polygon is traversed in the "wrong" direction instead of returning
         *   the area for the rest of the earth.
         * @return PolygonResult(<i>num</i>, <i>perimeter</i>, <i>area</i>) where
         *   <i>num</i> is the number of vertices, <i>perimeter</i> is the perimeter
         *   of the polygon or the length of the polyline (meters), and <i>area</i>
         *   is the area of the polygon (meters<sup>2</sup>) or Double.NaN of
         *   <i>polyline</i> is true in the constructor.
         * <p>
         * <i>lat</i> should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public PolygonResult TestPoint(double lat, double lon,
                                       bool reverse, bool sign)
        {
            if (_num == 0)
                return new PolygonResult(1, 0, _polyline ? double.NaN : 0);

            double perimeter = _perimetersum.Sum();
            double tempsum = _polyline ? 0 : _areasum.Sum();
            int crossings = _crossings;
            int num = _num + 1;
            for (int i = 0; i < (_polyline ? 1 : 2); ++i)
            {
                var g =
                  _earth.Inverse(i == 0 ? _lat1 : lat, i == 0 ? _lon1 : lon,
                                 i != 0 ? _lat0 : lat, i != 0 ? _lon0 : lon,
                                 _mask);
                perimeter += g.s12;
                if (!_polyline)
                {
                    tempsum += g.S12;
                    crossings += transit(i == 0 ? _lon1 : lon,
                                         i != 0 ? _lon0 : lon);
                }
            }

            if (_polyline)
                return new PolygonResult(num, perimeter, double.NaN);

            return new PolygonResult(num, perimeter,
                                     AreaReduceB(tempsum, _area0, crossings,
                                                 reverse, sign));
        }

        /**
         * Return the results assuming a tentative final test point is added via an
         * azimuth and distance; however, the data for the test point is not saved.
         * This lets you report a running result for the perimeter and area as the
         * user moves the mouse cursor.  Ordinary floating point arithmetic is used
         * to accumulate the data for the test point; thus the area and perimeter
         * returned are less accurate than if AddPoint and Compute are used.
         * <p>
         * @param azi azimuth at current point (degrees).
         * @param s distance from current point to final test point (meters).
         * @param reverse if true then clockwise (instead of counter-clockwise)
         *   traversal counts as a positive area.
         * @param sign if true then return a signed result for the area if
         *   the polygon is traversed in the "wrong" direction instead of returning
         *   the area for the rest of the earth.
         * @return PolygonResult(<i>num</i>, <i>perimeter</i>, <i>area</i>) where
         *   <i>num</i> is the number of vertices, <i>perimeter</i> is the perimeter
         *   of the polygon or the length of the polyline (meters), and <i>area</i>
         *   is the area of the polygon (meters<sup>2</sup>) or Double.NaN of
         *   <i>polyline</i> is true in the constructor.
         **********************************************************************/
        public PolygonResult TestEdge(double azi, double s,
                                      bool reverse, bool sign)
        {
            if (_num == 0)              // we don't have a starting point!
                return new PolygonResult(0, double.NaN, double.NaN);

            int num = _num + 1;
            double perimeter = _perimetersum.Sum() + s;
            if (_polyline)
                return new PolygonResult(num, perimeter, double.NaN);

            double tempsum = _areasum.Sum();
            int crossings = _crossings;
            {
                double lat, lon, s12, S12, t;
                var g =
                  _earth.Direct(_lat1, _lon1, azi, false, s, _mask);
                tempsum += g.S12;
                crossings += transitdirect(_lon1, g.lon2);
                crossings += transit(g.lon2, _lon0);
                g = _earth.Inverse(g.lat2, g.lon2, _lat0, _lon0, _mask);
                perimeter += g.s12;
                tempsum += g.S12;
            }

            return new PolygonResult(num, perimeter,
                                     AreaReduceB(tempsum, _area0, crossings,
                                                 reverse, sign));
        }

        /**
         * @return <i>a</i> the equatorial radius of the ellipsoid (meters).  This is
         *   the value inherited from the Geodesic object used in the constructor.
         **********************************************************************/
        public double EquatorialRadius() { return _earth.EquatorialRadius(); }

        /**
         * @return <i>f</i> the flattening of the ellipsoid.  This is the value
         *   inherited from the Geodesic object used in the constructor.
         **********************************************************************/
        public double Flattening() { return _earth.Flattening(); }

        /**
         * Report the previous vertex added to the polygon or polyline.
         * <p>
         * @return Pair(<i>lat</i>, <i>lon</i>), the current latitude and longitude.
         * <p>
         * If no points have been added, then Double.NaN is returned.  Otherwise,
         * <i>lon</i> will be in the range [&minus;180&deg;, 180&deg;].
         **********************************************************************/
        public (double first, double second) CurrentPoint() { return (_lat1, _lon1); }

        /**
         * @deprecated An old name for {@link #EquatorialRadius()}.
         * @return <i>a</i> the equatorial radius of the ellipsoid (meters).
         **********************************************************************/
       [Obsolete]
      public double MajorRadius() { return EquatorialRadius(); }
    }

}
