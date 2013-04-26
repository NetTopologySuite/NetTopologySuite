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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;

namespace NetTopologySuite.Windows.Forms
{
    ///<summary>
    /// Writes <see cref="IGeometry"/>s int System.Drawing.Drawing2D's <see cref="System.Drawing.Drawing2D.GraphicsPath"/>s
    /// of the appropriate type.
    /// This supports rendering geometries using System.Drawing.
    /// The GraphicsPathWriter allows supplying a <see cref="IPointTransformation"/>
    /// class, to transform coordinates from model space into view space.
    /// This is useful if a client is providing its own transformation
    /// logic, rather than relying on <see cref="System.Drawing.Drawing2D.Matrix"/>.
    /// <para/>
    /// The writer supports removing duplicate consecutive points
    /// (via the <see cref="RemoveDuplicatePoints"/> property) 
    /// as well as true <b>decimation</b>
    /// (via the <see cref="Decimation"/> property. 
    /// Enabling one of these strategies can substantially improve 
    /// rendering speed for large geometries.
    /// It is only necessary to enable one strategy.
    /// Using decimation is preferred, but this requires 
    /// determining a distance below which input geometry vertices
    /// can be considered unique (which may not always be feasible).
    /// If neither strategy is enabled, all vertices
    /// of the input <tt>Geometry</tt>
    /// will be represented in the output <tt>GraphicsPath</tt>.
    /// </summary>
    public class GraphicsPathWriter
    {
        /// <summary>
        /// The point transformation used by default.
        /// </summary>
        public static readonly IPointTransformation DefaultPointTransformation = new IdentityPointTransformation();

        /// <summary>
        /// The point shape factory used by default.
        /// </summary>
        public static readonly IPointShapeFactory DefaultPointFactory = new Square(3.0);

        private readonly IPointTransformation _pointTransformer = DefaultPointTransformation;
        private readonly IPointShapeFactory _pointFactory = DefaultPointFactory;

        ///<summary>
        /// Creates a new GraphicsPathWriter with a specified point transformation and point shape factory.
        ///</summary>
        /// <param name="pointTransformer">A transformation from model to view space to use </param>
        /// <param name="pointFactory">The PointShapeFactory to use</param>
        public GraphicsPathWriter(IPointTransformation pointTransformer, IPointShapeFactory pointFactory)
        {
            if (pointTransformer != null)
                _pointTransformer = pointTransformer;
            if (pointFactory != null)
                _pointFactory = pointFactory;
        }

        ///<summary>
        /// Creates a new GraphicsPathWriter with a specified point transformation and the default point shape factory.
        ///</summary>
        /// <param name="pointTransformer">A transformation from model to view space to use </param>
        public GraphicsPathWriter(IPointTransformation pointTransformer)
            : this(pointTransformer, null)
        {
        }

        ///<summary>
        /// Creates a new GraphicsPathWriter with the default (identity) point transformation.
        ///</summary>
        public GraphicsPathWriter()
        {
        }

        /// <summary>
        /// Gets or sets whether duplicate consecutive points should be eliminated.
        /// This can reduce the size of the generated Shapes
        /// and improve rendering speed, especially in situations
        /// where a transform reduces the extent of the geometry.
        /// <para/>
        /// The default is <tt>false</tt>.
        /// </summary>
        public bool RemoveDuplicatePoints { get; set; }

        /// <summary>
        /// Gets or sets the decimation distance used to determine
        /// whether vertices of the input geometry are 
        /// considered to be duplicate and thus removed.
        /// The distance is axis distance, not Euclidean distance.
        /// The distance is specified in the input geometry coordinate system
        /// (NOT the transformed output coordinate system).
        /// <para/>
        /// When rendering to a screen image, a suitably small distance should be used
        /// to avoid obvious rendering defects.  
        /// A distance equivalent to 2 pixels or less is recommended
        /// (and perhaps even smaller to avoid any chance of visible artifacts).
        /// <para/>
        /// The default distance is 0.0, which disables decimation.
        /// </summary>
        public double Decimation { get; set; }

