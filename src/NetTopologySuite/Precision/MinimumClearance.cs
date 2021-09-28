using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.Distance;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Computes the Minimum Clearance of a <see cref="Geometry"/>.
    /// <para/>
    /// The <b>Minimum Clearance</b> is a measure of
    /// what magnitude of perturbation of
    /// the vertices of a geometry can be tolerated
    /// before the geometry becomes topologically invalid.
    /// The smaller the Minimum Clearance distance,
    /// the less vertex perturbation the geometry can tolerate
    /// before becoming invalid.
    /// </summary>
    /// <remarks>
    /// The concept was introduced by Thompson and Van Oosterom
    /// [TV06], based on earlier work by Milenkovic [Mi88].
    /// <para/>
    /// The Minimum Clearance of a geometry G
    /// is defined to be the value <i>r</i>
    /// such that "the movement of all points by a distance
    /// of <i>r</i> in any direction will
    /// guarantee to leave the geometry valid" [TV06].
    /// An equivalent constructive definition [Mi88] is that
    /// <i>r</i> is the largest value such:
    /// <list type="bullet">
    /// <item><description>No two distinct vertices of G are closer than <i>r</i>.</description></item>
    /// <item><description>No vertex of G is closer than <i>r</i> to an edge of G of which the vertex is not an endpoint</description></item>
    /// </list>
    /// The following image shows an example of the Minimum Clearance
    /// of a simple polygon.
    /// <para/>
    /// <center><img src="/images/minClearance.png" alt="minimum clearance"/></center>
    /// <para/>
    /// If G has only a single vertex (i.e. is a
    /// <see cref="Point"/>), the value of the minimum clearance
    /// is <see cref="double.MaxValue"/>.
    /// <para/>
    /// If G is a <see cref="IPuntal"/> or <see cref="ILineal"/> geometry,
    /// then in fact no amount of perturbation
    /// will render the geometry invalid.
    /// In this case a Minimum Clearance is still computed
    /// based on the vertex and segment distances
    /// according to the constructive definition.
    /// <para/>
    /// It is possible for no Minimum Clearance to exist.
    /// For instance, a <see cref="MultiPoint"/> with all members identical
    /// has no Minimum Clearance
    /// (i.e. no amount of perturbation will cause
    /// the member points to become non-identical).
    /// Empty geometries also have no such distance.
    /// The lack of a meaningful MinimumClearance distance is detected
    /// and suitable values are returned by
    /// <see cref="GetDistance()"/> and <see cref="GetLine()"/>.
    /// <para/>
    /// The computation of Minimum Clearance utilizes
    /// the <see cref="STRtree{TItem}.NearestNeighbour(IItemDistance{Envelope,TItem})"/>
    /// method to provide good performance even for
    /// large inputs.
    /// <para/>
    /// An interesting note is that for the case of <see cref="MultiPoint"/>s,
    /// the computed Minimum Clearance line
    /// effectively determines the Nearest Neighbours in the collection.
    /// <h3>References</h3>
    /// <list type="bullet">
    /// <item><description>[Mi88] Milenkovic, V. J.,
    /// <i>Verifiable implementations of geometric algorithms
    /// using finite precision arithmetic</i>.
    /// in Artificial Intelligence, 377-401. 1988</description></item>
    /// <item><description>[TV06] Thompson, Rod and van Oosterom, Peter,
    /// <i>Interchange of Spatial Data-Inhibiting Factors</i>,
    /// Agile 2006, Visegrad, Hungary. 2006</description></item>
    /// </list>
    /// </remarks>
    /// /// <author>Martin Davis</author>
    public class MinimumClearance
    {
        /// <summary>
        /// Computes the Minimum Clearance distance for
        /// the given Geometry.
        /// </summary>
        /// <param name="g">The input geometry</param>
        /// <returns>The minimum clearance</returns>
        public static double GetDistance(Geometry g)
        {
            var rp = new MinimumClearance(g);
            return rp.GetDistance();
        }

        /// <summary>
        /// Gets a LineString containing two points
        /// which are at the Minimum Clearance distance
        /// for the given Geometry.
        /// </summary>
        /// <param name="g">The input geometry</param>
        /// <returns>The value of the minimum clearance distance<br/>
        /// or <c>LINESTRING EMPTY</c> if no minimum clearance distance exists.</returns>
        public static Geometry GetLine(Geometry g)
        {
            var rp = new MinimumClearance(g);
            return rp.GetLine();
        }

        private readonly Geometry _inputGeom;
        private double _minClearance;
        private Coordinate[] _minClearancePts;

        /// <summary>
        /// Creates an object to compute the Minimum Clearance for the given Geometry
        /// </summary>
        /// <param name="geom">The input geometry</param>
        public MinimumClearance(Geometry geom)
        {
            _inputGeom = geom;
        }

        /// <summary>
        /// Gets the Minimum Clearance distance.
        /// <para>If no distance exists
        ///  (e.g. in the case of two identical points)
        /// <see cref="double.MaxValue"/> is returned.</para>
        /// </summary>
        /// <returns>
        /// The value of the minimum clearance distance<br/>
        /// or <see cref="double.MaxValue"/> if no Minimum Clearance distance exists
        /// </returns>
        public double GetDistance()
        {
            Compute();
            return _minClearance;
        }

        /// <summary>
        /// Gets a LineString containing two points
        /// which are at the Minimum Clearance distance.<para/>
        /// If no distance could be found
        /// (e.g. in the case of two identical points)
        /// <c>LINESTRING EMPTY</c> is returned.
        /// </summary>
        /// <returns>The value of the minimum clearance distance, <br/>
        /// or <c>LINESTRING EMPTY</c> if no minimum clearance distance exists.</returns>
        public LineString GetLine()
        {
            Compute();
            // return empty line string if no min pts where found
            if (_minClearancePts == null || _minClearancePts[0] == null)
                return _inputGeom.Factory.CreateLineString();
            return _inputGeom.Factory.CreateLineString(_minClearancePts);
        }

        private void Compute()
        {
            // already computed
            if (_minClearancePts != null) return;

            // initialize to "No Distance Exists" state
            _minClearancePts = new Coordinate[2];
            _minClearance = double.MaxValue;

            // handle empty geometries
            if (_inputGeom.IsEmpty)
            {
                return;
            }

            var geomTree = FacetSequenceTreeBuilder.BuildSTRtree(_inputGeom);

            var nearest = geomTree.NearestNeighbour(new MinClearanceDistance());
            var mcd = new MinClearanceDistance();
            _minClearance = mcd.Distance(
                nearest[0],
                nearest[1]);
            _minClearancePts = mcd.Coordinates;
        }

        /// <summary>
        /// Implements the MinimumClearance distance function:
        /// <code>
        /// dist(p1, p2) =
        ///     p1 != p2 : p1.distance(p2)
        ///     p1 == p2 : Double.MAX
        ///
        /// dist(p, seg) =
        ///     p != seq.p1 &amp;&amp; p != seg.p2
        ///         ? seg.distance(p)
        ///         : Double.MaxValue
        /// </code>
        /// Also computes the values of the nearest points, if any.
        /// </summary>
        /// <author>Martin Davis</author>
        private class MinClearanceDistance : IItemDistance<Envelope, FacetSequence>
        {
            private double _minDist = double.MaxValue;
            private readonly Coordinate[] _minPts = new Coordinate[2];

            public Coordinate[] Coordinates => _minPts;

            public double Distance(IBoundable<Envelope, FacetSequence> b1, IBoundable<Envelope, FacetSequence> b2)
            {
                var fs1 = b1.Item;
                var fs2 = b2.Item;
                _minDist = double.MaxValue;
                return Distance(fs1, fs2);
            }

            public double Distance(FacetSequence fs1, FacetSequence fs2)
            {
                // compute MinClearance distance metric
                VertexDistance(fs1, fs2);
                if (fs1.Count == 1 && fs2.Count == 1) return _minDist;
                if (_minDist <= 0.0) return _minDist;
                SegmentDistance(fs1, fs2);
                if (_minDist <= 0.0) return _minDist;
                SegmentDistance(fs2, fs1);
                return _minDist;
            }

            private double VertexDistance(FacetSequence fs1, FacetSequence fs2)
            {
                for (int i1 = 0; i1 < fs1.Count; i1++)
                {
                    for (int i2 = 0; i2 < fs2.Count; i2++)
                    {
                        var p1 = fs1.GetCoordinate(i1);
                        var p2 = fs2.GetCoordinate(i2);
                        if (!p1.Equals2D(p2))
                        {
                            double d = p1.Distance(p2);
                            if (d < _minDist)
                            {
                                _minDist = d;
                                _minPts[0] = p1;
                                _minPts[1] = p2;
                                if (d == 0.0)
                                    return d;
                            }
                        }
                    }
                }
                return _minDist;
            }

            private double SegmentDistance(FacetSequence fs1, FacetSequence fs2)
            {
                for (int i1 = 0; i1 < fs1.Count; i1++)
                {
                    for (int i2 = 1; i2 < fs2.Count; i2++)
                    {
                        var p = fs1.GetCoordinate(i1);

                        var seg0 = fs2.GetCoordinate(i2 - 1);
                        var seg1 = fs2.GetCoordinate(i2);

                        if (!(p.Equals2D(seg0) || p.Equals2D(seg1)))
                        {
                            double d = DistanceComputer.PointToSegment(p, seg0, seg1);
                            if (d < _minDist)
                            {
                                _minDist = d;
                                UpdatePts(p, seg0, seg1);
                                if (d == 0.0)
                                    return d;
                            }
                        }
                    }
                }
                return _minDist;
            }

            private void UpdatePts(Coordinate p, Coordinate seg0, Coordinate seg1)
            {
                _minPts[0] = p;
                var seg = new LineSegment(seg0, seg1);
                _minPts[1] = seg.ClosestPoint(p).Copy();
            }
        }
    }
}
