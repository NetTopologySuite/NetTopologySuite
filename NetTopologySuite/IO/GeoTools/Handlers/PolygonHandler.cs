using System;
using System.Collections;
using System.Diagnostics;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Converts a Shapefile point to a OGIS Polygon.
	/// </summary>
	public class PolygonHandler : ShapeHandler
	{		
		/// <summary>
		/// Initializes a new instance of the PolygonHandler class.
		/// </summary>
		public PolygonHandler() { }	

		/// <summary>
		/// The ShapeType this handler handles.
		/// </summary>
        public override ShapeGeometryTypes ShapeType
		{
			get
			{
                return ShapeGeometryTypes.Polygon;
			}
		}

		/// <summary>
		/// Reads a stream and converts the shapefile record to an equilivent geometry object.
		/// </summary>
		/// <param name="file">The stream to read.</param>
		/// <param name="geometryFactory">The geometry factory to use when making the object.</param>
		/// <returns>The Geometry object that represents the shape file record.</returns>
		public override Geometry Read(BigEndianBinaryReader file, GeometryFactory geometryFactory)
		{
			int shapeTypeNum = file.ReadInt32();
            ShapeGeometryTypes shapeType = (ShapeGeometryTypes)Enum.Parse(typeof(ShapeGeometryTypes), shapeTypeNum.ToString());
            if ( ! ( shapeType == ShapeGeometryTypes.Polygon  || shapeType == ShapeGeometryTypes.PolygonM ||
                     shapeType == ShapeGeometryTypes.PolygonZ || shapeType == ShapeGeometryTypes.PolygonZM))	
				throw new ShapefileException("Attempting to load a non-polygon as polygon.");

			// Read and for now ignore bounds.
			double[] box = new double[4];
			for (int i = 0; i < 4; i++) 
				box[i] = file.ReadDouble();

			int[] partOffsets;        
			int numParts = file.ReadInt32();
			int numPoints = file.ReadInt32();
			partOffsets = new int[numParts];
			for (int i = 0; i < numParts; i++)
				partOffsets[i] = file.ReadInt32();

			ArrayList shells = new ArrayList();
			ArrayList holes = new ArrayList();

			int start, finish, length;
			for (int part = 0; part < numParts; part++)
			{
				start = partOffsets[part];
				if (part == numParts - 1)
					 finish = numPoints;
				else finish = partOffsets[part + 1];
				length = finish - start;
                CoordinateList points = new CoordinateList();
				points.Capacity = length;
				for (int i = 0; i < length; i++)
				{
					Coordinate external = new Coordinate(file.ReadDouble(), file.ReadDouble() );					
                    geometryFactory.PrecisionModel.MakePrecise(ref external);
                    Coordinate internalCoord = external;
					points.Add(internalCoord);
				}

				LinearRing ring = geometryFactory.CreateLinearRing((Coordinate[])points.ToArray(typeof(Coordinate)));
				
                // If shape have only a part, jump orientation check and add to shells
                if (numParts == 1)
                    shells.Add(ring);
                else
                {
                    // Orientation check
                    if (CGAlgorithms.IsCCW((Coordinate[])points.ToArray(typeof(Coordinate))))
                        holes.Add(ring);
                    else shells.Add(ring);
                }
			}

			// Now we have a list of all shells and all holes
			ArrayList holesForShells = new ArrayList(shells.Count);
			for (int i = 0; i < shells.Count; i++)
				holesForShells.Add(new ArrayList());
			// Find holes
			for (int i = 0; i < holes.Count; i++)
			{
				LinearRing testRing = (LinearRing) holes[i];
				LinearRing minShell = null;
				Envelope minEnv = null;
				Envelope testEnv = testRing.EnvelopeInternal;
				Coordinate testPt = testRing.GetCoordinateN(0);
				LinearRing tryRing;
				for (int j = 0; j < shells.Count; j++)
				{
					tryRing = (LinearRing) shells[j];
					Envelope tryEnv = tryRing.EnvelopeInternal;
					if (minShell != null) 
						minEnv = minShell.EnvelopeInternal;
					bool isContained = false;
					CoordinateList coordList = new CoordinateList(tryRing.Coordinates);
					if (tryEnv.Contains(testEnv)
                        && (CGAlgorithms.IsPointInRing(testPt, (Coordinate[])coordList.ToArray(typeof(Coordinate))) 
                        || (PointInList(testPt, coordList)))) 				
						isContained = true;

                    // Check if this new containing ring is smaller than the current minimum ring
                    if (isContained)
                    {
                        if (minShell == null || minEnv.Contains(tryEnv))
                            minShell = tryRing;             

                        // Suggested by Brian Macomber and added 3/28/2006:
                        // holes were being found but never added to the holesForShells array
                        // so when converted to geometry by the factory, the inner rings were never created.
                        ArrayList holesForThisShell = (ArrayList) holesForShells[j];
                        holesForThisShell.Add(testRing);
                    }
				}
			}

			Polygon[] polygons = new Polygon[shells.Count];
			for (int i = 0; i < shells.Count; i++)			
				polygons[i] = geometryFactory.CreatePolygon((LinearRing) shells[i], 
                    (LinearRing[])((ArrayList) holesForShells[i]).ToArray(typeof(LinearRing)));
        
			if (polygons.Length == 1)
				return polygons[0];
			// It's a multi part
			return geometryFactory.CreateMultiPolygon(polygons);
		}

		/// <summary>
		/// Writes a Geometry to the given binary wirter.
		/// </summary>
		/// <param name="geometry">The geometry to write.</param>
		/// <param name="file">The file stream to write to.</param>
		/// <param name="geometryFactory">The geometry factory to use.</param>
		public override void Write(Geometry geometry, System.IO.BinaryWriter file, GeometryFactory geometryFactory)
		{
            // Diego Guidi say's: his check seems to be not useful and slow the operations...
			//  if (!geometry.IsValid)    
			//	Trace.WriteLine("Invalid polygon being written.");

			GeometryCollection multi;
			if(geometry is GeometryCollection)
				multi = (GeometryCollection) geometry;
			else 
			{
				GeometryFactory gf = new GeometryFactory(geometry.PrecisionModel);				
				multi = gf.CreateMultiPolygon( new Polygon[]{(Polygon) geometry} );
			}

			file.Write(int.Parse(Enum.Format(typeof(ShapeGeometryTypes), this.ShapeType, "d")));
        
			Envelope box = multi.EnvelopeInternal;
			Envelope bounds = ShapeHandler.GetEnvelopeExternal(geometryFactory.PrecisionModel,  box);
			file.Write(bounds.MinX);
			file.Write(bounds.MinY);
			file.Write(bounds.MaxX);
			file.Write(bounds.MaxY);
        
			int numParts = GetNumParts(multi);
			int numPoints = multi.NumPoints;
			file.Write(numParts);
			file.Write(numPoints);
        			
			// write the offsets to the points
			int offset=0;
			for (int part = 0; part < multi.NumGeometries; part++)
			{
				// offset to the shell points
				Polygon polygon = (Polygon)multi.Geometries[part];
				file.Write(offset);
				offset = offset + polygon.ExteriorRing.NumPoints;

				// offstes to the holes
				foreach (LinearRing ring in polygon.InteriorRings)
				{
					file.Write(offset);
					offset = offset + ring.NumPoints;
				}	
			}

			// write the points 
			for (int part = 0; part < multi.NumGeometries; part++)
			{
				Polygon poly = (Polygon)multi.Geometries[part];
				Coordinate[] points = poly.ExteriorRing.Coordinates;
				WriteCoords(new CoordinateList(points), file, geometryFactory);
				foreach(LinearRing ring in poly.InteriorRings)
				{
					Coordinate[] points2 = ring.Coordinates;					
					WriteCoords(new CoordinateList(points2), file, geometryFactory);
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="file"></param>
        /// <param name="geometryFactory"></param>
		private void WriteCoords(CoordinateList points, System.IO.BinaryWriter file, GeometryFactory geometryFactory)
		{
			Coordinate external;
			foreach (Coordinate point in points)
			{
				// external = geometryFactory.PrecisionModel.ToExternal(point);
                external = point;
				file.Write(external.X);
				file.Write(external.Y);
			}
		}

		/// <summary>
		/// Gets the length of the shapefile record using the geometry passed in.
		/// </summary>
		/// <param name="geometry">The geometry to get the length for.</param>
		/// <returns>The length in bytes this geometry is going to use when written out as a shapefile record.</returns>
		public override int GetLength(Geometry geometry)
		{
			int numParts=GetNumParts(geometry);
			return (22 + (2 * numParts) + geometry.NumPoints * 8);
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
		private int GetNumParts(Geometry geometry)
		{
			int numParts=0;
			if (geometry is MultiPolygon)
            {
                MultiPolygon mpoly = geometry as MultiPolygon;
                foreach (Polygon poly in mpoly.Geometries)
					numParts = numParts + poly.InteriorRings.Length + 1;
            }
			else if (geometry is Polygon)
				numParts = ((Polygon)geometry).InteriorRings.Length + 1;
			else throw new InvalidOperationException("Should not get here.");
			return numParts;
		}

		/// <summary>
		/// Test if a point is in a list of coordinates.
		/// </summary>
		/// <param name="testPoint">TestPoint the point to test for.</param>
		/// <param name="pointList">PointList the list of points to look through.</param>
		/// <returns>true if testPoint is a point in the pointList list.</returns>
		private bool PointInList(Coordinate testPoint, CoordinateList pointList) 
		{
			foreach(Coordinate p in pointList)
				if (p.Equals2D(testPoint))
					return true;
			return false;
		}
	}
}
