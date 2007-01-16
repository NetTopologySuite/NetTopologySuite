// Copyright 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
	/// <summary>
	/// Transformation for applying 
	/// </summary>
	internal class DatumTransform : MathTransform
	{
		protected IMathTransform _inverse;
		private Wgs84ConversionInfo _ToWgs94;
		double[] v;
		private bool _isInverse = false;
		public DatumTransform(Wgs84ConversionInfo towgs84) : this(towgs84,false)
		{
		}
		private DatumTransform(Wgs84ConversionInfo towgs84, bool isInverse)
		{
			_ToWgs94 = towgs84;
			v = _ToWgs94.GetAffineTransform();
			_isInverse = isInverse;
		}
		public override string WKT
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public override string XML
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public override IMathTransform Inverse()
		{
			if (_inverse == null)
				_inverse = new DatumTransform(_ToWgs94,!_isInverse);
			return _inverse;
		}

		private Point Apply(Point p)
		{
			return new Point(
				v[0] * p.X - v[3] * p.Y + v[2] * p.Z + v[4],
				v[3] * p.X + v[0] * p.Y - v[1] * p.Z + v[5],
			   -v[2] * p.X + v[1] * p.Y + v[0] * p.Z + v[6]);			
		}

		private Point ApplyInverted(Point p)
		{
			return new Point(
				v[0] * p.X + v[3] * p.Y - v[2] * p.Z - v[4],
			   -v[3] * p.X + v[0] * p.Y + v[1] * p.Z - v[5],
			    v[2] * p.X - v[1] * p.Y + v[0] * p.Z - v[6]);
		}

		public override Point Transform(Point point)
		{
            if (!_isInverse)
                 return Apply(point);
            else return ApplyInverted(point);
		}

		public override List<Point> TransformList(List<Point> points)
		{
			List<Point> pnts = new List<Point>(points.Count);
			foreach(Point p in points)
				pnts.Add(Transform(p));
			return pnts;
		}

		public override void Invert()
		{
			_isInverse = !_isInverse;
		}
	}
}
