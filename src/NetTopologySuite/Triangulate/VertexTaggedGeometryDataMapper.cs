using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// Creates a map between the vertex <see cref="Coordinate"/>s of a
    /// set of <see cref="Geometry"/>s,
    /// and the parent geometry, and transfers the source geometry
    /// data objects to geometry components tagged with the coordinates.
    /// </summary>
    /// <remarks>
    /// This class can be used in conjunction with <see cref="VoronoiDiagramBuilder"/>
    /// to transfer data objects from the input site geometries
    /// to the constructed Voronoi polygons.
    /// </remarks>
    /// <author>Martin Davis</author>
    /// <see cref="VoronoiDiagramBuilder"/>
    public class VertexTaggedGeometryDataMapper
    {
        private readonly IDictionary<Coordinate, object> _coordDataMap = new SortedDictionary<Coordinate, object>();

        /// <summary>
        /// Loads the vertices of a geometry and maps them with th the <see cref="Geometry.UserData"/>.
        /// </summary>
        /// <param name="geoms">A geometry</param>
        public void LoadSourceGeometries(Geometry geoms)
        {
            for (int i = 0; i < geoms.NumGeometries; i++)
            {
                var g = geoms.GetGeometryN(i);
                LoadVertices(g.Coordinates, g.UserData);
            }
        }

        /// <summary>
        /// Loads the vertices of a collection of geometries and maps them with the <see cref="Geometry.UserData"/>.
        /// </summary>
        /// <param name="geoms">A collection of geometry</param>
        public void LoadSourceGeometries(ICollection<Geometry> geoms)
        {
            foreach (var geom in geoms)
            {
                LoadVertices(geom.Coordinates, geom.UserData);
            }
        }

        /// <summary>
        /// Loads the vertices of the geometries of a <c>GeometryCollection</c> and maps them with the <see cref="Geometry.UserData"/>.
        /// </summary>
        /// <param name="geomColl">A <c>GeometryCollection</c></param>
        public void LoadSourceGeometries(GeometryCollection geomColl)
        {
            for (int i = 0; i < geomColl.NumGeometries; i++)
            {
                var geom = geomColl.GetGeometryN(i);
                LoadVertices(geom.Coordinates, geom.UserData);
            }
        }

        private void LoadVertices(Coordinate[] pts, object data)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                _coordDataMap.Add(pts[i], data);
            }
        }

        /// <summary>
        /// Gets a value indicating the coordinates.
        /// </summary>
        /// <returns>A list of <c>Coordinate</c>s.</returns>
        public IList<Coordinate> Coordinates => new List<Coordinate>(_coordDataMap.Keys);

        /// <summary>
        /// Input is assumed to be a multiGeometry
        /// in which every component has its userData
        /// set to be a Coordinate which is the key to the output data.
        /// The Coordinate is used to determine
        /// the output data object to be written back into the component.
        /// </summary>
        /// <param name="targetGeom" />
        public void TransferData(Geometry targetGeom)
        {
            for (int i = 0; i < targetGeom.NumGeometries; i++)
            {
                var geom = targetGeom.GetGeometryN(i);
                var vertexKey = (Coordinate)geom.UserData;
                if (vertexKey == null) continue;
                geom.UserData = _coordDataMap[vertexKey];
            }
        }
    }
}
