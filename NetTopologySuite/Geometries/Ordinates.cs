using System;
#if !useFullGeoAPI
namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Flags for Ordinate values
    /// </summary>
    [Flags]
    public enum Ordinates
    {
        /// <summary>
        /// No ordinates
        /// </summary>
        None = 0,

        /// <summary>
        /// Flag for the x-ordinate 
        /// </summary>
        X = 1,

        /// <summary>
        /// Flag for the y-ordinate 
        /// </summary>
        Y = 2,

        /// <summary>
        /// Flag for both x- and y-ordinate
        /// </summary>
        XY = X | Y,

        /// <summary>
        /// Flag for the z-ordinate 
        /// </summary>
        Z = 4,

        /// <summary>
        /// Flag for x-, y- and z-ordinate
        /// </summary>
        XYZ = XY | Z,

        /// <summary>
        /// Flag for the m-ordinate 
        /// </summary>
        M = 8,

        /// <summary>
        /// Flag for x-, y- and m-ordinate
        /// </summary>
        XYM = XY | M,

        /// <summary>
        /// Flag for x-, y-, z- and m-ordinate
        /// </summary>
        XYZM = XYZ | M

    }
}
#endif