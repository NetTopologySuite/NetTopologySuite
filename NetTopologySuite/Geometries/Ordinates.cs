using System;

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
        X = 1 << Ordinate.X,

        /// <summary>
        /// Flag for the y-ordinate 
        /// </summary>
        Y = 1 << Ordinate.Y,

        /// <summary>
        /// Flag for the z-ordinate 
        /// </summary>
        Z = 1 << Ordinate.Z,

        /// <summary>
        /// Flag for the m-ordinate 
        /// </summary>
        M = 1 << Ordinate.M,

        /// <summary>
        /// Flag for both x- and y-ordinate
        /// </summary>
        XY = X | Y,

        /// <summary>
        /// Flag for x-, y- and z-ordinate
        /// </summary>
        XYZ = XY | Z,

        /// <summary>
        /// Flag for x-, y- and m-ordinate
        /// </summary>
        XYM = XY | M,

        /// <summary>
        /// Flag for x-, y-, z- and m-ordinate
        /// </summary>
        XYZM = XYZ | M,

        /// <summary>
        /// Flag for the 1st spatial dimension.
        /// </summary>
        Spatial1 = 1 << Ordinate.Spatial1,

        /// <summary>
        /// Flag for the 2nd spatial dimension.
        /// </summary>
        Spatial2 = 1 << Ordinate.Spatial2,

        /// <summary>
        /// Flag for the 3rd spatial dimension.
        /// </summary>
        Spatial3 = 1 << Ordinate.Spatial3,

        /// <summary>
        /// Flag for the 4th spatial dimension.
        /// </summary>
        Spatial4 = 1 << Ordinate.Spatial4,

        /// <summary>
        /// Flag for the 5th spatial dimension.
        /// </summary>
        Spatial5 = 1 << Ordinate.Spatial5,

        /// <summary>
        /// Flag for the 6th spatial dimension.
        /// </summary>
        Spatial6 = 1 << Ordinate.Spatial6,

        /// <summary>
        /// Flag for the 7th spatial dimension.
        /// </summary>
        Spatial7 = 1 << Ordinate.Spatial7,

        /// <summary>
        /// Flag for the 8th spatial dimension.
        /// </summary>
        Spatial8 = 1 << Ordinate.Spatial8,

        /// <summary>
        /// Flag for the 9th spatial dimension.
        /// </summary>
        Spatial9 = 1 << Ordinate.Spatial9,

        /// <summary>
        /// Flag for the 10th spatial dimension.
        /// </summary>
        Spatial10 = 1 << Ordinate.Spatial10,

        /// <summary>
        /// Flag for the 11th spatial dimension.
        /// </summary>
        Spatial11 = 1 << Ordinate.Spatial11,

        /// <summary>
        /// Flag for the 12th spatial dimension.
        /// </summary>
        Spatial12 = 1 << Ordinate.Spatial12,

        /// <summary>
        /// Flag for the 13th spatial dimension.
        /// </summary>
        Spatial13 = 1 << Ordinate.Spatial13,

        /// <summary>
        /// Flag for the 14th spatial dimension.
        /// </summary>
        Spatial14 = 1 << Ordinate.Spatial14,

        /// <summary>
        /// Flag for the 15th spatial dimension.
        /// </summary>
        Spatial15 = 1 << Ordinate.Spatial15,

        /// <summary>
        /// Flag for the 16th spatial dimension.
        /// </summary>
        Spatial16 = 1 << Ordinate.Spatial16,

        /// <summary>
        /// Flags for all spatial ordinates.
        /// </summary>
        AllSpatialOrdinates = Spatial1 | Spatial2 | Spatial3 | Spatial4 | Spatial5 | Spatial6 | Spatial7 | Spatial8 | Spatial9 | Spatial10 | Spatial11 | Spatial12 | Spatial13 | Spatial14 | Spatial15 | Spatial16,

        /// <summary>
        /// Flag for the 1st measure dimension.
        /// </summary>
        Measure1 = 1 << Ordinate.Measure1,

        /// <summary>
        /// Flag for the 2nd measure dimension.
        /// </summary>
        Measure2 = 1 << Ordinate.Measure2,

        /// <summary>
        /// Flag for the 3rd measure dimension.
        /// </summary>
        Measure3 = 1 << Ordinate.Measure3,

        /// <summary>
        /// Flag for the 4th measure dimension.
        /// </summary>
        Measure4 = 1 << Ordinate.Measure4,

        /// <summary>
        /// Flag for the 5th measure dimension.
        /// </summary>
        Measure5 = 1 << Ordinate.Measure5,

        /// <summary>
        /// Flag for the 6th measure dimension.
        /// </summary>
        Measure6 = 1 << Ordinate.Measure6,

        /// <summary>
        /// Flag for the 7th measure dimension.
        /// </summary>
        Measure7 = 1 << Ordinate.Measure7,

        /// <summary>
        /// Flag for the 8th measure dimension.
        /// </summary>
        Measure8 = 1 << Ordinate.Measure8,

        /// <summary>
        /// Flag for the 9th measure dimension.
        /// </summary>
        Measure9 = 1 << Ordinate.Measure9,

        /// <summary>
        /// Flag for the 10th measure dimension.
        /// </summary>
        Measure10 = 1 << Ordinate.Measure10,

        /// <summary>
        /// Flag for the 11th measure dimension.
        /// </summary>
        Measure11 = 1 << Ordinate.Measure11,

        /// <summary>
        /// Flag for the 12th measure dimension.
        /// </summary>
        Measure12 = 1 << Ordinate.Measure12,

        /// <summary>
        /// Flag for the 13th measure dimension.
        /// </summary>
        Measure13 = 1 << Ordinate.Measure13,

        /// <summary>
        /// Flag for the 14th measure dimension.
        /// </summary>
        Measure14 = 1 << Ordinate.Measure14,

        /// <summary>
        /// Flag for the 15th measure dimension.
        /// </summary>
        Measure15 = 1 << Ordinate.Measure15,

        /// <summary>
        /// Flag for the 16th measure dimension.
        /// </summary>
        Measure16 = 1 << Ordinate.Measure16,

        /// <summary>
        /// Flags for all non-spatial ordinates.
        /// </summary>
        AllMeasureOrdinates = Measure1 | Measure2 | Measure3 | Measure4 | Measure5 | Measure6 | Measure7 | Measure8 | Measure9 | Measure10 | Measure11 | Measure12 | Measure13 | Measure14 | Measure15 | Measure16,

        /// <summary>
        /// Flags for all recognized ordinates.
        /// </summary>
        AllOrdinates = AllSpatialOrdinates | AllMeasureOrdinates,
    }
}
