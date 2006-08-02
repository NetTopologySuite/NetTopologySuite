using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGIS.GeoAPI.Util
{
    /// <summary>
    /// <para>
    /// Base interface for <see cref="ScopedName">generic scoped</see> and
    /// <see cref="LocalName">local name</see> structure for type and attribute
    /// name in the context of name spaces.
    /// </para>
    /// <para>
    /// The natural ordering for generic names is implementation dependent.
    /// A recommended practice is to compare lexicographically each
    /// element in the list of parsed names. Specific attributes of
    /// the name, such as how it treats case, may affect the ordering. In general, two names of
    /// different classes may not be compared. 
    /// </para>
    /// </summary>
    public interface GenericName: IComparable
    {
        /// <summary>
        /// Returns the scope (name space) of this generic name. If this name has no scope
        /// (e.g. is the root), then this method returns <c>null</c>.
        /// </summary>
        /// <value></value>                
        GenericName Scope { get; }

        /// <summary>
        /// Returns the sequence of <see cref="LocalName">local names</see> making this generic name.
        /// Each element in this list is like a directory name in a file path name.
        /// The length of this sequence is the generic name depth. 
        /// </summary>
        /// <returns></returns>
        IList<GenericName> GetParsedNames();

        /// <summary>
        /// Returns a view of this object as a scoped name,
        /// or <c>null</c> if this name has no scope.
        /// </summary>
        /// <returns></returns>
        ScopedName AsScopedName();

        /// <summary>
        /// Returns a view of this object as a local name. The local name returned by this method
        /// will have the same <see cref="LocalName.Scope">scope</see> than this generic name.
        /// </summary>
        /// <returns></returns>
        LocalName AsLocalName();

        /// <summary>
        /// Returns a string representation of this generic name. This string representation
        /// is local-independant. It contains all elements listed by <see cref="GetParsedNames"/>
        /// separated by an arbitrary character (usually {<c>:</c>} or {<c>/</c>}).
        /// This rule implies that the <see cref="ToString()" /> method for a
        /// <see cref="ScopedName">scoped name</see> will contains the scope, while the
        /// <see cref="ToString()" /> method for the <see cref="LocalName">local version</see> of
        /// the same name will not contains the scope.
        /// </summary>
        string ToString();

        /// <summary>
        /// Returns a local-dependent string representation of this generic name. This string
        /// is similar to the one returned by <see cref="ToString()" /> except that each element has
        /// been localized in the <see cref="InternationalString.ToString(CultureInfo)">specified culture info</see>
        /// . If no international string is available, then this method should
        /// returns an implementation mapping to <see cref="ToString()" /> for all locales.
        /// </summary>
        InternationalString ToInternationalString();
    }
}
