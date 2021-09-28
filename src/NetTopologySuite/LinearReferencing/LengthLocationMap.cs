using NetTopologySuite.Geometries;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Computes the <see cref="LinearLocation" /> for a given length
    /// along a linear <see cref="Geometry" />
    /// Negative lengths are measured in reverse from end of the linear geometry.
    /// Out-of-range values are clamped.
    /// </summary>
    public class LengthLocationMap
    {
        // TODO: cache computed cumulative length for each vertex
        // TODO: support user-defined measures
        // TODO: support measure index for fast mapping to a location

        /// <summary>
        /// Computes the <see cref="LinearLocation" /> for a
        /// given length along a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use.</param>
        /// <param name="length">The length index of the location.</param>
        /// <returns>The <see cref="LinearLocation" /> for the length.</returns>
        public static LinearLocation GetLocation(Geometry linearGeom, double length)
        {
            var locater = new LengthLocationMap(linearGeom);
            return locater.GetLocation(length);
        }

        /// <summary>
        /// Computes the <see cref="LinearLocation"/> for a
        /// given length along a linear <see cref="Geometry"/>,
        /// with control over how the location
        /// is resolved at component endpoints.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use</param>
        /// <param name="length">The length index of the location</param>
        /// <param name="resolveLower">If true lengths are resolved to the lowest possible index</param>
        public static LinearLocation GetLocation(Geometry linearGeom, double length, bool resolveLower)
        {
            var locater = new LengthLocationMap(linearGeom);
            return locater.GetLocation(length, resolveLower);
        }

        /// <summary>
        /// Computes the length for a given <see cref="LinearLocation" />
        /// on a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use.</param>
        /// <param name="loc">The <see cref="LinearLocation" /> index of the location.</param>
        /// <returns>The length for the <see cref="LinearLocation" />.</returns>
        public static double GetLength(Geometry linearGeom, LinearLocation loc)
        {
            var locater = new LengthLocationMap(linearGeom);
            return locater.GetLength(loc);
        }

        private readonly Geometry _linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthLocationMap"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LengthLocationMap(Geometry linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>
        /// Compute the <see cref="LinearLocation" /> corresponding to a length.
        /// Negative lengths are measured in reverse from end of the linear geometry.
        /// Out-of-range values are clamped.
        /// Ambiguous indexes are resolved to the lowest possible location value.
        /// </summary>
        /// <param name="length">The length index.</param>
        /// <returns>The corresponding <see cref="LinearLocation" />.</returns>
        public LinearLocation GetLocation(double length)
        {
            return GetLocation(length, true);
        }

        /// <summary>
        /// Compute the <see cref="LinearLocation"/> corresponding to a length.
        /// Negative lengths are measured in reverse from end of the linear geometry.
        /// Out-of-range values are clamped.
        /// Ambiguous indexes are resolved to the lowest or highest possible location value,
        /// depending on the value of <tt>resolveLower</tt>
        /// </summary>
        /// <param name="length">The length index</param>
        /// <param name="resolveLower"></param>
        /// <returns>The corresponding <see cref="LinearLocation" />.</returns>
        public LinearLocation GetLocation(double length, bool resolveLower)
        {
            double forwardLength = length;

            // negative values are measured from end of geometry
            if (length < 0.0)
            {
                double lineLen = _linearGeom.Length;
                forwardLength = lineLen + length;
            }
            var loc = GetLocationForward(forwardLength);
            if (resolveLower)
            {
                return loc;
            }
            return ResolveHigher(loc);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private LinearLocation GetLocationForward(double length)
        {
            if (length <= 0.0)
                return new LinearLocation();

            double totalLength = 0.0;

            var it = new LinearIterator(_linearGeom);
            while (it.HasNext())
            {
                /*
                 * Special handling is required for the situation when the
                 * length references exactly to a component endpoint.
                 * In this case, the endpoint location of the current component
                 * is returned,
                 * rather than the startpoint location of the next component.
                 * This produces consistent behaviour with the project method.
                 */
                if (it.IsEndOfLine)
                {
                    if (totalLength == length)
                    {
                        int compIndex = it.ComponentIndex;
                        int segIndex = it.VertexIndex;
                        return new LinearLocation(compIndex, segIndex, 0.0);
                    }
                }
                else
                {
                    var p0 = it.SegmentStart;
                    var p1 = it.SegmentEnd;
                    double segLen = p1.Distance(p0);
                    // length falls in this segment
                    if (totalLength + segLen > length)
                    {
                        double frac = (length - totalLength) / segLen;
                        int compIndex = it.ComponentIndex;
                        int segIndex = it.VertexIndex;
                        return new LinearLocation(compIndex, segIndex, frac);
                    }
                    totalLength += segLen;
                }

                it.Next();
            }
            // length is longer than line - return end location
            return LinearLocation.GetEndLocation(_linearGeom);
        }

        private LinearLocation ResolveHigher(LinearLocation loc)
        {
            if (!loc.IsEndpoint(_linearGeom))
                return loc;
            int compIndex = loc.ComponentIndex;
            // if last component can't resolve any higher
            if (compIndex >= _linearGeom.NumGeometries - 1) return loc;

            do
            {
                compIndex++;
            } while (compIndex < _linearGeom.NumGeometries - 1
                && _linearGeom.GetGeometryN(compIndex).Length == 0);
            // resolve to next higher location
            return new LinearLocation(compIndex, 0, 0.0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public double GetLength(LinearLocation loc)
        {
            double totalLength = 0.0;

            var it = new LinearIterator(_linearGeom);
            while (it.HasNext())
            {
                if (!it.IsEndOfLine)
                {
                    var p0 = it.SegmentStart;
                    var p1 = it.SegmentEnd;
                    double segLen = p1.Distance(p0);
                    // length falls in this segment
                    if (loc.ComponentIndex == it.ComponentIndex
                        && loc.SegmentIndex == it.VertexIndex)
                    {
                        return totalLength + segLen * loc.SegmentFraction;
                    }
                    totalLength += segLen;
                }
                it.Next();
            }
            return totalLength;
        }
    }
}
