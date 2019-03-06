using GeoAPI.Geometries;

namespace NetTopologySuite.Operation.Buffer
{
    public static class BufferFunctions
    {
        public static IGeometry BufferVariableWidth(ILineString line, double startWidth, double endWidth)
        {
            return VariableWidthBuffer.Buffer(line, startWidth, endWidth);
        }
    }
}
