using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Extension
{
    public static class IEnvelopeEx
    {
        public static double GetMin(this IEnvelope self, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return self.MinX;
                case Ordinate.Y:
                    return self.MinY;
                default:
                    throw new NotImplementedException();
            }
        }
        public static double GetMax(this IEnvelope self, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return self.MaxX;
                case Ordinate.Y:
                    return self.MaxY;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}