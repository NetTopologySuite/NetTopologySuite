using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary> 
    /// Specifies the precision model of the <see cref="ICoordinate"/>s 
    /// in a <see cref="Geometry{TCoordinate}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In other words, specifies the grid of allowable
    /// points for all <see cref="Geometry{TCoordinate}"/>s.
    /// The <see cref="MakePrecise"/> method allows rounding a coordinate to
    /// a "precise" value; that is, one whose precision is known exactly.
    /// Coordinates are assumed to be precise in geometries.
    /// That is, the coordinates are assumed to be rounded to the
    /// precision model given for the point.
    /// NTS input routines automatically round coordinates to the precision model
    /// before creating Geometries.
    /// All internal operations assume that coordinates are rounded to the precision model.
    /// Constructive methods (such as Boolean operations) always round computed
    /// coordinates to the appropriate precision model.
    /// </para>
    /// <para>
    /// Currently three types of precision model are supported:
    /// </para>
    /// <para>
    /// Floating: represents full Double precision floating point.
    /// This is the default precision model used in NTS
    /// FloatingSingle: represents single precision floating point.
    /// Fixed: represents a model with a fixed number of decimal places.
    /// </para>
    /// <para>
    /// A Fixed Precision Model is specified by a scale factor.
    /// The scale factor specifies the grid which numbers are rounded to.
    /// Input coordinates are mapped to fixed coordinates according to the following
    /// equations:
    /// <code>
    /// ntsPoint.X = Math.Round( (inputPoint.X * Scale ) / Scale;
    /// ntsPoint.Y = Math.Round( (inputPoint.Y * Scale ) / Scale;
    /// </code>
    /// Coordinates are represented internally as Double-precision values.
    /// Since .NET uses the IEEE-394 floating point standard, this
    /// provides 53 bits of precision. (Thus the maximum precisely representable
    /// integer is 9,007,199,254,740,992).
    /// NTS methods currently do not handle inputs with different precision models.
    /// </para>
    /// </remarks>
    [Serializable]
    public class PrecisionModel<TCoordinate> : IPrecisionModel<TCoordinate>, IEquatable<PrecisionModel<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private const Int32 FloatingPrecisionDigits = 16;
        private const Int32 FloatingSinglePrecisionDigits = 6;
        private const Int32 FixedPrecisionDigits = 1;

        /// <summary>  
        /// The maximum precise value representable in a Double. Since IEE754
        /// Double-precision numbers allow 53 bits of significand, the value is equal to
        /// 2^53 - 1.  This provides <i>almost</i> 16 decimal digits of precision.
        /// </summary>
        public const Double MaximumPreciseValue = 9007199254740992.0;

        private readonly PrecisionModelType _modelType;
        private readonly Double _scale;

        /// <summary> 
        /// Creates a <see cref="PrecisionModel{TCoordinate}"/> with a default precision
        /// of <see cref="PrecisionModelType.Floating"/>.
        /// </summary>
        public PrecisionModel()
        {
            // default is floating precision
            _modelType = PrecisionModelType.Floating;
        }

        /// <summary>
        /// Creates a <see cref="PrecisionModel{TCoordinate}"/> that specifies
        /// an explicit precision model type.
        /// If the model type is Fixed the scale factor will default to 1.
        /// </summary>
        /// <param name="modelType">
        /// The type of the precision model.
        /// </param>
        public PrecisionModel(PrecisionModelType modelType)
        {
            _modelType = modelType;

            if (modelType == PrecisionModelType.Fixed)
            {
                _scale = 1.0;
            }
        }

        /// <summary>  
        /// Creates a <see cref="PrecisionModel{TCoordinate}"/> that specifies Fixed precision.
        /// </summary>
        /// <param name="scale">
        /// Amount by which to multiply a coordinate after subtracting
        /// the offset, to obtain a precise coordinate.
        /// </param>  
        /// <remarks>
        /// Fixed-precision coordinates are represented as precise internal coordinates,
        /// which are rounded to the grid defined by the scale factor.
        /// </remarks>
        public PrecisionModel(Double scale)
        {
            _modelType = PrecisionModelType.Fixed;
            _scale = Math.Abs(scale);
        }

        /// <summary> 
        /// Copy constructor to create a new <see cref="PrecisionModel{TCoordinate}"/>
        /// from an existing one.
        /// </summary>
        /// <param name="pm">The precision model to copy.</param>
        public PrecisionModel(PrecisionModel<TCoordinate> pm)
        {
            _modelType = pm._modelType;
            _scale = pm._scale;
        }

        /// <summary> 
        /// Gets or sets the scale factor which determines the number of 
        /// decimal places in fixed precision.
        /// </summary>
        /// <value>    
        /// The amount by which to multiply a coordinate after subtracting
        /// the offset.
        /// </value>
        public Double Scale
        {
            get { return _scale; }
        }

        /// <summary> 
        /// Gets the type of this <see cref="PrecisionModel{TCoordinate}"/>.
        /// </summary>
        public PrecisionModelType PrecisionModelType
        {
            get
            {
                return _modelType;
            }
        }

        #region IPrecisionModel Members

        /// <summary> 
        /// Tests whether the precision model supports floating point.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the precision model supports floating point.
        /// </returns>
        public Boolean IsFloating
        {
            get
            {
                return _modelType == PrecisionModelType.Floating
                    || _modelType == PrecisionModelType.FloatingSingle;
            }
        }

        public ICoordinate MakePrecise(ICoordinate coord)
        {
            throw new NotImplementedException();
        }

        /// <summary> 
        /// Rounds a numeric value to the <see cref="PrecisionModel{TCoordinate}"/> 
        /// grid. Symmetric Arithmetic Rounding is used, to provide
        /// uniform rounding behavior no matter where the number is
        /// on the number line.
        /// </summary>
        /// <param name="val">
        /// The value to make precise according to the 
        /// <see cref="PrecisionModel{TCoordinate}"/>.
        /// </param>
        public Double MakePrecise(Double val)
        {
            if (_modelType == PrecisionModelType.FloatingSingle)
            {
                float floatSingleVal = (float)val;
                return floatSingleVal;
            }

            if (_modelType == PrecisionModelType.Fixed)
            {
                // return Math.Round(val * scale) / scale;         
                // Diego Guidi say's: i use the Java Round algorithm (used in JTS 1.6)
                // Java Rint method, used in JTS 1.5, was consistend with .NET Round algorithm
                return Math.Floor(((val * _scale) + 0.5d)) / _scale;
            }

            return val; // modelType == FLOATING - no rounding necessary
        }

        /// <summary>
        /// Returns the maximum number of significant digits provided by this
        /// precision model.
        /// Intended for use by routines which need to print out precise values.
        /// </summary>
        /// <returns>
        /// The maximum number of decimal places provided by this precision model.
        /// </returns>
        public Int32 MaximumSignificantDigits
        {
            get
            {
                switch (_modelType)
                {
                    case PrecisionModelType.Floating:
                        return FloatingPrecisionDigits;
                    case PrecisionModelType.FloatingSingle:
                        return FloatingSinglePrecisionDigits;
                    case PrecisionModelType.Fixed:
                        return FixedPrecisionDigits + (Int32)Math.Ceiling(Math.Log(Scale) / Math.Log(10));
                    default:
                        throw new ArgumentOutOfRangeException(_modelType.ToString());
                }
            }
        }
        #endregion

        #region IPrecisionModel<TCoordinate> Members
        /// <summary> 
        /// Rounds a <typeparamref name="TCoordinate"/> to the 
        /// <see cref="PrecisionModel{TCoordinate}"/> grid.
        /// </summary>
        /// <param name="coord">
        /// The coordinate to make precise according to the precision model.
        /// </param>
        public TCoordinate MakePrecise(TCoordinate coord)
        {
            // optimization for full precision
            if (_modelType == PrecisionModelType.Floating)
            {
                return coord;
            }

            Double x = MakePrecise(coord[Ordinates.X]);
            Double y = MakePrecise(coord[Ordinates.Y]);

            // MD says it's OK that we're not makePrecise'ing the z [Jon Aquino]
            // TODO: codekaizen - reevaluate making Z precise for 3D

            return Coordinates<TCoordinate>.DefaultCoordinateFactory.Create(x, y);
        }
        #endregion

        public override string ToString()
        {
            string description = "UNKNOWN";

            if (_modelType == PrecisionModelType.Floating)
            {
                description = "Floating";
            }
            else if (_modelType == PrecisionModelType.FloatingSingle)
            {
                description = "Floating-Single";
            }
            else if (_modelType == PrecisionModelType.Fixed)
            {
                description = "Fixed (Scale=" + Scale + ")";
            }

            return description;
        }

        public override Boolean Equals(object other)
        {
            return Equals(other as PrecisionModel<TCoordinate>);
        }

        public Boolean Equals(IPrecisionModel<TCoordinate> other)
        {
            return Equals(other as PrecisionModel<TCoordinate>);
        }

        public Boolean Equals(PrecisionModel<TCoordinate> other)
        {
            if (other == null)
            {
                return false;
            }

            return _modelType == other._modelType &&
                   _scale == other._scale;
        }

        public static Boolean operator ==(PrecisionModel<TCoordinate> left, PrecisionModel<TCoordinate> right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(PrecisionModel<TCoordinate> left, PrecisionModel<TCoordinate> right)
        {
            return !(left == right);
        }

        /// <summary> 
        /// Compares this <see cref="PrecisionModel{TCoordinate}"/> object with the 
        /// specified object for order.
        /// </summary>
        /// <param name="o">
        /// The <see cref="PrecisionModel{TCoordinate}"/> with which this 
        /// <see cref="PrecisionModel{TCoordinate}"/> is being compared.
        /// </param>
        /// <remarks>
        /// A <see cref="PrecisionModel{TCoordinate}"/> is greater than another if it 
        /// provides greater precision. The comparison is based on the value returned by the
        /// {getMaximumSignificantDigits) method.
        /// This comparison is not strictly accurate when comparing floating precision models
        /// to fixed models; however, it is correct when both models are either floating or fixed.
        /// </remarks>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this 
        /// <see cref="PrecisionModel{TCoordinate}"/> is less than, equal to, 
        /// or greater than the specified <see cref="PrecisionModel{TCoordinate}"/>.
        /// </returns>
        public Int32 CompareTo(object o)
        {
            return CompareTo(o as IPrecisionModel);
        }

        public Int32 CompareTo(IPrecisionModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            Int32 significantDigits = MaximumSignificantDigits;
            Int32 otherSignificantDigits = other.MaximumSignificantDigits;
            return (significantDigits).CompareTo(otherSignificantDigits);
        }

        #region IComparable<IPrecisionModel<TCoordinate>> Members

        public Int32 CompareTo(IPrecisionModel<TCoordinate> other)
        {
            return CompareTo((IPrecisionModel)other);
        }

        #endregion

        #region IEquatable<IPrecisionModel> Members

        public Boolean Equals(IPrecisionModel other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}