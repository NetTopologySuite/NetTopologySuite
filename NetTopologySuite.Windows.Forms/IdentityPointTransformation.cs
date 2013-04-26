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

using System.Drawing;
using GeoAPI.Geometries;

namespace NetTopologySuite.Windows.Forms
{
    /// <summary>
    /// Point transformation class, copies ordinates with no transformation
    /// </summary>
    public class IdentityPointTransformation : IPointTransformation
    {
        ///<summary>
        /// Transforms a <see cref="Coordinate"/> into a <see cref="PointF"/>.
        ///</summary>
        ///<param name="model">The model coordinate</param>
        ///<param name="view">The view point</param>
        public void Transform(Coordinate model, ref PointF view)
        {
            view.X = (float)model.X;
            view.Y = (float)model.Y;
        }

        ///<summary>
        /// Transforms a <see cref="Coordinate"/> into a <see cref="PointF"/>.
        ///</summary>
        ///<param name="model">The model coordinate</param>
        /// <returns>A point for the view</returns>
        public PointF Transform(Coordinate model)
        {
            return new PointF((float)model.X, (float)model.Y);
        }

        /// <summary>
        /// Transforms an array of <see cref="Coordinate"/>s into an array of <see cref="PointF"/>s.
        /// </summary>
        /// <param name="model">An array of <see cref="Coordinate"/>s</param>
        /// <returns>An array of <see cref="PointF"/>s</returns>
        public PointF[] Transform(Coordinate[] model)
        {
            var ret = new PointF[model.Length];
            for (var i = 0; i < model.Length; i++)
                ret[i] = new PointF((float)model[i].X, (float)model[i].Y);
            return ret;
        }

        private static PointF Transform(double modelX, double modelY)
        {
            return new PointF((float)modelX, (float)modelY);
        }

        public PointF[] Transform(ICoordinateSequence modelSequence)
        {
            var res = new PointF[modelSequence.Count];
            for (var i = 0; i < modelSequence.Count; i++)
            {
                res[i] = Transform(modelSequence.GetOrdinate(0, Ordinate.X), 
                                   modelSequence.GetOrdinate(0, Ordinate.Y));
            }
            return res;
        }
    }

    public class InvertYPointTransformation : IPointTransformation
    {
        private readonly float _yOffset;

        public InvertYPointTransformation(float yOffset)
        {
            _yOffset = yOffset;
        }

        public void Transform(Coordinate model, ref PointF view)
        {
            view.X = (float)model.X;
            view.Y = _yOffset - (float)model.Y;
        }

        public PointF Transform(Coordinate model)
        {
            return new PointF((float) model.X, _yOffset - (float) model.Y);
        }

        private PointF Transform(double modelX, double modelY)
        {
            return new PointF((float) modelX, _yOffset - (float) modelY);
        }

        public PointF[] Transform(Coordinate[] model)
        {
            var ret = new PointF[model.Length];
            for (var i = 0; i < model.Length; i++)
                ret[i] = new PointF((float)model[i].X, _yOffset - (float)model[i].Y);
            return ret;
        }

        public PointF[] Transform(ICoordinateSequence modelSequence)
        {
            var res = new PointF[modelSequence.Count];
            for (var i = 0; i < modelSequence.Count; i++)
            {
                res[i] = Transform(modelSequence.GetOrdinate(0, Ordinate.X), 
                                   modelSequence.GetOrdinate(0, Ordinate.Y));
            }
            return res;
        }
    }
}