using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A Depth object records the topological depth of the sides
    /// of an Edge for up to two Geometries.
    /// </summary>
    public class Depth
    {
        private const Int32 Null = -1;

        public static Int32 DepthAtLocation(Locations location)
        {
            if (location == Locations.Exterior)
            {
                return 0;
            }

            if (location == Locations.Interior)
            {
                return 1;
            }

            return Null;
        }

        private Int32[,] depth = new Int32[2,3];

        public Depth()
        {
            // initialize depth array to a sentinel value
            for (Int32 i = 0; i < 2; i++)
            {
                for (Int32 j = 0; j < 3; j++)
                {
                    depth[i, j] = Null;
                }
            }
        }

        public Int32 GetDepth(Int32 geomIndex, Positions posIndex)
        {
            return depth[geomIndex, (Int32) posIndex];
        }

        public void SetDepth(Int32 geomIndex, Positions posIndex, Int32 depthValue)
        {
            depth[geomIndex, (Int32) posIndex] = depthValue;
        }

        /// <summary>
        /// Calls GetDepth and SetDepth.
        /// </summary>
        public Int32 this[Int32 geomIndex, Positions posIndex]
        {
            get { return GetDepth(geomIndex, posIndex); }
            set { SetDepth(geomIndex, posIndex, value); }
        }

        public Locations GetLocation(Int32 geomIndex, Positions posIndex)
        {
            if (depth[geomIndex, (Int32) posIndex] <= 0)
            {
                return Locations.Exterior;
            }
            return Locations.Interior;
        }

        public void Add(Int32 geomIndex, Positions posIndex, Locations location)
        {
            if (location == Locations.Interior)
            {
                depth[geomIndex, (Int32) posIndex]++;
            }
        }

        /// <summary>
        /// A Depth object is null (has never been initialized) if all depths are null.
        /// </summary>
        public Boolean IsNull()
        {
            for (Int32 i = 0; i < 2; i++)
            {
                for (Int32 j = 0; j < 3; j++)
                {
                    if (depth[i, j] != Null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Boolean IsNull(Int32 geomIndex)
        {
            return depth[geomIndex, 1] == Null;
        }

        public Boolean IsNull(Int32 geomIndex, Positions posIndex)
        {
            return depth[geomIndex, (Int32) posIndex] == Null;
        }

        public void Add(Label lbl)
        {
            for (Int32 i = 0; i < 2; i++)
            {
                for (Int32 j = 1; j < 3; j++)
                {
                    Locations loc = lbl.GetLocation(i, (Positions) j);

                    if (loc == Locations.Exterior || loc == Locations.Interior)
                    {
                        // initialize depth if it is null, otherwise add this location value
                        if (IsNull(i, (Positions) j))
                        {
                            depth[i, j] = DepthAtLocation(loc);
                        }
                        else
                        {
                            depth[i, j] += DepthAtLocation(loc);
                        }
                    }
                }
            }
        }

        public Int32 GetDelta(Int32 geomIndex)
        {
            return depth[geomIndex, (Int32) Positions.Right] - depth[geomIndex, (Int32) Positions.Left];
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
            for (Int32 i = 0; i < 2; i++)
            {
                if (! IsNull(i))
                {
                    Int32 minDepth = depth[i, 1];

                    if (depth[i, 2] < minDepth)
                    {
                        minDepth = depth[i, 2];
                    }

                    if (minDepth < 0)
                    {
                        minDepth = 0;
                    }

                    for (Int32 j = 1; j < 3; j++)
                    {
                        Int32 newValue = 0;

                        if (depth[i, j] > minDepth)
                        {
                            newValue = 1;
                        }

                        depth[i, j] = newValue;
                    }
                }
            }
        }

        public override string ToString()
        {
            return "A: " + depth[0, 1] + "," + depth[0, 2]
                   + " B: " + depth[1, 1] + "," + depth[1, 2];
        }
    }
}