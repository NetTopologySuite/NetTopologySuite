using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Specifies the precision model of the <c>Coordinate</c>s in a <c>Geometry</c>.
    /// In other words, specifies the grid of allowable
    /// points for all <c>Geometry</c>s.
    /// </summary>
    /// <remarks>
    /// The <c>MakePrecise</c> method allows rounding a coordinate to
    /// a "precise" value; that is, one whose
    /// precision is known exactly.
    /// Coordinates are assumed to be precise in geometries.
    /// That is, the coordinates are assumed to be rounded to the
    /// precision model given for the point.
    /// NTS input routines automatically round coordinates to the precision model
    /// before creating Geometries.
    /// All internal operations
    /// assume that coordinates are rounded to the precision model.
    /// Constructive methods (such as bool operations) always round computed
    /// coordinates to the appropriate precision model.
    /// Currently three types of precision model are supported:
    /// <list type="table">
    /// <item><term>Floating</term><description>Represents full double precision floating point.
    /// This is the default precision model used in NTS</description></item>
    /// <item><term>FloatingSingle</term><description>Represents single precision floating point</description></item>
    /// <item><term>Fixed</term><description>Represents a model with a fixed number of decimal places.A Fixed Precision Model is specified by a scale factor.
    /// The scale factor specifies the size of the grid which numbers are rounded to.</description></item>
    /// </list>
    /// Input coordinates are mapped to fixed coordinates according to the following
    /// equations:
    /// <list type="bullet">
    /// <item><description><c>jtsPt.X = Math.Round( inputPt.X * scale, MidPointRounding.AwayFromZero ) / scale )</c></description></item>
    /// <item><description><c>jtsPt.Y = Math.Round( inputPt.Y * scale, MidPointRounding.AwayFromZero ) / scale )</c></description></item>
    /// </list>
    /// <para>
    /// For example, to specify 3 decimal places of precision, use a scale factor
    /// of 1000. To specify -3 decimal places of precision (i.e. rounding to
    /// the nearest 1000), use a scale factor of 0.001.
    /// </para>
    /// Coordinates are represented internally as Java double-precision values.
    /// Since .NET uses the IEEE-394 floating point standard, this
    /// provides 53 bits of precision. (Thus the maximum precisely representable
    /// <i>integer</i> is 9,007,199,254,740,992 - or almost 16 decimal digits of precision).
    /// <para/>
    /// NTS binary methods currently do not handle inputs which have different precision models.
    /// The precision model of any constructed geometric value is undefined.
    /// </remarks>
    [Serializable]
    public class PrecisionModel: IComparable, IComparable<PrecisionModel>
    {
        /// <summary>
        /// Determines which of two <see cref="PrecisionModel"/>s is the most precise
        /// </summary>
        /// <param name="pm1">A precision model</param>
        /// <param name="pm2">A precision model</param>
        /// <returns>The PrecisionModel which is most precise</returns>
        public static PrecisionModel MostPrecise(PrecisionModel pm1, PrecisionModel pm2)
        {
            if (pm1.CompareTo(pm2) >= 0)
                return pm1;
            return pm2;
        }

        private const int FloatingPrecisionDigits = 16;
        private const int FloatingSinglePrecisionDigits = 6;
        private const int FixedPrecisionDigits = 1;

        /// <summary>
        /// The maximum precise value representable in a double. Since IEE754
        /// double-precision numbers allow 53 bits of mantissa, the value is equal to
        /// 2^53 - 1.  This provides <i>almost</i> 16 decimal digits of precision.
        /// </summary>
        public const double MaximumPreciseValue = 9007199254740992.0;

        /// <summary>
        /// The type of PrecisionModel this represents.
        /// </summary>
        private readonly PrecisionModels _modelType;

        /// <summary>
        /// The scale factor which determines the number of decimal places in fixed precision.
        /// </summary>
        private readonly double _scale;

        /// <summary>
        /// Gets a value indicating a precision model with double precision.
        /// </summary>
        /// <returns>A double precision model</returns>
        public static Lazy<PrecisionModel> Floating =>
            new Lazy<PrecisionModel>(() => new PrecisionModel(PrecisionModels.Floating));

        /// <summary>
        /// Gets a value indicating a precision model with single precision.
        /// </summary>
        /// <returns>A single precision model</returns>
        public static Lazy<PrecisionModel> FloatingSingle =>
            new Lazy<PrecisionModel>(() => new PrecisionModel(PrecisionModels.FloatingSingle));

        /// <summary>
        /// Gets a value indicating a precision model with a scale of <c>1</c>.
        /// </summary>
        /// <returns>A fixed precision model</returns>
        public static Lazy<PrecisionModel> Fixed =>
            new Lazy<PrecisionModel>(() => new PrecisionModel(PrecisionModels.Fixed));


        /// <summary>
        /// Creates a <c>PrecisionModel</c> with a default precision
        /// of Floating.
        /// </summary>
        public PrecisionModel()
        {
            // default is floating precision
            _modelType = PrecisionModels.Floating;
        }

        /// <summary>
        /// Creates a <c>PrecisionModel</c> that specifies
        /// an explicit precision model type.
        /// If the model type is Fixed the scale factor will default to 1.
        /// </summary>
        /// <param name="modelType">
        /// The type of the precision model.
        /// </param>
        public PrecisionModel(PrecisionModels modelType)
        {
            _modelType = modelType;

            if (modelType == PrecisionModels.Fixed)
                _scale = 1.0;
        }

        /// <summary>
        /// Creates a <c>PrecisionModel</c> that specifies Fixed precision.
        /// Fixed-precision coordinates are represented as precise internal coordinates,
        /// which are rounded to the grid defined by the scale factor.
        /// </summary>
        /// <param name="scale">
        /// Amount by which to multiply a coordinate, to obtain a precise coordinate.
        /// </param>
        public PrecisionModel(double scale)
        {
            _modelType = PrecisionModels.Fixed;
            _scale = scale;
        }

        /// <summary>
        /// Copy constructor to create a new <c>PrecisionModel</c>
        /// from an existing one.
        /// </summary>
        /// <param name="pm"></param>
        public PrecisionModel(PrecisionModel pm)
        {
            _modelType = pm._modelType;
            _scale = pm._scale;
        }


        /// <summary>
        /// Tests whether the precision model supports floating point.
        /// </summary>
        /// <returns><c>true</c> if the precision model supports floating point.</returns>
        public bool IsFloating => _modelType == PrecisionModels.Floating || _modelType == PrecisionModels.FloatingSingle;

        /// <summary>
        /// Returns the maximum number of significant digits provided by this
        /// precision model.
        /// Intended for use by routines which need to print out precise values.
        /// </summary>
        /// <returns>
        /// The maximum number of decimal places provided by this precision model.
        /// </returns>
        public int MaximumSignificantDigits
        {
            get
            {
                switch (_modelType)
                {
                    case PrecisionModels.Floating:
                        return FloatingPrecisionDigits;
                    case PrecisionModels.FloatingSingle:
                        return FloatingSinglePrecisionDigits;
                    case PrecisionModels.Fixed:
                        return FixedPrecisionDigits + (int)Math.Ceiling(Math.Log(Scale) / Math.Log(10));
                    default:
                        throw new ArgumentOutOfRangeException(_modelType.ToString());
                }
            }
        }

        /// <summary>
        /// Returns the scale factor used to specify a fixed precision model.
        /// </summary>
        /// <remarks>
        /// The number of decimal places of precision is
        /// equal to the base-10 logarithm of the scale factor.
        /// Non-integral and negative scale factors are supported.
        /// Negative scale factors indicate that the places
        /// of precision is to the left of the decimal point.
        /// </remarks>
        /// <returns>
        /// The scale factor for the fixed precision model
        /// </returns>
        public double Scale
        {
            get => _scale;
            set => throw new NotSupportedException();
        }

        ///// <summary>
        ///// Gets the type of this PrecisionModel.
        ///// </summary>
        ///// <returns></returns>
        //public PrecisionModels GetPrecisionModelType()
        //{
        //    return _modelType;
        //}

        /// <summary>
        /// Gets the type of this PrecisionModel.
        /// </summary>
        /// <returns></returns>
        public PrecisionModels PrecisionModelType => _modelType;

        /// <summary>
        /// Rounds a numeric value to the PrecisionModel grid.
        /// Symmetric Arithmetic Rounding is used, to provide
        /// uniform rounding behaviour no matter where the number is
        /// on the number line.
        /// </summary>
        /// <remarks>
        /// This method has no effect on NaN values
        /// </remarks>
        /// <param name="val"></param>
        public double MakePrecise(double val)
        {
            // don't change NaN values
            if (double.IsNaN(val)) return val;

            if (_modelType == PrecisionModels.FloatingSingle)
            {
                float floatSingleVal = (float)val;
                return floatSingleVal;
            }

            if (_modelType == PrecisionModels.Fixed)
            {
                /*.Net's default rounding algorithm is "Bankers Rounding" which turned
                 * out to be no good for JTS/NTS geometry operations */
                // return Math.Round(val * scale) / scale;

                // This is "Asymmetric Arithmetic Rounding"
                // http://en.wikipedia.org/wiki/Rounding#Round_half_up
                return Math.Floor(val * _scale + 0.5d) / _scale;
            }
            return val;     // modelType == FLOATING - no rounding necessary
        }

        /// <summary>
        /// Rounds a Coordinate to the PrecisionModel grid.
        /// </summary>
        /// <param name="coord"></param>
        public void MakePrecise(Coordinate coord)
        {
            // optimization for full precision
            if (_modelType == PrecisionModels.Floating)
                return;

            coord.X = MakePrecise(coord.X);
            coord.Y = MakePrecise(coord.Y);
            //MD says it's OK that we're not makePrecise'ing the z [Jon Aquino]
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string description = "UNKNOWN";
            if (_modelType == PrecisionModels.Floating)
                description = "Floating";
            else if (_modelType == PrecisionModels.FloatingSingle)
                description = "Floating-Single";
            else if (_modelType == PrecisionModels.Fixed)
                description = "Fixed (Scale=" + Scale + ")";
            return description;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (!(other is PrecisionModel))
                return false;

            return Equals((PrecisionModel)other);
        }


        /// <inheritdoc cref="object.GetHashCode()"/>>
        public override int GetHashCode()
        {
            const int prime = 31;
            int result = prime + _modelType.GetHashCode();
            long temp = BitConverter.DoubleToInt64Bits(_scale);
            result = prime * result + (int)(temp ^ (temp >> 32));
            return result;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="otherPrecisionModel"></param>
        /// <returns></returns>
        public bool Equals(PrecisionModel otherPrecisionModel)
        {
            return _modelType == otherPrecisionModel.PrecisionModelType &&
                    _scale == otherPrecisionModel.Scale;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(PrecisionModel obj1, PrecisionModel obj2)
        {
            return Equals(obj1, obj2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(PrecisionModel obj1, PrecisionModel obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// Compares this <c>PrecisionModel</c> object with the specified object for order.
        /// A PrecisionModel is greater than another if it provides greater precision.
        /// The comparison is based on the value returned by the
        /// {getMaximumSignificantDigits) method.
        /// This comparison is not strictly accurate when comparing floating precision models
        /// to fixed models; however, it is correct when both models are either floating or fixed.
        /// </summary>
        /// <param name="o">
        /// The <c>PrecisionModel</c> with which this <c>PrecisionModel</c>
        /// is being compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>PrecisionModel</c>
        /// is less than, equal to, or greater than the specified <c>PrecisionModel</c>.
        /// </returns>
        public int CompareTo(object o)
        {
            return CompareTo((PrecisionModel)o);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(PrecisionModel other)
        {
            int sigDigits = MaximumSignificantDigits;
            int otherSigDigits = other.MaximumSignificantDigits;
            return (sigDigits).CompareTo(otherSigDigits);
        }
    }
}
