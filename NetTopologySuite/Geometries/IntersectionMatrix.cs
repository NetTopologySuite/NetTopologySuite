using System;
using System.Text;

namespace GeoAPI.Geometries
{
    /// <summary>  
    /// Models a <b>Dimensionally Extended Nine-Intersection Model (DE-9IM)</b> matrix. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// DE-9IM matrices (such as "212FF1FF2")
    /// specify the topological relationship between two <see cref="IGeometry"/>s. 
    /// This class can also represent matrix patterns (such as "T*T******")
    /// which are used for matching instances of DE-9IM matrices.
    /// </para>
    /// <para>
    /// Methods are provided to:
    /// <list type="Bullet">
    /// <item>Set and query the elements of the matrix in a convenient fashion.</item>
    /// <item>Convert to and from the standard string representation (specified in SFS Section 2.1.13.2).</item>
    /// <item>Test to see if a matrix matches a given pattern string.</item>
    /// </list>
    /// </para>
    /// For a description of the DE-9IM and the spatial predicates derived from it, see the <i>
    /// <see href="http://www.opengis.org/techno/specs.htm">OGC 99-049 OpenGIS Simple Features Specification for SQL.</see></i> as well as 
    /// <i>OGC 06-103r4 OpenGIS 
    /// Implementation Standard for Geographic information - 
    /// Simple feature access - Part 1: Common architecture</i>
    /// (which provides some further details on certain predicate specifications).
    /// <para>
    /// The entries of the matrix are defined by the constants in the <see cref="Dimension"/> enum.
    /// The indices of the matrix represent the topological locations 
    /// that occur in a geometry (Interior, Boundary, Exterior).  
    /// These are provided as constants in the <see cref="Location"/> enum.
    /// </para>
    /// </remarks>
    public class IntersectionMatrix
    {
        /// <summary>  
        /// Internal representation of this <see cref="IntersectionMatrix" />.
        /// </summary>
        private readonly Dimension[,] _matrix;

        /// <summary>  
        /// Creates an <see cref="IntersectionMatrix" /> with <c>null</c> location values.
        /// </summary>
        public IntersectionMatrix()
        {
            _matrix = new Dimension[3, 3];
            SetAll(Dimension.False);
        }

        /// <summary>
        /// Creates an <see cref="IntersectionMatrix" /> with the given dimension
        /// symbols.
        /// </summary>
        /// <param name="elements">A string of nine dimension symbols in row major order.</param>
        public IntersectionMatrix(string elements)
            : this()
        {
            Set(elements);
        }

        /// <summary> 
        /// Creates an <see cref="IntersectionMatrix" /> with the same elements as
        /// <c>other</c>.
        /// </summary>
        /// <param name="other">An <see cref="IntersectionMatrix" /> to copy.</param>         
        public IntersectionMatrix(IntersectionMatrix other)
            : this()
        {
            _matrix[(int)Location.Interior, (int)Location.Interior] = other._matrix[(int)Location.Interior, (int)Location.Interior];
            _matrix[(int)Location.Interior, (int)Location.Boundary] = other._matrix[(int)Location.Interior, (int)Location.Boundary];
            _matrix[(int)Location.Interior, (int)Location.Exterior] = other._matrix[(int)Location.Interior, (int)Location.Exterior];
            _matrix[(int)Location.Boundary, (int)Location.Interior] = other._matrix[(int)Location.Boundary, (int)Location.Interior];
            _matrix[(int)Location.Boundary, (int)Location.Boundary] = other._matrix[(int)Location.Boundary, (int)Location.Boundary];
            _matrix[(int)Location.Boundary, (int)Location.Exterior] = other._matrix[(int)Location.Boundary, (int)Location.Exterior];
            _matrix[(int)Location.Exterior, (int)Location.Interior] = other._matrix[(int)Location.Exterior, (int)Location.Interior];
            _matrix[(int)Location.Exterior, (int)Location.Boundary] = other._matrix[(int)Location.Exterior, (int)Location.Boundary];
            _matrix[(int)Location.Exterior, (int)Location.Exterior] = other._matrix[(int)Location.Exterior, (int)Location.Exterior];
        }

        /// <summary> 
        /// Adds one matrix to another.
        /// Addition is defined by taking the maximum dimension value of each position
        /// in the summand matrices.
        /// </summary>
        /// <param name="im">The matrix to add.</param>        
        public void Add(IntersectionMatrix im)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    SetAtLeast((Location)i, (Location)j, im.Get((Location)i, (Location)j));
        }


