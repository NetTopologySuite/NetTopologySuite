using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Serves to probe linear rings
    /// </summary>
    /// <author>Bruno.Labrecque@mddep.gouv.qc.ca</author>
    internal class ProbeLinearRing : IComparer<ILinearRing>
    {

        internal enum Order
        {
            Ascending,
            Descending
        }

        internal ProbeLinearRing()
            :this(Order.Descending)
        {
        }

        internal ProbeLinearRing(Order order)
        {
            switch (order)
            {
                case Order.Ascending:
                    _r1 = 1;
                    _r2 = -1;
                    break;
                case Order.Descending:
                    _r1 = -1;
                    _r2 = 1;
                    break;
            }
        }

        private readonly int _r1;

        private readonly int _r2;

        public int Compare(ILinearRing x, ILinearRing y)
        {
            var pm = PrecisionModel.MostPrecise(x.PrecisionModel, y.PrecisionModel);
            var geometryFactory = new GeometryFactory(pm);

            var p1 = geometryFactory.CreatePolygon(x, null);
            var p2 = geometryFactory.CreatePolygon(y, null); ;
            if (p1.Area < p2.Area)
                return _r1;
            return p1.Area > p2.Area ? _r2 : 0;
        }
    }
}
