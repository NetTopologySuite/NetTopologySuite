using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Records topological information about an 
    /// edge representing a piece of linework (lineString or polygon ring)
    /// from a single source geometry.
    /// This information is carried through the noding process
    /// (which may result in many noded edges sharing the same information object).
    /// It is then used to populate the topology info fields
    /// in <see cref="Edge"/>s (possibly via merging).
    /// That information is used to construct the topology graph <see cref="OverlayLabel"/>s.
    /// </summary>
    /// <autor>Martin Davis</autor>
    internal sealed class EdgeSourceInfo
    {
        public EdgeSourceInfo(int index, int depthDelta, bool isHole)
        {
            Index = index;
            Dimension = Dimension.Surface;
            DepthDelta = depthDelta;
            IsHole = isHole;
        }

        public EdgeSourceInfo(int index)
        {
            Index = index;
            Dimension = Dimension.Curve;
            IsHole = false;
            DepthDelta = 0;
        }

        public int Index { get; }

        public Dimension Dimension { get; } // = -999;

        public int DepthDelta { get; }

        public bool IsHole { get; }

        public override string ToString()
        {
            return Edge.InfoString(Index, Dimension, IsHole, DepthDelta);
        }
    }
}
