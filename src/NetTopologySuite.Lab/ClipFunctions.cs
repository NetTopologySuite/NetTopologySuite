using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Clip;
using NetTopologySuite.Geometries;

namespace NetTopologySuite
{
    public class ClipFunctions
    {

        public static Geometry ClipPoly(Geometry geom, Geometry rectangle)
        {
            return RectangleClipPolygon.Clip(geom, rectangle);
        }

        public static Geometry ClipPolyPrecise(Geometry geom, Geometry rectangle, double scaleFactor)
        {
            return RectangleClipPolygon.Clip(geom, rectangle, new PrecisionModel(scaleFactor));
        }

        public static Geometry ClipByIntersection(Geometry geom, Geometry rectangle)
        {
            // short-circuit check
            var rectEnv = rectangle.EnvelopeInternal;
            if (rectEnv.Contains(geom.EnvelopeInternal)) return geom.Copy();

            return rectangle.Intersection(geom);
        }

        public static Geometry ClipByIntersectionOpt(Geometry geom, Geometry rectangle)
        {
            // short-circuit check
            var rectEnv = rectangle.EnvelopeInternal;
            if (rectEnv.Contains(geom.EnvelopeInternal)) return geom.Copy();
            if (!rectangle.Intersects(geom)) return null;
            return rectangle.Intersection(geom);
        }
    }
}
