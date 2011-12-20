using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.Distance;

namespace NetTopologySuite.Precision
{
    /**
     * Computes the Minimum Clearance of a {@link Geometry}.
     * <p>
     * The <b>Minimum Clearance</b> is a measure of
     * what magnitude of perturbation of
     * the vertices of a geometry can be tolerated
     * before the geometry becomes topologically invalid.
     * The smaller the Minimum Clearance distance,
     * the less vertex pertubation the geometry can tolerate
     * before becoming invalid.
     * <p>
     * The concept was introduced by Thompson and Van Oosterom
     * [TV06], based on earlier work by Milenkovic [Mi88].
     * <p>
     * The Minimum Clearance of a geometry G
     * is defined to be the value <i>r</i>
     * such that "the movement of all points by a distance
     * of <i>r</i> in any direction will
     * guarantee to leave the geometry valid" [TV06].
     * An equivalent constructive definition [Mi88] is that
     * <i>r</i> is the largest value such:
     * <ol>
     * <li>No two distinct vertices of G are closer than <i>r</i>
     * <li>No vertex of G is closer than <i>r</i> to an edge of G
     * of which the vertex is not an endpoint
     * </ol>
     * The following image shows an example of the Minimum Clearance
     * of a simple polygon.
     * <p>
     * <center><img src='doc-files/minClearance.png'></center>
     * <p>
     * If G has only a single vertex (i.e. is a
     * {@link Point}), the value of the minimum clearance
     * is {@link Double#MAX_VALUE}.
     * <p>
     * If G is a {@link Puntal} or {@link Lineal} geometry,
     * then in fact no amount of perturbation
     * will render the geometry invalid.
     * In this case a Minimum Clearance is still computed
     * based on the vertex and segment distances
     * according to the constructive definition.
     * <p>
     * It is possible for no Minimum Clearance to exist.
     * For instance, a {@link MultiPoint} with all members identical
     * has no Minimum Clearance
     * (i.e. no amount of perturbation will cause
     * the member points to become non-identical).
     * Empty geometries also have no such distance.
     * The lack of a meaningful MinimumClearance distance is detected
     * and suitable values are returned by
     * {@link #getDistance()} and {@link #getLine()}.
     * <p>
     * The computation of Minimum Clearance utilizes
     * the {@link STRtree#nearestNeighbour(ItemDistance)}
     * method to provide good performance even for
     * large inputs.
     * <p>
     * An interesting note is that for the case of {@link MultiPoint}s,
     * the computed Minimum Clearance line
     * effectively determines the Nearest Neighbours in the collection.
     *
     * <h3>References</h3>
     * <ul>
     * <li>[Mi88] Milenkovic, V. J.,
     * <i>Verifiable implementations of geometric algorithms
     * using finite precision arithmetic</i>.
     * in Artificial Intelligence, 377-401. 1988
     * <li>[TV06] Thompson, Rod and van Oosterom, Peter,
     * <i>Interchange of Spatial Data-Inhibiting Factors</i>,
     * Agile 2006, Visegrad, Hungary. 2006
     * </ul>
     *
     * @author Martin Davis
     *
     */

    public class MinimumClearance
    {
        /**
   * Computes the Minimum Clearance distance for
   * the given Geometry.
   *
   * @param g the input geometry
   * @return the Minimum Clearance distance
   */

        public static double GetDistance(IGeometry g)
        {
            var rp = new MinimumClearance(g);
            return rp.GetDistance();
        }

        /**
   * Gets a LineString containing two points
   * which are at the Minimum Clearance distance
   * for the given Geometry.
   *
   * @param g the input geometry
   * @return the value of the minimum clearance distance
   * @return <tt>LINESTRING EMPTY</tt> if no Minimum Clearance distance exists
   */

        public static IGeometry GetLine(IGeometry g)
        {
            var rp = new MinimumClearance(g);
            return rp.GetLine();
        }

