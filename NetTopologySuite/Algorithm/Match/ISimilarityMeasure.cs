using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Algorithm.Match
{
    ///<summary>
    /// An interface for classes which measures the degree of similarity between two <see cref="IGeometry{TCoordinate}"/>s.
    /// The computed measure lies in the range [0, 1].
    /// Higher measures indicate a great degree of similarity.
    /// A measure of 1.0 indicates that the input geometries are identical
    /// A measure of 0.0 indicates that the geometries
    /// have essentially no similarity.
    /// The precise definition of "identical" and "no similarity" may depend on the 
    /// exact algorithm being used.
    ///</summary>
    public interface ISimilarityMeasure<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
    {

        Double Measure(IGeometry<TCoordinate> g1, IGeometry<TCoordinate> g2);
    }
}
