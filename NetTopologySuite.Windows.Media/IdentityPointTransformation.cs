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

using GeoAPI.Geometries;
using WpfPoint = System.Windows.Point;

namespace NetTopologySuite.Windows.Media
{
    /// <summary>
    /// Copies point ordinates with no transformtaion
    /// </summary>
    /// <author>Martin Davis</author>
    public class IdentityPointTransformation : IPointTransformation
    {
	    public void Transform(Coordinate model, ref WpfPoint view)
	    {
	        view.X = model.X;
	        view.Y = model.Y;
	    }

        public WpfPoint Transform(Coordinate model)
        {
            return new WpfPoint(model.X, model.Y);
        }

        public WpfPoint[] Transform (Coordinate[] model)
        {
            var ret = new WpfPoint[model.Length];
            for (var i = 0; i < model.Length; i++ )
                ret[i] = new WpfPoint(model[i].X, model[i].Y);
                return ret;
        }

        private static WpfPoint Transform(double x, double y)
        {
            return new WpfPoint(x, y);
        }

        public WpfPoint[] Transform(ICoordinateSequence modelSequence)
        {
            var res = new WpfPoint[modelSequence.Count];
            for (var i = 0; i < modelSequence.Count; i++)
            {
                res[i] = Transform(modelSequence.GetOrdinate(i, Ordinate.X),
                                   modelSequence.GetOrdinate(i, Ordinate.Y));
            }
            return res;
        }
    }

    /// <summary>
    /// Transforms coordinates by inverting the y ordinate and adding an offset
    /// </summary>
    /// <author>Martin Davis</author>
    public class InvertYPointTransformation : IPointTransformation
    {
        private readonly double _yOffset;

        public InvertYPointTransformation(double yOffset)
        {
            _yOffset = yOffset;
        }
        
        public void Transform(Coordinate model, ref WpfPoint view)
        {
            view.X = model.X;
            view.Y = _yOffset-model.Y;
        }

        public WpfPoint Transform(Coordinate model)
        {
            return new WpfPoint(model.X, _yOffset - model.Y);
        }

        public WpfPoint[] Transform(Coordinate[] model)
        {
            var ret = new WpfPoint[model.Length];
            for (var i = 0; i < model.Length; i++)
                ret[i] = new WpfPoint(model[i].X, _yOffset-model[i].Y);
            return ret;

        }

        private WpfPoint Transform(double x, double y)
        {
            return new WpfPoint(x, _yOffset - y);
        }

        public WpfPoint[] Transform(ICoordinateSequence modelSequence)
        {
            var res = new WpfPoint[modelSequence.Count];
            for (var i = 0; i < modelSequence.Count; i++)
            {
                res[i] = Transform(modelSequence.GetOrdinate(i, Ordinate.X),
                                   modelSequence.GetOrdinate(i, Ordinate.Y));
            }
            return res;
        }

    }
}

