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
using System.Diagnostics;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries.LightStructs;

namespace SharpMap.CoordinateSystems.Transformations
{
	internal class ConcatenatedTransform : MathTransform
	{
		protected IMathTransform _inverse;


		public ConcatenatedTransform()
			: this(new List<ICoordinateTransformation>())
		{
		}

		public ConcatenatedTransform(List<ICoordinateTransformation> transformlist)
		{
			_CoordinateTransformationList = transformlist;
		}

		private List<ICoordinateTransformation> _CoordinateTransformationList;

		public List<ICoordinateTransformation> CoordinateTransformationList
		{
			get { return _CoordinateTransformationList; }
			set
			{
				_CoordinateTransformationList = value;
				_inverse = null;
			}
		}
		
		public override Point Transform(Point point)
		{			
			Point pnt = (Point) point.Clone();
            foreach (ICoordinateTransformation ct in _CoordinateTransformationList)
                pnt = ct.MathTransform.Transform(pnt);            
			return pnt;			
		}

		public override List<Point> TransformList(List<Point> points)
		{
			List<Point> pnts = new List<Point>(points.Count);
			foreach (ICoordinateTransformation ct in _CoordinateTransformationList)
				pnts = ct.MathTransform.TransformList(pnts);
			return pnts;
		}

		/// <summary>
		/// Returns the inverse of this conversion.
		/// </summary>
		/// <returns>IMathTransform that is the reverse of the current conversion.</returns>
		public override IMathTransform Inverse()
		{
			if (_inverse == null)
			{
				_inverse = new ConcatenatedTransform(_CoordinateTransformationList);
				_inverse.Invert();
			}
			return _inverse;
		}
		
		/// <summary>
		/// Reverses the transformation
		/// </summary>
		public override void Invert()
		{
			_CoordinateTransformationList.Reverse();
			foreach (ICoordinateTransformation ic in _CoordinateTransformationList)
				ic.MathTransform.Invert();
		}

		public override string WKT
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public override string XML
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}
	}
}
