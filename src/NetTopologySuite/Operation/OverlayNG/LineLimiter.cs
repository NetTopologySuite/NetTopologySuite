using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Limits the segments in a list of segments
    /// to those which intersect an envelope.
    /// This creates zero or more sections of the input segment sequences,
    /// containing only line segments which intersect the limit envelope.
    /// Segments are not clipped, since that can move
    /// line segments enough to alter topology,
    /// and it happens in the overlay in any case.
    /// This can substantially reduce the number of vertices which need to be
    /// processed during overlay.
    /// <para/>
    /// This optimization is only applicable to Line geometries,
    /// since it does not maintain the closed topology of rings.
    /// Polygonal geometries are optimized using the <see cref="RingClipper"/>.
    /// </summary>
    /// <seealso cref="RingClipper"/>
    /// <author>Martin Davis</author>
    public sealed class LineLimiter
    {
        private readonly Envelope _limitEnv;
        private CoordinateList _ptList;
        private Coordinate _lastOutside;
        private List<Coordinate[]> _sections;

        /// <summary>
        /// Creates a new limiter for a given envelope.
        /// </summary>
        /// <param name="env">The envelope to limit to</param>
        public LineLimiter(Envelope env)
        {
            _limitEnv = env ?? throw new ArgumentNullException(nameof(env));
        }

        /// <summary>
        /// Limits a list of segments.
        /// </summary>
        /// <param name="pts">The segment sequence to limit</param>
        /// <returns>The sections which intersect the limit envelope</returns>
        public List<Coordinate[]> Limit(IEnumerable<Coordinate> pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException(nameof(pts));
            }

            _lastOutside = null;
            _ptList = null;
            _sections = new List<Coordinate[]>();

            foreach (var p in pts)
            {
                if (_limitEnv.Intersects(p))
                    AddPoint(p);
                else
                {
                    AddOutside(p);
                }
            }
            // finish last section, if any
            FinishSection();
            return _sections;
        }

        private void AddPoint(Coordinate p)
        {
            if (p == null) return;
            StartSection();
            _ptList.Add(p, false);
        }

        private void AddOutside(Coordinate p)
        {
            bool segIntersects = IsLastSegmentIntersecting(p);
            if (!segIntersects)
            {
                FinishSection();
            }
            else
            {
                AddPoint(_lastOutside);
                AddPoint(p);
            }
            _lastOutside = p;
        }

        private bool IsLastSegmentIntersecting(Coordinate p)
        {
            if (_lastOutside == null)
            {
                // last point must have been inside
                if (IsSectionOpen())
                    return true;
                return false;
            }
            return _limitEnv.Intersects(_lastOutside, p);
        }

        private bool IsSectionOpen()
        {
            return _ptList != null;
        }

        private void StartSection()
        {
            if (_ptList == null)
            {
                _ptList = new CoordinateList();
            }
            if (_lastOutside != null)
            {
                _ptList.Add(_lastOutside, false);
            }
            _lastOutside = null;
        }

        private void FinishSection()
        {
            if (_ptList == null)
                return;
            // finish off this section
            if (_lastOutside != null)
            {
                _ptList.Add(_lastOutside, false);
                _lastOutside = null;
            }

            var section = _ptList.ToCoordinateArray();
            _sections.Add(section);
            _ptList = null;
        }

    }
}
