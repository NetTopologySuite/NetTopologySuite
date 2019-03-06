using GeoAPI.Geometries;

namespace NetTopologySuite.Operation.Buffer
{
    public static class BufferFunctions
    {
        /// <summary>
        /// Buffer a line by a width varying along the line.
        /// </summary>
        /// <param name="startWidth">Start width.</param>
        /// <param name="endWidth">End width.</param>
        public static IGeometry BufferVariableWidth(ILineString line, double startWidth, double endWidth)
        {
            return VariableWidthBuffer.Buffer(line, startWidth, endWidth);
        }
    }
}
