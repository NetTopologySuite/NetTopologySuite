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
using GeoAPI.CoordinateSystems.Transformations;
using NPack.Interfaces;

#if DOTNET35
using Caster = System.Linq.Enumerable;
using Enumerable = System.Linq.Enumerable;
using Processor = System.Linq.Enumerable;
#else
using Caster = GeoAPI.DataStructures.Caster;
using Enumerable = GeoAPI.DataStructures.Enumerable;
using Processor = GeoAPI.DataStructures.Processor;
#endif

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
    internal class ConcatenatedTransform<TCoordinate> : MathTransform<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly List<ICoordinateTransformation<TCoordinate>> _transforms;
        private readonly Boolean _isInverse;

        public ConcatenatedTransform(IEnumerable<ICoordinateTransformation<TCoordinate>> transforms,
                                     ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(null, coordinateFactory)
        {
            _transforms = new List<ICoordinateTransformation<TCoordinate>>();
            foreach (ICoordinateTransformation<TCoordinate> transform in transforms)
            {
                if ( transform != null ) _transforms.Add(transform);
            }
        }

        public ConcatenatedTransform(IEnumerable<ICoordinateTransformation<TCoordinate>> transforms,
                                     ICoordinateFactory<TCoordinate> coordinateFactory,
                                     IMathTransform<TCoordinate> inverse)
            : this(transforms, coordinateFactory)
        {
            if (inverse == null) throw new ArgumentNullException("inverse");

            Invert();
            Inverse = inverse;
            _isInverse = true;
        }

        public IEnumerable<ICoordinateTransformation<TCoordinate>> Transforms
        {
            get { return _transforms; }
        }

        /// <summary>
        /// Transforms a coordinate.
        /// </summary>
        public override TCoordinate Transform(TCoordinate point)
        {
            foreach (ICoordinateTransformation<TCoordinate> ct in _transforms)
            {
                point = ct.MathTransform.Transform(point);
            }

            return point;
        }

        /// <summary>
        /// Transforms a set of coordinates.
        /// </summary>
        public override IEnumerable<TCoordinate> Transform(IEnumerable<TCoordinate> points)
        {
            IEnumerable<TCoordinate> transformed = points;

            foreach (ICoordinateTransformation<TCoordinate> ct in _transforms)
            {
                transformed = ct.MathTransform.Transform(transformed);
            }

            return transformed;
        }

        public override ICoordinateSequence<TCoordinate> Transform(ICoordinateSequence<TCoordinate> points)
        {
            ICoordinateSequence<TCoordinate> ret = points.CoordinateSequenceFactory.Create( points.Dimension );

            foreach ( TCoordinate c in points )
                ret.Add( c );

            return ret;
        }

        /// <summary>
        /// Returns the inverse of this conversion.
        /// </summary>
        /// <returns>
        /// <see cref="IMathTransform"/> that is the reverse of the current conversion.
        /// </returns>
        protected override IMathTransform ComputeInverse(IMathTransform setAsInverse)
        {
            IMathTransform<TCoordinate> typedInverse = setAsInverse as IMathTransform<TCoordinate>;
            IMathTransform inverse = new ConcatenatedTransform<TCoordinate>(_transforms,
                                                                            CoordinateFactory,
                                                                            typedInverse);
            return inverse;
        }

        /// <summary>
        /// Reverses the transformation
        /// </summary>
        private void Invert()
        {
            List<ICoordinateTransformation<TCoordinate>> reversed =
                new List<ICoordinateTransformation<TCoordinate>>(_transforms);

            _transforms.Clear();
            reversed.Reverse();

            foreach (ICoordinateTransformation<TCoordinate> transformation in reversed)
            {
                 
                ICoordinateTransformation<TCoordinate> inverse =
                    transformation.Inverse;
                _transforms.Add(inverse);
            }
        }

        public override Int32 SourceDimension
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Int32 TargetDimension
        {
            get { throw new System.NotImplementedException(); }
        }

        public override ICoordinate Transform(ICoordinate coordinate)
        {
            foreach (ICoordinateTransformation<TCoordinate> ct in _transforms)
            {
                coordinate = ct.MathTransform.Transform(coordinate);
            }
            return coordinate;
        }

        public override IEnumerable<ICoordinate> Transform(IEnumerable<ICoordinate> points)
        {
            foreach (ICoordinate c in points)
                yield return Transform(c);
        }

        public override ICoordinateSequence Transform(ICoordinateSequence points)
        {
            ICoordinateSequence ret = points.CoordinateSequenceFactory.Create( points.Dimension );

            foreach (ICoordinate c in points)
                ret.Add(Transform(c));

            return ret;
        }

        public override Boolean IsInverse
        {
            get { return _isInverse; }
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
    }
}
