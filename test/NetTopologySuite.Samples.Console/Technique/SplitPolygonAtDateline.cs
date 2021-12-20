using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Valid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Samples.Technique
{
    public class SplitPolygonAtDateline
    {
        [STAThread]
        public static void main(string[] args)
        {
            string wkt = "MULTIPOINT ((170 3.5), (180 3.5), (180 -5), (188.927777777778 -5), (187.061666666667 -9.46444444444444), (185.718888888889 -12.4869444444444), (185.281944444444 -13.4555555555556), (184.472777777778 -15.225), (184.3275 -15.5397222222222), (184.171944444444 -15.9), (183.188888888889 -18.1488888888889), (183.019444444444 -18.5302777777778), (181.530555555556 -21.8), (180 -25), (177.333333333333 -25), (171.416666666667 -25), (168 -28), (163 -30), (163 -24), (163 -17.6666666666667), (161.25 -14), (163 -14), (166.875 -11.8), (170 -10), (170 3.5))";
            var rdr = new WKTReader();
            var geom = rdr.Read(wkt);
            var mps = (MultiPoint)geom;
            var coords = mps.Coordinates.ToList();

            var wtr = new WKTWriter();

            var resGeoms = ToPolyExOp(coords, null);
            for (int i = 0; i < resGeoms.NumGeometries; i++)
            {
                var resGeom = resGeoms.GetGeometryN(i);
                string resWkt = wtr.Write(resGeom);
                Debug.Print("geometry {0} = {1}", i, resWkt);
            }
        }

        /// <summary>
        /// This routine takes a list of Coordinates and attempts to convert them to a poly geometry (polyline or polygon). Special care
        /// is taken to determine if the coordinates cross the dateline and if they do then there is special processing to split the geometry
        /// into several parts so that GIS can display without any issues. (Currently only polygon output is supported. Polyline support will
        /// come in a future update.)
        /// </summary>
        /// <param name="coordsInp">A list of coordinates that define a polyline or polygon.</param>
        /// <param name="projInp">The projection of the input coordinates.</param>
        /// <param name="IsInputLons0to360">
        /// True if the process should consider input longitudes in the range 0 to 360 degrees and thus any coordinate > +180 will denote a geometry
        /// that crosses the dateline. False if the process should auto detect if the geometry crosses the dateline based on general assumptions.
        /// (Not yet implemented)
        /// </param>
        /// <param name="outputType">The output geometry type.</param>
        /// <param name="densifyResolution">The value to use to densify the resulting geometry. Double.NaN for no densification.</param>
        /// <param name="isAttFixInvPolygons">True if attempt to fix invalid geometry, False otherwsie.</param>
        /// <returns>
        /// An IGeometry class that contains the result of converting the coordinates into a Polyline or Polygon.
        /// </returns>
        /// <remarks>
        /// Code by Abel G. Perez (Dec 19, 2021)
        /// https://github.com/NetTopologySuite/NetTopologySuite/discussions/496
        /// https://github.com/NetTopologySuite/NetTopologySuite/blob/develop/test/NetTopologySuite.Samples.Console/Operation/Polygonize/SplitPolygonExample.cs
        /// https://github.com/NetTopologySuite/NetTopologySuite/blob/84aef3c6a6cd3c2bfa5e3034ba161555cef5a117/test/NetTopologySuite.Samples.Console/Operation/Polygonize/SplitPolygonExample.cs#L24
        /// https://github.com/nettopologysuite/nettopologysuite/issues/129
        /// https://github.com/DotSpatial/DotSpatial/blob/e30421d6ee5b0cdf2b1fa01f95f8aabc50c1997a/Source/DotSpatial.Data/Shape.cs#L611
        /// https://github.com/DotSpatial/DotSpatial/issues/1029
        /// https://github.com/NetTopologySuite/ProjNet4GeoAPI/wiki/Well-Known-Text
        /// </remarks>
        public static Geometry ToPolyExOp(List<Coordinate> coordsInp, object projInp, bool IsInputLons0to360, OgcGeometryType outputType, double densifyResolution, bool isAttFixInvPolygons)
        {
            // ###################################################################################################################
            // check the output geometry type. currently only polygon output is supported.
            // ###################################################################################################################
            if (outputType != OgcGeometryType.Polygon)
            {
                throw new Exception("Unsupported output geometry type.");
            }

            //// ###################################################################################################################
            //// check the input projection info. currently only a geographic input projection is supported.
            //// ###################################################################################################################
            //if (projInp == null || !projInp.ToProj4String().StartsWith("GEOGCS") || !projInp.IsLatLon)
            //{
            //    throw new Exception("Unsupported projection for input. Must be a geographic projection.");
            //}

            // ###################################################################################################################
            // print original vertices
            // ###################################################################################################################
            DebugPrintCoords(coordsInp, "Initial number of coordinates");

            // ###################################################################################################################
            // the first thing we do is is to create a Longitude class and normalize x values between -180° to 0° to +180°
            // ###################################################################################################################
            var coordsNrm = new List<Coordinate>();
            Coordinate coordNrm;
            double lonNrm;
            foreach (var coordInp in coordsInp)
            {
                //lonNrm = Normalize(coordInp.X);
                //coordNrm = new Coordinate(lonNrm, coordInp.Y);
                //coordNrm.M = coordInp.M;
                //coordsNrm.Add(coordNrm);

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
            switch (outputType)
            {
                case OgcGeometryType.Polygon:
                    if (coordsNrm.Count < 3)
                        throw new Exception("Input list has fewer than 3 coordinates. To create a polygon, the minimum is 3 coordinates.");
                    break;
                default:
                    throw new Exception("Unsupported output geometry type.");
            }

            // ###################################################################################################################
            // now we want to determine if the coordinates cross the dateline.
            // ###################################################################################################################
            bool isCoordsCrossesDateline = IsPolygonCrossesDatelineEx(coordsNrm);
            Debug.Print("Coordinates crosses dateline={0}", isCoordsCrossesDateline);

            // ###################################################################################################################
            // create the polyline or polygon. if it does NOT cross the dateline then that is easy. if it does cross the dateline
            // then we have to get a bit more creative.
            // ###################################################################################################################
            Geometry resGeoms = Polygon.Empty;
            if (!isCoordsCrossesDateline)
            {
                // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                // the coordinates do NOT cross the dateline so create a ring and then a polygon geometry.
                // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                var pRing = new LinearRing(coordsNrm.ToArray());
                var pPolygon = new Polygon(pRing);
                resGeoms = pPolygon;

                // determine which hemisphere this polygon is in
                bool hHasNegLons = coordsNrm.Exists((c) => c.X < 0);
                if (hHasNegLons)
                {
                    Debug.Print("Western Hemishpere");
                }
                else
                {
                    Debug.Print("Eastern Hemishpere");
                }
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
                // define the cutter line that is based on the dateline with a shift to put it in
                // the same hemisphere as the shifted polygon.
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                var cutterLine = DateLineCutter(delNeg, delPos, shift);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // create a polygon from the shifted coordinates
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                var pRingShifted = new LinearRing(coordsShifted.ToArray());
                var pPolygonShifted = new Polygon(pRingShifted);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // split the polygon with a polyline cutter to get mutliple polygons
                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                var nodedLinework = pPolygonShifted.Boundary.Union(cutterLine);
                var polygons = Polygonize(nodedLinework);
                var output = new List<Geometry>();
                for (int i = 0; i < polygons.NumGeometries; i++)
                {
                    var candpoly = (Polygon)polygons.GetGeometryN(i);
                    if (pPolygonShifted.Contains(candpoly.InteriorPoint))
                        output.Add(candpoly);
                }

                resGeoms = pPolygonShifted.Factory.BuildGeometry(output.ToArray());

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // now we must take the resultant polygons and unshift them to their original location
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

                        // create a Longitude class and normalize x-coord to a value between -180° to 0° to + 180°
                        lonNrm = Normalize(qcord.X);
                        qcord.X = lonNrm;
                    }

                    // now we have to "fix" the points that are exactly at the dateline. the polygon part in the east hemisphere should have
                    // these points at +180 and the polygon part in the west hemisphere should have these points at -180. so we will determine
                    // how many of the polgon part vertices are in the positive hemisphere and how many are in the negative hemisphere and also
                    // exclude the dateline points. if most are negative then make the dateline coordinate -180 else make it +180.
                    var coordsP = geomCoords.FindAll(c => c.X >= 0 & Math.Abs(c.X) != 180);
                    var coordsN = geomCoords.FindAll(c => c.X < 0 & Math.Abs(c.X) != 180);
                    var coordsAtDl = geomCoords.FindAll(c => Math.Abs(c.X) == 180);
                    if (coordsP.Count >= coordsN.Count)
                    {
                        coordsAtDl.ForEach((c) => c.X = 180);
                    }
                    else
                    {
                        coordsAtDl.ForEach((c) => c.X = -180);
                    }

                    // determine which hemisphere this polygon is in
                    bool hasNegLons = geomCoords.Exists((c) => c.X < 0);
                    if (hasNegLons)
                    {
                        Debug.Print("Western Hemishpere");
                    }
                    else
                    {
                        Debug.Print("Eastern Hemishpere");
                    }
                }
            }

            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // here we will densify the polygon if the resolution value is passed in.
            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            if (!double.IsNaN(densifyResolution) && !double.IsInfinity(densifyResolution) && densifyResolution > 0d)
            {
                resGeoms = Densifier.Densify(resGeoms, densifyResolution);
            }

            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // finally if the option is passed in to attempt to fix invalid geometry then we will do that here
            // https://github.com/NetTopologySuite/NetTopologySuite/issues/124
            // Attempting to fix an invalid geometry by calling geometry.Buffer(0) is a common approach, and suggested in multiple places.
            // However it does not work in all cases.
            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            if (isAttFixInvPolygons)
            {
                var valOp = new IsValidOp(resGeoms);
                if (!valOp.IsValid)
                {
                    // method 1
                    resGeoms = resGeoms.Buffer(0);

                    // method 2
                    // PrecisionModel pm = new PrecisionModel(10000000000.0);
                    // resGeoms = new GeometryPrecisionReducer(pm).Reduce(resGeoms);
                }
            }

            // print final vertices
            DebugPrintCoords(resGeoms.Coordinates.ToList(), "Final number of coordinates");

            // success so return generated polygon (it could me multi-part)
            return resGeoms;
        }

        /// <summary>
        /// This routine takes a list of Coordinates and attempts to convert them to a polygon geometry. Special care is taken to determine
        /// if the polygon crosses the dateline and if it does then there is special processing to split the geometry into several parts so
        /// that GIS can display without any issues.
        /// </summary>
        /// <param name="coordsInp">A list of coordinates that define a polyline or polygon.</param>
        /// <param name="projInp">The projection of the input coordinates.</param>
        /// <param name="outputType">The output geometry type.</param>
        /// <returns>
        /// An IGeometry class that contains the result of converting the coordinates into a Polygon.
        /// </returns>
        public static Geometry ToPolyExOp(List<Coordinate> coordsInp, object projInp)
        {
            return ToPolyExOp(coordsInp, projInp, false, OgcGeometryType.Polygon, double.NaN, false);
        }

        /// <summary>
        /// Normalizes this instance.
        /// </summary>
        /// <returns>A <strong>Longitude</strong> containing the normalized value.</returns>
        /// <remarks>This function is used to ensure that an angular measurement is within the
        /// allowed bounds of 0° and 180°. If a value of 360° or 720° is passed, a value of 0°
        /// is returned since traveling around the Earth 360° or 720° brings you to the same
        /// place you started.</remarks>
        /// <see cref="DotSpatial.Positioning"/>
        public static double Normalize(double _decimalDegrees)
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
        public static bool IsPolygonCrossesDatelineEx(List<Coordinate> coordsInp)
        {
            // determine if the coordinates contain negative x-coords, positive x-coords, 0 x-coords, and +/-180 x-coords
            bool hasNegLons = coordsInp.Exists((c) => c.X < 0);
            bool hasPosLons = coordsInp.Exists((c) => c.X > 0);
            bool has000Lons = coordsInp.Exists((c) => c.X == 0);
            bool has180Lons = coordsInp.Exists((c) => Math.Abs(c.X) == 180);
            Debug.Print("HasNegLons={0} HasPosLons={1} Has000Lons={2} Has180Lons={3}", hasNegLons, hasPosLons, has000Lons, has180Lons);

            // if the longitudes are all the same sign then that means it didnt cross either the prime meridian or the date line. in this
            // case all we want is the date line crossing. if there is a mix of positive and negative longitudes then we have to determine
            // if any of those points are within +/- a delta from the date line. the delta is hard coded as 90 degrees.
            // the date line is at +180 or -180.
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
        public static bool IsCoordPairCrossesDateline(Coordinate coordA, Coordinate coordB)
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
        public static Geometry Polygonize(Geometry geom)
        {
            ICollection<Geometry> lines = LineStringExtracter.GetLines(geom);
            var pgnizer = new Polygonizer(false);
            pgnizer.Add(lines);
            var polys = pgnizer.GetPolygons().ToList();
            ICollection<Geometry> polyArray = GeometryFactory.ToGeometryArray(polys);
            return (Geometry)geom.Factory.BuildGeometry(polyArray);
        }

        /// <summary>
        /// Define the cutter line that is based on the dateline with a shift to put it in the same hemisphere as the polygon.
        /// </summary>
        /// <param name="delNeg"></param>
        /// <param name="delPos"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static LineString DateLineCutter(double delNeg, double delPos, double shift)
        {
            double lonSplit = shift - 180d;
            if (delNeg < delPos)
                lonSplit = 180d + shift;
            var cutterPtList = new List<Coordinate>();
            cutterPtList.Add(new Coordinate(lonSplit, +100));
            cutterPtList.Add(new Coordinate(lonSplit, -100));
            var cutterLine = new LineString(cutterPtList.ToArray());

            // we can desnify the line but this seems to confuse the splitting routine
            // cutterLine = (LineString)Densifier.Densify(cutterLine, 0.1); // densify to a specific resolution 0.1 degrees

            // return the "new" dateline that has been shifted
            return cutterLine;
        }

        public static void DebugPrintCoords(List<Coordinate> coords, string hdr)
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

    }
}
