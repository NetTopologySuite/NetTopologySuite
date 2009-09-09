using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /**
     * Validates that a collection of {@link SegmentString}s is correctly noded.
     * Indexing is used to improve performance.
     * This class assumes that at least one round of noding has already been performed
     * (which may still leave intersections, due to rounding issues).
     * Does NOT check a-b-a collapse situations. 
     * Also does not check for endpt-interior vertex intersections.
     * This should not be a problem, since the noders should be
     * able to compute intersections between vertices correctly.
     * User may either test the valid condition, or request that a 
     * {@link TopologyException} 
     * be thrown.
     *
     * @version 1.7
     */
    public class FastNodingValidator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private LineIntersector<TCoordinate> _li;// = new RobustLineIntersector<TCoordinate>();

        private List<ISegmentString<TCoordinate>> _segStrings = new List<ISegmentString<TCoordinate>>();
        private InteriorIntersectionFinder<TCoordinate> _segInt = null;
        private Boolean _isValid = true;
        private IGeometryFactory<TCoordinate> _geoFactory;

        public FastNodingValidator(IGeometryFactory<TCoordinate> geoFactory, IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            _geoFactory = geoFactory;
            _li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(geoFactory);
            _segStrings.AddRange(segStrings);
        }

        /**
         * Checks for an intersection and 
         * reports if one is found.
         * 
         * @return true if the arrangement contains an interior intersection
         */
        public Boolean IsValid
        {
            get
            {
                Execute();
                return _isValid;
            }
        }

            /**
         * Returns an error message indicating the segments containing
         * the intersection.
         * 
         * @return an error message documenting the intersection location
         */
        public String GetErrorMessage()
        {
            if (IsValid)
                return "no intersections found";

            TCoordinate[] intSegs = _segInt.IntersectionSegments;
            return "found non-noded intersection between "
                + _geoFactory.WktWriter.Write(_geoFactory.CreateLineString(intSegs[0], intSegs[1]))
                + " and "
                + _geoFactory.WktWriter.Write(_geoFactory.CreateLineString(intSegs[2], intSegs[3]));
        }

        ///<summary>
        /// Checks for an intersection and throws
        /// a TopologyException if one is found.
        ///</summary>
        ///<exception cref="TopologyException">if an intersection is found</exception>
        public void CheckValid()
        {
            if (!IsValid)
                throw new TopologyException(GetErrorMessage(), _segInt.InteriorIntersection);
        }

        private void Execute()
        {
            if (_segInt != null)
                return;
            CheckInteriorIntersections();
        }

        private void CheckInteriorIntersections()
        {
            /**
             * MD - It may even be reliable to simply check whether 
             * end segments (of SegmentStrings) have an interior intersection,
             * since noding should have split any true interior intersections already.
             */
            _isValid = true;
            _segInt = new InteriorIntersectionFinder<TCoordinate>(_li);
            MonotoneChainIndexNoder<TCoordinate> noder = new MonotoneChainIndexNoder<TCoordinate>(_geoFactory, _segInt);
            noder.ComputeNodes(_segStrings); //.ComputeNodes(segStrings);
            if (_segInt.HasIntersection)
            {
                _isValid = false;
                return;
            }
        }
        
    }
}
