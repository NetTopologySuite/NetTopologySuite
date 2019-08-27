using System;
using System.Collections;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// CoordinateCompare is used in the sorting of arrays of Coordinate objects.
    /// Implements a lexicographic comparison.
    /// </summary>
    public class CoordinateCompare : IComparer
    {
        /*
        /// <summary>
        /// Constructor.
        /// </summary>
        public CoordinateCompare() { }
        */
        /// <summary>
        /// Compares two object and returns a value indicating whether one is less than, equal to or greater
        /// than the other.
        /// </summary>
        /// <param name="x">First Coordinate object to compare.</param>
        /// <param name="y">Second Coordinate object to compare.</param>
        /// <returns>
        ///&lt;table cellspacing="0" class="dtTABLE"&gt;
        ///&lt;TR VALIGN="top"&gt;
        ///    &lt;TH width=50%&gt;Value&lt;/TH&gt;
        ///&lt;TH width=50%&gt;Condition&lt;/TH&gt;
        ///&lt;/TR&gt;
        ///&lt;TR VALIGN="top"&gt;
        ///    &lt;TD width=50%&gt;Less than zero&lt;/TD&gt;
        ///&lt;TD width=50%&gt;&lt;I&gt;a&lt;/I&gt; is less than &lt;I&gt;b&lt;/I&gt;.&lt;/TD&gt;
        ///&lt;/TR&gt;
        ///&lt;TR VALIGN="top"&gt;
        ///    &lt;TD width=50%&gt;Zero&lt;/TD&gt;
        ///&lt;TD width=50%&gt;&lt;I&gt;a&lt;/I&gt; equals &lt;I&gt;b&lt;/I&gt;.&lt;/TD&gt;
        ///&lt;/TR&gt;
        ///&lt;TR VALIGN="top"&gt;
        ///    &lt;TD width=50%&gt;Greater than zero&lt;/TD&gt;
        ///&lt;TD width=50%&gt;&lt;I&gt;a&lt;/I&gt; is greater than &lt;I&gt;b&lt;/I&gt;.&lt;/TD&gt;
        ///&lt;/TR&gt;
        ///&lt;/table&gt;
        /// </returns>
        /// <remarks>If a implements IComparable, then a. CompareTo (b) is returned; otherwise, if b
        /// implements IComparable, then b. CompareTo (a) is returned.
        /// Comparing a null reference (Nothing in Visual Basic) with any type is allowed and does not
        /// generate an exception when using IComparable. When sorting, a null reference (Nothing) is
        /// considered to be less than any other object.
        /// </remarks>
        public int Compare(object x, object y)
        {
            int returnValue;
            if (x is Coordinate && y is  Coordinate)
            {

                var coord1 = (Coordinate) x;
                var coord2 = (Coordinate) y;
                if (coord1.X < coord2.X)
                    returnValue = -1;
                else if (coord1.X > coord2.X)
                    returnValue = 1;
                else if (coord1.Y < coord2.Y)
                    returnValue = -1;
                else if (coord1.Y > coord2.Y)
                    returnValue = 1;
                else returnValue = 0;
            }
            else throw new ArgumentException("Wrong arguments type: Coordinate expected");
            return returnValue;
        }
    }
}
