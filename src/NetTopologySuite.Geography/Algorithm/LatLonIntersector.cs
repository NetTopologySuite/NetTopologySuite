using NetTopologySuite.Geography.Lib;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Algorithm
{
    internal class LatLonIntersector : LineIntersector
    {
        private readonly Geodesic _geodesic;

        public override int ComputeIntersect(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            throw new NotImplementedException();
        }

        public override void ComputeIntersection(Coordinate p, Coordinate p1, Coordinate p2)
        {
            var gl = new Geography.Lib.GeodesicLine();
            //gl.p
            throw new NotImplementedException();
        }
    }
}
