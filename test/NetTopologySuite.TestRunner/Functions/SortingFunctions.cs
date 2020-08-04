using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Shape.Fractal;

namespace Open.Topology.TestRunner.Functions
{
    public static class SortingFunctions
    {
        public static Geometry SortByLength(Geometry g)
        {
            var geoms = Components(g);

            // annotate geometries with length
            foreach (var geom in geoms)
                geom.UserData = geom.Length;

            geoms.Sort(new UserDataDoubleComparator());
            return g.Factory.BuildGeometry(geoms);
        }


        public static Geometry SortByArea(Geometry g)
        {
            var geoms = Components(g);

            // annotate geometries with area
            foreach (var geom in geoms)
                geom.UserData = geom.Area;

            geoms.Sort(UserDataDoubleComparator.Instance);
            return g.Factory.BuildGeometry(geoms);
        }

        private static List<Geometry> Components(Geometry g)
        {
            var comp = new List<Geometry>();
            for (int i = 0; i < g.NumGeometries; i++)
            {
                comp.Add(g.GetGeometryN(i));
            }
            return comp;
        }

        public static Geometry SortByHilbertCode(Geometry g)
        {
            var geoms = Components(g);
            var env = g.EnvelopeInternal;
            // use level one less than max to avoid hitting negative integers
            int level = 15;
            int maxOrd = HilbertCode.MaxOrdinate(level);

            double strideX = env.Width / maxOrd;
            double strideY = env.Height / maxOrd;

            foreach (var geom in geoms)
            {
                var centre = geom.EnvelopeInternal.Centre;
                int x = (int)((centre.X - env.MinX) / strideX);
                int y = (int)((centre.Y - env.MinY) / strideY);
                int code = HilbertCode.Encode(level, x, y);
                geom.UserData = code;
            }

            geoms.Sort(UserDataIntComparator.Instance);

            return g.Factory.BuildGeometry(geoms);
        }

        public static Geometry SortByMortonCode(Geometry g)
        {
            var geoms = Components(g);
            var env = g.EnvelopeInternal;
            // use level one less than max to avoid hitting negative integers
            int level = 15;
            int maxOrd = MortonCode.MaxOrdinate(level);

            double strideX = env.Width / maxOrd;
            double strideY = env.Height / maxOrd;

            foreach (var geom in geoms)
            {
                var centre = geom.EnvelopeInternal.Centre;
                int x = (int)((centre.X - env.MinX) / strideX);
                int y = (int)((centre.Y - env.MinY) / strideY);
                int code = MortonCode.Encode(x, y);
                geom.UserData = code;
            }

            geoms.Sort(UserDataIntComparator.Instance);

            return g.Factory.BuildGeometry(geoms);
        }

        private class UserDataIntComparator : Comparer<Geometry>
        {

            public static Comparer<Geometry> Instance { get; } = new UserDataIntComparator();

            public override int Compare(Geometry g1, Geometry g2)
            {
                return ((int)g1.UserData).CompareTo((int)g2.UserData);
            }
        }

        private class UserDataDoubleComparator : Comparer<Geometry>
        {
            public static Comparer<Geometry> Instance { get; } = new UserDataDoubleComparator();

            public override int Compare(Geometry g1, Geometry g2)
            {
                return ((double)g1.UserData).CompareTo((double)g2.UserData);
            }
        }


    }
}
