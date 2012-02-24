#region "License"

/*
 *  The attached / following is part of NetTopologySuite.Coordinates.Simple.
 *  
 *  NetTopologySuite.Coordinates.Simple is free software © 2009 Ingenieurgruppe IVV GmbH & Co. KG, 
 *  www.ivv-aachen.de; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/.
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  This work was derived from NetTopologySuite.Coordinates.ManagedBufferedCoordinate
 *  by codekaizen
 *  
 *  Author: Felix Obermaier 2009
 *  
 */
#endregion
using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;

namespace NetTopologySuite.Coordinates.Simple
{
    /// <summary> 
    /// Specifies the precision model of the <see cref="ICoordinate"/>s 
    /// in a <see cref="IGeometry{Coordinate}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="MakePrecise(Coordinate)"/> method allows for rounding a coordinate to a "precise" value; 
    /// that is, one whose precision is known exactly. Coordinates are assumed to be precise in 
    /// geometries; the coordinates are assumed to be rounded to the precision model given for the 
    /// geometry. NTS factory routines automatically round
    /// coordinates to the precision model before creating geometries. All internal operations 
    /// assume that coordinates are  rounded to the precision model. Generative methods 
    /// (such as boolean operations) always round computed coordinates to the appropriate 
    /// precision model. 
    /// </para>
    /// <para>
    /// Currently three types of precision model are supported:
    /// <list type="bullet">
    /// <item>
    /// <term><see cref="GeoAPI.Coordinates.PrecisionModelType.DoubleFloating"/></term>
    /// <description>
    /// Represents full double precision floating point. This is the default 
    /// precision model used in NTS.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="GeoAPI.Coordinates.PrecisionModelType.SingleFloating"/></term>
    /// <description>
    /// Represents single precision floating point. 
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="GeoAPI.Coordinates.PrecisionModelType.Fixed"/></term>
    /// <description>
    /// Represents a model with a fixed number of decimal places. A fixed
    /// <see cref="IPrecisionModel{Coordinate}"/> is specified by a scale factor. 
    /// The scale factor specifies the grid which numbers are rounded to. Input coordinates 
    /// are mapped to fixed coordinates according to the following equations 
    /// (known as arithmetic asymmetric rounding, since it moves away from zero if positive,
    /// and towards zero if negative): 
    /// <code>
    /// Double x = Math.Floor((inputPt.X * scale) + 0.5) / scale 
    /// Double y = Math.Floor((inputPt.Y * scale) + 0.5) / scale 
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// Coordinates are represented internally as a <see cref="Double"/> value. 
    /// Since the CLI uses the IEEE-754 floating point standard, this provides 53 bits of precision. 
    /// (Thus the maximum precisely representable integer is 9,007,199,254,740,992). 
    /// NTS methods currently do not handle inputs with different precision models. 
    /// </para>
    /// </remarks>
    [Serializable]
    public class PrecisionModel : IPrecisionModel<Coordinate>
    {
        private const Int32 FloatingPrecisionDigits = 16;
        private const Int32 FloatingSinglePrecisionDigits = 6;
        private const Int32 FixedPrecisionDigits = 1;

        /// <summary>  
        /// The maximum precise value representable in a Double. Since IEEE-754
        /// Double-precision numbers allow 53 bits of significand, the value is equal to
        /// 2^53 - 1.  This provides <i>almost</i> 16 decimal digits of precision.
        /// </summary>
        public const Double MaximumPreciseValue = 9007199254740992.0;

        //public static Boolean operator ==(PrecisionModel<Coordinate> left, PrecisionModel<Coordinate> right)
        //{
        //    return Equals(left, right);
        //}

        //public static Boolean operator !=(PrecisionModel<Coordinate> left, PrecisionModel<Coordinate> right)
        //{
        //    return !(left == right);
        //}

        private readonly ICoordinateFactory<Coordinate> _coordFactory;
        private readonly PrecisionModelType _modelType;
        private readonly Double _scale;

