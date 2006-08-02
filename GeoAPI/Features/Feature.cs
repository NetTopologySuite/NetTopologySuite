using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGIS.GeoAPI.Feature
{
    public interface Feature
    {
        /**
     * Returns the description of this feature's type.
     */
    FeatureType FeatureType{ get; }

    /**
     * Returns the value of the named attribute of this {@code Feature}.
     * If the maximum cardinality of this attribute is one, then this method
     * returns the value of the attribute.  Otherwise, if the maximum
     * cardinality of this attribute is greater than one, then this method will
     * return an instance of {@link Collection}.
     *
     * @param name The name of the feature attribute to retrieve.
     *
     * @throws IllegalArgumentException If an attribute of the given name does
     *   not exist in this feature's type.
     */
    object getAttribute(string name);

    /**
     * Returns the extent of the geometries of this feature.  Can return null
     * if the extent is not known, not easy to calculate, or this feature has
     * no geometry.
     */
   // Envelope getBounds();

    /**
     * Returns the value of the indexed attribute of this {@code Feature}.
     * If the maximum cardinality of this attribute is one, then this method
     * returns the value of the attribute.  Otherwise, if the maximum
     * cardinality of this attribute is greater than one, then this method will
     * return an instance of {@link Collection}.
     *
     * @param index The index of the feature attribute to retrieve.  This index
     *   is the same as the index of the corresponding {@link FeatureAttributeDescriptor}
     *   in the list returned by {@link FeatureType#getAttributeDescriptors()}.
     *
     * @throws IndexOutOfBoundsException If the index is negative or greater than
     *   the number of possible attributes minus one.
     */
    object GetAttribute(int index);

    /**
     * Sets the value of the named attribute.  The value can either be a
     * single object, if the maximum cardinality of the given attribute is one,
     * or the value can be an instance of {@link Collection} if
     * the attribute's maximum cardinality is greater than one.
     *
     * @param name The name of the attribute whose value to set.
     * @param value The new value of the given attribute.
     *
     * @throws IllegalArgumentException If {@code value} is a collection (other than a
     *   {@linkplain Collections#singleton singleton}) and it's a single-valued attribute,
     *   or if the given name does not match any of the attributes of this feature.
     *
     * @throws ClassCastException If the attribute type is a type other than {@link Object}
     *   in the {@link FeatureType} and an incorrect type is passed in.
     */
    //void setAttribute(String name, Object value) throws IllegalArgumentException, ClassCastException;

    /**
     * Sets the value of the given attribute.  The value can either be a
     * single object, if the maximum cardinality of the given attribute is one,
     * or the value can be an instance of {@link Collection} if
     * the attribute's maximum cardinality is greater than one.
     *
     * @param index Zero based index of the attribute to set.
     * @param value The new value of the given attribute.
     *
     * @throws IndexOutOfBoundsException if the index is negative or greater than the number
     *   of attributes of this feature minute one.
     *
     * @throws IllegalArgumentException If {@code value} is a collection (other than a
     *   {@linkplain Collections#singleton singleton}) and it's a single-valued attribute.
     *
     * @throws ClassCastException If the attribute type is a type other than {@link Object}
     *   in the {@link FeatureType} and an incorrect type is passed in.
     */
    //void setAttribute(int index, Object value)
    //        throws IndexOutOfBoundsException, IllegalArgumentException, ClassCastException;

    /**
     * Returns a String that uniquely identifies this {@code Feature} instance with this
     * Java virtual machine (and perhaps uniquely in a broader scope as well).
     * This value is not necessarily one of the attributes of this feature.
     * Some features may implement this method by concatenating this feature's
     * type name with the String values of all of the primary key attributes.
     * (This is only a suggestion, however, and a given {@code Feature} implementation
     * may choose to compute the ID in whatever way makes sense.)
     */
    string ID{ get; }

    /**
     * Returns the collection in which we are contained.
     */
    //FeatureCollection getParent();
    }
}
