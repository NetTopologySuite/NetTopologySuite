using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Extension
{
    public static class IEnvelopeEx
    {
        public static double GetMin(this IEnvelope self, Ordinates ordinate)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                    return self.MinX;
                case Ordinates.Y:
                    return self.MinY;
                default:
                    throw new NotImplementedException();
            }
        }
        public static double GetMax(this IEnvelope self, Ordinates ordinate)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                    return self.MaxX;
                case Ordinates.Y:
                    return self.MaxY;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}