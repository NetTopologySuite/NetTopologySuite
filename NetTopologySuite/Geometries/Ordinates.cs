using System;

namespace GeoAPI.Geometries
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
        XYZM = XYZ | M,

        /// <summary>
        /// Flag for ordinate at index 2
        /// </summary>
        Ordinate2 = 1 << Ordinate.Ordinate2,
        /// <summary>
        /// Flag for ordinate at index 2
        /// </summary>
        Ordinate3 = 1 << Ordinate.Ordinate3,
        /// <summary>
        /// Flag for ordinate at index 2
        /// </summary>
        Ordinate4 = 1 << Ordinate.Ordinate4,
        /// <summary>
        /// Flag for ordinate at index 2
        /// </summary>
        Ordinate5 = 1 << Ordinate.Ordinate5,
        /// <summary>
        /// Flag for ordinate at index 2
        /// </summary>
        Ordinate6 = 1 << Ordinate.Ordinate6,
        /// <summary>
        /// Flag for ordinate at index 2
        /// </summary>
        Ordinate7 = 1 << Ordinate.Ordinate7,
        /// <summary>
        /// Flag for ordinate at index 2
        /// </summary>
        Ordinate8 = 1 << Ordinate.Ordinate8,
        /// <summary>
        /// Flag for ordinate at index 2
        /// </summary>
        Ordinate9 = 1 << Ordinate.Ordinate9,
        /// <summary>
        /// Flag for ordinate at index 10
        /// </summary>
        Ordinate10 = 1 << Ordinate.Ordinate10,
        /// <summary>
        /// Flag for ordinate at index 11
        /// </summary>
        Ordinate11 = 1 << Ordinate.Ordinate11,
        /// <summary>
        /// Flag for ordinate at index 12
        /// </summary>
        Ordinate12 = 1 << Ordinate.Ordinate12,
        /// <summary>
        /// Flag for ordinate at index 13
        /// </summary>
        Ordinate13 = 1 << Ordinate.Ordinate13,
        /// <summary>
        /// Flag for ordinate at index 14
        /// </summary>
        Ordinate14 = 1 << Ordinate.Ordinate14,
        /// <summary>
        /// Flag for ordinate at index 15
        /// </summary>
        Ordinate15 = 1 << Ordinate.Ordinate15,
        /// <summary>
        /// Flag for ordinate at index 16
        /// </summary>
        Ordinate16 = 1 << Ordinate.Ordinate16,
        /// <summary>
        /// Flag for ordinate at index 17
        /// </summary>
        Ordinate17 = 1 << Ordinate.Ordinate17,
        /// <summary>
        /// Flag for ordinate at index 18
        /// </summary>
        Ordinate18 = 1 << Ordinate.Ordinate18,
        /// <summary>
        /// Flag for ordinate at index 19
        /// </summary>
        Ordinate19 = 1 << Ordinate.Ordinate19,
        /// <summary>
        /// Flag for ordinate at index 20
        /// </summary>
        Ordinate20 = 1 << Ordinate.Ordinate20,
        /// <summary>
        /// Flag for ordinate at index 21
        /// </summary>
        Ordinate21 = 1 << Ordinate.Ordinate21,
        /// <summary>
        /// Flag for ordinate at index 22
        /// </summary>
        Ordinate22 = 1 << Ordinate.Ordinate22,
        /// <summary>
        /// Flag for ordinate at index 23
        /// </summary>
        Ordinate23 = 1 << Ordinate.Ordinate23,
        /// <summary>
        /// Flag for ordinate at index 24
        /// </summary>
        Ordinate24 = 1 << Ordinate.Ordinate24,
        /// <summary>
        /// Flag for ordinate at index 25
        /// </summary>
        Ordinate25 = 1 << Ordinate.Ordinate25,
        /// <summary>
        /// Flag for ordinate at index 26
        /// </summary>
        Ordinate26 = 1 << Ordinate.Ordinate26,
        /// <summary>
        /// Flag for ordinate at index 27
        /// </summary>
        Ordinate27 = 1 << Ordinate.Ordinate27,
        /// <summary>
        /// Flag for ordinate at index 28
        /// </summary>
        Ordinate28 = 1 << Ordinate.Ordinate28,
        /// <summary>
        /// Flag for ordinate at index 29
        /// </summary>
        Ordinate29 = 1 << Ordinate.Ordinate29,
        /// <summary>
        /// Flag for ordinate at index 30
        /// </summary>
        Ordinate30 = 1 << Ordinate.Ordinate30,
        /// <summary>
        /// Flag for ordinate at index 31
        /// </summary>
        Ordinate31 = 1 << Ordinate.Ordinate31,
        /// <summary>
        /// Flag for ordinate at index 32
        /// </summary>
        Ordinate32 = 1 << Ordinate.Ordinate32,
    }
}