        /// <summary>
        /// Tests if the dimension value matches <tt>TRUE</tt>
        /// (i.e.  has value 0, 1, 2 or TRUE).
        /// </summary>
        /// <param name="actualDimensionValue">A number that can be stored in the <c>IntersectionMatrix</c>.
        /// Possible values are <c>{<see cref="Dimension.True"/>, <see cref="Dimension.False"/>, <see cref="Dimension.Dontcare"/>, <see cref="Dimension.Point"/>, <see cref="Dimension.Curve"/>, <see cref="Dimension.Surface"/>}</c></param>
        /// <returns><c>true</c> if the dimension value matches <see cref="Dimension.True"/></returns>
        public static bool IsTrue(Dimension actualDimensionValue)
        {
            if (actualDimensionValue >= 0 || actualDimensionValue == Dimension.True)
            {
                return true;
            }
            return false;
        }

        /// <summary>  
        /// Tests if the dimension value satisfies the dimension symbol.
        /// </summary>
        /// <param name="actualDimensionValue">
        /// a number that can be stored in the <c>IntersectionMatrix</c>.
        /// Possible values are <c>{True, False, Dontcare, 0, 1, 2}</c>.
        /// </param>
        /// <param name="requiredDimensionSymbol">
        /// A character used in the string
        /// representation of an <see cref="IntersectionMatrix" />. 
        /// Possible values are <c>T, F, * , 0, 1, 2</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the dimension symbol encompasses the dimension value.        
        /// </returns>        
        public static bool Matches(Dimension actualDimensionValue, char requiredDimensionSymbol)
        {
            if (requiredDimensionSymbol == DimensionUtility.SymDontcare)
                return true;
            if (requiredDimensionSymbol == DimensionUtility.SymTrue && (actualDimensionValue >= Dimension.Point || actualDimensionValue == Dimension.True))
                return true;
            if (requiredDimensionSymbol == DimensionUtility.SymFalse && actualDimensionValue == Dimension.False)
                return true;
            if (requiredDimensionSymbol == DimensionUtility.SymP && actualDimensionValue == Dimension.Point)
                return true;
            if (requiredDimensionSymbol == DimensionUtility.SymL && actualDimensionValue == Dimension.Curve)
                return true;
            if (requiredDimensionSymbol == DimensionUtility.SymA && actualDimensionValue == Dimension.Surface)
                return true;
            return false;
        }

        /// <summary>
        /// Tests if each of the actual dimension symbols in a matrix string satisfies the
        /// corresponding required dimension symbol in a pattern string.
        /// </summary>
        /// <param name="actualDimensionSymbols">
        /// Nine dimension symbols to validate.
        /// Possible values are <c>T, F, * , 0, 1, 2</c>.
        /// </param>
        /// <param name="requiredDimensionSymbols">
        /// Nine dimension symbols to validate
        /// against. Possible values are <c>T, F, * , 0, 1, 2</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if each of the required dimension
        /// symbols encompass the corresponding actual dimension symbol.
        /// </returns>
        public static bool Matches(string actualDimensionSymbols, string requiredDimensionSymbols)
        {
            IntersectionMatrix m = new IntersectionMatrix(actualDimensionSymbols);
            return m.Matches(requiredDimensionSymbols);
        }

        /// <summary>
        /// Changes the value of one of this <see cref="IntersectionMatrix" /> elements.
        /// </summary>
        /// <param name="row">
        /// The row of this <see cref="IntersectionMatrix" />,
        /// indicating the interior, boundary or exterior of the first <see cref="IGeometry"/>
        /// </param>
        /// <param name="column">
        /// The column of this <see cref="IntersectionMatrix" />,
        /// indicating the interior, boundary or exterior of the second <see cref="IGeometry"/>
        /// </param>
        /// <param name="dimensionValue">The new value of the element</param>        
        public void Set(Location row, Location column, Dimension dimensionValue)
        {
            _matrix[(int)row, (int)column] = dimensionValue;
        }

