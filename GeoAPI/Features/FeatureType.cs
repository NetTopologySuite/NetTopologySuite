using System;
using System.Collections.Generic;
using System.Text;

using OpenGIS.GeoAPI.Util;

namespace OpenGIS.GeoAPI.Feature
{
    /// <summary>
    /// <para>
    /// Describes a feature in an application namespace. A <see cref="FeatureType" />
    /// represents features as an object that contains zero or more attribute
    /// objects, one of which will generally be a geometry, but no geometry and
    /// multiple geometries are allowed. Note that instances of this class are
    /// henceforth referred to as schemas.
    /// </para>
    /// <para>
    /// With one exception, the type of an attribute is considered to be its cannonical
    /// definition by the <see cref="FeatureType" />. For example, an attribute type might be
    /// a <c>Sequence</c> object, which contains a float public
    /// field called <c>PPQ</c>. The fact that this attribute exists is not known by the
    /// <see cref="FeatureType" /> itself. If a caller asks this <see cref="FeatureType" /> for all of
    /// its attributes, the <see cref="FeatureType" /> will tell the caller that it has an attribute
    /// of type <c>Sequence</c>, but not that this attribute has a sub-attribute
    /// (field) called <c>PPQ</c>. It is the responsibility of the callers to understand
    /// the objects it is asking for and manipulate them appropriately. The sole exception
    /// is if the type stored in the <see cref="FeatureType" /> is a <see cref="Feature" /> type. In this
    /// case, all information about sub-attributes are stored and passed to calling classes
    /// upon request. The style of reference (XPath) is defined in and mediated by
    /// <see cref="FeatureType" /> implementations.
    /// </para>
    /// <para>
    /// It is the responsibility of the implementing class to ensure that the <see cref="FeatureType" />
    /// is always in a valid state. This means that each attribute tuple must be fully initialized
    /// and valid. The minimum valid <see cref="FeatureType" /> is one with nulls for namespace, type, and
    /// attributes; this is clearly a trivial case, since it is so constrained that it would not allow
    /// for any feature construction. 
    /// </para>
    /// <para>
    /// There are a few conventions of which implementers of this interface
    /// must be aware in order to successfully manage a <see cref="FeatureType" />:
    /// <ul>
    ///   <li><p><b>Immutability</b><br>
    ///       <em>Feature types must be implemented as immutable objects!</em></p></li>
    ///
    ///   <li><p><b>XPath</b><br>
    ///       XPath is the standard used to access all attributes (flat, nested, and multiple),
    ///       via a single, unified string. Using XPath to access attributes has the convenient
    ///       side-benefit of making them appear to be non-nested and non-multiple to callers with
    ///       no awareness of XPath. This greatly simplifies accessing and manipulating data. However,
    ///       it does put extra burden on the implementers of <see cref="FeatureType" /> to understand and
    ///       correctly implement XPath pointers. Note that the {@link Feature} object does not
    ///       understand XPath at all and relies on implementors of this interface to interpret
    ///       XPath references. Fortunately, XPath is quite simple and has a clearly written
    ///       <a href="http://www.w3.org/TR/xpath">specification</a>.</p></li>
    ///
    ///   <li><p><b>Feature Creation</b><br>
    ///       <see cref="FeatureType" /> also must provide methods for the creation of {@link Feature}s.
    ///       The creating <see cref="FeatureType" /> should check to see if the passed in objects validate
    ///       against its attribute types, and if it does should return a new feature.</p></li>
    /// </ul>
    /// </para>
    /// </summary>
    public interface FeatureType
    {
        /// <summary>
        /// Returns the name of this <see cref="FeatureType" />, including any namespace prefix.
        /// <para>
        /// The typical usage of these <see cref="GenericName" />s will be as follows:  In most
        /// cases, the <see cref="GenericName" /> will have a local part that is be the name of the
        /// XML element used to encode such features as GML.  The scope of the name
        /// will either be null (if the XML element is to have no namespace) or will
        /// be a <see cref="LocalName" /> whose <see cref="LocalName.ToString">toString()</see> gives
        /// the URI of an XML namespace.
        /// </para>
        /// </summary>
        GenericName getName();

        /// <summary>
        /// In cases where features are to be encoded as GML, the namespace portion
        /// of the name of the type must be mapped to a prefix in an "xmlns" XML
        /// attribute.  If the data provider desires to do so, he may return a
        /// prefix from this method to indicate a preference for this mapping.  It
        /// is also valid to return null, indicating that the data provider doesn't
        /// know or doesn't care.
        /// </summary>
        string PreferredPrefix { get; }

        /// <summary>
        /// Returns a list of descriptors that lists all of the attributes that
        /// a <see cref="Feature" /> of this type will have.
        /// </summary>
        System.Collections.IList AttributeDescriptors { get; }

        /// <summary>
        /// Returns the descriptor of the shape that should be used for "default"
        /// drawing of features of this type.  This may only be null when features of
        /// this type do not have geometric attributes.
        /// </summary>
        FeatureAttributeDescriptor DefaultShapeAttribute { get; }

        /// <summary>
        /// Returns true if features of this type can be cast to <see cref="FeatureCollection" />.
        /// </summary>
        bool IsCollectionType { get; }

        /// <summary>
        /// Returns the <see cref="FeatureType" />s that could potentially be child <see cref="FeatureType" />s of
        /// this feature type.  Each returned element may in turn have child 
        /// <c>FeatureType</c> instances of its own.
        /// </summary>
        System.Collections.IList ChildTypes { get; }

        /// <summary>
        /// <para>
        /// Returns a new, unpopulated instance of this type of <see cref="Feature" />.
        /// When the object is returned, all of its attributes are null.
        /// </para>
        /// <para>
        /// The returned object will be an instance of <see cref="FeatureCollection" />
        /// if and only if the <see cref="IsCollectionType" />} method returns <c>true</c>.
        /// </para>
        /// <para>
        /// The <see cref="Feature" /> that is returned is not persisted or displayed
        /// until the caller takes further action, such as adding the feature to a
        /// collection that is backed by a data store.
        /// </para>
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// If this feature type does not support the creation of new instances.
        /// </exception>
        Feature CreateFeature();
    }
}
