using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{        
    /// <summary> 
    /// Specifies the precision model of the <c>Coordinate</c>s in a <c>Geometry</c>.
    /// In other words, specifies the grid of allowable
    /// points for all <c>Geometry</c>s.
    /// The <c>makePrecise</c> method allows rounding a coordinate to
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
    /// <para>
    /// Floating: represents full double precision floating point.
    /// This is the default precision model used in NTS
    /// FloatingSingle: represents single precision floating point.
    /// Fixed: represents a model with a fixed number of decimal places.
    /// </para>
    /// A Fixed Precision Model is specified by a scale factor.
    /// The scale factor specifies the grid which numbers are rounded to.
    /// Input coordinates are mapped to fixed coordinates according to the following
    /// equations:
    ///  jtsPt.x = round( (inputPt.x * scale ) / scale
    ///  jtsPt.y = round( (inputPt.y * scale ) / scale
    /// Coordinates are represented internally as double-precision values.
    /// Since .NET uses the IEEE-394 floating point standard, this
    /// provides 53 bits of precision. (Thus the maximum precisely representable
    /// integer is 9,007,199,254,740,992).
    /// NTS methods currently do not handle inputs with different precision models.
    /// </summary>
    [Serializable]
    public class PrecisionModel : IPrecisionModel
    {
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
        private PrecisionModels modelType;

        /// <summary> 
        /// The scale factor which determines the number of decimal places in fixed precision.
        /// </summary>
        private double scale;

        /// <summary> 
        /// Creates a <c>PrecisionModel</c> with a default precision
        /// of Floating.
        /// </summary>
        public PrecisionModel() 
        {
            // default is floating precision
            modelType = PrecisionModels.Floating;
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
            this.modelType = modelType;

            if (modelType == PrecisionModels.Fixed)
                Scale = 1.0;            
        }

        /// <summary>  
        /// Creates a <c>PrecisionModel</c> that specifies Fixed precision.
        /// Fixed-precision coordinates are represented as precise internal coordinates,
        /// which are rounded to the grid defined by the scale factor.        
        /// </summary>
        /// <param name="scale">
        /// Amount by which to multiply a coordinate after subtracting
        /// the offset, to obtain a precise coordinate
        /// </param>
        /// <param name="offsetX">Not used.</param>
        /// <param name="offsetY">Not used.</param>
        [Obsolete("Offsets are no longer supported, since internal representation is rounded floating point")]
        public PrecisionModel(double scale, double offsetX, double offsetY) 
        {
            modelType = PrecisionModels.Fixed;
            Scale = scale;
        }

        /// <summary>  
        /// Creates a <c>PrecisionModel</c> that specifies Fixed precision.
        /// Fixed-precision coordinates are represented as precise internal coordinates,
        /// which are rounded to the grid defined by the scale factor.
        /// </summary>
        /// <param name="scale">
        /// Amount by which to multiply a coordinate after subtracting
        /// the offset, to obtain a precise coordinate.
        /// </param>  
        public PrecisionModel(double scale) 
        {
            modelType = PrecisionModels.Fixed;
            Scale = scale;
        }

        /// <summary> 
        /// Copy constructor to create a new <c>PrecisionModel</c>
        /// from an existing one.
        /// </summary>
        /// <param name="pm"></param>
        public PrecisionModel(PrecisionModel pm) 
        {
            modelType = pm.modelType;
            scale = pm.scale;
        }

        /// <summary>
        /// Return HashCode.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        /// <summary> 
        /// Tests whether the precision model supports floating point.
        /// </summary>
        /// <returns><c>true</c> if the precision model supports floating point.</returns>
        public bool IsFloating
        {
            get
            {
                return modelType == PrecisionModels.Floating || modelType == PrecisionModels.FloatingSingle;
            }
        }

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
                switch (modelType)
                {
                    case PrecisionModels.Floating:
                        return FloatingPrecisionDigits;
                    case PrecisionModels.FloatingSingle:
                        return FloatingSinglePrecisionDigits;
                    case PrecisionModels.Fixed:
                        return FixedPrecisionDigits + (int)Math.Ceiling(Math.Log(Scale) / Math.Log(10));
                    default:
                        throw new ArgumentOutOfRangeException(modelType.ToString());
                }
            }
	    }

        /// <summary>
        /// Returns the multiplying factor used to obtain a precise coordinate.
        /// This method is private because PrecisionModel is intended to
        /// be an immutable (value) type.
        /// </summary>
        /// <returns>    
        /// the amount by which to multiply a coordinate after subtracting
        /// the offset.
        /// </returns>
        public double Scale
        {
            get { return scale; }
            set { this.scale = Math.Abs(value); }
        }

        /// <summary> 
        /// Gets the type of this PrecisionModel.
        /// </summary>
        /// <returns></returns>
        public PrecisionModels GetPrecisionModelType()
        {                        
            return modelType;
        }

        /// <summary> 
        /// Gets the type of this PrecisionModel.
        /// </summary>
        /// <returns></returns>
        public PrecisionModels PrecisionModelType
        {
            get { return modelType; }            
        }

        /// <summary> 
        /// Returns the x-offset used to obtain a precise coordinate.
        /// </summary>
        /// <returns>
        /// The amount by which to subtract the x-coordinate before
        /// multiplying by the scale.
        /// </returns>
        [Obsolete("Offsets are no longer used")]
        public double OffsetX
        {
            get { return 0; }
        }

        /// <summary> 
        /// Returns the y-offset used to obtain a precise coordinate.
        /// </summary>
        /// <returns> 
        /// The amount by which to subtract the y-coordinate before
        /// multiplying by the scale
        /// </returns>
        [Obsolete("Offsets are no longer used")]
        public double OffsetY
        {
            get { return 0; }
        }

        /// <summary>  
        /// Sets <c>internal</c> to the precise representation of <c>external</c>.
        /// </summary>
        /// <param name="cexternal">The original coordinate.</param>
        /// <param name="cinternal">
        /// The coordinate whose values will be changed to the
        /// precise representation of <c>external</c>.
        /// </param>        
        [Obsolete("Use MakePrecise instead")]
        public void ToInternal(ICoordinate cexternal, ICoordinate cinternal) 
        {
            if (IsFloating) 
            {
                cinternal.X = cexternal.X;
                cinternal.Y = cexternal.Y;
            }
            else 
            {
                cinternal.X = MakePrecise(cexternal.X);
                cinternal.Y = MakePrecise(cexternal.Y);
            }
            cinternal.Z = cexternal.Z;
        }

        /// <summary>  
        /// Returns the precise representation of <c>external</c>.
        /// </summary>
        /// <param name="cexternal">The original coordinate.</param>
        /// <returns>
        /// The coordinate whose values will be changed to the precise
        /// representation of <c>external</c>
        /// </returns>
        [Obsolete("Use MakePrecise instead")]
        public ICoordinate ToInternal(ICoordinate cexternal) 
        {
            ICoordinate cinternal = new Coordinate(cexternal);
            MakePrecise( cinternal);
            return cinternal;
        }

        /// <summary>
        /// Returns the external representation of <c>internal</c>.
        /// </summary>
        /// <param name="cinternal">The original coordinate.</param>
        /// <returns>
        /// The coordinate whose values will be changed to the
        /// external representation of <c>internal</c>.
        /// </returns>
        [Obsolete("No longer needed, since internal representation is same as external representation")]
        public ICoordinate ToExternal(ICoordinate cinternal) 
        {
            ICoordinate cexternal = new Coordinate(cinternal);
            return cexternal;
        }

        /// <summary>  
        /// Sets <c>external</c> to the external representation of <c>internal</c>.
        /// </summary>
        /// <param name="cinternal">The original coordinate.</param>
        /// <param name="cexternal">
        /// The coordinate whose values will be changed to the
        /// external representation of <c>internal</c>.
        /// </param>
        [Obsolete("No longer needed, since internal representation is same as external representation")]
        public void ToExternal(ICoordinate cinternal, ICoordinate cexternal) 
        {
            cexternal.X = cinternal.X;
            cexternal.Y = cinternal.Y;
        }

        /// <summary> 
        /// Rounds a numeric value to the PrecisionModel grid.
        /// Symmetric Arithmetic Rounding is used, to provide
        /// uniform rounding behaviour no matter where the number is
        /// on the number line.
        /// </summary>
        /// <param name="val"></param>
        public double MakePrecise(double val) 
        {
  	        if (modelType == PrecisionModels.FloatingSingle)
            {                
  		        float floatSingleVal = (float) val;
  		        return (double)floatSingleVal;
  	        }  	        
            if (modelType == PrecisionModels.Fixed) 
  		        // return Math.Round(val * scale) / scale;          // Diego Guidi say's: i use the Java Round algorithm (used in JTS 1.6)
                                                                    // Java Rint method, used in JTS 1.5, was consistend with .NET Round algorithm
                return Math.Floor(((val * scale) + 0.5d)) / scale;
            return val;     // modelType == FLOATING - no rounding necessary
        }

        /// <summary> 
        /// Rounds a Coordinate to the PrecisionModel grid.
        /// </summary>
        /// <param name="coord"></param>
        public void MakePrecise(ICoordinate coord)
        {
            // optimization for full precision
            if (modelType == PrecisionModels.Floating) 
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
  	        if (modelType == PrecisionModels.Floating)
  		        description = "Floating";  	        
            else if (modelType == PrecisionModels.FloatingSingle)
  		        description = "Floating-Single";  	        
            else if (modelType == PrecisionModels.Fixed) 
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

            if (!(other is IPrecisionModel))
                return false;            

            return Equals((IPrecisionModel) other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherPrecisionModel"></param>
        /// <returns></returns>
        public bool Equals(IPrecisionModel otherPrecisionModel)
        {
            return  modelType == otherPrecisionModel.PrecisionModelType &&
                    scale == otherPrecisionModel.Scale;
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
            return CompareTo((IPrecisionModel) o);   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IPrecisionModel other)
        {
            int sigDigits = MaximumSignificantDigits;
            int otherSigDigits = other.MaximumSignificantDigits;
            return (sigDigits).CompareTo(otherSigDigits);
        }
    }
}
