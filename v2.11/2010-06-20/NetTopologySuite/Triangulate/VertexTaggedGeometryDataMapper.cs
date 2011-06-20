using System;
using System.Collections.Generic;
using C5;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate
{
    ///<summary>
    ///Creates a map between the vertex {@link Coordinate}s of a 
    ///set of <see cref="IGeometry{TCoordinate}"/>s,
    ///and the parent geometry, and transfers the source geometry
    ///data objects to geometry components tagged with the coordinates.
    ///<para>
    ///This class can be used in conjunction with <see cref="VoronoiDiagramBuilder{TCoordinate}"/>
    ///to transfer data objects from the input site geometries
    ///to the constructed Voronou polygons.
    ///</summary>
    /// <see cref="VoronoiDiagramBuilder{TCoordinate}"/>
    ///<typeparam name="TCoordinate"></typeparam>
    public class VertexTaggedGeometryDataMapper<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible

    {
        private readonly TreeDictionary<TCoordinate, IGeometry<TCoordinate>> _coordDataMap = new TreeDictionary<TCoordinate, IGeometry<TCoordinate>>();

        public VertexTaggedGeometryDataMapper()
        {

        }

        public void LoadSourceGeometries(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            foreach(IGeometry<TCoordinate> geom in geoms)
            {
                LoadVertices(geom.Coordinates, geom.UserData);
            }
        }

        public void LoadSourceGeometries(IGeometryCollection<TCoordinate> geomColl)
        {
            foreach (IGeometry<TCoordinate> geom in geomColl)
            {
                LoadVertices(geom.Coordinates, geom.UserData);
            }
            
        }

        private void LoadVertices(IEnumerable<TCoordinate> pts, Object data)
        {
            foreach (TCoordinate pt in pts)
            {
                _coordDataMap.Add(pt, (IGeometry<TCoordinate>)data);
            }
        }

        public ISorted<TCoordinate> GetCoordinates()
        {
            return _coordDataMap.Keys;
        }

        ///<summary>
        /// Input is assumed to be a multiGeometry in which every component has its userData
        /// set to be a Coordinate which is the key to the output data.
        /// The Coordinate is used to determine the output data object to be written back 
        /// into the component. 
        ///</summary>
        ///<param name="targetGeom"></param>
        public void TransferData(IGeometry<TCoordinate> targetGeom)
        {
            IGeometryCollection<TCoordinate> tg = targetGeom as IGeometryCollection<TCoordinate>;
            if (tg != null)
            {   
                foreach (IGeometry<TCoordinate> geom in tg)
                {
                    TransferData(geom);
                }
                return;
            }

            TCoordinate vertexKey = (TCoordinate)targetGeom.UserData;
            if (!(Equals(vertexKey, null) || vertexKey.IsEmpty))
                targetGeom.UserData = _coordDataMap[vertexKey];

       }
    }
}
