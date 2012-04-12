using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace Open.Topology.TestRunner.Functions
{
    public class OrientationFunctions
    {
        public static int orientationIndex(IGeometry segment, IGeometry ptGeom)
        {
            if (segment.NumPoints != 2 || ptGeom.NumPoints != 1)
            {
                throw new ArgumentException("A must have two points and B must have one");
            }
            var segPt = segment.Coordinates;

            var p = ptGeom.Coordinate;
            var index = CGAlgorithms.OrientationIndex(segPt[0], segPt[1], p);
            return index;
        }

        public static int orientationIndexDD(IGeometry segment, IGeometry ptGeom)
        {
            if (segment.NumPoints != 2 || ptGeom.NumPoints != 1)
            {
                throw new ArgumentException("A must have two points and B must have one");
            }
            var segPt = segment.Coordinates;

            var p = ptGeom.Coordinate;
            var index = CGAlgorithmsDD.OrientationIndex(segPt[0], segPt[1], p);
            return index;
        }
    }
}