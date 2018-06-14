using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Validates that a collection of <see cref="ISegmentString"/>s is correctly noded.
    /// Indexing is used to improve performance.
    ///</summary>
    /// <remarks>
    /// <para>
    /// In the most common use case, validation stops after a single
    /// non-noded intersection is detected,
    /// but the class can be requested to detect all intersections
    /// by using the <see cref="FindAllIntersections"/> property.
    /// <para/>
    /// The validator does not check for a-b-a topology collapse situations.
    /// <para/>
    /// The validator does not check for endpoint-interior vertex intersections.
    /// This should not be a problem, since the JTS noders should be
    /// able to compute intersections between vertices correctly.
    /// </para>
    /// <para>
    /// The client may either test the <see cref="IsValid"/> condition,
    /// or request that a suitable <see cref="TopologyException"/> be thrown.
    /// </para>
    /// </remarks>
    public class FastNodingValidator
    {
        public static IList<Coordinate> ComputeIntersections(IEnumerable<ISegmentString> segStrings)
        {
            var nv = new FastNodingValidator(segStrings);
            nv.FindAllIntersections = true;
            bool temp = nv.IsValid;
            return nv.Intersections;
        }

        private readonly LineIntersector _li = new RobustLineIntersector();

        private readonly List<ISegmentString> _segStrings = new List<ISegmentString>();
        private InteriorIntersectionFinder _segInt;
        private bool _isValid = true;

        /// <summary>
        /// Creates a new noding validator for a given set of linework.
        /// </summary>
        /// <param name="segStrings">A collection of <see cref="ISegmentString"/>s</param>
        public FastNodingValidator(IEnumerable<ISegmentString> segStrings)
        {
            _segStrings.AddRange(segStrings);
        }

        /// <summary>
        /// Gets or sets whether all intersections should be found.
        /// </summary>
        public bool FindAllIntersections { get; set; }

        /// <summary>
        /// Gets a list of all intersections found.
        /// <remarks>
        /// Intersections are represented as <see cref="Coordinate"/>s.
        /// List is empty if none were found.
        /// </remarks>
        /// </summary>
        public IList<Coordinate> Intersections => _segInt.Intersections;

        ///<summary>
        /// Checks for an intersection and reports if one is found.
        ///</summary>
        public bool IsValid
        {
            get
            {
                Execute();
                return _isValid;
            }
        }

        ///<summary>
        /// Returns an error message indicating the segments containing the intersection.
        ///</summary>
        ///<returns>an error message documenting the intersection location</returns>
        public string GetErrorMessage()
        {
            if (IsValid)
                return "no intersections found";

            var intSegs = _segInt.IntersectionSegments;
            return "found non-noded intersection between "
                + WKTWriter.ToLineString(intSegs[0], intSegs[1])
                + " and "
                + WKTWriter.ToLineString(intSegs[2], intSegs[3]);
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
            /*
             * MD - It may even be reliable to simply check whether
             * end segments (of SegmentStrings) have an interior intersection,
             * since noding should have split any true interior intersections already.
             */
            _isValid = true;
            _segInt = new InteriorIntersectionFinder(_li);
            _segInt.FindAllIntersections = FindAllIntersections;
            var noder = new MCIndexNoder(_segInt);
            noder.ComputeNodes(_segStrings); //.ComputeNodes(segStrings);
            if (_segInt.HasIntersection)
            {
                _isValid = false;
                return;
            }
        }
    }
}