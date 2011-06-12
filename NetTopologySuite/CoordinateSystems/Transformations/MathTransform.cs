// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// This file is part of Proj.Net.
// Proj.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Proj.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Proj.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using NPack;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
    public abstract class MathTransform : IMathTransform
    {
        private readonly List<Parameter> _parameters;
        private readonly ICoordinateFactory _coordinateFactory;
        private IMathTransform _inverse;

        protected MathTransform(IEnumerable<Parameter> parameters, 
                                ICoordinateFactory coordinateFactory)
        {
            _parameters = new List<Parameter>(parameters ?? new Parameter[0]);
            _coordinateFactory = coordinateFactory;
        }

        public Int32 ParameterCount
        {
            get { return _parameters.Count; }
        }

        #region IMathTransform Members

        public abstract Int32 SourceDimension { get; }

        public abstract Int32 TargetDimension { get; }

        public virtual Boolean IsIdentity { get { return false; } }

        public abstract Boolean IsInverse { get; }

        /// <summary>
        /// Returns the Well-Known Text for this object
        /// as defined in the simple features specification.
        /// </summary>
        public virtual String Wkt
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (IsInverse)
                {
                    sb.Append("INVERSE_MT[");
                }

                sb.AppendFormat("PARAM_MT[\"{0}\"", Name);

                foreach (Parameter parameter in Parameters)
                {
                    sb.AppendFormat(", {0}", parameter.Wkt);
                }

                sb.Append("]");

                if (IsInverse)
                {
                    sb.Append("]");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets an XML representation of this object
        /// </summary>
        public virtual String Xml
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<CT_MathTransform>");

                if (IsInverse)
                {
                    sb.AppendFormat("<CT_InverseTransform Name=\"{0}\">", ClassName);
                }
                else
                {
                    sb.AppendFormat("<CT_ParameterizedMathTransform Name=\"{0}\">", ClassName);
                }

                foreach (Parameter parameter in Parameters)
                {
                    sb.Append(parameter.Xml);
                }

                if (IsInverse)
                {
                    sb.Append("</CT_InverseTransform>");
                }
                else
                {
                    sb.Append("</CT_ParameterizedMathTransform>");
                }

                sb.Append("</CT_MathTransform>");
                return sb.ToString();
            }
        }

        protected virtual String Name
        {
            get { throw new NotImplementedException(); }
        }

        protected virtual String ClassName
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IMatrix<DoubleComponent> Derivative(ICoordinate point)
        {
            throw new TransformException("Can't compute derivative.");
        }

        public virtual IEnumerable<ICoordinate> GetCodomainConvexHull(IEnumerable<ICoordinate> points)
        {
            throw new TransformException("Can't compute co-domain convex hull.");
        }

        public virtual DomainFlags GetDomainFlags(IEnumerable<ICoordinate> points)
        {
            throw new TransformException("Can't compute domain flags for convex hull.");
        }

        public IMathTransform Inverse
        {
            get
            {
                if (_inverse == null)
                {
                    _inverse = ComputeInverse(this);
                }

                return _inverse;
            }
            protected set
            {
                if (_inverse != null)
                {
                    throw new InvalidOperationException("Inverse can only be set once.");
                }

                _inverse = value;
                //_isInverse = true;
            }
        }

        public abstract ICoordinate Transform(ICoordinate coordinate);

        public abstract IEnumerable<ICoordinate> Transform(IEnumerable<ICoordinate> points);

        public abstract ICoordinateSequence Transform(ICoordinateSequence points);

        #endregion

        protected TCoordinate CreateCoordinate<TCoordinate>(Double x, Double y, TCoordinate coordinate)
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
        {
            ICoordinateFactory<TCoordinate> factory = CoordinateFactory as ICoordinateFactory<TCoordinate>;
            if ( factory == null)
                throw new Exception("Where does this coordinate factory come from?");

            switch(coordinate.ComponentCount)
            {
                case 2:
                    return factory.Create(x, y);
                case 3:
                    return !coordinate.ContainsOrdinate(Ordinates.Z)
                               ?
                                   factory.Create(x, y, coordinate[Ordinates.W])
                               :
                                   factory.Create3D(x, y, coordinate[Ordinates.Z]);
                case 4:
                    return factory.Create3D(x, y, coordinate[Ordinates.Z], coordinate[Ordinates.W]);
                default:
                    throw new InvalidDataException("coordinate");
            }
        }

        protected ICoordinate CreateCoordinate(Double x, Double y)
        {
            return _coordinateFactory.Create(x, y);
        }

        protected ICoordinate CreateCoordinate(Double x, Double y, Double w)
        {
            return _coordinateFactory.Create(x, y, w);
        }

        protected ICoordinate CreateCoordinate3D(Double x, Double y, Double z)
        {
            return _coordinateFactory.Create3D(x, y, z);
        }

        protected ICoordinate CreateCoordinate3D(Double x, Double y, Double z, Double w)
        {
            return _coordinateFactory.Create3D(x, y, z, w);
        }

        protected ICoordinateFactory CoordinateFactory
        {
            get { return _coordinateFactory; }
        }

        protected IEnumerable<Parameter> Parameters
        {
            get
            {
                foreach (Parameter parameter in _parameters)
                {
                    yield return parameter;
                }
            }
        }

        /// <summary>
        /// Gets the parameter at the given index.
        /// </summary>
        /// <param name="index">Index of parameter.</param>
        /// <returns>The parameter at the given index.</returns>
        protected Parameter GetParameterInternal(Int32 index)
        {
            if (index < 0 || index >= _parameters.Count)
            {
                throw new ArgumentOutOfRangeException("index", 
                                                      index,
                                                      "Parameter index out of range.");
            }

            return _parameters[index];
        }

        /// <summary>
        /// Gets an named parameter of the projection.
        /// </summary>
        /// <remarks>The parameter name is case insensitive</remarks>
        /// <param name="name">Name of parameter</param>
        /// <returns>parameter or null if not found</returns>
        protected Parameter GetParameterInternal(String name)
        {
            foreach (Parameter parameter in Parameters)
            {
                if (parameter.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return parameter;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the inverse of the concrete transformation.
        /// </summary>
        /// <returns>
        /// The <see cref="IMathTransform"/> that is the inverse of the transformation, if one exists.
        /// </returns>
        protected abstract IMathTransform ComputeInverse(IMathTransform setAsInverse);
    }
}