        private readonly IGeometry _inputGeom;
        private double _minClearance;
        private Coordinate[] _minClearancePts;

        /**
   * Creates an object to compute the Minimum Clearance
   * for the given Geometry
   *
   * @param geom the input geometry
   */

        public MinimumClearance(IGeometry geom)
        {
            _inputGeom = geom;
        }

        /**
   * Gets the Minimum Clearance distance.
   * <p>
   * If no distance exists
   * (e.g. in the case of two identical points)
   * <tt>Double.MAX_VALUE</tt> is returned.
   *
   * @return the value of the minimum clearance distance
   * @return <tt>Double.MAX_VALUE</tt> if no Minimum Clearance distance exists
   */

        public double GetDistance()
        {
            Compute();
            return _minClearance;
        }

        /**
   * Gets a LineString containing two points
   * which are at the Minimum Clearance distance.
   * <p>
   * If no distance could be found
   * (e.g. in the case of two identical points)
   * <tt>LINESTRING EMPTY</tt> is returned.
   *
   * @return the value of the minimum clearance distance
   * @return <tt>LINESTRING EMPTY</tt> if no Minimum Clearance distance exists
   */

        public ILineString GetLine()
        {
            Compute();
            // return empty line string if no min pts where found
            if (_minClearancePts == null || _minClearancePts[0] == null)
                return _inputGeom.Factory.CreateLineString((Coordinate[])null);
            return _inputGeom.Factory.CreateLineString(_minClearancePts);
        }

        private void Compute()
        {
            // already computed
            if (_minClearancePts != null) return;

            // initialize to "No Distance Exists" state
            _minClearancePts = new Coordinate[2];
            _minClearance = Double.MaxValue;

            // handle empty geometries
            if (_inputGeom.IsEmpty)
            {
                return;
            }

            var geomTree = FacetSequenceTreeBuilder.BuildSTRtree(_inputGeom);

            var nearest = geomTree.NearestNeighbour(new MinClearanceDistance());
            var mcd = new MinClearanceDistance();
            _minClearance = mcd.Distance(
                (FacetSequence)nearest[0],
                (FacetSequence)nearest[1]);
            _minClearancePts = mcd.Coordinates;
        }

        /**
   * Implements the MinimumClearance distance function:
   * <ul>
   * <li>dist(p1, p2) =
   * <ul>
   * <li>p1 != p2 : p1.distance(p2)
   * <li>p1 == p2 : Double.MAX
   * </ul>
   * <li>dist(p, seg) =
   * <ul>
   * <li>p != seq.p1 && p != seg.p2 : seg.distance(p)
   * <li>ELSE : Double.MAX
   * </ul>
   * </ul>
   * Also computes the values of the nearest points, if any.
   *
   * @author Martin Davis
   *
   */

        private class MinClearanceDistance : IItemDistance
        {
            private double _minDist = Double.MaxValue;
            private readonly Coordinate[] _minPts = new Coordinate[2];

            public Coordinate[] Coordinates
            {
                get { return _minPts; }
            }

            public double Distance(ItemBoundable b1, ItemBoundable b2)
            {
                var fs1 = (FacetSequence)b1.Item;
                var fs2 = (FacetSequence)b2.Item;
                _minDist = Double.MaxValue;
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
                        Coordinate p1 = fs1.GetCoordinate(i1);
                        Coordinate p2 = fs2.GetCoordinate(i2);
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
                        Coordinate p = fs1.GetCoordinate(i1);

                        Coordinate seg0 = fs2.GetCoordinate(i2 - 1);
                        Coordinate seg1 = fs2.GetCoordinate(i2);

                        if (!(p.Equals2D(seg0) || p.Equals2D(seg1)))
                        {
                            double d = CGAlgorithms.DistancePointLine(p, seg0, seg1);
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
                _minPts[1] = new Coordinate(seg.ClosestPoint(p));
            }
        }
    }
}