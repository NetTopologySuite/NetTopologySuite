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
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
    /// <summary>
    /// The <see cref="GeographicTransform{TCoordinate}"/> class is 
    /// implemented on geographic transformation objects and implements 
    /// datum transformations between geographic coordinate systems.
    /// </summary>
    public class GeographicTransform<TCoordinate> : MathTransform<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private IGeographicCoordinateSystem<TCoordinate> _source;
        private IGeographicCoordinateSystem<TCoordinate> _target;

        internal GeographicTransform(IGeographicCoordinateSystem<TCoordinate> source,
                                     IGeographicCoordinateSystem<TCoordinate> target,
                                     ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(null, coordinateFactory)
        {
            _source = source;
            _target = target;
        }

        #region IGeographicTransform Members

        /// <summary>
        /// Gets or sets the source geographic coordinate system for the transformation.
        /// </summary>
        public IGeographicCoordinateSystem<TCoordinate> Source
        {
            get { return _source; }
            set { _source = value; }
        }

        /// <summary>
        /// Gets or sets the target geographic coordinate system for the transformation.
        /// </summary>
        public IGeographicCoordinateSystem<TCoordinate> Target
        {
            get { return _target; }
            set { _target = value; }
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
            return Transform((TCoordinate) coordinate);
            //throw new System.NotImplementedException();
        }

        public override IEnumerable<ICoordinate> Transform(IEnumerable<ICoordinate> points)
        {
            throw new System.NotImplementedException();
        }

        public override ICoordinateSequence Transform(ICoordinateSequence points)
        {
            throw new System.NotImplementedException();
        }

        public override Boolean IsInverse
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// Returns the Well-Known Text for this object
        /// as defined in the simple features specification. [NOT IMPLEMENTED].
        /// </summary>
        public override String Wkt
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets an XML representation of this object [NOT IMPLEMENTED].
        /// </summary>
        public override String Xml
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        protected override IMathTransform ComputeInverse(IMathTransform setAsInverse)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms a coordinate point. 
        /// The passed parameter point should not be modified.
        /// </summary>
        public override TCoordinate Transform(TCoordinate point)
        {
            Double value = point[Ordinates.X];

            value /= Source.AngularUnit.RadiansPerUnit;
            value -= Source.PrimeMeridian.Longitude / Source.PrimeMeridian.AngularUnit.RadiansPerUnit;
            value += Target.PrimeMeridian.Longitude / Target.PrimeMeridian.AngularUnit.RadiansPerUnit;
            value *= Source.AngularUnit.RadiansPerUnit;

            //Int32 componentCount = point.ComponentCount;
            //Double[] coordValues = new Double[componentCount];
            //coordValues[0] = value;

            return point.ComponentCount == 2
                       ? CreateCoordinate(value, point[Ordinates.Y])
                       : CreateCoordinate(value, point[Ordinates.Y], point[Ordinates.Z]);
        }

        /// <summary>
        /// Transforms a list of coordinates.
        /// </summary>
        public override IEnumerable<TCoordinate> Transform(IEnumerable<TCoordinate> points)
        {
            foreach (TCoordinate point in points)
            {
                yield return Transform(point);
            }
        }

        public override ICoordinateSequence<TCoordinate> Transform(ICoordinateSequence<TCoordinate> points)
        {
            throw new System.NotImplementedException();
        }
    }
}