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
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
    public abstract class GeoMathTransform<TCoordinate> : MathTransform<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly Double _e;
        private readonly Double _e2;
        private readonly Double _semiMajor;
        private readonly Double _reciprocalSemiMajor;
        private readonly Double _semiMinor;

        protected GeoMathTransform(IEnumerable<Parameter> parameters,
                                   ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {

            Parameter semiMajorParam = null, semiMinorParam = null;

            foreach (Parameter p in parameters)
            {
                String name = p.Name;

                if (name.Equals("semi_major", StringComparison.OrdinalIgnoreCase))
                {
                    semiMajorParam = p;
                }

                if (name.Equals("semi_minor", StringComparison.OrdinalIgnoreCase))
                {
                    semiMinorParam = p;
                }
            }

            if (ReferenceEquals(semiMajorParam, null))
            {
                throw new ArgumentException("Missing projection parameter 'semi_major'.");
            }

            if (ReferenceEquals(semiMinorParam, null))
            {
                throw new ArgumentException("Missing projection parameter 'semi_minor'.");
            }

            _semiMajor = semiMajorParam.Value;
            _semiMinor = semiMinorParam.Value;
            _reciprocalSemiMajor = 1.0d / _semiMajor;

            //_e2 = 1.0d - Math.Pow(_semiMinor / _semiMajor, 2d);
            _e2 = 1.0d - Math.Pow(_semiMinor, 2d) / Math.Pow(_semiMajor, 2d);
            _e = Math.Sqrt(_e2);
        }

        public Double SemiMajor
        {
            get { return _semiMajor; }
        }

        public Double ReciprocalSemiMajor
        {
            get { return _reciprocalSemiMajor; }
        }

        public Double SemiMinor
        {
            get { return _semiMinor; }
        }

        protected Double E
        {
            get { return _e; }
        }

        protected Double E2
        {
            get { return _e2; }
        }
    }
}
