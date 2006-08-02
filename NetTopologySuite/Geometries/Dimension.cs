using System;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Constants representing the dimensions of a point, a curve and a surface.
    /// Also, constants representing the dimensions of the empty point and
    /// non-empty geometries, and a wildcard dimension meaning "any dimension".
    /// </summary>
    public enum Dimensions : int
    {
        /// <summary>
        /// Dimension value of a point (0).
        /// </summary>
        Point = 0,

        /// <summary>
        /// Dimension value of a curve (1).
        /// </summary>
        Curve = 1,

        /// <summary>
        /// Dimension value of a surface (2).
        /// </summary>
        Surface = 2,

        /// <summary>
        /// Dimension value of a empty point (-1).
        /// </summary>
        False = -1,

        /// <summary>
        /// Dimension value of non-empty geometries (= {Point,Curve,A}).
        /// </summary>
        True = -2,

        /// <summary>
        /// Dimension value for any dimension (= {False, True}).
        /// </summary>
        Dontcare = -3
    }

    /// <summary>
    /// Class containing static methods for conversions
    /// between Dimension values and characters.
    /// </summary>
    public class Dimension
    {
        private Dimension() { }

        /// <summary>
        /// Converts the dimension value to a dimension symbol,
        /// for example, <c>True => 'T'</c>
        /// </summary>
        /// <param name="dimensionValue">Number that can be stored in the <c>IntersectionMatrix</c>.
        /// Possible values are <c>True, False, Dontcare, 0, 1, 2</c>.</param>
        /// <returns>Character for use in the string representation of an <c>IntersectionMatrix</c>.
        /// Possible values are <c>T, F, * , 0, 1, 2</c>.</returns>
        public static char ToDimensionSymbol(Dimensions dimensionValue)
        {
            switch (dimensionValue)
            {
                case Dimensions.False:
                    return 'F';
                case Dimensions.True:
                    return 'T';
                case Dimensions.Dontcare:
                    return '*';
                case Dimensions.Point:
                    return '0';
                case Dimensions.Curve:
                    return '1';
                case Dimensions.Surface:
                    return '2';
                default:
                    throw new ArgumentOutOfRangeException
                        ("Unknown dimension value: " + dimensionValue);
            }
        }

        /// <summary>
        /// Converts the dimension symbol to a dimension value,
        /// for example, <c>'*' => Dontcare</c>
        /// </summary>
        /// <param name="dimensionSymbol">Character for use in the string representation of an <c>IntersectionMatrix</c>.
        /// Possible values are <c>T, F, * , 0, 1, 2</c>.</param>
        /// <returns>Number that can be stored in the <c>IntersectionMatrix</c>.
        /// Possible values are <c>True, False, Dontcare, 0, 1, 2</c>.</returns>
        public static Dimensions ToDimensionValue(char dimensionSymbol)
        {
            switch (Char.ToUpper(dimensionSymbol))
            {
                case 'F':
                    return Dimensions.False;
                case 'T':
                    return Dimensions.True;
                case '*':
                    return Dimensions.Dontcare;
                case '0':
                    return Dimensions.Point;
                case '1':
                    return Dimensions.Curve;
                case '2':
                    return Dimensions.Surface;
                default:
                    throw new ArgumentOutOfRangeException
                        ("Unknown dimension symbol: " + dimensionSymbol);
            }
        }
    }
}
