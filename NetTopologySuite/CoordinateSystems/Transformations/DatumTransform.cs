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
using GeoAPI.DataStructures;
using NPack;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
    internal class InverseDatumTransform<TCoordinate> : DatumTransform<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InverseDatumTransform{TCoordinate}"/> class.
        /// </summary>
        protected internal InverseDatumTransform(Wgs84ConversionInfo towgs84,
                                                 ICoordinateFactory<TCoordinate> coordinateFactory,
                                                 IMatrixFactory<DoubleComponent> matrixFactory,
                                                 DatumTransform<TCoordinate> transform)
            : base(towgs84, coordinateFactory, matrixFactory)
        {
            Inverse = transform;
        }

        public override Boolean IsInverse
        {
            get { return true; }
        }
    }

    internal class DatumTransform<TCoordinate> : MathTransform<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly IMatrixFactory<DoubleComponent> _matrixFactory;
        private readonly Wgs84ConversionInfo _toWgs84;
        private readonly IAffineTransformMatrix<DoubleComponent> _transform;
        private readonly IAffineTransformMatrix<DoubleComponent> _inverseTransform;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatumTransform{TCoordinate}"/> class.
        /// </summary>
        protected internal DatumTransform(Wgs84ConversionInfo towgs84,
                                          ICoordinateFactory<TCoordinate> coordinateFactory,
                                          IMatrixFactory<DoubleComponent> matrixFactory)
            : base(null, coordinateFactory)
        {
            _toWgs84 = towgs84;
            _matrixFactory = matrixFactory;
            _transform = _toWgs84.GetAffineTransform(matrixFactory);
            _inverseTransform = _transform.Inverse;
        }

        /// <summary>
        /// Gets the dimension of input points.
        /// </summary>
        public override Int32 SourceDimension
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the dimension of output points.
        /// </summary>
        public override Int32 TargetDimension
        {
            get { throw new NotImplementedException(); }
        }

        public override ICoordinate Transform(ICoordinate coordinate)
        {
            return Transform((TCoordinate)coordinate);
        }

        public override IEnumerable<ICoordinate> Transform(IEnumerable<ICoordinate> points)
        {
            foreach (TCoordinate c in Transform(Caster.Downcast<TCoordinate, ICoordinate>(points)))
                yield return c;
        }

        public override ICoordinateSequence Transform(ICoordinateSequence points)
        {
            return Transform((ICoordinateSequence<TCoordinate>)points);
        }

        public override Boolean IsInverse
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a Well-Known Text representation of this object.
        /// </summary>
        /// <value></value>
        public override String Wkt
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets an XML representation of this object.
        /// </summary>
        /// <value></value>
        public override String Xml
        {
            get { throw new NotImplementedException(); }
        }

        protected override IMathTransform ComputeInverse(IMathTransform setAsInverse)
        {
            return new InverseDatumTransform<TCoordinate>(_toWgs84,
                                                          CoordinateFactory,
                                                          _matrixFactory,
                                                          this);
        }

        private TCoordinate applyTransformToPoint(TCoordinate p)
        {
            return CoordinateFactory.Create(_transform.TransformVector(p));

            //return CoordinateFactory.Create( new [] {
            //        _transform[0]*p[0] - _transform[3]*p[1] + _transform[2]*p[2] + _transform[4],
            //        _transform[3]*p[0] + _transform[0]*p[1] - _transform[1]*p[2] + _transform[5],
            //        -_transform[2]*p[0] + _transform[1]*p[1] + _transform[0]*p[2] + _transform[6] });,
            //    );
        }

        private TCoordinate applyInvertedTransformToPoint(TCoordinate p)
        {
            return CoordinateFactory.Create(_inverseTransform.TransformVector(p));
            //return _coordinateFactory(
            //        _transform[0]*p[0] + _transform[3]*p[1] - _transform[2]*p[2] - _transform[4],
            //        -_transform[3]*p[0] + _transform[0]*p[1] + _transform[1]*p[2] - _transform[5],
            //        _transform[2]*p[0] - _transform[1]*p[1] + _transform[0]*p[2] - _transform[6],
            //    );
        }

        /// <summary>
        /// Transforms a coordinate point. The passed parameter point should not be modified.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override TCoordinate Transform(TCoordinate point)
        {
            return !IsInverse
                       ? applyTransformToPoint(point)
                       : applyInvertedTransformToPoint(point);
        }

        /// <summary>
        /// Transforms a list of coordinate point ordinal values.
        /// </summary>
        /// <remarks>
        /// This method is provided for efficiently transforming many points. The supplied array
        /// of ordinal values will contain packed ordinal values. For example, if the source
        /// dimension is 3, then the ordinals will be packed in this order (x0,y0,z0,x1,y1,z1 ...).
        /// The size of the passed array must be an integer multiple of DimSource. The returned
        /// ordinal values are packed in a similar way. In some DCPs. the ordinals may be
        /// transformed in-place, and the returned array may be the same as the passed array.
        /// So any client code should not attempt to reuse the passed ordinal values (although
        /// they can certainly reuse the passed array). If there is any problem then the server
        /// implementation will throw an exception. If this happens then the client should not
        /// make any assumptions about the state of the ordinal values.
        /// </remarks>
        public override IEnumerable<TCoordinate> Transform(IEnumerable<TCoordinate> points)
        {
            return Caster.Downcast<TCoordinate, IVector<DoubleComponent>>(
                _transform.TransformVectors(Caster.Upcast<IVector<DoubleComponent>, TCoordinate>(points)));
        }

        public override ICoordinateSequence<TCoordinate> Transform(ICoordinateSequence<TCoordinate> points)
        {
            throw new System.NotImplementedException();
        }
    }
}