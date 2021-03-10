using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Index.Flatbush
{
    /// <summary>
    /// The guts of <see cref="FlatbushSpatialIndex{T}"/>, ported from https://github.com/mourner/flatbush.
    /// Extracted here so that the JIT doesn't have to compile a whole extra version of all this
    /// stuff per type parameter.
    /// </summary>
    internal sealed class Flatbush
    {
        private readonly int _numItems;

        private readonly int _nodeSize;

        private readonly int[] _levelBounds;

        private readonly int[] _indices;

        private readonly Box[] _boxes;

        private int _pos;

        private Box _dataBounds = Box.Empty;

        public Flatbush(int numItems, int nodeSize)
        {
            _numItems = numItems;
            _nodeSize = nodeSize;

            // calculate the total number of nodes in the R-tree to allocate space for
            // and the index of each tree level (used in search later)
            int n = numItems;
            int numNodes = n;
            var levelBounds = new List<int> { n };
            do
            {
                n = (int)Math.Ceiling(n / (double)_nodeSize);
                numNodes += n;
                levelBounds.Add(numNodes);
            } while (n != 1);

            _levelBounds = levelBounds.ToArray();
            _indices = new int[numNodes];
            _boxes = new Box[numNodes];
        }

        public int Add(double minX, double minY, double maxX, double maxY)
        {
            int pos = _pos++;

            _indices[pos] = pos;
            ref var itemBox = ref _boxes[pos];
            itemBox = new Box(minX, minY, maxX, maxY);
            _dataBounds.ExpandToInclude(in itemBox);
            return pos;
        }

        public void Finish()
        {
            if (_pos != _numItems)
            {
                throw new InvalidOperationException($"Inserted {_pos} items, but {_numItems} were expected.");
            }

            if (_numItems <= _nodeSize)
            {
                // only one node, skip sorting and just fill the root box
                _boxes[_pos++] = _dataBounds;
                return;
            }

            double width = _dataBounds.Width;
            if (width == 0)
            {
                width = 1;
            }

            double height = _dataBounds.Height;
            if (height == 0)
            {
                height = 1;
            }

            Span<uint> hilbertValues = stackalloc uint[0];
            hilbertValues = _numItems < 100 ? stackalloc uint[_numItems] : new uint[_numItems];
            const uint hilbertMax = (1 << 16) - 1;

            // map item centers into Hilbert coordinate space and calculate Hilbert values
            for (int i = 0; i < hilbertValues.Length; i++)
            {
                ref readonly var box = ref _boxes[i];
                uint x = (uint)Math.Floor(hilbertMax * (((box.MinX + box.MaxX) / 2) - _dataBounds.MinX) / width);
                uint y = (uint)Math.Floor(hilbertMax * (((box.MinY + box.MaxY) / 2) - _dataBounds.MinY) / height);
                hilbertValues[i] = Hilbert(x, y);
            }

            // sort items by their Hilbert value (for packing later)
            Sort(hilbertValues, _boxes, _indices, 0, _numItems - 1, _nodeSize);

            // generate nodes at each tree level, bottom-up
            int pos = 0;
            foreach (int end in _levelBounds.AsSpan(0, _levelBounds.Length - 1))
            {
                // generate a parent node for each block of consecutive <nodeSize> nodes
                while (pos < end)
                {
                    // calculate bbox for the new node and add the new node to the tree data
                    _indices[_pos] = pos;
                    ref var nodeBounds = ref _boxes[_pos++];
                    nodeBounds = Box.Empty;
                    for (int i = 0; i < _nodeSize && pos < end; i++)
                    {
                        nodeBounds.ExpandToInclude(in _boxes[pos++]);
                    }
                }
            }
        }

        public void Search(double minX, double minY, double maxX, double maxY, Action<int> found)
        {
            if (_pos != _boxes.Length)
            {
                throw new InvalidOperationException("Data not yet indexed - call Finish().");
            }

            var queryBox = new Box(minX, minY, maxX, maxY);

            // reference uses it as a stack but calls it "queue"...
            var queue = new Stack<int>();
            queue.Push(_boxes.Length - 1);
            while (queue.Count != 0)
            {
                int nodeIndex = queue.Pop();

                // find the end index of the node
                int end = Math.Min(nodeIndex + _nodeSize, UpperBound(nodeIndex, _levelBounds));

                // search through child nodes
                for (int pos = nodeIndex; pos < end; pos++)
                {
                    // check if node bbox intersects with query bbox
                    if (!queryBox.Intersects(_boxes[pos]))
                    {
                        continue;
                    }

                    int index = _indices[pos];
                    if (nodeIndex < _numItems)
                    {
                        // leaf item
                        found(index);
                    }
                    else
                    {
                        // node; add it to the search queue
                        queue.Push(index);
                    }
                }
            }
        }

        private static uint Hilbert(uint x, uint y)
        {
            // Fast Hilbert curve algorithm by http://threadlocalmutex.com/
            // Ported from C++ https://github.com/rawrunprotected/hilbert_curves (public
            // domain)
            uint a = x ^ y;
            uint b = 0xFFFF ^ a;
            uint c = 0xFFFF ^ (x | y);
            uint d = x & (y ^ 0xFFFF);

            uint A = a | (b >> 1);
            uint B = (a >> 1) ^ a;
            uint C = ((c >> 1) ^ (b & (d >> 1))) ^ c;
            uint D = ((a & (c >> 1)) ^ (d >> 1)) ^ d;

            a = A;
            b = B;
            c = C;
            d = D;
            A = ((a & (a >> 2)) ^ (b & (b >> 2)));
            B = ((a & (b >> 2)) ^ (b & ((a ^ b) >> 2)));
            C ^= ((a & (c >> 2)) ^ (b & (d >> 2)));
            D ^= ((b & (c >> 2)) ^ ((a ^ b) & (d >> 2)));

            a = A;
            b = B;
            c = C;
            d = D;
            A = ((a & (a >> 4)) ^ (b & (b >> 4)));
            B = ((a & (b >> 4)) ^ (b & ((a ^ b) >> 4)));
            C ^= ((a & (c >> 4)) ^ (b & (d >> 4)));
            D ^= ((b & (c >> 4)) ^ ((a ^ b) & (d >> 4)));

            a = A;
            b = B;
            c = C;
            d = D;
            C ^= ((a & (c >> 8)) ^ (b & (d >> 8)));
            D ^= ((b & (c >> 8)) ^ ((a ^ b) & (d >> 8)));

            a = C ^ (C >> 1);
            b = D ^ (D >> 1);

            uint i0 = x ^ y;
            uint i1 = b | (0xFFFF ^ (i0 | a));

            i0 = (i0 | (i0 << 8)) & 0x00FF00FF;
            i0 = (i0 | (i0 << 4)) & 0x0F0F0F0F;
            i0 = (i0 | (i0 << 2)) & 0x33333333;
            i0 = (i0 | (i0 << 1)) & 0x55555555;

            i1 = (i1 | (i1 << 8)) & 0x00FF00FF;
            i1 = (i1 | (i1 << 4)) & 0x0F0F0F0F;
            i1 = (i1 | (i1 << 2)) & 0x33333333;
            i1 = (i1 | (i1 << 1)) & 0x55555555;

            return (uint)((i1 << 1) | i0);
        }

        // custom quicksort that partially sorts bbox data alongside the hilbert values
        private static void Sort(Span<uint> values, Span<Box> boxes, Span<int> indices, int left, int right, int nodeSize)
        {
            while (true)
            {
                if (left / nodeSize >= right / nodeSize)
                {
                    return;
                }

                uint pivot = values[(left + right) >> 1];
                int i = left - 1;
                int j = right + 1;
                while (true)
                {
                    do
                    {
                        i++;
                    } while (values[i] < pivot);

                    do
                    {
                        j--;
                    } while (values[j] > pivot);

                    if (i >= j)
                    {
                        break;
                    }

                    Swap(ref values[i], ref values[j]);
                    Swap(ref boxes[i], ref boxes[j]);
                    Swap(ref indices[i], ref indices[j]);
                }

                // coreclr's JIT doesn't tend to do a great job optimizing tail-recursive calls into
                // loops.  let's do that optimization manually.  the true recursive call should be
                // on the smaller side, per Sedgewick.
                int leftCount = j - left;
                int rightCount = right - j - 1;
                if (rightCount > leftCount)
                {
                    Sort(values, boxes, indices, left, j, nodeSize);
                    left = j + 1;
                }
                else
                {
                    Sort(values, boxes, indices, j + 1, right, nodeSize);
                    right = j;
                }
            }
        }

        // binary search for the first value in the array bigger than the given
        private static int UpperBound(int value, int[] arr)
        {
            int i = 0;
            int j = arr.Length - 1;
            while (i < j)
            {
                int m = (i + j) >> 1;
                if (arr[m] > value)
                {
                    j = m;
                }
                else
                {
                    i = m + 1;
                }
            }

            return arr[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref T a, ref T b)
        {
            var t = a;
            a = b;
            b = t;
        }

        private struct Box
        {
            public static readonly Box Empty = new Box(double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity);

            public double MinX;

            public double MinY;

            public double MaxX;

            public double MaxY;

            public Box(double minX, double minY, double maxX, double maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            public double Width => MaxX - MinX;

            public double Height => MaxY - MinY;

            public bool Intersects(in Box other)
            {
                return
                    other.MaxX >= MinX &&
                    other.MaxY >= MinY &&
                    other.MinX <= MaxX &&
                    other.MinY <= MaxY;
            }

            public void ExpandToInclude(in Box other)
            {
                if (other.MinX < MinX)
                {
                    MinX = other.MinX;
                }

                if (other.MinY < MinY)
                {
                    MinY = other.MinY;
                }

                if (other.MaxX > MaxX)
                {
                    MaxX = other.MaxX;
                }

                if (other.MaxY > MaxY)
                {
                    MaxY = other.MaxY;
                }
            }

            public override string ToString()
            {
                return $"[{MinX} : {MaxX}, {MinY} : {MaxY}]";
            }
        }
    }
}
