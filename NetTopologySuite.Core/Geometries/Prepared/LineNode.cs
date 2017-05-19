using GeoAPI.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    public class LineNode
    {
        public const int Forward = 0;
        public const int Backward = 1;

        public LineNode(SegmentNode node, Location locForward, Location locBackward)
        {
            Node = node;
            Location[Forward] = locForward;
            Location[Backward] = locBackward;
        }

        public Location[] Location { get; } = {GeoAPI.Geometries.Location.Exterior, GeoAPI.Geometries.Location.Exterior}
            ;

        public SegmentNode Node { get; }

        public void MergeLabel(Location locForward, Location locBackward)
        {
            Location[Forward] = MergeLocation(Location[Forward], locForward);
            Location[Backward] = MergeLocation(Location[Backward], locForward);
        }

        private static Location MergeLocation(Location loc1, Location loc2)
        {
            var mergeLoc = loc1;
            if (loc2 == GeoAPI.Geometries.Location.Interior)
                mergeLoc = GeoAPI.Geometries.Location.Interior;
            return mergeLoc;
        }

        public Location GetLocation(int position)
        {
            return Location[position];
        }
    }
}