using GisSharpBlog.NetTopologySuite.Noding;
using LocationsEnum = GeoAPI.Geometries.Locations;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    public class LineNode
    {
        public const int Forward = 0;
        public const int Backward = 1;

        private readonly SegmentNode _node;
        private readonly LocationsEnum[] _location = new[] { LocationsEnum.Exterior, LocationsEnum.Exterior };

        public LineNode(SegmentNode node, LocationsEnum locForward, LocationsEnum locBackward)
        {
            _node = node;
            _location[Forward] = locForward;
            _location[Backward] = locBackward;
        }

        public void MergeLabel(LocationsEnum locForward, LocationsEnum locBackward)
        {
            _location[Forward] = MergeLocation(_location[Forward], locForward);
            _location[Backward] = MergeLocation(_location[Backward], locForward);
        }

        private static LocationsEnum MergeLocation(LocationsEnum loc1, LocationsEnum loc2)
        {
            LocationsEnum mergeLoc = loc1;
            if (loc2 == LocationsEnum.Interior)
            {
                mergeLoc = LocationsEnum.Interior;
            }
            return mergeLoc;
        }

        public LocationsEnum[] Locations
        {
            get { return _location; }
        }

        public LocationsEnum GetLocation(int position)
        {
            return _location[position];
        }

        public SegmentNode Node
        {
            get { return _node; }
        }

    }
}