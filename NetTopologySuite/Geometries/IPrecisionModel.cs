using System;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public enum PrecisionModels
    {
        /// <summary> 
        /// Floating precision corresponds to the standard 
        /// double-precision floating-point representation, which is
        /// based on the IEEE-754 standard
        /// </summary>
        Floating = 0,

        /// <summary>
        /// Floating single precision corresponds to the standard
        /// single-precision floating-point representation, which is
        /// based on the IEEE-754 standard
        /// </summary>
        FloatingSingle = 1,

        /// <summary> 
        /// Fixed Precision indicates that coordinates have a fixed number of decimal places.
        /// The number of decimal places is determined by the log10 of the scale factor.
        /// </summary>
        Fixed = 2,
    }

    /// <summary>
    /// Interface for classes specifying the precision model of the <c>Coordinate</c>s in a <c>IGeometry</c>.
    /// In other words, specifies the grid of allowable points for all <c>IGeometry</c>s.
    /// </summary>
    public interface IPrecisionModel : IComparable, IComparable<IPrecisionModel>
    {
        /// <summary>
        /// Gets a value indicating the <see cref="PrecisionModels">precision model</see> type
        /// </summary>
        PrecisionModels PrecisionModelType { get; }
        
        /// <summary>
        /// Gets a value indicating if this precision model has floating precision
        /// </summary>
        bool IsFloating { get; }

        /// <summary>
        /// Gets a value indicating the maximum precision digits
        /// </summary>
        int MaximumSignificantDigits { get; }
        
        /// <summary>
        /// Gets a value indicating the scale factor of a fixed precision model
        /// </summary>
        /// <remarks>
        /// The number of decimal places of precision is
        /// equal to the base-10 logarithm of the scale factor.
        /// Non-integral and negative scale factors are supported.
        /// Negative scale factors indicate that the places
        /// of precision is to the left of the decimal point.
        /// </remarks>
        double Scale { get; }

        /// <summary>
        /// Function to compute a precised value of <paramref name="val"/>
        /// </summary>
        /// <param name="val">The value to precise</param>
        /// <returns>The precised value</returns>
        double MakePrecise(double val);

        /// <summary>
        /// Method to precise <paramref name="coord"/>.
        /// </summary>
        /// <param name="coord">The coordinate to precise</param>
        void MakePrecise(Coordinate coord);
    }
}
