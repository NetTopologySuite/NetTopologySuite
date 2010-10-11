using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.Noding
{
    public class OrientedCoordinateSequence<TCoordinate> : IComparable<OrientedCoordinateSequence<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private static Boolean orientation(ICoordinateSequence<TCoordinate> sequence)
        {
            return sequence.IncreasingDirection == 1;
        }

        private readonly ICoordinateSequence<TCoordinate> _sequence;
        private readonly Boolean _orientation;

        public OrientedCoordinateSequence(ICoordinateSequence<TCoordinate> sequence)
        {
            _sequence = sequence;
            _orientation = orientation(sequence);
        }

        protected Boolean Orientation
        {
            get { return _orientation; }
        }

        protected Int32 Count
        {
            get { return _sequence.Count; }
        }

        protected ICoordinateSequence<TCoordinate> Coordinates
        {
            get { return _sequence; }
        }

        private static int CompareOriented(OrientedCoordinateSequence<TCoordinate> pts1,
                                     OrientedCoordinateSequence<TCoordinate> pts2)
        {
            Boolean orientation1 = pts1.Orientation;
            Boolean orientation2 = pts2.Orientation;

            int dir1 = orientation1 ? 1 : -1;
            int dir2 = orientation2 ? 1 : -1;
            int limit1 = orientation1 ? pts1.Count : -1;
            int limit2 = orientation2 ? pts2.Count : -1;

            int i1 = orientation1 ? 0 : pts1.Count - 1;
            int i2 = orientation2 ? 0 : pts2.Count - 1;
            int comp = 0;

            while (true)
            {
                int compPt = pts1.Coordinates[i1].CompareTo(pts2.Coordinates[i2]);
                if (compPt != 0)
                    return compPt;
                i1 += dir1;
                i2 += dir2;
                Boolean done1 = i1 == limit1;
                Boolean done2 = i2 == limit2;
                if (done1 && !done2) return -1;
                if (!done1 && done2) return 1;
                if (done1 && done2) return 0;
            }

        }

        #region IComparable<OrientedCoordinateSequence<TCoordinate>> Member

        public int CompareTo(OrientedCoordinateSequence<TCoordinate> other)
        {
            return CompareOriented(this, other);
        }

        #endregion
    }
}