        /// <summary> 
        /// Creates a <see cref="PrecisionModel"/> with a default precision
        /// of <see cref="GeoAPI.Coordinates.PrecisionModelType.DoubleFloating"/>.
        /// </summary>
        /// <param name="coordinateFactory">
        /// The coordinate factory to use to creat coordinates.
        /// </param>
        public PrecisionModel(ICoordinateFactory<Coordinate> coordinateFactory)
            : this(coordinateFactory, PrecisionModelType.DoubleFloating) { }

        /// <summary>
        /// Creates a <see cref="PrecisionModel"/> that specifies
        /// an explicit precision model type.
        /// If the model type is Fixed the scale factor will default to 1.
        /// </summary>
        /// <param name="coordinateFactory">
        /// The coordinate factory to use to creat coordinates.
        /// </param>
        /// <param name="modelType">
        /// The type of the precision model.
        /// </param>
        public PrecisionModel(ICoordinateFactory<Coordinate> coordinateFactory,
                              PrecisionModelType modelType)
        {
            _coordFactory = coordinateFactory;
            _modelType = modelType;

            if (modelType == PrecisionModelType.Fixed)
            {
                _scale = 1.0;
            }
        }

        /// <summary>  
        /// Creates a <see cref="PrecisionModel"/> that specifies Fixed precision.
        /// </summary>
        /// <param name="coordinateFactory">
        /// The coordinate factory to use to creat coordinates.
        /// </param>
        /// <param name="scale">
        /// Amount by which to multiply a coordinate after subtracting
        /// the offset, to obtain a precise coordinate.
        /// </param>  
        /// <remarks>
        /// Fixed-precision coordinates are represented as precise internal coordinates,
        /// which are rounded to the grid defined by the scale factor.
        /// </remarks>
        public PrecisionModel(ICoordinateFactory<Coordinate> coordinateFactory,
                              Double scale)
            : this(coordinateFactory, PrecisionModelType.Fixed)
        {
            _scale = Math.Abs(scale);
        }

        /// <summary> 
        /// Copy constructor to create a new <see cref="PrecisionModel"/>
        /// from an existing one.
        /// </summary>
        /// <param name="coordinateFactory">
        /// The coordinate factory to use to creat coordinates.
        /// </param>
        /// <param name="pm">The <see cref="IPrecisionModel"/> to copy.</param>
        public PrecisionModel(ICoordinateFactory<Coordinate> coordinateFactory,
                              IPrecisionModel pm)
        {
            _coordFactory = coordinateFactory;
            _modelType = pm == null ? PrecisionModelType.DoubleFloating : pm.PrecisionModelType;
            _scale = pm == null ? 1.0 : pm.Scale;
        }

        /// <summary> 
        /// Copy constructor to create a new <see cref="PrecisionModel"/>
        /// from an existing one.
        /// </summary>
        /// <param name="pm">The precision model to copy.</param>
        public PrecisionModel(PrecisionModel pm)
        {
            _coordFactory = pm._coordFactory;
            _modelType = pm._modelType;
            _scale = pm._scale;
        }

        public ICoordinateFactory<Coordinate> CoordinateFactory
        {
            get { return _coordFactory; }
        }

        public Double Scale
        {
            get { return _scale; }
        }

        public PrecisionModelType PrecisionModelType
        {
            get { return _modelType; }
        }

        #region IPrecisionModel Members

        public Boolean IsFloating
        {
            get
            {
                return _modelType == PrecisionModelType.DoubleFloating
                    || _modelType == PrecisionModelType.SingleFloating;
            }
        }

        public Double MakePrecise(Double val)
        {
            switch (_modelType)
            {
                case PrecisionModelType.DoubleFloating:
                    return val; // modelType == FLOATING - no rounding necessary
                case PrecisionModelType.SingleFloating:
                    Single floatSingleVal = (Single)val;
                    return floatSingleVal;
                case PrecisionModelType.Fixed:
                    // return Math.Round(val * scale) / scale;         
                    // [dguidi] I implemented the Java Math.Round algorithm (used since JTS 1.6).
                    //          Java's Math.Rint method, used previous to JTS 1.6, was 
                    //          the same as the default .Net Math.Round algorithm -
                    //          "Banker's Rounding" (ASTM E-29)
                    // [codekaizen] Investigated using the symmetric rounding mode which is also available via
                    //              Math.Round(val * _scale, MidpointRounding.AwayFromZero) / scale;
                    //              however, I can't tell if symmetric would cause any more problems 
                    //              than asymmetric arithmetic rounding.
                    return Math.Floor(((val * _scale) + 0.5d)) / _scale;
                default:
                    throw new InvalidOperationException(
                            "Unknown precision model type: " + _modelType);
            }
        }

