using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Supports linear referencing along a linear <see cref="Geometry" />
    /// using the length along the line as the index.
    /// Negative length values are taken as measured in the reverse direction
    /// from the end of the geometry.
    /// Out-of-range index values are handled by clamping
    /// them to the valid range of values.
    /// Non-simple lines (i.e. which loop back to cross or touch
    /// themselves) are supported.
    /// </summary>
    public class LengthIndexedLine
    {
        private IGeometry linearGeom = null;
 
        /// <summary>
        /// Constructs an object which allows a linear <see cref="Geometry" />
        /// to be linearly referenced using length as an index.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to reference along.</param>
        public LengthIndexedLine(IGeometry linearGeom)
        {
            this.linearGeom = linearGeom;
        }

        /// <summary>
        /// Computes the <see cref="Coordinate" /> for the point
        /// on the line at the given index.
        /// If the index is out of range the first or last point on the
        /// line will be returned.
        /// </summary>
        /// <param name="index">The index of the desired point.</param>
        /// <returns>The <see cref="Coordinate" /> at the given index.</returns>
        public ICoordinate ExtractPoint(double index)
        {
            LinearLocation loc = LengthLocationMap.GetLocation(linearGeom, index);
            return loc.GetCoordinate(linearGeom);
        }
        
        /// <summary>
        /// Computes the <see cref="LineString" /> for the interval
        /// on the line between the given indices.
        /// If the <paramref name="endIndex" /> lies before the <paramref name="startIndex" />,
        /// the computed geometry is reversed.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public IGeometry ExtractLine(double startIndex, double endIndex)
        {
            LocationIndexedLine lil = new LocationIndexedLine(linearGeom);
            LinearLocation startLoc = LocationOf(startIndex);
            LinearLocation endLoc = LocationOf(endIndex);
            return ExtractLineByLocation.Extract(linearGeom, startLoc, endLoc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private LinearLocation LocationOf(double index)
        {
            return LengthLocationMap.GetLocation(linearGeom, index);
        }

        /// <summary>
        /// Computes the minimum index for a point on the line.
        /// If the line is not simple (i.e. loops back on itself)
        /// a single point may have more than one possible index.
        /// In this case, the smallest index is returned.
        /// The supplied point does not necessarily have to lie precisely
        /// on the line, but if it is far from the line the accuracy and
        /// performance of this function is not guaranteed.
        /// Use <see cref="Project" /> to compute a guaranteed result for points
        /// which may be far from the line.
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <returns>The minimum index of the point.</returns>
        public double IndexOf(ICoordinate pt)
        {
            return LengthIndexOfPoint.IndexOf(linearGeom, pt);
        }

        /// <summary>
        /// Finds the index for a point on the line
        /// which is greater than the given index.
        /// If no such index exists, returns <paramref name="minIndex" />.
        /// This method can be used to determine all indexes for
        /// a point which occurs more than once on a non-simple line.
        /// It can also be used to disambiguate cases where the given point lies
        /// slightly off the line and is equidistant from two different
        /// points on the line.
        /// The supplied point does not necessarily have to lie precisely
        /// on the line, but if it is far from the line the accuracy and
        /// performance of this function is not guaranteed.
        /// Use <see cref="Project" /> to compute a guaranteed result for points
        /// which may be far from the line.
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <param name="minIndex">The value the returned index must be greater than.</param>
        /// <returns>The index of the point greater than the given minimum index.</returns>
        public double IndexOfAfter(ICoordinate pt, double minIndex)
        {
            return LengthIndexOfPoint.IndexOfAfter(linearGeom, pt, minIndex);
        }

        /// <summary>
        /// Computes the indices for a subline of the line.
        /// (The subline must conform to the line; that is,
        /// all vertices in the subline (except possibly the first and last)
        /// must be vertices of the line and occcur in the same order).
        /// </summary>
        /// <param name="subLine">A subLine of the line.</param>
        /// <returns>A pair of indices for the start and end of the subline..</returns>
        public double[] IndicesOf(IGeometry subLine)
        {
            LinearLocation[] locIndex = LocationIndexOfLine.IndicesOf(linearGeom, subLine);
            double[] index = new double[] 
            {
                LengthLocationMap.GetLength(linearGeom, locIndex[0]),
                LengthLocationMap.GetLength(linearGeom, locIndex[1]), 
            };
            return index;
        }

        /// <summary>
        /// Computes the index for the closest point on the line to the given point.
        /// If more than one point has the closest distance the first one along the line is returned.
        /// (The point does not necessarily have to lie precisely on the line.)
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public double Project(ICoordinate pt)
        {
            return LengthIndexOfPoint.IndexOf(linearGeom, pt);
        }

        /// <summary>
        /// Returns the index of the start of the line.
        /// </summary>
        public double StartIndex
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the index of the end of the line.
        /// </summary>
        public double EndIndex
        {
            get
            {
                return linearGeom.Length;
            }
        }

        /// <summary>
        /// Tests whether an index is in the valid index range for the line.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><c>true</c> if the index is in the valid range.</returns>
        public bool IsValidIndex(double index)
        {
            return (index >= StartIndex && index <= EndIndex);
        }

        /// <summary>
        /// Computes a valid index for this line
        /// by clamping the given index to the valid range of index values
        /// </summary>
        /// <param name="index"></param>
        /// <returns>A valid index value.</returns>
        public double ClampIndex(double index)
        {            
            if (index < StartIndex) 
                return StartIndex;            
            if (index > EndIndex) 
                return EndIndex;
            return index;
        }
    }
}