        /// <summary>
        /// Changes the elements of this <see cref="IntersectionMatrix" /> to the
        /// dimension symbols in <c>dimensionSymbols</c>.
        /// </summary>
        /// <param name="dimensionSymbols">
        /// Nine dimension symbols to which to set this <see cref="IntersectionMatrix" />
        /// s elements. Possible values are <c>{T, F, * , 0, 1, 2}</c>
        /// </param>
        public void Set(string dimensionSymbols)
        {
            for (int i = 0; i < dimensionSymbols.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                _matrix[row, col] = DimensionUtility.ToDimensionValue(dimensionSymbols[i]);
            }
        }

        /// <summary>
        /// Changes the specified element to <c>minimumDimensionValue</c> if the element is less.
        /// </summary>
        /// <param name="row">
        /// The row of this <see cref="IntersectionMatrix" />, 
        /// indicating the interior, boundary or exterior of the first <see cref="IGeometry"/>.
        /// </param>
        /// <param name="column">
        /// The column of this <see cref="IntersectionMatrix" />, 
        /// indicating the interior, boundary or exterior of the second <see cref="IGeometry"/>.
        /// </param>
        /// <param name="minimumDimensionValue">
        /// The dimension value with which to compare the
        /// element. The order of dimension values from least to greatest is
        /// <c>True, False, Dontcare, 0, 1, 2</c>.
        /// </param>
        public void SetAtLeast(Location row, Location column, Dimension minimumDimensionValue)
        {
            if (_matrix[(int)row, (int)column] < minimumDimensionValue)
                _matrix[(int)row, (int)column] = minimumDimensionValue;
        }

        /// <summary>
        /// If row >= 0 and column >= 0, changes the specified element to <c>minimumDimensionValue</c>
        /// if the element is less. Does nothing if row is smaller to 0 or column is smaller to 0.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="minimumDimensionValue"></param>
        public void SetAtLeastIfValid(Location row, Location column, Dimension minimumDimensionValue)
        {
            if (row >= Location.Interior && column >= Location.Interior)
                SetAtLeast(row, column, minimumDimensionValue);
        }

        /// <summary>
        /// For each element in this <see cref="IntersectionMatrix" />, changes the
        /// element to the corresponding minimum dimension symbol if the element is
        /// less.
        /// </summary>
        /// <param name="minimumDimensionSymbols"> 
        /// Nine dimension symbols with which to
        /// compare the elements of this <see cref="IntersectionMatrix" />. The
        /// order of dimension values from least to greatest is <c>Dontcare, True, False, 0, 1, 2</c>.
        /// </param>
        public void SetAtLeast(string minimumDimensionSymbols)
        {
            for (int i = 0; i < minimumDimensionSymbols.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                SetAtLeast((Location)row, (Location)col, DimensionUtility.ToDimensionValue(minimumDimensionSymbols[i]));
            }
        }

        /// <summary>  
        /// Changes the elements of this <see cref="IntersectionMatrix" /> to <c>dimensionValue</c>.
        /// </summary>
        /// <param name="dimensionValue">
        /// The dimension value to which to set this <see cref="IntersectionMatrix" />
        /// s elements. Possible values <c>True, False, Dontcare, 0, 1, 2}</c>.
        /// </param>         
        public void SetAll(Dimension dimensionValue)
        {
            for (int ai = 0; ai < 3; ai++)
                for (int bi = 0; bi < 3; bi++)
                    _matrix[ai, bi] = dimensionValue;
        }

        /// <summary>
        /// Returns the value of one of this <see cref="IntersectionMatrix" />s
        /// elements.
        /// </summary>
        /// <param name="row">
        /// The row of this <see cref="IntersectionMatrix" />, indicating
        /// the interior, boundary or exterior of the first <see cref="IGeometry"/>.
        /// </param>
        /// <param name="column">
        /// The column of this <see cref="IntersectionMatrix" />,
        /// indicating the interior, boundary or exterior of the second <see cref="IGeometry"/>.
        /// </param>
        /// <returns>The dimension value at the given matrix position.</returns>
        public Dimension Get(Location row, Location column)
        {
            return _matrix[(int)row, (int)column];
        }

