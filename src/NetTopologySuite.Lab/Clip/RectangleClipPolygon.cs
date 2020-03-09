using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Clip
{
    /// <summary>
    /// Clips polygonal geometry to a rectangle.
    /// This implementation is faster, more robust,
    /// and less sensitive to invalid input than
    /// <see cref="Geometry.Intersection(Geometry)"/>.
    /// <para/>
    /// It can also enforce a supplied precision model on the computed result.
    /// The inputs do not have to meet the precision model.
    /// This allows clipping using integer coordinates
    /// in the output, for example.
    /// </summary>
    /// <author>mdavis</author>
    public class RectangleClipPolygon
    {

        private const int ENV_LEFT = 3;
        private const int ENV_TOP = 2;
        private const int ENV_RIGHT = 1;
        private const int ENV_BOTTOM = 0;

        public static Geometry Clip(Geometry geom, Geometry rectangle)
        {
            var ctt = new RectangleClipPolygon(rectangle);
            var result = ctt.Clip(geom);
            return result;
        }

        public static Geometry Clip(Geometry geom, Geometry rectangle, PrecisionModel pm)
        {
            var ctt = new RectangleClipPolygon(rectangle, pm);
            var result = ctt.Clip(geom);
            return result;
        }

        private Envelope clipEnv;
        private double clipEnvMinY;
        private double clipEnvMaxY;
        private double clipEnvMinX;
        private double clipEnvMaxX;
        private PrecisionModel precModel;

        public RectangleClipPolygon(Geometry clipRectangle)
            : this(clipRectangle, new PrecisionModel(PrecisionModels.Floating))
        {
        }

        public RectangleClipPolygon(Geometry clipRectangle, PrecisionModel pm)
        {
            clipEnv = clipRectangle.EnvelopeInternal;
            clipEnvMinY = clipEnv.MinY;
            clipEnvMaxY = clipEnv.MaxY;
            clipEnvMinX = clipEnv.MinX;
            clipEnvMaxX = clipEnv.MaxX;

            precModel = pm;
        }

        public Geometry Clip(Geometry geom)
        {
            // TODO: handle MultiPolygons
            var polyClip = ClipPolygon((Polygon)geom);
            return FixTopology(polyClip);
        }

        /// <summary>
        /// The clipped geometry may be invalid
        /// (due to coincident linework from clipping to edges, 
        /// or due to precision reduction if performed).
        /// This method fixed the geometry topology to be valid.
        /// <para/>
        /// Currently uses the buffer(0) trick.
        /// This should work in most cases(but need to verify this).
        /// But it may produce unexpected results if the input polygon
        /// was invalid inside the clip area.
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns>A fixed geometry</returns>
        /// <seealso cref="Precision.GeometryPrecisionReducer"/>
        private Geometry FixTopology(Geometry geom)
        {
            // TODO: do this in a better way (could use GeometryPrecisionReducer?)
            // TODO: ensure this meets required precision model (see GeometryPrecisionReducer)
            return geom.Buffer(0d);
        }

        private Polygon ClipPolygon(Polygon poly)
        {
            var shell = (LinearRing)poly.ExteriorRing;
            var shellClip = ClipRing(shell);

            var holesClip = ClipHoles(poly);

            var polyClip = poly.Factory.CreatePolygon(shellClip, holesClip);
            return polyClip;
        }

        private LinearRing[] ClipHoles(Polygon poly)
        {
            var holesClip = new List<LinearRing>();
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var holeClip = ClipRing((LinearRing)poly.GetInteriorRingN(i));
                if (holeClip != null)
                {
                    holesClip.Add(holeClip);
                }
            }
            return holesClip.ToArray();
        }

        private LinearRing ClipRing(LinearRing ring)
        {
            var pts = ClipRingToBox(ring.Coordinates);
            return ring.Factory.CreateLinearRing(pts);
        }

        /// <summary>
        /// Clips ring to rectangle box.
        /// This follows the Sutherland-Hodgson algorithm.
        /// </summary>
        /// <param name="ring"></param>
        /// <returns></returns>
        private Coordinate[] ClipRingToBox(Coordinate[] ring)
        {
            var coords = ring;
            for (int edgeIndex = 0; edgeIndex < 4; edgeIndex++)
            {
                coords = ClipRingToBoxEdge(coords, edgeIndex);
            }
            return coords;
        }

        /// <summary>
        /// Clips ring to a axis-parallel line defined by a single box edge.
        /// </summary>
        /// <param name="coords">An array of coordinates</param>
        /// <param name="edgeIndex">An edge index</param>
        private Coordinate[] ClipRingToBoxEdge(Coordinate[] coords, int edgeIndex)
        {
            var clipCoords = new CoordinateList();

            var p0 = coords[coords.Length - 1];
            for (int i = 0; i < coords.Length; i++)
            {
                var p1 = coords[i];
                if (IsInsideEdge(p1, edgeIndex))
                {
                    if (!IsInsideEdge(p0, edgeIndex))
                    {
                        var intPt = IntersectionPrecise(p0, p1, edgeIndex);
                        clipCoords.Add(intPt, false);
                    }
                    // TODO: avoid copying so much?
                    clipCoords.Add(MakePrecise(p1.Copy()), false);

                }
                else if (IsInsideEdge(p0, edgeIndex))
                {
                    var intPt = IntersectionPrecise(p0, p1, edgeIndex);
                    clipCoords.Add(intPt, false);
                }
                p0 = p1;
            }
            clipCoords.CloseRing();
            return clipCoords.ToCoordinateArray();
        }

        private Coordinate MakePrecise(Coordinate coord)
        {
            if (precModel == null)
                return coord;
            precModel.MakePrecise(coord);
            return coord;
        }

        private Coordinate IntersectionPrecise(Coordinate a, Coordinate b, int edgeIndex)
        {
            return MakePrecise(Intersection(a, b, edgeIndex));
        }

        // TODO: test that intersection computatin is robust
        // e.g. how are nearly horizontal/vertical lines handled?
        /// <summary>
        /// <para/>
        /// Due to the nature of the S-H algorithm,
        /// it should never happen that the
        /// computation of intersection line slope is infinite
        /// (i.e.encounters division-by-zero).
        /// </summary>
        /// <param name="a">A coordinate</param>
        /// <param name="b">A coordinate</param>
        /// <param name="edgeIndex">An index of an edge</param>
        /// <returns>A coordinate</returns>
        private Coordinate Intersection(Coordinate a, Coordinate b, int edgeIndex)
        {
            switch (edgeIndex)
            {
                case ENV_BOTTOM:
                    return new Coordinate(IntersectionLineY(a, b, clipEnvMinY), clipEnvMinY);
                case ENV_RIGHT:
                    return new Coordinate(clipEnvMaxX, IntersectionLineX(a, b, clipEnvMaxX));
                case ENV_TOP:
                    return new Coordinate(IntersectionLineY(a, b, clipEnvMaxY), clipEnvMaxY);
                case ENV_LEFT:
                default:
                    return new Coordinate(clipEnvMinX, IntersectionLineX(a, b, clipEnvMinX));
            }
        }

        private double IntersectionLineY(Coordinate a, Coordinate b, double y)
        {
            double m = (b.X - a.X) / (b.Y - a.Y);
            double intercept = (y - a.Y) * m;
            return a.X + intercept;
        }

        private double IntersectionLineX(Coordinate a, Coordinate b, double x)
        {
            double m = (b.Y - a.Y) / (b.X - a.X);
            double intercept = (x - a.X) * m;
            return a.Y + intercept;
        }

        private bool IsInsideEdge(Coordinate p, int edgeIndex)
        {
            switch (edgeIndex)
            {
                case ENV_BOTTOM: // bottom
                    return p.Y > clipEnvMinY;
                case ENV_RIGHT: // right
                    return p.X < clipEnvMaxX;
                case ENV_TOP: // top
                    return p.Y < clipEnvMaxY;
                case ENV_LEFT:
                default: // left
                    return p.X > clipEnvMinX;
            }
        }

    }
}
