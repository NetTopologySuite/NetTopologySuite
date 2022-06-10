using NetTopologySuite.Geometries;

namespace NetTopologySuite.Simplify
{
    internal class LinkedRing
    {

        private const int NoCoordIndex = -1;

        private readonly Coordinate[] _coord;
        private readonly int[] _next = null;
        private readonly int[] _prev = null;
        private int _size;

        public LinkedRing(Coordinate[] pts)
        {
            _coord = pts;
            _size = pts.Length - 1;
            _next = CreateNextLinks(_size);
            _prev = CreatePrevLinks(_size);
        }

        private static int[] CreateNextLinks(int size)
        {
            int[] next = new int[size];
            for (int i = 0; i < size; i++)
            {
                next[i] = i + 1;
            }
            next[size - 1] = 0;
            return next;
        }

        private static int[] CreatePrevLinks(int size)
        {
            int[] prev = new int[size];
            for (int i = 0; i < size; i++)
            {
                prev[i] = i - 1;
            }
            prev[0] = size - 1;
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
            return index >= 0 && index < _prev.Length
                && _prev[index] != NoCoordIndex;
        }

        public void RemoveAt(int index)
        {
            int iprev = _prev[index];
            int inext = _next[index];
            _next[iprev] = inext;
            _prev[inext] = iprev;
            _prev[index] = NoCoordIndex;
            _next[index] = NoCoordIndex;
            _size--;
        }

        public Coordinate[] Coordinates
        {
            get
            {
                var coords = new CoordinateList(Count);
                for (int i = 0; i < _coord.Length - 1; i++)
                {
                    if (_prev[i] != NoCoordIndex)
                    {
                        coords.Add(_coord[i].Copy(), false);
                    }
                }
                coords.CloseRing();
                return coords.ToCoordinateArray();
            }
        }
    }
}
