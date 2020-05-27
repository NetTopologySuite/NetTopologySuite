using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Limits the segments in a list of segments
     * to those which intersect an envelope.
     * This creates zero or more sections of the input segment sequences,
     * containing only line segments which intersect the limit envelope.
     * Segments are not clipped, since that happens in the overlay.
     * This can substantially reduce the number of vertices which need to be
     * processed during overlay.
     * 
     * @author Martin Davis
     *
     * @see RingClipper
     */
    public class LineLimiter
    {
        private readonly Envelope _limitEnv;
        private CoordinateList _ptList;
        private Coordinate _lastOutside;
        private List<Coordinate[]> _sections;

        /**
         * Creates a new limiter for a given envelope.
         * 
         * @param env the envelope to limit to
         */
        public LineLimiter(Envelope env)
        {
            _limitEnv = env;
        }

        /**
         * Limits a list of segments.
         * 
         * @param pts the segment sequence to limit
         * @return the sections which intersect the limit envelope
         */
        public List<Coordinate[]> Limit(Coordinate[] pts)
        {
            _lastOutside = null;
            _ptList = null;
            _sections = new List<Coordinate[]>();

            for (int i = 0; i < pts.Length; i++)
            {
                var p = pts[i];
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
