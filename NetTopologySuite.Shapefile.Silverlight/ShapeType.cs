// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Shapefile
{
    /// <summary>
    /// <see cref="GisSharpBlog.NetTopologySuite.Shapefile"/> geometry types.
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        /// Null shape with no geometric data.
        /// </summary>
        Null = 0,

        /// <summary>
        /// A point.
        /// </summary>
        /// <remarks>
        /// A point consists of a Double-precision coordinate in 2D space.
        /// SharpMap interprets this as <see cref="IPoint"/>.
        /// </remarks>
        Point = 1,

        /// <summary>
        /// A connected line segment or segments.
        /// </summary>
        /// <remarks>
        /// PolyLine is an ordered set of vertices that consists of one or more parts. 
        /// A part is a connected sequence of two or more points. Parts may or may not 
        /// be connected to one another. Parts may or may not intersect one another.
        /// SharpMap interprets this as either 
        /// <see cref="ILineString"/> 
        /// or <see cref="IMultiLineString"/>.
        /// </remarks>
        PolyLine = 3,

        /// <summary>
        /// A connected line segment with at least one closure.
        /// </summary>
        /// <remarks>
        /// A polygon consists of one or more rings. A ring is a connected sequence of four or more
        /// points that form a closed, non-self-intersecting loop. A polygon may contain multiple
        /// outer rings. The order of vertices or orientation for a ring indicates which side of the ring
        /// is the interior of the polygon. The neighborhood to the right of an observer walking along
        /// the ring in vertex order is the neighborhood inside the polygon. Vertices of rings defining
        /// holes in polygons are in a counterclockwise direction. Vertices for a single, ringed
        /// polygon are, therefore, always in clockwise order. The rings of a polygon are referred to
        /// as its parts.
        /// SharpMap interprets this as either <see cref="IPolygon"/> 
        /// or <see cref="IMultiPolygon"/>.
        /// </remarks>
        Polygon = 5,

        /// <summary>
        /// A set of <see cref="ShapeType.Point">points</see>.
        /// </summary>
        /// <remarks>
        /// SharpMap interprets this as <see cref="IMultiPoint"/>.
        /// </remarks>
        MultiPoint = 8,

        /// <summary>
        /// A 3D <see cref="ShapeType.Point">point</see>.
        /// </summary>
        /// <remarks>
        /// A PointZ has 3 components to the coordinate vector, allowing it to be positioned 
        /// anywhere in 3D space.
        /// SharpMap interprets this as <see cref="IPoint3D"/>.
        /// </remarks>
        PointZ = 11,

        /// <summary>
        /// A 3D <see cref="ShapeType.PolyLine"/>, consisting of <see cref="PointZ"/> points.
        /// </summary>
        /// <remarks>
        /// A PolyLineZ consists of one or more parts. A part is a connected sequence of two or
        /// more points. Parts may or may not be connected to one another. Parts may or may not
        /// intersect one another.
        /// SharpMap interprets this as <see cref="ILineString"/> 
        /// or <see cref="IMultiLineString"/>.
        /// </remarks>
        PolyLineZ = 13,

        /// <summary>
        /// A 3D <see cref="ShapeType.Polygon"/>, consisting of <see cref="PointZ"/> points.
        /// </summary>
        /// <remarks>
        /// A PolygonZ consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// A PolygonZ may contain multiple outer rings. The rings of a PolygonZ are referred to as
        /// its parts.
        /// SharpMap interprets this as either <see cref="IPolygon"/> 
        /// or <see cref="IMultiPolygon"/>.
        /// </remarks>
        PolygonZ = 15,

        /// <summary>
        /// A set of <see cref="PointZ"/>s.
        /// </summary>
        /// <remarks>
        /// SharpMap interprets this as <see cref="IMultiPoint"/>.
        /// </remarks>
        MultiPointZ = 18,

        /// <summary>
        /// A <see cref="ShapeType.Point"/> plus a measure value as a Double-precision floating point.
        /// </summary>
        /// <remarks>
        /// A PointM consists of a Double-precision coordinate in the order 'X', 'Y', and a measure 'M'.
        /// SharpMap interprets this as <see cref="IPoint"/>.
        /// </remarks>
        PointM = 21,

        /// <summary>
        /// A <see cref="ShapeType.PolyLine"/>, consisting of <see cref="PointM"/> points.
        /// </summary>
        /// <remarks>
        /// A shapefile PolyLineM consists of one or more parts. A part is a connected sequence of
        /// two or more points. Parts may or may not be connected to one another. Parts may or may
        /// not intersect one another.
        /// SharpMap interprets this as <see cref="ILineString"/> 
        /// or <see cref="IMultiLineString"/>.
        /// </remarks>
        PolyLineM = 23,

        /// <summary>
        /// A <see cref="ShapeType.PolyLine"/>, consisting of <see cref="PointM"/> points.
        /// </summary>
        /// <remarks>
        /// A PolygonM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// SharpMap interprets this as either <see cref="IPolygon"/> 
        /// or <see cref="IMultiPolygon"/>.
        /// </remarks>
        PolygonM = 25,

        /// <summary>
        /// A set of <see cref="PointM"/>s.
        /// </summary>
        /// <remarks>
        /// SharpMap interprets this as <see cref="IMultiPoint"/>.
        /// </remarks>
        MultiPointM = 28,

        /// <summary>
        /// A set of surface patches.
        /// </summary>
        /// <remarks>
        /// A MultiPatch consists of a number of surface patches. Each surface patch describes a
        /// surface. The surface patches of a MultiPatch are referred to as its parts, and the type of
        /// part controls how the order of vertices of an MultiPatch part is interpreted.
        /// SharpMap doesn't support this feature type.
        /// </remarks>
        MultiPatch = 31
    }
}