        public Int32 MaximumSignificantDigits
        {
            get
            {
                switch (_modelType)
                {
                    case PrecisionModelType.DoubleFloating:
                        return FloatingPrecisionDigits;
                    case PrecisionModelType.SingleFloating:
                        return FloatingSinglePrecisionDigits;
                    case PrecisionModelType.Fixed:
                        return FixedPrecisionDigits + (Int32)Math.Ceiling(Math.Log(Scale) / Math.Log(10));
                    default:
                        throw new InvalidOperationException(
                            "Unknown precision model type: " + _modelType);
                }
            }
        }
        #endregion

        #region IPrecisionModel<Coordinate> Members
        public Coordinate MakePrecise(Coordinate coord)
        {
            // optimization for full precision
            if (_modelType == PrecisionModelType.DoubleFloating)
            {
                return coord;
            }

            Double x = MakePrecise(coord[GeoAPI.Coordinates.Ordinates.X]);
            Double y = MakePrecise(coord[GeoAPI.Coordinates.Ordinates.Y]);

            // MD says it's OK that we're not makePrecise'ing the z [Jon Aquino]
            // TODO: codekaizen - reevaluate making Z precise for 3D

            return _coordFactory.Create(x, y);
        }
        #endregion

        public override string ToString()
        {
            switch (_modelType)
            {
                case PrecisionModelType.DoubleFloating:
                    return "Floating";
                case PrecisionModelType.SingleFloating:
                    return "Floating-Single";
                case PrecisionModelType.Fixed:
                    return "Fixed (Scale = " + Scale + ")";
                default:
                    return "Unknown";
            }
        }

        public Boolean Equals(IPrecisionModel<Coordinate> other)
        {
            if (other == null)
            {
                return false;
            }

            return _modelType == other.PrecisionModelType &&
                   _scale == other.Scale;
        }

        #region IComparable<IPrecisionModel<Coordinate>> Members

        /// <summary> 
        /// Compares this <see cref="PrecisionModel"/> object with the 
        /// specified object for order.
        /// </summary>
        /// <param name="other">
        /// The <see cref="PrecisionModel"/> with which this 
        /// <see cref="PrecisionModel"/> is being compared.
        /// </param>
        /// <remarks>
        /// A <see cref="PrecisionModel"/> is greater than another if it 
        /// provides greater precision. The comparison is based on the value returned by
        /// <see cref="MaximumSignificantDigits"/>.
        /// This comparison is not strictly accurate when comparing floating precision models
        /// to fixed models; however, it is correct when both models are either floating or fixed.
        /// </remarks>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this 
        /// <see cref="PrecisionModel"/> is less than, equal to, 
        /// or greater than the specified <see cref="PrecisionModel"/>.
        /// </returns>
        public Int32 CompareTo(IPrecisionModel<Coordinate> other)
        {
            return (this as IComparable<IPrecisionModel>).CompareTo(other);
        }

        #endregion

        #region IEquatable<IPrecisionModel> Members

        public Boolean Equals(IPrecisionModel other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Explicit IPrecisionModel Members
        ICoordinateFactory IPrecisionModel.CoordinateFactory
        {
            get { return _coordFactory; }
        }

        ICoordinate IPrecisionModel.MakePrecise(ICoordinate coord)
        {
            return MakePrecise(_coordFactory.Create(coord));
        }
        #endregion

        #region IComparable<IPrecisionModel> Members
        Int32 IComparable<IPrecisionModel>.CompareTo(IPrecisionModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            Int32 significantDigits = MaximumSignificantDigits;
            Int32 otherSignificantDigits = other.MaximumSignificantDigits;
            return (significantDigits).CompareTo(otherSignificantDigits);
        }
        #endregion
    }
}