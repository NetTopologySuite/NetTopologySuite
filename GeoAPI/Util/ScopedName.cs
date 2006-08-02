using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGIS.GeoAPI.Util
{
    /// <summary>
    /// Fully qualified identifier for an object.
    /// A <see cref="ScopedName" /> contains a <see cref="LocalName" /> as <see cref="AsLocalName" /> head
    /// and a <see cref="GenericName" />, which may be a <see cref="LocalName" /> or an other
    /// <see cref="ScopedName" />, as <see cref="Scope" /> tail.
    /// </summary>
    public interface ScopedName : GenericName
    { 
        /// <summary>
        /// Returns the scope of this name.
        /// </summary>
        new GenericName Scope { get; }

        /// <summary>
        /// Returns a view of this object as a local name. This is the last element in the
        /// sequence of <see cref="GetParsedNames">parsed names</see>. The local name returned
        /// by this method will still have the same <see cref="LocalName.Scope">scope</see>
        /// than this scoped name. Note however that the string returned by
        /// <see cref="LocalName.ToString" /> will differs.
        /// </summary>
        new LocalName AsLocalName();

        /// <summary>
        /// Returns a locale-independent string representation of this name, including its scope.
        /// </summary>
        new string ToString();
    }
}
