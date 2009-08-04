using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
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
        /// to <see cref="List{T}<NodedSegmentString{TCoordinate}"/>.
        /// The SegmentString data item is set to be the source Geometry.
        ///</summary>
        ///<param name="geom">the geometry to extract from</param>
        ///<returns>a List of SegmentStrings</returns>
        public static List<NodedSegmentString<TCoordinate>> ExtractSegmentStrings(IGeometry<TCoordinate> geom)
        {
            List<NodedSegmentString<TCoordinate>> segStr = new List<NodedSegmentString<TCoordinate>>();
            foreach (ILineString<TCoordinate> line in GeometryFilter.Filter<ILineString<TCoordinate>, TCoordinate>(geom)
                ) // LinearComponentExtracter<TCoordinate>.GetLines(geom) )
            {
                ICoordinateSequence<TCoordinate> pts = line.Coordinates;
                segStr.Add(new NodedSegmentString<TCoordinate>(pts, null));
            }
            return segStr;
        }
    }
}