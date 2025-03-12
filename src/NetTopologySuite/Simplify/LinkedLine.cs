using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Simplify
{
    internal sealed class LinkedLine
    {
        private const int NoCoordIndex = -1;

        private readonly Coordinate[] _coord;
        private readonly bool _isRing;
        private int _size;
        private readonly int[] _next = null;
        private readonly int[] _prev = null;

        public LinkedLine(Coordinate[] pts)
        {
            _coord = pts;
            _isRing = CoordinateArrays.IsRing(pts);
            _size = _isRing ? pts.Length - 1 : pts.Length;
            _next = CreateNextLinks(_size);
            _prev = CreatePrevLinks(_size);
        }

        public bool IsRing => _isRing;

        public bool IsCorner(int i)
        {
            if (!IsRing
                && (i == 0 || i == _coord.Length - 1))
                return false;
            return true;
        }

        private int[] CreateNextLinks(int size)
        {
            int[] next = new int[size];
            for (int i = 0; i < size; i++)
            {
                next[i] = i + 1;
            }
            next[size - 1] = _isRing ? 0 : NoCoordIndex;
            return next;
        }

        private int[] CreatePrevLinks(int size)
        {
            int[] prev = new int[size];
            for (int i = 0; i < size; i++)
            {
                prev[i] = i - 1;
            }
            prev[0] = _isRing ? size - 1 : NoCoordIndex;
            return prev;
        }

        public int Count => _size;

        public int Next(int i)
        {
            return _next[i];
        }

        public int Prev(int i)
        {
            return _prev[i];
        }

        public Coordinate GetCoordinate(int index)
        {
            return _coord[index];
        }

        public Coordinate PrevCoordinate(int index)
        {
            return _coord[Prev(index)];
        }

        public Coordinate NextCoordinate(int index)
        {
            return _coord[Next(index)];
        }

        public bool HasCoordinate(int index)
        {
            //-- if not a ring, endpoints are alway present
            if (!_isRing && (index == 0 || index == _coord.Length - 1))
                return true;
            return index >= 0
                && index < _prev.Length
                && _prev[index] != NoCoordIndex;
        }

        public void Remove(int index)
        {
            int iprev = _prev[index];
            int inext = _next[index];
            if (iprev != NoCoordIndex) _next[iprev] = inext;
            if (inext != NoCoordIndex) _prev[inext] = iprev;
            _prev[index] = NoCoordIndex;
            _next[index] = NoCoordIndex;
            _size--;
        }

        public Coordinate[] Coordinates
        {
            get
            {
                var coords = new CoordinateList();
                int len = _isRing ? _coord.Length - 1 : _coord.Length;
                for (int i = 0; i < len; i++)
                {
                    if (HasCoordinate(i))
                    {
                        coords.Add(_coord[i].Copy(), false);
                    }
                }
                if (_isRing)
                {
                    coords.CloseRing();
                }
                return coords.ToCoordinateArray();
            }
        }

        public override string ToString()
        {
            return WKTWriter.ToLineString(Coordinates);
        }
    }
}
