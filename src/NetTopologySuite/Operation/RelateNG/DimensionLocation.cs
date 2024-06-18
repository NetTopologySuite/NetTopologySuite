using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Codes which combine a geometry <see cref="Geometries.Dimension"/> and a <see cref="Geometries.Location"/>
    /// </summary>
    /// <author>Martin Davis</author>
    internal static class DimensionLocation
    {

        public const int EXTERIOR = (int)Geometries.Location.Exterior;
        public const int POINT_INTERIOR = 103;
        public const int LINE_INTERIOR = 110;
        public const int LINE_BOUNDARY = 111;
        public const int AREA_INTERIOR = 120;
        public const int AREA_BOUNDARY = 121;

        public static int LocationArea(Location loc)
        {
            switch (loc)
            {
                case Geometries.Location.Interior: return AREA_INTERIOR;
                case Geometries.Location.Boundary: return AREA_BOUNDARY;
            }
            return EXTERIOR;
        }

        public static int LocationLine(Location loc)
        {
            switch (loc)
            {
                case Geometries.Location.Interior: return LINE_INTERIOR;
                case Geometries.Location.Boundary: return LINE_BOUNDARY;
            }
            return EXTERIOR;
        }

        public static int LocationPoint(Location loc)
        {
            switch (loc)
            {
                case Geometries.Location.Interior: return POINT_INTERIOR;
            }
            return EXTERIOR;
        }

        public static Location Location(int dimLoc)
        {
            switch (dimLoc)
            {
                case POINT_INTERIOR:
                case LINE_INTERIOR:
                case AREA_INTERIOR:
                    return Geometries.Location.Interior;
                case LINE_BOUNDARY:
                case AREA_BOUNDARY:
                    return Geometries.Location.Boundary;
            }
            return Geometries.Location.Exterior;
        }

        public static Dimension Dimension(int dimLoc)
        {
            switch (dimLoc)
            {
                case POINT_INTERIOR:
                    return Geometries.Dimension.P;
                case LINE_INTERIOR:
                case LINE_BOUNDARY:
                    return Geometries.Dimension.L;
                case AREA_INTERIOR:
                case AREA_BOUNDARY:
                    return Geometries.Dimension.A;
            }
            return Geometries.Dimension.False;
        }

        public static Dimension Dimension(int dimLoc, Dimension exteriorDim)
        {
            if (dimLoc == EXTERIOR)
                return exteriorDim;
            return Dimension(dimLoc);
        }

    }
}
