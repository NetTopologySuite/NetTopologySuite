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


using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;
using System.Collections.Generic;


namespace NetTopologySuite.Hull
{

    // Original Author: Eric Grosso, eric.grosso.os@gmail.com
    // Conversion to C#: Stefan Steiger 
    public class Edge
    {
        // ID of the edge 
        private int id;

        // Geometry of the edge 
        private LineSegment geometry;

        // Indicator to know if the edge is a border edge of the triangulation framework
        private bool border;

        // Origin vertex of the edge
        private Vertex oV;

        // End vertex of the edge
        private Vertex eV;

        // Triangles in relationship with this edge
        private List<Triangle> triangles = new List<Triangle>();

        // Edges in relationship with this edge
        private List<Edge> incidentEdges = new List<Edge>();


        public Edge()
        { }
        
        
        // ID of the edge
        public Edge(int id)
        {
            this.id = id;
        }


        
        // ID of the edge
        // geometry of the edge (segment)
        public Edge(int id, LineSegment geometry)
        {
            this.id = id;
            this.geometry = geometry;
        }

        // Constructor
        // ID of the edge
        //  border
        // 		defines if the edge is a border edge
        // 		or not in the triangulation framework
        public Edge(int id, bool border)
        {
            this.id = id;
            this.border = border;
        }


        // ID of the edge
        // geometry of the edge (segment)
        // border
        //     defines if the edge is a border edge
        // 	   or not in the triangulation framework
        public Edge(int id, LineSegment geometry, bool border)
        {
            this.id = id;
            this.geometry = geometry;
            this.border = border;
        }

        // Constructor.
        // ID of the edge
        // geometry of the edge (segment)
        // oV: origin vertex
        // eV: end vertex
        // border: 
        //		defines if the edge is a border edge
        //		or not in the triangulation framework
        public Edge(int id, LineSegment geometry, Vertex oV, Vertex eV, bool border)
        {
            this.id = id;
            this.geometry = geometry;
            this.oV = oV;
            this.eV = eV;
            this.border = border;
        }
        
        
        // Returns the ID of the edge.
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }


        // Returns the geometry of the edge.
        public LineSegment Geometry
        {
            get { return this.geometry; }
            set { this.geometry = value; }
        }


        // Returns true if the edge is a border edge
        // of the triangulation framework, false otherwise.
        // 		true if the edge is a border edge,
        // 		false otherwise
        public bool Border
        {
            get { return this.border; }
            set { this.border = value; }
        }


        // Returns the origin vertex of the edge.
        public Vertex OV
        {
            get { return this.oV; }
            set { this.oV = value; }
        }


        // Returns the end vertex of the edge.
        public Vertex EV
        {
            get { return this.eV; }
            set { this.eV = value; }
        }
        
        
        // Defines/Returns the triangles in relationship with the edge.
        public List<Triangle> Triangles
        {
            get { return this.triangles; }
            set { this.triangles = value; }
        }


        // Gets/Sets the edges in relationship with the edge.
        public List<Edge> IncidentEdges
        {
            get { return this.incidentEdges; }
            set { this.incidentEdges = value; }
        }


        // Add a triangle in relationship with the edge.
        // returns true if added, false otherwise
        public bool AddTriangle(Triangle triangle)
        {
            this.Triangles.Add(triangle);
            return true;
        }
        

        // Add triangles in relationship with the edge.
        // returns true if added, false otherwise
        public bool AddTriangles(List<Triangle> triangles)
        {
            this.Triangles.AddRange(triangles);
            return true;
        }


        // Remove a triangle in relationship with the edge.
        // returns true if removed, false otherwise
        public bool RemoveTriangle(Triangle triangle)
        {
            return this.Triangles.Remove(triangle);
        }


        // Remove triangles in relationship with the edge. 
        // returns true if removed, false otherwise
        public bool RemoveTriangles(List<Triangle> trianglesToRemove)
        {
            foreach (var t in trianglesToRemove)
            {
                this.triangles.Remove(t);
            }
            
            return true;
        }


        // Add an incident edge in relationship with the edge.
        // returns true if added, false otherwise
        public bool AddIncidentEdge(Edge edge)
        {
            this.incidentEdges.Add(edge);
            return true;
        }


        // Add incident edges in relationship with the edge. 
        // return true if added, false otherwise
        public bool AddIncidentEdges(List<Edge> edges)
        {
            this.incidentEdges.AddRange(edges);
            return true;
        }


        // Remove an incident edge in relationship with the edge.
		// returns true if removed, false otherwise
        public bool RemoveIncidentEdge(Edge edge)
        {
            return this.incidentEdges.Remove(edge);
        }
        
        
        // Remove incident edges in relationship with the edge.
		// returns true if removed, false otherwise
        public bool RemoveAllIncidentEdges(List<Edge> edgesToRemove)
        {
            foreach (var t in edgesToRemove)
            {
                this.incidentEdges.Remove(t);
            }
            
            return true;
        }
        
        
    }
    
    
}
