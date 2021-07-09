/*
 * This file is part of the OpenSphere project which aims to
 * develop geospatial algorithms.
 * 
 * Copyright (C) 2012 Eric Grosso
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
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 *
 * For more information, contact:
 * Eric Grosso, eric.grosso.os@gmail.com
 * 
 */

namespace NetTopologySuite.Hull
{

	using System.Collections.Generic;


	// Original Author: Eric Grosso, eric.grosso.os@gmail.com
	// Converted to C#: Stefan Steiger
	public class Triangle
	{

		// ID of the triangle
		private int id;

		// Indicator to know if the triangle is a border triangle
		// of the triangulation framework 
		private bool border;

		// Edges which compose the triangle
		private List<Edge> edges = new System.Collections.Generic.List<Edge>();

		// Neighbour triangles of this triangle
		private List<Triangle> neighbours = new System.Collections.Generic.List<Triangle>();



		public Triangle()
		{ }


		// Constructor.
		// ID of the triangle
		public Triangle(int id)
		{
			this.id = id;
		}

		// Constructor.
		// ID of the triangle
		// border defines if the triangle is a border triangle or not in the triangulation framework 
		public Triangle(int id, bool border)
		{
			this.id = id;
			this.border = border;
		}

		// Defines/Returns the ID of the triangle.
		// id ID of the triangle
		public int Id
		{
			get{ return this.id; }
			set{ this.id = value; }	
		}
		
		
		// Returns/Defines the indicator to know if the triangle
		// is a border triangle of the triangulation framework.
		// true if the triangle is a border triangle, false otherwise
		public bool Border
		{
			get { return this.border; }
			set { this.border = value; }
		}
		
		
		// Defines/Returns the edges which compose the triangle.
		// return the edges which compose the triangle
		public List<Edge> Edges
		{
			get { return this.edges; }
			set { this.edges = value; }
		}
		
		
		// Defines/Returns the neighbour triangles of the triangle.
		// return the neighbour triangles of the triangle
		public List<Triangle> Neighbours
		{
			get
			{
				return this.neighbours;
			}
			set
			{
				this.neighbours = value;
			}
		}
		
		
		// Add an edge to the triangle.
		// @return true if added, false otherwise 
		public bool AddEdge(Edge edge)
		{
			this.edges.Add(edge);
			return true;
		}


		// Add edges to the triangle.
		// return true if added, false otherwise
		public bool AddEdges(List<Edge> edges)
		{
			this.edges.AddRange(edges);
			return true;
		}


		// Remove an edge of the triangle.
		// return true if removed, false otherwise
		public bool RemoveEdge(Edge edge)
		{
			return this.edges.Remove(edge);
		}
		
		
		// Remove edges of the triangle.
		// return true if removed, false otherwise
		public bool RemoveEdges(List<Edge> edgesToRemove)
		{
			foreach (var thisEdge in edgesToRemove)
			{
				this.edges.Remove(thisEdge);
			}
			
			return true;
		}


		// Add a neighbour triangle to the triangle.
		// return true if added, false otherwise
		public bool AddNeighbour(Triangle triangle)
		{
			this.neighbours.Add(triangle);
			return true;
		}
		
		
		// Add neighbour triangles to the triangle.
		// return true if added, false otherwise
		public bool AddNeighbours(List<Triangle> triangles)
		{
			this.neighbours.AddRange(triangles);
			return true;
		}
		
		
		// Remove a neighbour triangle of the triangle.
		// return true if removed, false otherwise
		public bool RemoveNeighbour(Triangle triangle)
		{
			return this.neighbours.Remove(triangle);
		}


		// Remove neighbour triangles of the triangle.
		// return true if removed, false otherwise
		public bool RemoveNeighbours(List<Triangle> triangles)
		{
			foreach (var t in triangles)
			{
				this.neighbours.Remove(t);
			}
			
			return true;
		}


	}


}
