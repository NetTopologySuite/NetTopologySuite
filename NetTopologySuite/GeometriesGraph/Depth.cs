using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A <see cref="Depth"/> records the topological depth of the sides
    /// of an <see cref="Edge{TCoordinate}"/> for up to two 
    /// <see cref="Geometry{TCoordinate}"/> instances.
    /// </summary>
    public class Depth
    {
        public static Int32? DepthAtLocation(Locations location)
        {
            if (location == Locations.Exterior)
            {
                return 0;
            }

            if (location == Locations.Interior)
            {
                return 1;
            }

            return null;
        }

        private Int32? _depth00;
        private Int32? _depth01;
        private Int32? _depth02;
        private Int32? _depth10;
        private Int32? _depth11;
        private Int32? _depth12;

        [Obsolete("V2.0: Use indexer instead.")]
        public Int32? GetDepth(Int32 geometryIndex, Positions position)
        {
            return this[geometryIndex, position];
        }

        [Obsolete("V2.0: Use indexer instead.")]
        public void SetDepth(Int32 geometryIndex, Positions position, Int32? depthValue)
        {
            this[geometryIndex, position] = depthValue;
        }

        public Int32? this[Int32 geometryIndex, Positions position]
        {
            get
            {
                if (geometryIndex == 0)
                {
                    switch (position)
                    {
                        case Positions.On:
                            return _depth00;
                        case Positions.Left:
                            return _depth01;
                        case Positions.Right:
                            return _depth02;
                        case Positions.Parallel:
                        default:
                            return null;
                    }
                }
                else if (geometryIndex == 1)
                {
                    switch (position)
                    {
                        case Positions.On:
                            return _depth10;
                        case Positions.Left:
                            return _depth11;
                        case Positions.Right:
                            return _depth12;
                        case Positions.Parallel:
                        default:
                            return null;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("geometryIndex", geometryIndex,
                                                          "Geometry index must be 0 or 1.");
                }
            }
            set
            {
                if (geometryIndex == 0)
                {
                    switch (position)
                    {
                        case Positions.On:
                            _depth00 = value;
                            break;
                        case Positions.Left:
                            _depth01 = value;
                            break;
                        case Positions.Right:
                            _depth02 = value;
                            break;
                        case Positions.Parallel:
                            break;
                        default:
                            break;
                    }
                }
                else if (geometryIndex == 1)
                {
                    switch (position)
                    {
                        case Positions.On:
                            _depth10 = value;
                            break;
                        case Positions.Left:
                            _depth11 = value;
                            break;
                        case Positions.Right:
                            _depth12 = value;
                            break;
                        case Positions.Parallel:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("geometryIndex", geometryIndex,
                                                          "Geometry index must be 0 or 1.");
                }
            }
        }

        public Locations GetLocation(Int32 geometryIndex, Positions position)
        {
            if (this[geometryIndex, position] <= 0)
            {
                return Locations.Exterior;
            }

            return Locations.Interior;
        }

        public void Add(Int32 geometryIndex, Positions position, Locations location)
        {
            if (location == Locations.Interior)
            {
                this[geometryIndex, position]++;
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
                    if (this[i, (Positions)j] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Boolean IsNull(Int32 geometryIndex)
        {
            return this[geometryIndex, (Positions)1] == null;
        }

        public Boolean IsNull(Int32 geometryIndex, Positions position)
        {
            return this[geometryIndex, position] == null;
        }

        public void Add(Label label)
        {
            for (Int32 i = 0; i < 2; i++)
            {
                for (Int32 j = 1; j < 3; j++)
                {
                    Locations loc = label.GetLocation(i, (Positions)j);

                    if (loc == Locations.Exterior || loc == Locations.Interior)
                    {
                        // initialize depth if it is null, otherwise add this location value
                        if (IsNull(i, (Positions)j))
                        {
                            this[i, (Positions)j] = DepthAtLocation(loc);
                        }
                        else
                        {
                            this[i, (Positions)j] += DepthAtLocation(loc);
                        }
                    }
                }
            }
        }

        public Int32? GetDelta(Int32 geometryIndex)
        {
            return this[geometryIndex, Positions.Right] - this[geometryIndex, Positions.Left];
        }

        /// <summary>
        /// Normalize the depths for each point, if they are non-null.
        /// </summary>
        /// <remarks>
        /// A normalized depth
        /// has depth values in the set { 0, 1 }.
        /// Normalizing the depths
        /// involves reducing the depths by the same amount so that at least
        /// one of them is 0.  If the remaining value is > 0, it is set to 1.
        /// </remarks>
        public void Normalize()
        {
            for (Int32 i = 0; i < 2; i++)
            {
                if (!IsNull(i))
                {
                    Int32? minDepth = this[i, (Positions)1];

                    if (this[i, (Positions)2] < minDepth)
                    {
                        minDepth = this[i, (Positions)2];
                    }

                    if (minDepth < 0)
                    {
                        minDepth = 0;
                    }

                    for (Int32 j = 1; j < 3; j++)
                    {
                        Int32 newValue = 0;

                        if (this[i, (Positions)j] > minDepth)
                        {
                            newValue = 1;
                        }

                        this[i, (Positions)j] = newValue;
                    }
                }
            }
        }

        public override string ToString()
        {
            return "A: " + this[0, Positions.Left] + "," + this[0, Positions.Right]
                   + " B: " + this[1, Positions.Left] + "," + this[1, Positions.Right];
        }
    }
}