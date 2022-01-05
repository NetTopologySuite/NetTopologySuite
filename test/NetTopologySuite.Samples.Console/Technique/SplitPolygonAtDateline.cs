using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Precision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Samples.Technique
{
    /// <summary>
    /// Presents a technique for converting coordinates to a polygon. With the coordinates all in one hemisphere it is a
    /// fairly simple process. However, when the geometry crosses the dateline it becomes a more challenging problem.
    /// This routine attempts to do both. That is to say, it converts coordinates to a poly geometry regardless if the
    /// geometry traverses the dateline or not. If the geometry does cross the dateline then it is split up into multiple
    /// parts. This benefits the user in two ways: first, any GIS will be able to display the geometry in a proper way regardless of map projection
    /// and two, the GIS will be able to perform geoprocessing functions on the geometry. Note that feature classes do
    /// not support features with multiple parts that share the same edge. Thus, it is left up to the user on how to
    /// commit the geometries to a feature class. Suggestions are to save the multiple geometries as individual features
    /// or to slightly alter the edges of each geometry so as to save all parts as a single multi-part feature. For the
    /// latter option, the algorithm provides an input to trim edges of parts so that they can be used in a single feature.
    /// </summary>
    public class SplitPolygonAtDateline
    {
        [STAThread]
        public static void main()
        {
            // ========================================================================================
            // inputs
            // ========================================================================================
            string wktPrj = "GEOGCS['WGS 84',DATUM['WGS_1984',SPHEROID['WGS 84',6378137,298.257223563,AUTHORITY['EPSG','7030']],AUTHORITY['EPSG','6326']]," +
                            "PRIMEM['Greenwich',0,AUTHORITY['EPSG','8901']],UNIT['degree',0.0174532925199433,AUTHORITY['EPSG','9122']],AUTHORITY['EPSG','4326']]";

            string wktGeom = "MULTIPOINT ((170 3.5), (180 3.5), (180 -5), (188.927777777778 -5), (187.061666666667 -9.46444444444444), " +
                             "(185.718888888889 -12.4869444444444), (185.281944444444 -13.4555555555556), (184.472777777778 -15.225), (184.3275 -15.5397222222222), " +
                             "(184.171944444444 -15.9), (183.188888888889 -18.1488888888889), (183.019444444444 -18.5302777777778), (181.530555555556 -21.8), " +
                             "(180 -25), (177.333333333333 -25), (171.416666666667 -25), (168 -28), (163 -30), (163 -24), (163 -17.6666666666667), (161.25 -14), " +
                             "(163 -14), (166.875 -11.8), (170 -10), (170 3.5))";
            var rdr = new WKTReader();
            var geom = rdr.Read(wktGeom);
            var mps = (MultiPoint)geom;
            var coords = mps.Coordinates.ToList();

            var wtr = new WKTWriter();

            // ========================================================================================
            // function with defaults
            // ========================================================================================
            var resGeoms = ToPolyExOp(coords, wktPrj);
            for (int i = 0; i < resGeoms.NumGeometries; i++)
            {
                var resGeom = resGeoms.GetGeometryN(i);
                string resWkt = wtr.Write(resGeom);
                Debug.Print("geometry {0} = {1}", i, resWkt);
                DebugPrintCoords(resGeom.Coordinates.ToList(), "resGeom geom [" + i.ToString() + "] count");
            }

            // ========================================================================================
            // function with specific parameter inputs. trim 0.75 on parts in the Eastern hemisphere and
            // densify every 0.5 and attempt to fix invalid polygons.
            // ========================================================================================
            resGeoms = ToPolyExOp(coords, wktPrj, OgcGeometryType.Polygon, 0.75, 0.5, true);
            for (int i = 0; i < resGeoms.NumGeometries; i++)
            {
                var resGeom = resGeoms.GetGeometryN(i);
                string resWkt = wtr.Write(resGeom);
                Debug.Print("geometry {0} = {1}", i, resWkt);
                DebugPrintCoords(resGeom.Coordinates.ToList(), "resGeom geom [" + i.ToString() + "] count");
            }

            // ========================================================================================
            // function with specific parameter inputs. trim 0.75 on parts in the Western hemisphere and
            // densify every 0.25 and do not attempt to fix invalid polygons.
            // ========================================================================================
            resGeoms = ToPolyExOp(coords, wktPrj, OgcGeometryType.Polygon, -0.75, 0.25, false);
            for (int i = 0; i < resGeoms.NumGeometries; i++)
            {
                var resGeom = resGeoms.GetGeometryN(i);
                string resWkt = wtr.Write(resGeom);
                Debug.Print("geometry {0} = {1}", i, resWkt);
                DebugPrintCoords(resGeom.Coordinates.ToList(), "resGeom geom [" + i.ToString() + "] count");
            }
        }

        /// <summary>
        /// This routine takes a list of Coordinates and attempts to convert them to a poly geometry (polyline or polygon). Special care
        /// is taken to determine if the coordinates cross the dateline and if they do then there is special processing to split the geometry
        /// into multiple parts so that GIS can display and geoprocess without any issues. (Currently only polygon output is supported. Polyline
        /// support will come in a future update.)
        /// </summary>
        /// <param name="inpCoords">A list of coordinates that define a polyline or polygon.</param>
        /// <param name="inpProj4Wkt">The projection of the input coordinates as a PROJ4 WKT string.</param>
        /// <param name="outType">The output geometry type.</param>
        /// <param name="outTrimGap">
        /// The value used to trim the resulting geometry parts so they do not exactly touch the dateline. A positive value will trim only the
        /// parts that are in the Eastern hemisphere and a negative value will trim only the parts that are in the Western hemisphere. This is
        /// useful if the resultant geometries are to be used as multiple parts of a single feature. Double.NaN for no trim gap. If the resulting
        /// geometry will be inserted into a feature class then consider the tolerance for that feature class. "The default value for the x,y tolerance
        /// is 10 times the default x,y resolution, and this is recommended for most cases" and "If coordinates are in latitude-longitude, the default
        /// x,y resolution is 0.000000001 degrees". Thus a default tolerance = 10 * res = 10 * 0.000000001 degrees = 0.00000001 degrees.
        /// https://desktop.arcgis.com/en/arcmap/latest/manage-data/geodatabases/feature-class-basics.htm
        /// </param>
        /// <param name="outDensifyResolution">The value used to densify the resulting geometry. Double.NaN for no densification.</param>
        /// <param name="isAttFixOutInvPolygons">True if the process should attempt to fix invalid geometry, False otherwsie.</param>
        /// <returns>
        /// A Geometry class that contains the result of converting the coordinates into a Polyline or Polygon. If the geometry crosses the Dateline
        /// then the result is multiple geometries wrt to the Eastern and Western hemisphere.
        /// </returns>
        /// <remarks>
        /// Code by Abel G. Perez (Dec 19, 2021)
        /// Todo: This algorithm assumes the polygon is small to medium in width. Typically if the width of the polygon is <180° then the algorithm
        /// should work well. This is because the core of the process shifts all coordinates to either the Eastern or Western hemisphere. A better
        /// approach may be to shift all coordinates to the 0° to +360° range. This will allow polygons with larger widths.  This will get implemented
        /// in a future update if required. However, it really depends on how the user inputs the coordinates. If the input already has coordinates in
        /// the 0° to +360° range then that is straightforward to detect when a geometry crosses the dateline. But if the inputs are all in the standard
        /// -180° to 0° to +180° range then the assumption has to be made if the geometry closes towards the prime meridian or towards the dateline.
        /// The current algorithm determines this by simply calculting which critical meridian is the closest.
        /// https://github.com/NetTopologySuite/NetTopologySuite/discussions/496
        /// https://github.com/NetTopologySuite/NetTopologySuite/blob/develop/test/NetTopologySuite.Samples.Console/Operation/Polygonize/SplitPolygonExample.cs
        /// https://github.com/NetTopologySuite/NetTopologySuite/blob/84aef3c6a6cd3c2bfa5e3034ba161555cef5a117/test/NetTopologySuite.Samples.Console/Operation/Polygonize/SplitPolygonExample.cs#L24
        /// https://github.com/nettopologysuite/nettopologysuite/issues/129
        /// https://github.com/DotSpatial/DotSpatial/blob/e30421d6ee5b0cdf2b1fa01f95f8aabc50c1997a/Source/DotSpatial.Data/Shape.cs#L611
        /// https://github.com/DotSpatial/DotSpatial/issues/1029
        /// https://github.com/NetTopologySuite/ProjNet4GeoAPI/wiki/Well-Known-Text
        /// https://desktop.arcgis.com/en/arcmap/10.3/manage-data/editing-fundamentals/creating-and-editing-multipart-polygons.htm
        /// https://desktop.arcgis.com/en/arcmap/10.3/guide-books/map-projections/what-happens-to-features-at-180-dateline-.htm
        /// </remarks>
        public static Geometry ToPolyExOp(List<Coordinate> inpCoords, string inpProj4Wkt, OgcGeometryType outType, double outTrimGap, double outDensifyResolution, bool isAttFixOutInvPolygons)
        {
            Debug.Print("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");

            // ###################################################################################################################
            // check the output geometry type. currently only polygon output is supported. Polylines should use a similar process
            // with some slight modifications.
            // ###################################################################################################################
            switch (outType)
            {
                case OgcGeometryType.LineString:
                    //this will be supported in the future
                    throw new Exception("Unsupported output geometry type.");
                case OgcGeometryType.Polygon:
                    //this is supported
                    break;
                default:
                    // all other output types are not supported
                    throw new Exception("Unsupported output geometry type.");
            }

            // ###################################################################################################################
            // check the input projection info. currently only a geographic input projection is supported. we make many assumptions
            // based on a geographic coordinate system. if the user wishes to use a projected coordinate system then they will have
            // to reproject the points first and then use this process.
            // ###################################################################################################################
            if (inpProj4Wkt == null || !inpProj4Wkt.StartsWith("GEOGCS"))
            {
                throw new Exception("Unsupported projection for input. Must be a geographic coordinate system.");
            }

            // print original vertices
            DebugPrintCoords(inpCoords, "Initial number of coordinates");

            // ###################################################################################################################
            // determine if any longitudes < +180° and also > +180°. this will be an indication that the input coordinates are based
            // on longitude values in the range 0° to +360°. thus any longitudes that traverse the dateline will do so if they are
            // on BOTH sides of the +180° meridian. if the longitudes are only on one side of the the +180° meridian then that means
            // they are all in either the Western hemisphere OR Easter hemisphere. this is a more defined method to tell the algorithm
            // that these coordinates do in fact cross the dateline. otherwise, if the coordinates are in the standard range of
            // -180° to 0° to +180° then we have to make certain assumptions later in the process.
            // ###################################################################################################################
            bool hasLonsLt180 = inpCoords.Exists(c => c.X < 180);
            bool hasLonsGt180 = inpCoords.Exists(c => c.X > 180);
            bool isLonsTravP180 = false;
            if (hasLonsLt180 && hasLonsGt180)
            {
                isLonsTravP180 = true;
            }
            Debug.Print("hasLonsLt180={0} hasLonsGt180={1} isLonsTravP180={2}", hasLonsLt180, hasLonsGt180, isLonsTravP180);

            // ###################################################################################################################
            // normalize x values between -180° to 0° to +180°
            // ###################################################################################################################
            var coordsNrm = new List<Coordinate>();
            Coordinate coordNrm;
            double lonNrm;
            foreach (var coordInp in inpCoords)
            {
                lonNrm = Normalize(coordInp.X);
                coordNrm = coordInp.Create(lonNrm, coordInp.Y, coordInp.Z, coordInp.M);
                coordsNrm.Add(coordNrm);
            }

            // ###################################################################################################################
            // to create a polygon, the list of coodinates must be closed. in other words the first coordinate has to be the same
            // as the last coordinate to form a closed ring. we will check that here.
            // ###################################################################################################################
            int idxLast = coordsNrm.Count - 1;
            if (!coordsNrm[0].Equals2D(coordsNrm[idxLast]))
            {
                coordsNrm.Add(coordsNrm[0]);
            }
            DebugPrintCoords(coordsNrm, "Normalized number of coordinates");

            // check the minimum number of coordinates for an output geometry
            switch (outType)
            {
                case OgcGeometryType.LineString:
                    if (coordsNrm.Count < 2)
                        throw new Exception("Input list has fewer than 3 coordinates. To create a polyline, the minimum is 2 coordinates.");
                    break;
                case OgcGeometryType.Polygon:
                    if (coordsNrm.Count < 3)
                        throw new Exception("Input list has fewer than 3 coordinates. To create a polygon, the minimum is 3 coordinates.");
                    break;
                default:
                    throw new Exception("Unsupported output geometry type.");
            }

            // ###################################################################################################################
            // now we want to determine if the coordinates cross the dateline. from above we have already determined if the longitudes
            // have values that are < +180° and also > +180° and thus crossed the dateline since it is assumed they are in the range
            // 0° to +360°. If we cannot make that determination then we have to assume the longitudes are in the standard range of
            // -180° to 0° to +180°. if the case is the latter, then we have to go through some more calculations to determine if the
            // coordinates cross the dateline.
            // ###################################################################################################################
            bool doCoordsCrossDateline = false; //default is coordinates do not cross dateline
            if (isLonsTravP180)
            {
                doCoordsCrossDateline = true;
            }
            else
            {
                doCoordsCrossDateline = IsPolygonCrossesDatelineEx(coordsNrm);
            }
            Debug.Print("doCoordsCrossDateline={0}", doCoordsCrossDateline);

            // ###################################################################################################################
            // create the polyline or polygon. if it does NOT cross the dateline then that is easy. if it does cross the dateline
            // then we have to get a bit more creative.
            // ###################################################################################################################
            Geometry resGeoms = Polygon.Empty;
            if (!doCoordsCrossDateline)
            {
                // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                // the coordinates do NOT cross the dateline so create a ring and then a polygon geometry. simple!
                // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                var pRing = new LinearRing(coordsNrm.ToArray());
                var pPolygon = new Polygon(pRing);
                resGeoms = pPolygon;

                // print to debug which hemisphere this geometry is in since it doesnt cross the dateline
                DebugPrintHemi(coordsNrm, "original geometry is all in ");
            }
            else
            {
                // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                // the coordinates do cross the dateline so we must get a little creative to split the polygon at the dateline to
                // create two or more polygon parts. the gist of this algorithm is to shift the entire polygon into either the positive
                // or negative hemisphere and then split it there using avaiable functions. once that is accomplished we then unshift
                // the polygon parts to where they were originally.
                // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // we need to find the coordinate closest to the dateline so we will calculate the
                // min postive x-coordinate in the list and the max negative x-coordinate in the list.
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                var coordsPos = coordsNrm.FindAll(c => c.X >= 0);
                double xClosestToDlPos = coordsPos.Min(c => c.X);
                var coordsNeg = coordsNrm.FindAll(c => c.X < 0);
                double xClosestToDlNeg = coordsNeg.Max(c => c.X);
                Debug.Print("xClosestToDlNeg={0} xClosestToDlPos={1}", xClosestToDlNeg, xClosestToDlPos);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // determine which absolute value is the shortest from the dateline
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                double delPos = 180d - xClosestToDlPos;
                double delNeg = xClosestToDlNeg + 180d;
                double pad = 0.1d; // a small pad just to make sure you dont touch the dateline
                delPos += pad;
                delNeg += pad;
                double shift = delPos;
                if (delNeg < delPos)
                    shift = -1 * delNeg;
                Debug.Print("delNeg={0} delPos={1} pad={2} shift={3}", delNeg, delPos, pad, shift);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // shift coordinates to put them ALL in the same hemisphere
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                var coordsShifted = new List<Coordinate>();
                for (int q = 0; q < coordsNrm.Count; q++)
                {
                    var qcord = coordsNrm[q];

                    // create a Longitude class, shift the x-coord, and normalize to a value between -180° to 0° to + 180°
                    lonNrm = Normalize(qcord.X + shift);
                    var coordShifted = new Coordinate(lonNrm, qcord.Y);

                    // add to the shifted coordinate list
                    coordsShifted.Add(coordShifted);
                }

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // create a single polygon from the shifted coordinates
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                var pRingShifted = new LinearRing(coordsShifted.ToArray());
                var pPolygonShifted = new Polygon(pRingShifted);
                DebugPrintCoords(pPolygonShifted.Coordinates.ToList(), "pPolygonShifted count");
                var valOp = new IsValidOp(pPolygonShifted);
                Debug.Print("pPolygonShifted isValid = {0}", valOp.IsValid);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // now we determine if we will cut the polygon with a cutter line or a cutter polugon
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                Geometry nodedLinework = null;
                if (double.IsInfinity(outTrimGap) || double.IsNaN(outTrimGap))
                {
                    // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                    // use a cutter line to split the main polygon
                    // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    // define the cutter line that is based on the dateline with a shift to put it in the same hemisphere as the shifted
                    // polygon.
                    var cutterLine = DatelineCutterPolyline(delNeg, delPos, shift);
                    DebugPrintCoords(cutterLine.Coordinates.ToList(), "cutterLine count");

                    // perform a Union to split the polygon with a polyline
                    nodedLinework = pPolygonShifted.Boundary.Union(cutterLine);
                }
                else
                {
                    // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                    // use a cutter polygon to split the main polygon
                    // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    // define the cutter polygon that is based on the dateline with a shift to put it in the same hemisphere as the shifted
                    // polygon. Plus we also add the trim gap as the cutter polygon width.
                    var cutterPgon = DatelineCutterPolygon(delNeg, delPos, shift, outTrimGap);
                    DebugPrintCoords(cutterPgon.Coordinates.ToList(), "cutterPgon count");
                    var valOp2 = new IsValidOp(cutterPgon);
                    Debug.Print("cutterPgon isValid = {0}", valOp2.IsValid);

                    // perform an Erase to split the polygon with a polygon. this consists of an Intersection and then a Difference.
                    //var intx = pPolygonShifted.Boundary.Intersection(cutterPgon);
                    //Debug.Print("intx={0}", intx.AsText());
                    //nodedLinework = pPolygonShifted.Boundary.SymmetricDifference(intx);
                    nodedLinework = pPolygonShifted.Difference(cutterPgon); // why use the geometry rather than the geometry's boundary???
                    //nodedLinework = nodedLinework.Boundary.Intersection(cutterPgon.Boundary);
                }

                Debug.Print("nodedLinework is {0}", nodedLinework.GeometryType);
                if (nodedLinework.GeometryType == "MultiLineString")
                {
                    var mls = (MultiLineString)nodedLinework;
                    for (int i = 0; i < mls.Geometries.Count(); i++)
                    {
                        var geom = mls.GetGeometryN(i);
                        DebugPrintCoords(geom.Coordinates.ToList(), "nodedLinework geom [" + i.ToString() + "] count");
                    }
                }

                var wtr = new WKTWriter();
                string resWkt = wtr.Write(nodedLinework);
                Debug.Print("nodedLinework = {0}", resWkt);
                DebugPrintCoords(nodedLinework.Coordinates.ToList(), "nodedLinework count");
                var errs = PolygonizeAllErrors(nodedLinework);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // convert the resultant MULTILINESTRING to get mutliple polygons
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                var polygons = Polygonize(nodedLinework, false);
                Debug.Print("Polygonize count={0}", polygons.NumGeometries);
                resWkt = wtr.Write(polygons);
                Debug.Print("polygons = {0}", resWkt);
                var output = new List<Geometry>();
                for (int i = 0; i < polygons.NumGeometries; i++)
                {
                    var candpoly = (Polygon)polygons.GetGeometryN(i);
                    if (pPolygonShifted.Contains(candpoly.InteriorPoint))
                        output.Add(candpoly);
                }
                Debug.Print("output count={0}", output.Count);

                resGeoms = pPolygonShifted.Factory.BuildGeometry(output.ToArray());
                Debug.Print("geometry parts from splitting at dateline={0}", resGeoms.NumGeometries);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // now we must take the resultant geometry parts and unshift them to their original location
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                for (int w = 0; w < resGeoms.NumGeometries; w++)
                {
                    var geom = resGeoms.GetGeometryN(w);

                    // cycle through all the coordinates and unshift them
                    var geomCoords = geom.Coordinates.ToList();
                    for (int q = 0; q < geomCoords.Count; q++)
                    {
                        var qcord = geomCoords[q];

                        // unshift the x-coord
                        qcord.X -= shift;

                        // normalize x-coord to a value between -180° to 0° to + 180°
                        lonNrm = Normalize(qcord.X);
                        qcord.X = lonNrm;
                    }

                    // now we have to "fix" the points that are exactly at the dateline. the polygon part in the east hemisphere should have
                    // these points at +180 and the polygon part in the west hemisphere should have these points at -180. so we will determine
                    // how many of the polygon part vertices are in the positive hemisphere and how many are in the negative hemisphere and also
                    // exclude the dateline points. if most are negative then make the dateline coordinate -180 else make it +180.
                    var coordsInPos = geomCoords.FindAll(c => c.X >= 0 & Math.Abs(c.X) != 180);
                    var coordsInNeg = geomCoords.FindAll(c => c.X < 0 & Math.Abs(c.X) != 180);
                    var coordsAtDtl = geomCoords.FindAll(c => Math.Abs(c.X) == 180);
                    if (coordsInPos.Count >= coordsInNeg.Count)
                    {
                        coordsAtDtl.ForEach(c => c.X = 180);
                    }
                    else
                    {
                        coordsAtDtl.ForEach((c) => c.X = -180);
                    }


                    // print to debug which hemisphere this geometry is in since it doesnt cross the dateline
                    string pfx = string.Format("unshifted geometry part {0} is in ", w);
                    DebugPrintHemi(geomCoords, pfx);
                }
            }

            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // here we will densify the polygon if the resolution value is passed in.
            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            if (!double.IsNaN(outDensifyResolution) && !double.IsInfinity(outDensifyResolution) && outDensifyResolution > 0d)
            {
                resGeoms = Densifier.Densify(resGeoms, outDensifyResolution);
            }

            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // finally if the option is passed in to attempt to fix invalid geometry then we will do that here
            // https://github.com/NetTopologySuite/NetTopologySuite/issues/124
            // Attempting to fix an invalid geometry by calling geometry.Buffer(0) is a common approach, and
            // suggested in multiple places. However it does not work in all cases.
            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            if (isAttFixOutInvPolygons)
            {
                var valOp = new IsValidOp(resGeoms);
                Debug.Print("resGeoms isValid = {0}", valOp.IsValid);
                if (!valOp.IsValid)
                {
                    // method 1
                    resGeoms = resGeoms.Buffer(0);
                    Debug.Print("resGeoms were fixed");

                    // method 2
                    //var pm = new PrecisionModel(10000000000.0);
                    //resGeoms = new GeometryPrecisionReducer(pm).Reduce(resGeoms);
                }
            }

            // print final vertices of all geometries
            DebugPrintCoords(resGeoms.Coordinates.ToList(), "Final number of coordinates");

            // success so return generated geometry (it could me multi-part)
            return resGeoms;
        }

        /// <summary>
        /// This routine takes a list of Coordinates and attempts to convert them to a poly geometry (polyline or polygon). Special care
        /// is taken to determine if the coordinates cross the dateline and if they do then there is special processing to split the geometry
        /// into multiple parts so that GIS can display and geoprocess without any issues. (Currently only polygon output is supported. Polyline
        /// support will come in a future update.)
        /// </summary>
        /// <param name="coordsInp">A list of coordinates that define a polyline or polygon.</param>
        /// <param name="proj4WktInp">The projection of the input coordinates as a PROJ4 WKT string.</param>
        /// <returns>
        /// A Geometry class that contains the result of converting the coordinates into a Polyline or Polygon. The result could be multiple
        /// geometries.
        /// </returns>
        private static Geometry ToPolyExOp(List<Coordinate> coordsInp, string proj4WktInp)
        {
            return ToPolyExOp(coordsInp, proj4WktInp, OgcGeometryType.Polygon, double.NaN, double.NaN, false);
        }

        /// <summary>
        /// Normalizes the input longitude.
        /// </summary>
        /// <returns>A <strong>Longitude</strong> containing the normalized value.</returns>
        /// <remarks>This function is used to ensure that an angular measurement is within the
        /// allowed bounds of 0° and 180°. If a value of 360° or 720° is passed, a value of 0°
        /// is returned since traveling around the Earth 360° or 720° brings you to the same
        /// place you started.</remarks>
        /// <see cref="DotSpatial.Positioning"/>
        private static double Normalize(double _decimalDegrees)
        {
            // Is the value not a number, infinity, or already normalized?
            if (double.IsInfinity(_decimalDegrees) || double.IsNaN(_decimalDegrees))
                return _decimalDegrees;
            // If we're off the eastern edge (180E) wrap back around from the west
            if (_decimalDegrees > 180)
                return (-180 + (_decimalDegrees % 180));
            // If we're off the western edge (180W) wrap back around from the east
            return _decimalDegrees < -180 ? (180 + (_decimalDegrees % 180)) : _decimalDegrees;
        }

        /// <summary>
        /// Determines if a polygon crosses the dateline. This is slightly different than checking a LineString as the Polygon is
        /// closed. So a segment exists between the last vertex and the first vertex.
        /// </summary>
        /// <param name="coordsInp">The input list of coordinates.</param>
        /// <returns>True if the coordinates as a polygon cross the dateline, False otherwise.</returns>
        private static bool IsPolygonCrossesDatelineEx(List<Coordinate> coordsInp)
        {
            // determine if the coordinates contain negative x-coords, positive x-coords, 0 x-coords, and +/-180 x-coords
            bool hasNegLons = coordsInp.Exists(c => c.X < 0);
            bool hasPosLons = coordsInp.Exists(c => c.X > 0);
            bool has000Lons = coordsInp.Exists(c => c.X == 0);
            bool has180Lons = coordsInp.Exists(c => Math.Abs(c.X) == 180);
            Debug.Print("hasNegLons={0} hasPosLons={1} has000Lons={2} has180Lons={3}", hasNegLons, hasPosLons, has000Lons, has180Lons);

            // if the longitudes are all the same sign then that means it didnt cross either the prime meridian or the date line. in this
            // case all we want is the date line crossing. if there is a mix of positive and negative longitudes then we have to determine
            // if any of those point pairs are closer to the dateline or the prime meridian. this is purely an educated guess that distance
            // from a critical meridian is what determines which way around the globe the geometry will wrap.
            if (!(hasNegLons & hasPosLons))
            {
                Debug.Print("Feature does NOT cross dateline.");
                return false;
            }
            else
            {
                Debug.Print("Feature MAY cross dateline. Analyzing all coordinates now...");

                // cycle through through all the vertices and determine if any segment crosses the dateline
                int idxLast = coordsInp.Count - 1;
                int idxPrior;
                Coordinate vtxCurr;
                Coordinate vtxPrior;
                for (int i = 0, loopTo = idxLast; i <= loopTo; i++)
                {
                    // get the current vertex
                    vtxCurr = coordsInp[i];

                    // get the prior vertex
                    if (i == 0)
                        idxPrior = idxLast;
                    else
                        idxPrior = i - 1;
                    vtxPrior = coordsInp[idxPrior];

                    // check for same or different longitude signs. this will let us know that the segment crossed the dateline -OR- the prime meridian.
                    bool isCrossesCriticalMeridain = false;
                    if (Math.Sign(vtxCurr.X) != Math.Sign(vtxPrior.X))
                        isCrossesCriticalMeridain = true;

                    // further check if the segment crosses the dateline.
                    bool isCrossesDateline = false;
                    if (isCrossesCriticalMeridain)
                    {
                        // check if the segment crosses the dateline
                        isCrossesDateline = IsCoordPairCrossesDateline(vtxPrior, vtxCurr);
                    }

                    Debug.Print(" ==>i={0} vtxPrior.X={1} vtxPrior.y={2}", i, vtxPrior.X, vtxPrior.Y);
                    Debug.Print(" ==>i={0} vtxCurr.X={1} vtxCurr.y={2}", i, vtxCurr.X, vtxCurr.Y);
                    Debug.Print(" ==>i={0} IsCrossesCriticalMeridain={1} IsCrossesDateline={2}", i, isCrossesCriticalMeridain, isCrossesDateline);

                    // if it crosses the dateline then return
                    if (isCrossesDateline)
                    {
                        return true;
                    }
                }

                // if we make it to this point then that means the polygon did NOT cross the dateline
                return false;
            }
        }

        /// <summary>
        /// This check calculates x distance between the two points going in opposite directions across the globe. the shortest segment wins and
        /// that is the one we use to determine if the segment crosses the dateline. should work for most applications but this can be tweaked.
        /// </summary>
        /// <param name="coordA">First coordinate.</param>
        /// <param name="coordB">Second coordinate.</param>
        /// <returns>True if the coordinate pair crosses the dateline, False otherwise.</returns>
        private static bool IsCoordPairCrossesDateline(Coordinate coordA, Coordinate coordB)
        {
            // if longitudes are same sign then does not cross the dateline
            if (Math.Sign(coordA.X) == Math.Sign(coordB.X))
                return false;

            // compute distance between points towards the prime meridian
            double distPm = Math.Abs(coordA.X) + Math.Abs(coordB.X);

            // compute distance between points towards the dateline
            double distDl = 360d - distPm;

            // Debug.Print(" ==>distPm={0} distDl={1}", distPm, distDl)

            // if the two distances are the same then its a tough call because the distance around the globe in either
            // direction is the same. so we will opt for simplicity and choose the directon towards the prime meridian.
            if (distPm < distDl)
            {
                return false;
            }
            else if (distPm == distDl)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Convert the geometry to a polyon.
        /// </summary>
        /// <param name="geom">The input geometry.</param>
        /// <returns>A polyon geometry.</returns>
        private static Geometry Polygonize(Geometry geom)
        {
            ICollection<Geometry> lines = LineStringExtracter.GetLines(geom);
            Debug.Print("lines count={0}", lines.Count);
            var pgnizer = new Polygonizer(false);
            pgnizer.Add(lines);
            var polys = pgnizer.GetPolygons().ToList();
            Debug.Print("polys count={0}", polys.Count);
            ICollection<Geometry> polyArray = GeometryFactory.ToGeometryArray(polys);
            Debug.Print("polyArray count={0}", polyArray.Count);
            return (Geometry)geom.Factory.BuildGeometry(polyArray);
        }



        // https://github.com/NetTopologySuite/NetTopologySuite/blob/c838c5061aa0f13d1fa65f828a868d790302ec06/test/NetTopologySuite.TestRunner/Functions/PolygonizeFunctions.cs
        private static Geometry Polygonize(Geometry g, bool extractOnlyPolygonal)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var polygonizer = new Polygonizer(extractOnlyPolygonal);
            polygonizer.Add(lines);
            return polygonizer.GetGeometry();
        }


        public static Geometry PolygonizeAllErrors(Geometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);

            var errs = new List<Geometry>();
            var dangles = polygonizer.GetDangles();
            errs.AddRange(dangles);
            Debug.Print("GetDangles errs count={0}", dangles.Count);

            var cutedges = polygonizer.GetCutEdges();
            errs.AddRange(cutedges);
            Debug.Print("GetCutEdges errs count={0}", cutedges.Count);

            var invringlines = polygonizer.GetInvalidRingLines();
            errs.AddRange(invringlines);
            Debug.Print("GetInvalidRingLines errs count={0}", invringlines.Count);

            return g.Factory.BuildGeometry(errs);
        }


        /// <summary>
        /// Define the cutter polyline that is based on the dateline with a shift to put it in the same hemisphere as the polygon.
        /// </summary>
        /// <param name="delNeg"></param>
        /// <param name="delPos"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        private static LineString DatelineCutterPolyline(double delNeg, double delPos, double shift)
        {
            Debug.Print("DatelineCutterPolyline: delNeg={0} delPos={1} shift={2}", delNeg, delPos, shift);

            double lonSplit = shift - 180d;
            if (delNeg < delPos)
                lonSplit = 180d + shift;
            Debug.Print("  lonSplit={0}", lonSplit);

            // build a series of coordinates
            var cutterPtList = new List<Coordinate>();
            cutterPtList.Add(new Coordinate(lonSplit, +100));
            cutterPtList.Add(new Coordinate(lonSplit, -100));

            // create a polyline from the coordinates
            var cutterLine = new LineString(cutterPtList.ToArray());

            // we can densify the line but this seems to confuse the splitting routine
            // cutterLine = (LineString)Densifier.Densify(cutterLine, 0.1); // densify to a specific resolution 0.1 degrees

            // return the "new" dateline polyline that has been shifted
            return cutterLine;
        }

        /// <summary>
        /// Define the cutter polygon that is based on the dateline with a shift to put it in the same hemisphere as the polygon.
        /// We also use the trim gap as the polygon width.
        /// </summary>
        /// <param name="delNeg"></param>
        /// <param name="delPos"></param>
        /// <param name="shift"></param>
        /// <param name="trimGap"></param>
        /// <returns></returns>
        private static Polygon DatelineCutterPolygon(double delNeg, double delPos, double shift, double trimGap)
        {
            Debug.Print("DatelineCutterPolygon: delNeg={0} delPos={1} shift={2} trimGap={3}", delNeg, delPos, shift, trimGap);

            if (double.IsInfinity(trimGap) || double.IsNaN(trimGap))
            {
                throw new Exception("Invalid value for trim gap");
            }

            double lonSplit = shift - 180d;
            if (delNeg < delPos)
                lonSplit = 180d + shift;
            Debug.Print("  lonSplit={0} lonSplit+trimGap={1}", lonSplit, lonSplit + trimGap);

            // build a clockwise series of coordinates
            var cutterPtList = new List<Coordinate>();
            cutterPtList.Add(new Coordinate(lonSplit, -100));
            cutterPtList.Add(new Coordinate(lonSplit, +100));
            cutterPtList.Add(new Coordinate(lonSplit + trimGap, +100));
            cutterPtList.Add(new Coordinate(lonSplit + trimGap, -100));
            cutterPtList.Add(new Coordinate(lonSplit, -100));

            // create a polygon from the coordinates
            var pRing = new LinearRing(cutterPtList.ToArray());
            var cutterPolygon = new Polygon(pRing);

            // we can densify the polygon but this seems to confuse the difference routine
            // cutterPolygon = (Polygon)Densifier.Densify(cutterPolygon, 0.1); // densify to a specific resolution 0.1 degrees

            // return the "new" dateline polygon that has been shifted and also has a gap from the dateline
            return cutterPolygon;
        }

        private static void DebugPrintCoords(List<Coordinate> coords, string hdr)
        {
            Debug.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Debug.Print(hdr + "={0}", coords.Count);
            Debug.Print("kIdx,x,y");

            Coordinate coordK;
            for (int k = 0; k <= coords.Count - 1; k++)
            {
                coordK = coords[k];
                Debug.Print("{0},{1},{2}", k, coordK.X, coordK.Y);
            }

            Debug.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        private static void DebugPrintGeom(Geometry geom, string hdr)
        {
            var wtr = new WKTWriter();
            string resWkt = wtr.Write(geom);
            Debug.Print("geom = {0}", resWkt);
            DebugPrintCoords(geom.Coordinates.ToList(), "geom count");
        }

        private static void DebugPrintHemi(List<Coordinate> coords, string prefix)
        {
            // determine which hemisphere this geometry is in and print to debug
            bool hHasNegLons = coords.Exists(c => c.X < 0);
            if (hHasNegLons)
            {
                Debug.Print(prefix + "Western Hemishpere");
            }
            else
            {
                Debug.Print(prefix + "Eastern Hemishpere");
            }
        }
    }
}
