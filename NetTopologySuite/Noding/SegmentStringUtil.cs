using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    ///<summary>
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class SegmentStringUtil<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /**
         * 
         * 
         * @param geom 
         * @return 
         */

        ///<summary>
        /// Extracts all linear components from a given <see cref="IGeometry{TCoordinate}"/>
        /// to <see cref="EdgeList{TCoordinate}"/>.
        /// The SegmentString data item is set to be the source Geometry.
        ///</summary>
        ///<param name="geom">the geometry to extract from</param>
        ///<returns>a List of SegmentStrings</returns>
        public static EdgeList<TCoordinate> ExtractSegmentStrings(IGeometry<TCoordinate> geom)
        {
            EdgeList<TCoordinate> segStr = new EdgeList<TCoordinate>(geom.Factory);
            foreach (ILineString<TCoordinate> line in LinearComponentExtracter<TCoordinate>.GetLines(geom))
            {
                ICoordinateSequence<TCoordinate> pts = line.Coordinates;
                segStr.Add(new Edge<TCoordinate>(geom.Factory, pts));
            }
            return segStr;
        }
    }
}