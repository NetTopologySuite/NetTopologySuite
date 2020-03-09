using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;

namespace NetTopologySuite
{
    public class ClipFunctions
    {

        public static Geometry clipPoly(Geometry geom, Geometry rectangle)
        {
            return RectangleClipPolygon.clip(geom, rectangle);
        }

        public static Geometry clipPolyPrecise(Geometry geom, Geometry rectangle, double scaleFactor)
        {
            return RectangleClipPolygon.clip(geom, rectangle, new PrecisionModel(scaleFactor));
        }
    }
}
