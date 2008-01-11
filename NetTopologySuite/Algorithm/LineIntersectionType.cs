
namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    public enum LineIntersectionType
    {
        // These numbers indicate the number of intersections
        // which are present in the intersection between two lines
        // or segments. Do not reorder.
        DoesNotIntersect = 0,
        Intersects = 1,
        Collinear = 2
    }
}
