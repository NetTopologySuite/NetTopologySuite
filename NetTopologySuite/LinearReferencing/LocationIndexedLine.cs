using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Supports linear referencing along a linear <see cref="Geometry" />
    /// using <see cref="LinearLocation" />s as the index.
    /// </summary>
    public class LocationIndexedLine
    {
        private IGeometry linearGeom = null;

        /// <summary>
        /// Constructs an object which allows linear referencing along
        /// a given linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom"></param>
        public LocationIndexedLine(IGeometry linearGeom)
        {
            this.linearGeom = linearGeom;
            CheckGeometryType();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckGeometryType()
        {
            if (!(linearGeom is ILineString || linearGeom is IMultiLineString))
                throw new ArgumentException("Input geometry must be linear", "linearGeom");
        }

        /// <summary>
        /// Computes the <see cref="Coordinate" />for the point on the line at the given index.
        /// If the <paramref name="index" /> is out of range,
        /// the first or last point on the line will be returned.
        /// </summary>
        /// <param name="index">The index of the desired point.</param>
        /// <returns>The <see cref="Coordinate" /> at the given index.</returns>
        public ICoordinate ExtractPoint(LinearLocation index)
        {
            return index.GetCoordinate(linearGeom);
        }

        /// <summary>
        /// Computes the <see cref="LineString" /> for the interval
        /// on the line between the given indices.
        /// </summary>
        /// <param name="startIndex">The index of the start of the interval.</param>
        /// <param name="endIndex">The index of the end of the interval.</param>
        /// <returns>The linear interval between the indices.</returns>
        public IGeometry ExtractLine(LinearLocation startIndex, LinearLocation endIndex)
        {
            return ExtractLineByLocation.Extract(linearGeom, startIndex, endIndex);
        }

        /// <summary>
        /// Computes the index for a given point on the line.
        /// The supplied point does not necessarily have to lie precisely
        /// on the line, but if it is far from the line the accuracy and
        /// performance of this function is not guaranteed.
        /// Use <see cref="Project" /> to compute a guaranteed result for points
        /// which may be far from the line.
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <returns>The index of the point.</returns>
        public LinearLocation IndexOf(ICoordinate pt)
        {
            return LocationIndexOfPoint.IndexOf(linearGeom, pt);
        }

        /// <summary>
        /// Computes the indices for a subline of the line.
        /// (The subline must conform to the line; that is,
        /// all vertices in the subline (except possibly the first and last)
        /// must be vertices of the line and occcur in the same order).
        /// </summary>
        /// <param name="subLine">A subLine of the line.</param>
        /// <returns>A pair of indices for the start and end of the subline.</returns>
        public LinearLocation[] IndicesOf(IGeometry subLine)
        {
            return LocationIndexOfLine.IndicesOf(linearGeom, subLine);
        }

        /// <summary>
        /// Computes the index for the closest point on the line to the given point.
        /// If more than one point has the closest distance the first one along the line is returned.
        /// (The point does not necessarily have to lie precisely on the line.)
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <returns>The index of the point.</returns>
        public LinearLocation Project(ICoordinate pt)
        {
            return LocationIndexOfPoint.IndexOf(linearGeom, pt);
        }

        /// <summary>
        /// Returns the index of the start of the line.
        /// </summary>
        public LinearLocation StartIndex
        {
            get
            {
                return new LinearLocation();
            }
        }

        /// <summary>
        /// Returns the index of the end of the line.
        /// </summary>
        public LinearLocation EndIndex
        {
            get
            {
                return LinearLocation.GetEndLocation(linearGeom);
            }
        }

        /// <summary>
        /// Tests whether an index is in the valid index range for the line.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><c>true</c> if the index is in the valid range.</returns>
        public bool isValidIndex(LinearLocation index)
        {
            return index.IsValid(linearGeom);
        }

        /// <summary>
        /// Computes a valid index for this line by clamping 
        /// the given index to the valid range of index values.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>A valid index value.</returns>
        public LinearLocation ClampIndex(LinearLocation index)
        {
            LinearLocation loc = (LinearLocation) index.Clone();
            loc.Clamp(linearGeom);
            return loc;
        }
    }
}
