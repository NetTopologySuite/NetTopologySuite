namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public enum Positions //: sbyte
    {
        /// <summary>
        ///  An indicator that a Location is <c>on</c> a GraphComponent (0)
        /// </summary>
        On = 0,

        /// <summary>
        /// An indicator that a Location is to the <c>left</c> of a GraphComponent (1)
        /// </summary>
        Left = 1,

        /// <summary> 
        /// An indicator that a Location is to the <c>right</c> of a GraphComponent (2)
        /// </summary> 
        Right = 2,

        /// <summary> 
        /// An indicator that a Location is <c>is parallel to x-axis</c> of a GraphComponent (-1)
        /// /// </summary> 
        Parallel = -1,
    }
}