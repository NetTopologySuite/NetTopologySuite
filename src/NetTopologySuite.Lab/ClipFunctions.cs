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
    }
}