        /// <summary>
        /// See methods Get(int, int) and Set(int, int, int value)
        /// </summary>         
        public Dimension this[Location row, Location column]
        {
            get
            {
                return Get(row, column);
            }
            set
            {
                Set(row, column, value);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IntersectionMatrix" /> is FF*FF****.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the two <see cref="IGeometry"/>'s related by
        /// this <see cref="IntersectionMatrix" /> are disjoint.
        /// </returns>
        public bool IsDisjoint()
        {
            return
                _matrix[(int)Location.Interior, (int)Location.Interior] == Dimension.False &&
                _matrix[(int)Location.Interior, (int)Location.Boundary] == Dimension.False &&
                _matrix[(int)Location.Boundary, (int)Location.Interior] == Dimension.False &&
                _matrix[(int)Location.Boundary, (int)Location.Boundary] == Dimension.False;
        }

        /// <summary>
        /// Returns <c>true</c> if <c>isDisjoint</c> returns false.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the two <see cref="IGeometry"/>'s related by
        /// this <see cref="IntersectionMatrix" /> intersect.
        /// </returns>
        public bool IsIntersects()
        {
            return !IsDisjoint();
        }

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IntersectionMatrix" /> is
        /// FT*******, F**T***** or F***T****.
        /// </summary>
        /// <param name="dimensionOfGeometryA">The dimension of the first <see cref="IGeometry"/>.</param>
        /// <param name="dimensionOfGeometryB">The dimension of the second <see cref="IGeometry"/>.</param>
        /// <returns>
        /// <c>true</c> if the two <see cref="IGeometry"/>
        /// s related by this <see cref="IntersectionMatrix" /> touch; Returns false
        /// if both <see cref="IGeometry"/>s are points.
        /// </returns>
        public bool IsTouches(Dimension dimensionOfGeometryA, Dimension dimensionOfGeometryB)
        {
            if (dimensionOfGeometryA > dimensionOfGeometryB)
                //no need to get transpose because pattern matrix is symmetrical
                return IsTouches(dimensionOfGeometryB, dimensionOfGeometryA);

            if ((dimensionOfGeometryA == Dimension.Surface && dimensionOfGeometryB == Dimension.Surface) ||
                (dimensionOfGeometryA == Dimension.Curve && dimensionOfGeometryB == Dimension.Curve) ||
                (dimensionOfGeometryA == Dimension.Curve && dimensionOfGeometryB == Dimension.Surface) ||
                (dimensionOfGeometryA == Dimension.Point && dimensionOfGeometryB == Dimension.Surface) ||
                (dimensionOfGeometryA == Dimension.Point && dimensionOfGeometryB == Dimension.Curve))
                return _matrix[(int)Location.Interior, (int)Location.Interior] == Dimension.False &&
                        (IsTrue(_matrix[(int)Location.Interior, (int)Location.Boundary]) ||
                         IsTrue(_matrix[(int)Location.Boundary, (int)Location.Interior]) ||
                         IsTrue(_matrix[(int)Location.Boundary, (int)Location.Boundary]));

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IntersectionMatrix" /> is
        ///  T*T****** (for a point and a curve, a point and an area or a line
        /// and an area) 0******** (for two curves).
        /// </summary>
        /// <param name="dimensionOfGeometryA">The dimension of the first <see cref="IGeometry"/>.</param>
        /// <param name="dimensionOfGeometryB">The dimension of the second <see cref="IGeometry"/>.</param>
        /// <returns>
        /// <c>true</c> if the two <see cref="IGeometry"/>
        /// s related by this <see cref="IntersectionMatrix" /> cross. For this
        /// function to return <c>true</c>, the <see cref="IGeometry"/>s must
        /// be a point and a curve; a point and a surface; two curves; or a curve
        /// and a surface.
        /// </returns>
        public bool IsCrosses(Dimension dimensionOfGeometryA, Dimension dimensionOfGeometryB)
        {
            if ((dimensionOfGeometryA == Dimension.Point && dimensionOfGeometryB == Dimension.Curve) ||
                (dimensionOfGeometryA == Dimension.Point && dimensionOfGeometryB == Dimension.Surface) ||
                (dimensionOfGeometryA == Dimension.Curve && dimensionOfGeometryB == Dimension.Surface))
                return IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior]) &&
                       IsTrue(_matrix[(int)Location.Interior, (int)Location.Exterior]);

            if ((dimensionOfGeometryA == Dimension.Curve && dimensionOfGeometryB == Dimension.Point) ||
                (dimensionOfGeometryA == Dimension.Surface && dimensionOfGeometryB == Dimension.Point) ||
                (dimensionOfGeometryA == Dimension.Surface && dimensionOfGeometryB == Dimension.Curve))
                return IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior]) &&
                       IsTrue(_matrix[(int)Location.Exterior, (int)Location.Interior]);

