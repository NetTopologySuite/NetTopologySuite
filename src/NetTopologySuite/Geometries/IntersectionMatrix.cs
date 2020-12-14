using System;
using System.Text;

namespace NetTopologySuite.Geometries
{
    /// <summary>  
    /// Models a <b>Dimensionally Extended Nine-Intersection Model (DE-9IM)</b> matrix. 
    /// </summary>
    /// <remarks>
    /// DE-9IM matrix values (such as "212FF1FF2")
    /// specify the topological relationship between two <see cref="Geometry"/>s. 
    /// This class can also represent matrix patterns (such as "T*T******")
    /// which are used for matching instances of DE-9IM matrices.
    /// <para/>
    /// DE-9IM matrices are 3x3 matrices with integer entries.
    /// The matrix indices {0,1,2}
    /// represent the topological locations
    /// that occur in a geometry(Interior, Boundary, Exterior).
    /// These are provided by the constants
    /// <see cref="Location.Interior"/>, <see cref="Location.Boundary"/>, and <see cref="Location.Exterior"/>.
    /// <para/>
    /// When used to specify the topological relationship between two geometries,
    /// the matrix entries represent the possible dimensions of each intersection:
    /// <see cref="Dimension.A"/> = 2, <see cref="Dimension.L"/> = 1, <see cref="Dimension.P"/> = 0 and <see cref="Dimension.False"/> = -1.<br/>
    /// When used to represent a matrix pattern entries can have the additional values
    /// <see cref="Dimension.True"/> ("T") and <see cref="Dimension.Dontcare"/> ("*"). 
    /// <para/>
    /// For a description of the DE-9IM and the spatial predicates derived from it, see the following references:
    /// <list type="bullet">
    /// <item><description><i><see href="http://portal.opengeospatial.org/files/?artifact_id=829">OGC 99-049 OpenGIS Simple Features Specification for SQL.</see></i></description></item>
    /// <item><description>
    ///     <i><a href="http://portal.opengeospatial.org/files/?artifact_id=25355">OGC 06-103r4 OpenGIS Implementation Standard for Geographic information - Simple feature access - Part 1: Common architecture</a></i>, 
    ///     Section 6.1.15 (which provides some further details on certain predicate specifications).</description></item>
    /// <item><description>Wikipedia article on <a href="https://en.wikipedia.org/wiki/DE-9IM">DE-9IM</a></description></item>
    /// </list>
    /// <para/>
    /// Methods are provided to:
    /// <list type="bullet">
    /// <item><description>set and query the elements of the matrix in a convenient fashion</description></item>
    /// <item><description>convert to and from the standard string representation(specified in SFS Section 2.1.13.2).</description></item>
    /// <item><description>test if a matrix matches a given pattern string.</description></item>
    /// <item><description>test if a matrix(possibly with geometry dimensions) matches a standard named spatial predicate</description></item>
    /// </list>
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
            var m = new IntersectionMatrix(actualDimensionSymbols);
            return m.Matches(requiredDimensionSymbols);
        }

        /// <summary>
        /// Changes the value of one of this <see cref="IntersectionMatrix" /> elements.
        /// </summary>
        /// <param name="row">
        /// The row of this <see cref="IntersectionMatrix" />,
        /// indicating the interior, boundary or exterior of the first <see cref="Geometry"/>
        /// </param>
        /// <param name="column">
        /// The column of this <see cref="IntersectionMatrix" />,
        /// indicating the interior, boundary or exterior of the second <see cref="Geometry"/>
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
        /// indicating the interior, boundary or exterior of the first <see cref="Geometry"/>.
        /// </param>
        /// <param name="column">
        /// The column of this <see cref="IntersectionMatrix" />, 
        /// indicating the interior, boundary or exterior of the second <see cref="Geometry"/>.
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
        /// the interior, boundary or exterior of the first <see cref="Geometry"/>.
        /// </param>
        /// <param name="column">
        /// The column of this <see cref="IntersectionMatrix" />,
        /// indicating the interior, boundary or exterior of the second <see cref="Geometry"/>.
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
        /// Tests if this matrix matches <c>[FF*FF****]</c>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the two <see cref="Geometry"/>'s related by
        /// this matrix are disjoint.
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
        /// Tests if <c>isDisjoint</c> returns <see langword="false"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the two <see cref="Geometry"/>'s related by
        /// this matrix intersect.
        /// </returns>
        public bool IsIntersects()
        {
            return !IsDisjoint();
        }

        /// <summary>
        /// Tests if this matrix matches
        /// <c>[FT*******]</c>, <c>[F**T*****]</c> or <c>[F***T****]</c>.
        /// </summary>
        /// <param name="dimensionOfGeometryA">The dimension of the first <see cref="Geometry"/>.</param>
        /// <param name="dimensionOfGeometryB">The dimension of the second <see cref="Geometry"/>.</param>
        /// <returns>
        /// <c>true</c> if the two <see cref="Geometry"/>
        /// s related by this matrix touch; Returns false
        /// if both <see cref="Geometry"/>s are points.
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
        /// Tests whether this geometry crosses the
        /// specified geometry.
        /// </summary>
        /// <remarks>
        /// The <c>crosses</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The geometries have some but not all interior points in common.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches</description></item>
        ///   <list type="bullet">
        ///     <item><description><c>[T*T******]</c> (for P/L, P/A, and L/A situations)</description></item>
        ///     <item><description><c>[T*****T**]</c> (for L/P, L/A, and A/L situations)</description></item>
        ///     <item><description><c>[0********]</c> (for L/L situations)</description></item>
        ///   </list>
        /// </list>
        /// For any other combination of dimensions this predicate returns <c>false</c>.
        /// <para/>
        /// The SFS defined this predicate only for P/L, P/A, L/L, and L/A situations.
        /// JTS extends the definition to apply to L/P, A/P and A/L situations as well.
        /// This makes the relation symmetric.
        /// </remarks>
        /// <param name="dimensionOfGeometryA">The dimension of the first <see cref="Geometry"/>.</param>
        /// <param name="dimensionOfGeometryB">The dimension of the second <see cref="Geometry"/>.</param>
        /// <returns>
        /// <c>true</c> if the two <see cref="Geometry"/>
        /// s related by this matrix cross. 
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
        /// Tests whether this matrix matches <c>[T*F**F***]</c>.
        /// </summary>
        /// <returns><c>true</c> if the first <see cref="Geometry"/> is within the second.</returns>
        public bool IsWithin()
        {
            return IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior]) &&
                   _matrix[(int)Location.Interior, (int)Location.Exterior] == Dimension.False &&
                   _matrix[(int)Location.Boundary, (int)Location.Exterior] == Dimension.False;
        }

        /// <summary> 
        /// Tests whether this matrix matches <c>[T*****FF*]</c>
        /// </summary>
        /// <returns><c>true</c> if the first <see cref="Geometry"/> contains the second.</returns>
        public bool IsContains()
        {
            return IsTrue(_matrix[(int)Location.Interior, (int)Location.Interior]) &&
                   _matrix[(int)Location.Exterior, (int)Location.Interior] == Dimension.False &&
                   _matrix[(int)Location.Exterior, (int)Location.Boundary] == Dimension.False;
        }

        /// <summary>
        /// Tests if this matrix matches
        ///    <c>[T*****FF*]</c>
        /// or <c>[*T****FF*]</c>
        /// or <c>[***T**FF*]</c>
        /// or <c>[****T*FF*]</c>.
        /// </summary>
        /// <returns><c>true</c> if the first <see cref="Geometry"/> covers the second</returns>
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
        /// Tests if this matrix matches
        ///    <c>[T*F**F***]</c>
        /// or <c>[*TF**F***]</c>
        /// or <c>[**FT*F***]</c>
        /// or <c>[**F*TF***]</c>.
        /// </summary>
        /// <returns><c>true</c> if the first <see cref="Geometry"/> is covered by the second</returns>
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
        /// this matrix matches the pattern <c>[T*F**FFF*]</c>.
        /// <para/>
        /// <b>Note:</b> This pattern differs from the one stated in 
        /// <i>Simple feature access - Part 1: Common architecture</i>.
        /// That document states the pattern as <c>[TFFFTFFFT]</c>.  This would
        /// specify that
        /// two identical <tt>POINT</tt>s are not equal, which is not desirable behaviour.
        /// The pattern used here has been corrected to compute equality in this situation.
        /// </summary>
        /// <param name="dimensionOfGeometryA">The dimension of the first <see cref="Geometry"/>.</param>
        /// <param name="dimensionOfGeometryB">The dimension of the second <see cref="Geometry"/>.</param>
        /// <returns>
        /// <c>true</c> if the two <see cref="Geometry"/>s
        /// related by this matrix are equal; the
        /// <see cref="Geometry"/>s must have the same dimension to be equal.
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
        /// Tests if this matrix matches
        /// <list type="bullet">
        /// <item><description><c>[T*T***T**]</c> (for two points or two surfaces)</description></item>
        /// <item><description><c>[1*T***T**]</c> (for two curves)</description></item>
        /// </list>
        /// </summary>
        /// <param name="dimensionOfGeometryA">The dimension of the first <see cref="Geometry"/>.</param>
        /// <param name="dimensionOfGeometryB">The dimension of the second <see cref="Geometry"/>.</param>
        /// <returns>
        /// <c>true</c> if the two <see cref="Geometry"/>
        /// s related by this matrix overlap. For this
        /// function to return <c>true</c>, the <see cref="Geometry"/>s must
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
        /// Tests whether this matrix matches the given matrix pattern
        /// </summary>
        /// <param name="pattern"> 
        /// A pattern containing nine dimension symbols with which to
        /// compare the entries of this matrix.Possible
        /// symbol values are <c>{ T, F, * , 0, 1, 2}</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if this <see cref="IntersectionMatrix" />
        /// matches the required dimension symbols.
        /// </returns>
        public bool Matches(string pattern)
        {
            if (pattern.Length != 9)
                throw new ArgumentException("Should be length 9: " + pattern);

            for (int ai = 0; ai < 3; ai++)
                for (int bi = 0; bi < 3; bi++)
                    if (!Matches(_matrix[ai, bi], pattern[3 * ai + bi]))
                        return false;
            return true;
        }

        /// <summary>  
        /// Transposes this IntersectionMatrix.
        /// </summary>
        /// <returns>This <see cref="IntersectionMatrix" /> as a convenience,</returns>
        public IntersectionMatrix Transpose()
        {
            var temp = _matrix[1, 0];
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
            var buf = new StringBuilder("123456789");
            for (int ai = 0; ai < 3; ai++)
                for (int bi = 0; bi < 3; bi++)
                    buf[3 * ai + bi] = DimensionUtility.ToDimensionSymbol(_matrix[ai, bi]);
            return buf.ToString();
        }
    }
}
