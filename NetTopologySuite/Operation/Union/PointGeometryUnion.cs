#define Goletas
using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Union
{
    public class PointGeometryUnion
    {
        private readonly IGeometryFactory _geomFact;
        private readonly IGeometry _otherGeom;
        private readonly IGeometry _pointGeom;

        public PointGeometryUnion(IPoint pointGeom, IGeometry otherGeom)
        {
            _pointGeom = pointGeom;
            _otherGeom = otherGeom;
            _geomFact = otherGeom.Factory;
        }

        ///<summary>
        /// Computes the union of a <see cref="IPoint"/> geometry with 
        /// another arbitrary <see cref="IGeometry"/>.
        /// Does not copy any component geometries.
        ///</summary>
        ///<param name="pointGeom"></param>
        ///<param name="otherGeom"></param>
        ///<returns></returns>
        public static IGeometry Union(IPoint pointGeom, IGeometry otherGeom)
        {
            PointGeometryUnion unioner = new PointGeometryUnion(pointGeom, otherGeom);
            return unioner.Union();
        }

        public IGeometry Union()
        {
            PointLocator locater = new PointLocator();

            // use a set to eliminate duplicates, as required for union
#if Goletas
            HashSet<ICoordinate> exteriorCoords = new HashSet<ICoordinate>();
#else
            TreeSet exteriorCoords = new TreeSet();
#endif

            foreach (IPoint point in PointExtracter.GetPoints(_pointGeom))
            {
                ICoordinate coord = point.Coordinate;
                Locations loc = locater.Locate(coord, _otherGeom);

                if (loc == Locations.Exterior)
                {
                    exteriorCoords.Add(coord);
                }
            }

            // if no points are in exterior, return the other geom
            if (exteriorCoords.Count == 0)
            {
                return _otherGeom;
            }

            // make a puntal geometry of appropriate size
            IGeometry ptComp = null;
            ICoordinateSequence coords = _geomFact.CoordinateSequenceFactory.Create(exteriorCoords.ToArray());
            ptComp = coords.Count == 1 ? (IGeometry)_geomFact.CreatePoint(coords.GetCoordinate(0)) : _geomFact.CreateMultiPoint(coords);

            // add point component to the other geometry
            return GeometryCombiner.Combine(ptComp, _otherGeom);
        }
    }
}
