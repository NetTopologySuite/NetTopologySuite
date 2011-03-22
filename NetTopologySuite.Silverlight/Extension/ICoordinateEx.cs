using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Extension
{
    public static class ICoordinateEx
    {

        public static bool ContainsOrdinate(this ICoordinate self, Ordinates ordinate)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                case Ordinates.Y:
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        public static double GetOrdinate(this ICoordinate self, Ordinates ordinate)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                    return self.X;
                case Ordinates.Y:
                    return self.Y;
                case Ordinates.Z:
                    return self.Z;
                default:
                    throw new NotImplementedException();
            }

        }
    }
}