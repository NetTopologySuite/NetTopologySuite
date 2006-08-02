using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenGIS.GeoAPI.Util
{
    /// <summary>
    /// A <see cref="string" /> that has been internationalized into several <see cref="CultureInfo">culture</see>.
    /// This interface is used as a replacement for the <see cref="string" /> type whenever an attribute needs to be
    /// internationalization capable.
    /// The <see cref="IComparable">natural ordering</see>} is defined by the <see cref="string.CompareTo">lexicographical ordering</see>
    /// of strings in the default culture, as returned by <see cref="ToString" />.
    /// This string also defines the character sequence for this object.
    /// </summary>
    public interface InternationalString: IComparable
    {
        /// <summary>
        /// Returns this string in the given culture. If no string is available in the given culture,
        /// then some default culture is used. The default culture is implementation-dependent. It
        /// may or may not be the <see cref="CultureInfo.CurrentCulture" /> system default.        
        /// </summary>    
        /// <param name="cultureInfo">
        /// The desired culture for the string to be returned, or <c>null</c>
        /// for a string in the implementation default culture.
        /// </param>
        /// <returns>
        /// The string in the given culture if available, 
        /// or in the default culture otherwise.
        /// </returns>
        string ToString(CultureInfo cultureInfo);

        /// <summary>
        /// <para>
        /// Returns this string in the default culture. The default culture is implementation-dependent.
        /// It may or may not be the <see cref="CultureInfo.CurrentCulture">system default</see>. If the default
        /// culture is the <see cref="CultureInfo.CurrentCulture">system default</see> (a recommended practice),
        /// then invoking this method is equivalent to invoking
        /// <c>ToString(CultureInfo.CurrentCulture)</c>.
        /// </para>
        /// <returns>The string in the default locale.</returns>
        string ToString();
    }
}
