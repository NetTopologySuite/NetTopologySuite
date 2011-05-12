/*
 * The JTS Topology Suite is a collection of Java classes that
 * implement the fundamental operations required to validate a given
 * geo-spatial data set to a known topological specification.
 *
 * Copyright (C) 2001 Vivid Solutions
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * For more information, contact:
 *
 *     Vivid Solutions
 *     Suite #1A
 *     2328 Government Street
 *     Victoria BC  V8T 5G5
 *     Canada
 *
 *     (250)385-6040
 *     www.vividsolutions.com
 */

/**
 * Copies point ordinates with no transformation.
 * 
 * @author Martin Davis
 *
 */

using System.Drawing;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Windows.Forms
{
    public class IdentityPointTransformation : IPointTransformation
    {
	    public void Transform(ICoordinate model, ref PointF view)
	    {
	        view.X = (float) model.X;
	        view.Y = (float) model.Y;

	    }

        public PointF[] Transform (ICoordinate[] model)
        {
            var ret = new PointF[model.Length];
            for (var i = 0; i < model.Length; i++ )
                ret[i] = new PointF((float) model[i].X, (float) model[i].Y);
                return ret;
            
        }
    }
    public class InvertYPointTransformation : IPointTransformation
    {
        private readonly float _yOffset;

        public InvertYPointTransformation(float yOffset)
        {
            _yOffset = yOffset;
        }
        
        public void Transform(ICoordinate model, ref PointF view)
        {
            view.X = (float)model.X;
            view.Y = _yOffset-(float)model.Y;

        }

        public PointF[] Transform(ICoordinate[] model)
        {
            var ret = new PointF[model.Length];
            for (var i = 0; i < model.Length; i++)
                ret[i] = new PointF((float)model[i].X, _yOffset-(float)model[i].Y);
            return ret;

        }
    }
}

