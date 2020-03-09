#nullable disable
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer
{
    public static class BufferFunctions
    {
        public static Geometry BufferVariableWidth(LineString line, double startWidth, double endWidth)
        {
            return VariableWidthBuffer.Buffer(line, startWidth, endWidth);
        }
    }
}
