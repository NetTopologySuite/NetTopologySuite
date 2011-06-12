// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
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
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{

    public class InverseCoordinateTransformation<TCoordinate> : CoordinateTransformation<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        internal InverseCoordinateTransformation(CoordinateTransformation<TCoordinate> transform)
            : base(transform.Target, 
                   transform.Source, 
                   transform.TransformType, 
                   transform.MathTransform.Inverse, 
                   transform.Name, 
                   transform.Authority, 
                   transform.AuthorityCode, 
                   transform.AreaOfUse, 
                   transform.Remarks) { }
    }

    /// <summary>
    /// Describes a coordinate transformation. This class only describes a 
    /// coordinate transformation, it does not actually perform the transform 
    /// operation on points. To transform points you must use an instance of
    /// an <see cref="IMathTransform{TCoordinate}"/>.
    /// </summary>
    public class CoordinateTransformation<TCoordinate> : ICoordinateTransformation<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly ICoordinateSystem<TCoordinate> _source;
        private readonly ICoordinateSystem<TCoordinate> _target;
        private readonly TransformType _transformType;
        private readonly String _areaOfUse;
        private readonly String _authority;
        private readonly String _authorityCode;
        private readonly IMathTransform<TCoordinate> _mathTransform;
        private readonly String _name;
        private readonly String _remarks;
        private ICoordinateTransformation<TCoordinate> _inverse;

        /// <summary>
        /// Initializes an instance of a CoordinateTransformation
        /// </summary>
        /// <param name="source">Source coordinate system</param>
        /// <param name="target">Target coordinate system</param>
        /// <param name="transformType">Transformation type</param>
        /// <param name="mathTransform">Math transform</param>
        /// <param name="name">Name of transform</param>
        /// <param name="authority">Authority</param>
        /// <param name="authorityCode">Authority code</param>
        /// <param name="areaOfUse">Area of use</param>
        /// <param name="remarks">Remarks</param>
        internal CoordinateTransformation(ICoordinateSystem<TCoordinate> source,
                                          ICoordinateSystem<TCoordinate> target, 
                                          TransformType transformType,
                                          IMathTransform<TCoordinate> mathTransform, 
                                          String name, 
                                          String authority,
                                          String authorityCode, 
                                          String areaOfUse, 
                                          String remarks)
        {
            _target = target;
            _source = source;
            _transformType = transformType;
            _mathTransform = mathTransform;
            _name = name;
            _authority = authority;
            _authorityCode = authorityCode;
            _areaOfUse = areaOfUse;
            _remarks = remarks;
        }

        #region ICoordinateTransformation Members
		
        public String AreaOfUse
        {
            get { return _areaOfUse; }
        }

        public String Authority
        {
            get { return _authority; }
        }

        public String AuthorityCode
        {
            get { return _authorityCode; }
        }

        public IMathTransform<TCoordinate> MathTransform
        {
            get { return _mathTransform; }
        }

        public String Name
        {
            get { return _name; }
        }

        public String Remarks
        {
            get { return _remarks; }
        }

        public ICoordinateSystem<TCoordinate> Source
        {
            get { return _source; }
        }

        public ICoordinateSystem<TCoordinate> Target
        {
            get { return _target; }
        }

        public ICoordinateTransformation<TCoordinate> Inverse
        {
            get
            {
                if (_inverse == null)
                {
                    _inverse = new InverseCoordinateTransformation<TCoordinate>(this);
                }

                return _inverse;
            }
        }

        public IExtents<TCoordinate> Transform(IExtents<TCoordinate> extents, 
                                               IGeometryFactory<TCoordinate> factory)
        {
            TCoordinate min = MathTransform.Transform(extents.Min);
            TCoordinate max = MathTransform.Transform(extents.Max);
            return factory.CreateExtents(min, max);
        }

        public IGeometry<TCoordinate> Transform(IGeometry<TCoordinate> geometry, 
                                                IGeometryFactory<TCoordinate> factory)
        {
            ICoordinateSequence<TCoordinate> coordinates = MathTransform.Transform(geometry.Coordinates);

            IGeometry<TCoordinate> result = factory.CreateGeometry(coordinates, geometry.GeometryType);
            return result;
        }

        public IPoint<TCoordinate> Transform(IPoint<TCoordinate> point, 
                                             IGeometryFactory<TCoordinate> factory)
        {
            TCoordinate coordinate = MathTransform.Transform(point.Coordinate);
            return factory.CreatePoint(coordinate);
        }

        public IExtents<TCoordinate> InverseTransform(IExtents<TCoordinate> extents, IGeometryFactory<TCoordinate> factory)
        {
            return Inverse.Transform(extents, factory);
        }

        public IGeometry<TCoordinate> InverseTransform(IGeometry<TCoordinate> geometry, IGeometryFactory<TCoordinate> factory)
        {
            return Inverse.Transform(geometry, factory);
        }

        public IPoint<TCoordinate> InverseTransform(IPoint<TCoordinate> point, IGeometryFactory<TCoordinate> factory)
        {
            return Inverse.Transform(point, factory);
        }

        public TransformType TransformType
        {
            get { return _transformType; }
        }

        ICoordinateTransformation ICoordinateTransformation.Inverse
        {
            get { return Inverse; }
        }

        #endregion

        #region Explicit ICoordinateTransformation Members

        IMathTransform ICoordinateTransformation.MathTransform
        {
            get { return MathTransform; }
        }

        ICoordinateSystem ICoordinateTransformation.Source
        {
            get { return Source; }
        }

        ICoordinateSystem ICoordinateTransformation.Target
        {
            get { return Target; }
        }

        IExtents ICoordinateTransformation.Transform(IExtents extents, IGeometryFactory factory)
        {
            ICoordinate min = MathTransform.Transform(extents.Min);
            ICoordinate max = MathTransform.Transform(extents.Max);
            return factory.CreateExtents(min, max);
        }

        IGeometry ICoordinateTransformation.Transform(IGeometry geometry, IGeometryFactory factory)
        {
            if(geometry.GeometryType == OgcGeometryType.GeometryCollection)
            {
                return factory.CreateGeometryCollection(TransformImpl((IEnumerable<IGeometry>) geometry, factory));
            }

            return TransformImpl(geometry, factory);
        }

        IPoint ICoordinateTransformation.Transform(IPoint point, IGeometryFactory factory)
        {
            ICoordinate coordinate = MathTransform.Transform(point.Coordinate);
            return factory.CreatePoint(coordinate);
        }

        public IExtents InverseTransform(IExtents extents, IGeometryFactory factory)
        {
            return Inverse.Transform(extents, factory);
        }

        public IGeometry InverseTransform(IGeometry geometry, IGeometryFactory factory)
        {
            return Inverse.Transform(geometry, factory);
        }

        public IPoint InverseTransform(IPoint point, IGeometryFactory factory)
        {
            return Inverse.Transform(point, factory);
        }

        #endregion

        private IEnumerable<IGeometry> TransformImpl(IEnumerable<IGeometry> geometries, IGeometryFactory factory)
        {
            foreach (var geometry in geometries)
            {
                yield return TransformImpl(geometry, factory);
            }
        }

        private IGeometry TransformImpl(IGeometry geometry, IGeometryFactory factory)
        {
            ICoordinateSequence coordinates = MathTransform.Transform(geometry.Coordinates);

            IGeometry result = factory.CreateGeometry(coordinates, geometry.GeometryType);
            return result;
        }
    }
}