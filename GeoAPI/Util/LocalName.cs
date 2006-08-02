using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGIS.GeoAPI.Util
{
    /// <summary>
    /// Identifier within a name space for a local object. This could be the target object of the
    /// <see cref="GenericName" />, or a pointer to another name space (with a new <see cref="GenericName" />)
    /// one step closer to the target of the identifier.
    /// </summary>
    public interface LocalName : GenericName
    {
        /// <summary>
        /// Returns the scope (name space) of this generic name. This method returns the same
        /// value than the one returned by the <see cref="ScopedName">scoped</see> version of this
        /// name.
        /// </summary> 
        /*
         * In other words, the following relation shall be respected:
         * <blockquote><table border='0'><tr>
         *   <td nowrap>{@link ScopedName#asLocalName}</td>
         *   <td nowrap>{@code .getScope() ==}</td>
         *   <td nowrap align="right">{@link ScopedName}</td>
         *   <td nowrap>{@code .getScope()}</td>
         * </tr><tr>
         *   <td align="center"><font size=2>(a locale name)</font></td>
         *   <td>&nbsp;</td>
         *   <td align="center"><font size=2>(a scoped name)</font></td>
         *   <td>&nbsp;</td>
         * </tr></table></blockquote>
         */
        new GenericName Scope { get; }

        /// <summary>
        /// Returns the sequence of local name for this <see cref="GenericName">generic name</see>.
        /// Since this object is itself a locale name, this method always returns a singleton
        /// containing only <c>this</c>.
        /// </summary>
        new IList<GenericName> GetParsedNames();

        /// <summary>
        /// Returns a view of this object as a local name. Some implementations may
        /// returns <c>this</c> since this object is already a local name.
        /// </summary>
        new LocalName AsLocalName();

        /// <summary>
        /// Returns a locale-independant string representation of this local name.
        /// This string do not includes the scope, which is consistent with the
        /// <see cref="GetParsedNames">parsed names< definition.
        /// </summary>
        new string ToString();
    }
}
