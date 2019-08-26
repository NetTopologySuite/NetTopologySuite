using NetTopologySuite.Geometries;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Determines the location of a subline along a linear <see cref="Geometry" />.
    /// The location is reported as a pair of <see cref="LinearLocation" />s.
    /// NOTE: Currently this algorithm is not guaranteed to
    /// return the correct substring in some situations where
    /// an endpoint of the test line occurs more than once in the input line.
    /// (However, the common case of a ring is always handled correctly).
    /// </summary>
    public class LocationIndexOfLine
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="linearGeom"></param>
        /// <param name="subLine"></param>
        /// <returns></returns>
        public static LinearLocation[] IndicesOf(Geometry linearGeom, Geometry subLine)
        {
            /*
             * MD - this algorithm has been extracted into a class
             * because it is intended to validate that the subline truly is a subline,
             * and also to use the internal vertex information to unambiguously locate the subline.
             */
            var locater = new LocationIndexOfLine(linearGeom);
            return locater.IndicesOf(subLine);
        }

        private readonly Geometry _linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationIndexOfLine"/> class.
        /// </summary>
        /// <param name="linearGeom">The linear geom.</param>
        public LocationIndexOfLine(Geometry linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="subLine"></param>
        /// <returns></returns>
        public virtual LinearLocation[] IndicesOf(Geometry subLine)
        {
            var startPt = ((LineString) subLine.GetGeometryN(0)).GetCoordinateN(0);
            var lastLine = (LineString) subLine.GetGeometryN(subLine.NumGeometries - 1);
            var endPt = lastLine.GetCoordinateN(lastLine.NumPoints - 1);

            var locPt = new LocationIndexOfPoint(_linearGeom);
            var subLineLoc = new LinearLocation[2];
            subLineLoc[0] = locPt.IndexOf(startPt);

            // check for case where subline is zero length
            if (subLine.Length == 0)
                 subLineLoc[1] = (LinearLocation) subLineLoc[0].Copy();
            else subLineLoc[1] = locPt.IndexOfAfter(endPt, subLineLoc[0]);
            return subLineLoc;
        }
    }
}
