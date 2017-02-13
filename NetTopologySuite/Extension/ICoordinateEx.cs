using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Extension
{
    public static class ICoordinateEx
    {

        public static bool ContainsOrdinate(this ICoordinate self, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                case Ordinate.Y:
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        public static double GetOrdinate(this ICoordinate self, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return self.X;
                case Ordinate.Y:
                    return self.Y;
                case Ordinate.Z:
                    return self.Z;
                default:
                    throw new NotImplementedException();
            }

        }
    }
}