        ///<summary>
        /// Creates a <see cref="GraphicsPath"/> representing a <see cref="IGeometry"/>, according to the specified PointTransformation and PointShapeFactory (if relevant).
        ///</summary>
        ///<remarks>
        /// Note that GraphicsPaths do not preserve information about which elements in heterogeneous collections are 1D and which are 2D.
        /// For example, a GeometryCollection containing a ring and a disk will render as two disks if Graphics.FillPath is used,
        /// or as two rings if Graphics.DrawPath is used. To avoid this issue use separate shapes for the components.
        ///</remarks>
        public GraphicsPath ToShape(IGeometry geometry)
        {
            if (geometry.IsEmpty)
            {
                return new GraphicsPath();
            }
            
            var polygon = geometry as IPolygon;
            if (polygon != null)
            {
                return ToShape(polygon);
            }
            
            var lineString = geometry as ILineString;
            if (lineString != null)
            {
                return ToShape(lineString);
            }

            var multiLineString = geometry as IMultiLineString;
            if (multiLineString != null)
            {
                return ToShape(multiLineString);
            }
            
            var point = geometry as IPoint;
            if (point != null)
            {
                return ToShape(point);
            }

            var geometryCollection = geometry as IGeometryCollection;
            if (geometryCollection != null)
            {
                return ToShape(geometryCollection);
            }

            throw new ArgumentException(
                "Unrecognized Geometry class: " + geometry.GetType());
        }

        private GraphicsPath ToShape(IPolygon p)
        {
            var poly = new PolygonGraphicsPath();

            AppendRing(poly, p.ExteriorRing.Coordinates);
            for (int j = 0; j < p.NumInteriorRings; j++)
            {
                AppendRing(poly, p.GetInteriorRingN(j).Coordinates);
            }

            return poly.Path;
        }

        private void AppendRing(PolygonGraphicsPath poly, Coordinate[] coords)
        {
            GraphicsPath ring = null;

            var prevX = Single.NaN;
            var prevY = Single.NaN;
            
            Coordinate prev = null;
    
            var n = coords.Length - 1;
            /**
             * Don't include closing point.
             * Ring path will be closed explicitly, which provides a 
             * more accurate path representation.
             */
            for (var i = 0; i <= n; i++) 
            {
                if (Decimation > 0.0)
                {
                    var isDecimated = prev != null 
                        && Math.Abs(coords[i].X - prev.X) < Decimation
                        && Math.Abs(coords[i].Y - prev.Y) < Decimation;
                    if (isDecimated) 
                        continue;
                    prev = coords[i];
                }
		  
                var transPoint = TransformPoint(coords[i]);
			
                if (RemoveDuplicatePoints)
                {
                    // skip duplicate points (except the last point)
                    var isDup = transPoint.X == prevX && transPoint.Y == prevY;
                    if (isDup)
                    {
                        continue;
                    }
                    prevX = transPoint.X;
                    prevY = transPoint.Y;
                }
                poly.AddToRing(transPoint, ref ring);
            }
            // handle closing point
            poly.EndRing(ring);
        }

        private GraphicsPath ToShape(IGeometryCollection gc)
        {
            var shape = new GeometryCollectionGraphicsPath();
            // add components to GC shape
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                shape.Add(ToShape(g));
            }
            return shape.Path;
        }

        private GraphicsPath ToShape(IMultiLineString mls)
        {
            var path = new GraphicsPath();
            for (var i = 0; i < mls.NumGeometries; i++)
            {
                path.AddPath(ToShape((ILineString)mls.GetGeometryN(i)), false);
            }

            return path;
        }

        private GraphicsPath ToShape(ILineString lineString)
        {
            var shape = new GraphicsPath();
            
            var points = new List<PointF>(lineString.NumPoints);
            var prev = lineString.GetCoordinateN(0);
            var transPoint = TransformPoint(prev);
            points.Add(transPoint);

            var prevX = transPoint.X;
            var prevY = transPoint.Y;

            var n = lineString.NumPoints - 1;
            
            //int count = 0;
            for (var i = 1; i <= n; i++)
            {
                var currentCoord = lineString.GetCoordinateN(i);
                if (Decimation > 0.0)
                {
                    var isDecimated = prev != null
                            && Math.Abs(currentCoord.X - prev.X) < Decimation
                            && Math.Abs(currentCoord.Y - prev.Y) < Decimation;
                    
                    if (i < n && isDecimated)
                    {
                        continue;
                    }
                    prev = currentCoord;
                }

                transPoint = TransformPoint(lineString.GetCoordinateN(i));

                if (RemoveDuplicatePoints)
                {
                    // skip duplicate points (except the last point)
                    var isDup = transPoint.X == prevX && transPoint.Y == prevY;
                    if (i < n && isDup)
                        continue;
                    prevX = transPoint.X;
                    prevY = transPoint.Y;
                    //count++;
                }
                points.Add(transPoint);
            }
            //System.out.println(count);
            shape.AddLines(points.ToArray());
            return shape;
        }

        private GraphicsPath ToShape(IPoint point)
        {
            var viewPoint = TransformPoint(point.Coordinate);
            return _pointFactory.CreatePoint(viewPoint);
        }

        private PointF TransformPoint(Coordinate model)
        {
            return _pointTransformer.Transform(model);
        }
    }
}