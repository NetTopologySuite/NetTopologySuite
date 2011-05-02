using GeoAPI.Geometries;
// a version that don't use [,]
namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    using System;

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
        public static int DepthAtLocation(Locations location)
        {
            if (location == Locations.Exterior) 
                return 0;

            if (location == Locations.Interior) 
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
        public Locations GetLocation(int geomIndex, Positions posIndex)
        {
            int index = geomIndex == 0 ? 0 : 3;
            if (depth[index + (int)posIndex] <= 0) 
                return Locations.Exterior;
            return Locations.Interior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <param name="location"></param>
        public void Add(int geomIndex, Positions posIndex, Locations location)
        {
            if (location == Locations.Interior)
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
                    Locations loc = lbl.GetLocation(i, (Positions)j);
                    if (loc == Locations.Exterior || loc == Locations.Interior)
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
                if (! IsNull(i)) 
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
}
