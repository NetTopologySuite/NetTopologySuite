using GeoAPI.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    public class LineNode
    {
        public const int Forward = 0;
        public const int Backward = 1;

        private readonly SegmentNode _node;
        private readonly Location[] _location = new[] { GeoAPI.Geometries.Location.Exterior, GeoAPI.Geometries.Location.Exterior };

        public LineNode(SegmentNode node, Location locForward, Location locBackward)
        {
            _node = node;
            _location[Forward] = locForward;
            _location[Backward] = locBackward;
        }

        public void MergeLabel(Location locForward, Location locBackward)
        {
            _location[Forward] = MergeLocation(_location[Forward], locForward);
            _location[Backward] = MergeLocation(_location[Backward], locForward);
        }

        private static Location MergeLocation(Location loc1, Location loc2)
        {
            Location mergeLoc = loc1;
            if (loc2 == GeoAPI.Geometries.Location.Interior)
            {
                mergeLoc = GeoAPI.Geometries.Location.Interior;
            }
            return mergeLoc;
        }

        public Location[] Location
        {
            get { return _location; }
        }

        public Location GetLocation(int position)
        {
            return _location[position];
        }

        public SegmentNode Node
        {
            get { return _node; }
        }

    }
}