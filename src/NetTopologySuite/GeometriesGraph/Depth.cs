using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A Depth object records the topological depth of the sides
    /// of an Edge for up to two Geometries.
    /// </summary>
    public class Depth
    {
        /// <summary>
        ///
        /// </summary>
        private const int @null = -1;

        /// <summary>
        ///
        /// </summary>
        /// <param name="_location"></param>
        /// <returns></returns>
        public static int DepthAtLocation(Location _location)
        {
            if (_location == Location.Exterior)
                return 0;

            if (_location == Location.Interior)
                return 1;

            return @null;
        }

        private readonly int[,] _depth = new int[2,3];

        /// <summary>
        ///
        /// </summary>
        public Depth()
        {
            // initialize depth array to a sentinel value
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 3; j++)
                    _depth[i,j] = @null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        [Obsolete("Use GetDepth(int, Geometries.Position)")]
        public int GetDepth(int geomIndex, Positions posIndex) =>
            GetDepth(geomIndex, new Geometries.Position((int) posIndex));

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public int GetDepth(int geomIndex, Geometries.Position posIndex)
        {
            return _depth[geomIndex, (int)posIndex];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <param name="depthValue"></param>
        [Obsolete("Use SetDepth(int, Geometries.Position, int)")]
        public void SetDepth(int geomIndex, Positions posIndex, int depthValue)
            => SetDepth(geomIndex, new Geometries.Position((int)posIndex), depthValue);
        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <param name="depthValue"></param>
        public void SetDepth(int geomIndex, Geometries.Position posIndex, int depthValue)
        {
            _depth[geomIndex, (int)posIndex] = depthValue;
        }

        /// <summary>
        /// Calls GetDepth and SetDepth.
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        [Obsolete("Use GetDepth, SetDepth")]
        public int this[int geomIndex, Positions posIndex]
        {
            get => GetDepth(geomIndex, posIndex);
            set => SetDepth(geomIndex, posIndex, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        [Obsolete("Use GetLocation(int, Geometries.Position)")]
        public Location GetLocation(int geomIndex, Positions posIndex)
            => GetLocation(geomIndex, new Geometries.Position((int)posIndex));

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public Location GetLocation(int geomIndex, Geometries.Position posIndex)
        {
            if (_depth[geomIndex, (int)posIndex] <= 0)
                return Location.Exterior;
            return Location.Interior;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <param name="_location"></param>
        [Obsolete("This method is not used and will be removed in a later release.")]
        public void Add(int geomIndex, Positions posIndex, Location _location)
        {
            if (_location == Location.Interior)
                _depth[geomIndex, (int)posIndex]++;
        }

        /// <summary>
        /// A Depth object is null (has never been initialized) if all depths are null.
        /// </summary>
        /// <returns><c>true</c> if depth is null (has never been initialized)</returns>
        public bool IsNull()
        {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (_depth[i,j] != @null)
                            return false;
                    }
                }
                return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public bool IsNull(int geomIndex)
        {
            return _depth[geomIndex,1] == @null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        [Obsolete("Use IsNull(int, Geometries.Position)")]
        public bool IsNull(int geomIndex, Positions posIndex) =>
            IsNull(geomIndex, new Geometries.Position((int) posIndex));

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public bool IsNull(int geomIndex, Geometries.Position posIndex)
        {
            return _depth[geomIndex,(int)posIndex] == @null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lbl"></param>
        public void Add(Label lbl)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j < 3; j++)
                {
                    var loc = lbl.GetLocation(i, new Geometries.Position(j));
                    if (loc == Location.Exterior || loc == Location.Interior)
                    {
                        // initialize depth if it is null, otherwise add this location value
                        if (IsNull(i, new Geometries.Position(j)))
                             _depth[i,j]  = DepthAtLocation(loc);
                        else _depth[i,j] += DepthAtLocation(loc);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public int GetDelta(int geomIndex)
        {
            return _depth[geomIndex, (int)Geometries.Position.Right] - _depth[geomIndex, (int)Geometries.Position.Left];
        }

        /// <summary>
        /// Normalize the depths for each point, if they are non-null.
        /// A normalized depth
        /// has depth values in the set { 0, 1 }.
        /// Normalizing the depths
        /// involves reducing the depths by the same amount so that at least
        /// one of them is 0.  If the remaining value is > 0, it is set to 1.
        /// </summary>
        public void Normalize()
        {
            for (int i = 0; i < 2; i++)
            {
                if (! IsNull(i))
                {
                    int minDepth = _depth[i,1];
                    if (_depth[i,2] < minDepth)
                    minDepth = _depth[i,2];

                    if (minDepth < 0) minDepth = 0;
                    for (int j = 1; j < 3; j++)
                    {
                        int newValue = 0;
                        if (_depth[i,j] > minDepth)
                            newValue = 1;
                        _depth[i,j] = newValue;
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("A: {0},{1} B: {2},{3}", this._depth[0,1], this._depth[0,2], this._depth[1,1], this._depth[1,2]);
        }
    }

    /* Same class, but without [,]

    /// <summary>
    /// A Depth object records the topological depth of the sides
    /// of an Edge for up to two Geometries.
    /// </summary>
    public class Depth
    {
        /// <summary>
        ///
        /// </summary>
        private const int Null = -1;

        /// <summary>
        ///
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static int DepthAtLocation(Location location)
        {
            if (location == Location.Exterior)
                return 0;

            if (location == Location.Interior)
                return 1;

            return Null;
        }

        private int[] depth = new int[6];

        /// <summary>
        ///
        /// </summary>
        public Depth()
        {
            // initialize depth array to a sentinel value
            for (int i = 0; i < 6; i++)
                depth[i] = Null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public int GetDepth(int geomIndex, Positions posIndex)
        {
            int index = geomIndex == 0 ? 0 : 3;
            return depth[index + (int)posIndex];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <param name="depthValue"></param>
        public void SetDepth(int geomIndex, Positions posIndex, int depthValue)
        {
            int index = geomIndex == 0 ? 0 : 3;
            depth[index + (int)posIndex] = depthValue;
        }

        /// <summary>
        /// Calls GetDepth and SetDepth.
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public int this[int geomIndex, Positions posIndex]
        {
            get
            {
                return GetDepth(geomIndex, posIndex);
            }
            set
            {
                SetDepth(geomIndex, posIndex, value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public Location GetLocation(int geomIndex, Positions posIndex)
        {
            int index = geomIndex == 0 ? 0 : 3;
            if (depth[index + (int)posIndex] <= 0)
                return Location.Exterior;
            return Location.Interior;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <param name="location"></param>
        public void Add(int geomIndex, Positions posIndex, Location location)
        {
            if (location == Location.Interior)
            {
                int index = geomIndex == 0 ? 0 : 3;
                depth[index + (int)posIndex]++;
            }
        }

        /// <summary>
        /// A Depth object is null (has never been initialized) if all depths are null.
        /// </summary>
        public bool IsNull()
        {
            for (int i = 0; i < 6; i++)
                if (depth[i] != Null)
                    return false;
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public bool IsNull(int geomIndex)
        {
            int index = geomIndex == 0 ? 0 : 3;
            return depth[index + 1] == Null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public bool IsNull(int geomIndex, Positions posIndex)
        {
            int index = geomIndex == 0 ? 0 : 3;
            return depth[index + (int)posIndex] == Null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lbl"></param>
        public void Add(Label lbl)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j < 3; j++)
                {
                    Location loc = lbl.GetLocation(i, (Positions)j);
                    if (loc == Location.Exterior || loc == Location.Interior)
                    {
                        // initialize depth if it is null, otherwise add this location value
                        if (IsNull(i, (Positions)j))
                        {
                            int index = i == 0 ? 0 : 3;
                            depth[index + j] = DepthAtLocation(loc);
                        }
                        else
                        {
                            int index = i == 0 ? 0 : 3;
                            depth[index + j] += DepthAtLocation(loc);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public int GetDelta(int geomIndex)
        {
            int index = geomIndex == 0 ? 0 : 3;
            return depth[index + (int)Positions.Right] - depth[index + (int)Positions.Left];
        }

        /// <summary>
        /// Normalize the depths for each point, if they are non-null.
        /// A normalized depth
        /// has depth values in the set { 0, 1 }.
        /// Normalizing the depths
        /// involves reducing the depths by the same amount so that at least
        /// one of them is 0.  If the remaining value is > 0, it is set to 1.
        /// </summary>
        public void Normalize()
        {
            for (int i = 0; i < 2; i++)
            {
                if (!IsNull(i))
                {
                    int index = i == 0 ? 0 : 3;
                    int minDepth = depth[index + 1];
                    if (depth[index + 2] < minDepth)
                        minDepth = depth[index + 2];

                    if (minDepth < 0) minDepth = 0;
                    for (int j = 1; j < 3; j++)
                    {
                        int newValue = 0;
                        if (depth[index + j] > minDepth)
                            newValue = 1;
                        depth[index + j] = newValue;
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("A: {0},{1} B: {2},{3}", this.depth[1], this.depth[2], this.depth[4], this.depth[5]);
        }
    }
    */
}
