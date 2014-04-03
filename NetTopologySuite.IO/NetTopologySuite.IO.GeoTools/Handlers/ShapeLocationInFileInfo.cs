namespace NetTopologySuite.IO.Handlers
{
    public class ShapeLocationInFileInfo
    {
        public ShapeLocationInFileInfo(long offsetFromStartOfFile, int shapeIndex)
        {
            OffsetFromStartOfFile = offsetFromStartOfFile;
            ShapeIndex = shapeIndex;
        }

        public long OffsetFromStartOfFile { get; private set; }

        /// <summary>
        /// Zero based shape index in file.
        /// </summary>
        public int ShapeIndex { get; private set; }
    }
}