            if (dimensionOfGeometryA == Dimension.Curve && dimensionOfGeometryB == Dimension.Curve)
                return _matrix[(int)Location.Interior, (int)Location.Interior] == 0;

            return false;
        }

        /// <summary>  
        /// Returns <c>true</c> if this <see cref="IntersectionMatrix" /> is
        /// T*F**F***.
        /// </summary>
        /// <returns><c>true</c> if the first <see cref="IGeometry"/> is within the second.</returns>
        public bool IsWithin()
        {
            return IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior]) &&
                   _matrix[(int)Location.Interior, (int)Location.Exterior] == Dimension.False &&
                   _matrix[(int)Location.Boundary, (int)Location.Exterior] == Dimension.False;
        }

        /// <summary> 
        /// Returns <c>true</c> if this <see cref="IntersectionMatrix" /> is
        /// T*****FF*.
        /// </summary>
        /// <returns><c>true</c> if the first <see cref="IGeometry"/> contains the second.</returns>
        public bool IsContains()
        {
            return IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior]) &&
                   _matrix[(int)Location.Exterior, (int)Location.Interior] == Dimension.False &&
                   _matrix[(int)Location.Exterior, (int)Location.Boundary] == Dimension.False;
        }

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IntersectionMatrix" /> is <c>T*****FF*</c>
        /// or <c>*T****FF*</c> or <c>***T**FF*</c> or <c>****T*FF*</c>.
        /// </summary>
        /// <returns><c>true</c> if the first <see cref="IGeometry"/> covers the second</returns>
        public bool IsCovers()
        {
            bool hasPointInCommon = IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior])
                                    || IsTrue(_matrix[(int)Location.Interior, (int)Location.Boundary])
                                    || IsTrue(_matrix[(int)Location.Boundary, (int)Location.Interior])
                                    || IsTrue(_matrix[(int)Location.Boundary, (int)Location.Boundary]);

            return hasPointInCommon &&
                    _matrix[(int)Location.Exterior, (int)Location.Interior] == Dimension.False &&
                    _matrix[(int)Location.Exterior, (int)Location.Boundary] == Dimension.False;
        }

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IntersectionMatrix" /> is <c>T*F**F***</c>
        /// or <c>*TF**F***</c> or <c>**FT*F***</c> or <c>**F*TF***</c>
        /// </summary>
        /// <returns><c>true</c> if the first <see cref="IGeometry"/> is covered by the second</returns>
        public bool IsCoveredBy()
        {
            bool hasPointInCommon = Matches(_matrix[(int)Location.Interior, (int)Location.Interior], 'T')
                                    || Matches(_matrix[(int)Location.Interior, (int)Location.Boundary], 'T')
                                    || Matches(_matrix[(int)Location.Boundary, (int)Location.Interior], 'T')
                                    || Matches(_matrix[(int)Location.Boundary, (int)Location.Boundary], 'T');

            return hasPointInCommon &&
                _matrix[(int)Location.Interior, (int)Location.Exterior] == Dimension.False &&
                _matrix[(int)Location.Boundary, (int)Location.Exterior] == Dimension.False;
        }

        /// <summary> 
        /// Tests whether the argument dimensions are equal and 
        /// this <c>IntersectionMatrix</c> matches
        /// the pattern <tt>T*F**FFF*</tt>.
        /// <para/>
        /// <b>Note:</b> This pattern differs from the one stated in 
        /// <i>Simple feature access - Part 1: Common architecture</i>.
        /// That document states the pattern as <tt>TFFFTFFFT</tt>.  This would
        /// specify that
        /// two identical <tt>POINT</tt>s are not equal, which is not desirable behaviour.
        /// The pattern used here has been corrected to compute equality in this situation.
        /// </summary>
        /// <param name="dimensionOfGeometryA">The dimension of the first <see cref="IGeometry"/>.</param>
        /// <param name="dimensionOfGeometryB">The dimension of the second <see cref="IGeometry"/>.</param>
        /// <returns>
        /// <c>true</c> if the two <see cref="IGeometry"/>s
        /// related by this <see cref="IntersectionMatrix" /> are equal; the
        /// <see cref="IGeometry"/>s must have the same dimension to be equal.
        /// </returns>
        public bool IsEquals(Dimension dimensionOfGeometryA, Dimension dimensionOfGeometryB)
        {
            if (dimensionOfGeometryA != dimensionOfGeometryB)
                return false;

            return IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior]) &&
                   _matrix[(int)Location.Interior, (int)Location.Exterior] == Dimension.False &&
                   _matrix[(int)Location.Boundary, (int)Location.Exterior] == Dimension.False &&
                   _matrix[(int)Location.Exterior, (int)Location.Interior] == Dimension.False &&
                   _matrix[(int)Location.Exterior, (int)Location.Boundary] == Dimension.False;
        }

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IntersectionMatrix" /> is
        ///  T*T***T** (for two points or two surfaces)
        ///  1*T***T** (for two curves).
        /// </summary>
        /// <param name="dimensionOfGeometryA">The dimension of the first <see cref="IGeometry"/>.</param>
        /// <param name="dimensionOfGeometryB">The dimension of the second <see cref="IGeometry"/>.</param>
        /// <returns>
        /// <c>true</c> if the two <see cref="IGeometry"/>
        /// s related by this <see cref="IntersectionMatrix" /> overlap. For this
        /// function to return <c>true</c>, the <see cref="IGeometry"/>s must
        /// be two points, two curves or two surfaces.
        /// </returns>
        public bool IsOverlaps(Dimension dimensionOfGeometryA, Dimension dimensionOfGeometryB)
        {
            if ((dimensionOfGeometryA == Dimension.Point && dimensionOfGeometryB == Dimension.Point) ||
                (dimensionOfGeometryA == Dimension.Surface && dimensionOfGeometryB == Dimension.Surface))
                return IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior]) &&
                       IsTrue(_matrix[(int)Location.Interior, (int)Location.Exterior]) &&
                       IsTrue(_matrix[(int)Location.Exterior, (int)Location.Interior]);

            if (dimensionOfGeometryA == Dimension.Curve && dimensionOfGeometryB == Dimension.Curve)
                return _matrix[(int)Location.Interior, (int)Location.Interior] == Dimension.Curve &&
                       IsTrue(_matrix[(int)Location.Interior, (int)Location.Exterior]) &&
                       IsTrue(_matrix[(int)Location.Exterior, (int)Location.Interior]);

            return false;
        }

        /// <summary> 
        /// Returns whether the elements of this <see cref="IntersectionMatrix" />
        /// satisfies the required dimension symbols.
        /// </summary>
        /// <param name="requiredDimensionSymbols"> 
        /// Nine dimension symbols with which to
        /// compare the elements of this <see cref="IntersectionMatrix" />. Possible
        /// values are <c>{T, F, * , 0, 1, 2}</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if this <see cref="IntersectionMatrix" />
        /// matches the required dimension symbols.
        /// </returns>
        public bool Matches(string requiredDimensionSymbols)
        {
            if (requiredDimensionSymbols.Length != 9)
                throw new ArgumentException("Should be length 9: " + requiredDimensionSymbols);

            for (int ai = 0; ai < 3; ai++)
                for (int bi = 0; bi < 3; bi++)
                    if (!Matches(_matrix[ai, bi], requiredDimensionSymbols[3 * ai + bi]))
                        return false;
            return true;
        }

        /// <summary>  
        /// Transposes this IntersectionMatrix.
        /// </summary>
        /// <returns>This <see cref="IntersectionMatrix" /> as a convenience,</returns>
        public IntersectionMatrix Transpose()
        {
            Dimension temp = _matrix[1, 0];
            _matrix[1, 0] = _matrix[0, 1];
            _matrix[0, 1] = temp;

            temp = _matrix[2, 0];
            _matrix[2, 0] = _matrix[0, 2];
            _matrix[0, 2] = temp;

            temp = _matrix[2, 1];
            _matrix[2, 1] = _matrix[1, 2];
            _matrix[1, 2] = temp;

            return this;
        }

        /// <summary>
        /// Returns a nine-character <c>String</c> representation of this <see cref="IntersectionMatrix" />.
        /// </summary>
        /// <returns>
        /// The nine dimension symbols of this <see cref="IntersectionMatrix" />
        /// in row-major order.
        /// </returns>
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder("123456789");
            for (int ai = 0; ai < 3; ai++)
                for (int bi = 0; bi < 3; bi++)
                    buf[3 * ai + bi] = DimensionUtility.ToDimensionSymbol(_matrix[ai, bi]);
            return buf.ToString();
        }
    }
}
