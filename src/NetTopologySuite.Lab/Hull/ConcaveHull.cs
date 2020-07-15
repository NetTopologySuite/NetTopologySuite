
using System.Linq;
using System.Collections.Generic;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;


namespace NetTopologySuite.Hull
{


    // http://www.rotefabrik.free.fr/concave_hull/
    // https://observablehq.com/@mourner/adaptive-concave-hull
    // https://towardsdatascience.com/the-concave-hull-c649795c0f0f
    public class ConcaveHull
    {

        private GeometryFactory geomFactory;
        private readonly GeometryCollection geometries;
        private double threshold;


        public Dictionary<LineSegment, int> segments = new Dictionary<LineSegment, int>();
        public Dictionary<int, Edge> edges = new Dictionary<int, Edge>();
        public Dictionary<int, Triangle> triangles = new Dictionary<int, Triangle>();
        public SortedDictionary<int, Edge> lengths = new SortedDictionary<int, Edge>();

        public Dictionary<int, Edge> shortLengths = new Dictionary<int, Edge>();

        public Dictionary<Coordinate, int> coordinates = new Dictionary<Coordinate, int>();
        public Dictionary<int, Vertex> vertices = new Dictionary<int, Vertex>();



        public ConcaveHull(Geometry geometry, double threshold)
        {
            this.geometries = TransformIntoPointGeometryCollection(geometry);
            this.threshold = threshold;
            this.geomFactory = geometry.Factory;
        }


        public ConcaveHull(GeometryCollection geometries, double threshold)
        {
            this.geometries = TransformIntoPointGeometryCollection(geometries);
            this.threshold = threshold;
            this.geomFactory = geometries.Factory;
        }

        private static GeometryCollection TransformIntoPointGeometryCollection(Geometry geom)
        {
            var filter = new Utilities.UniqueCoordinateArrayFilter();
            geom.Apply(filter);
            var coord = filter.Coordinates;

            var geometries = new Geometry[coord.Length];
            for (int i = 0; i < coord.Length; i++)
            {
                var c = new Coordinate[] { coord[i] };
                var cs = new CoordinateArraySequence(c);
                geometries[i] = new Point(cs, geom.Factory);
            }

            return new GeometryCollection(geometries, geom.Factory);
        }

        // Transform into GeometryCollection. 
        // @param geom input geometry
        // @return a geometry collection
        private static GeometryCollection TransformIntoPointGeometryCollection(GeometryCollection gc)
        {
            var filter = new Utilities.UniqueCoordinateArrayFilter();
            gc.Apply(filter);
            var coord = filter.Coordinates;

            var geometries = new Geometry[coord.Length];
            for (int i = 0; i < coord.Length; i++)
            {
                var c = new Coordinate[] { coord[i] };
                var cs = new CoordinateArraySequence(c);
                geometries[i] = new Point(cs, gc.Factory);
            }

            return new GeometryCollection(geometries, gc.Factory);
        }



        // Returns a {@link Geometry} that represents the concave hull of the input
        // geometry according to the threshold.
        // The returned geometry contains the minimal number of points needed to
        // represent the concave hull.
        //
        // @return if the concave hull contains 3 or more points, a {@link Polygon};
        // 2 points, a {@link LineString};
        // 1 point, a {@link Point};
        // 0 points, an empty {@link GeometryCollection}.

        public Geometry GetResult()
        {

            if (this.geometries.NumGeometries == 0)
            {
                return this.geomFactory.CreateGeometryCollection(null);
            }

            if (this.geometries.NumGeometries == 1)
            {
                return this.geometries.GetGeometryN(0);
            }

            if (this.geometries.NumGeometries == 2)
            {
                return this.geomFactory.CreateLineString(this.geometries.Coordinates);
            }

            return ComputeConcaveHull();
        }


        public Geometry ComputeConcaveHull()
        {
            var cdtb = new ConformingDelaunayTriangulationBuilder();
            cdtb.SetSites(this.geometries);

            var qes = cdtb.GetSubdivision();

            var quadEdges = qes.GetEdges();
            var qeTriangles = QuadEdgeTriangle.CreateOn(qes);
            var qeVertices = qes.GetVertices(false);

            int iV = 0;
            foreach (var v in qeVertices)
            {
                this.coordinates[v.Coordinate] = iV;
                this.vertices[iV] = new Vertex(iV, v.Coordinate);
                iV++;
            }

            var qeFrameBorder = new List<QuadEdge>();
            var qeFrame = new List<QuadEdge>();
            var qeBorder = new List<QuadEdge>();

            // here each one more
            foreach (var qe in quadEdges)
            {
                if (qes.IsFrameBorderEdge(qe))
                {
                    qeFrameBorder.Add(qe);
                }

                if (qes.IsFrameEdge(qe))
                {
                    qeFrame.Add(qe);
                }
            } // Next qe 

            // border
            for (int j = 0; j < qeFrameBorder.Count; j++)
            {
                var q = qeFrameBorder[j];
                if (!qeFrame.Contains(q))
                {
                    qeBorder.Add(q);
                }

            } // Next j 

            // deletion of exterior edges
            foreach (var qe in qeFrame)
            {
                qes.Delete(qe);
            }



            var qeDistances = new Dictionary<QuadEdge, double>();

            foreach (var qe in quadEdges)
            {
                qeDistances.Add(qe, qe.ToLineSegment().Length);
            }


            var dc = new DoubleComparator(qeDistances);
            // This doesn't work with dictionary - missing duplicates ...
            var qeSorted = new List<KeyValuePair<QuadEdge, double>>();
            foreach (var thisDistance in qeDistances)
            {
                qeSorted.Add(thisDistance);
            }
            qeSorted.Sort(dc);


            // edges creation
            int i = 0;
            foreach (var kvp in qeSorted)
            {
                var s = kvp.Key.ToLineSegment();
                s.Normalize();

                int idS = this.coordinates[s.P0];
                int idD = this.coordinates[s.P1];
                var oV = this.vertices[idS];
                var eV = this.vertices[idD];

                Edge edge;
                if (qeBorder.Contains(kvp.Key))
                {
                    oV.IsBorder = true;
                    eV.IsBorder = true;

                    edge = new Edge(i, s, oV, eV, true);

                    if (s.Length < this.threshold)
                    {
                        this.shortLengths[i] = edge;
                    }
                    else
                    {
                        this.lengths[i] = edge;
                    }
                }
                else
                {
                    edge = new Edge(i, s, oV, eV, false);
                }

                this.edges[i] = edge;
                this.segments[s] = i;
                i++;
            } // Next qe 

            // hm of linesegment and hm of edges // with id as key
            // hm of triangles using hm of ls and connection with hm of edges

            i = 0;
            foreach (var qet in qeTriangles)
            {
                var sA = qet.GetEdge(0).ToLineSegment();
                var sB = qet.GetEdge(1).ToLineSegment();
                var sC = qet.GetEdge(2).ToLineSegment();

                sA.Normalize();
                sB.Normalize();
                sC.Normalize();

                var edgeA = this.edges[this.segments[sA]];
                var edgeB = this.edges[this.segments[sB]];
                var edgeC = this.edges[this.segments[sC]];

                var triangle = new Triangle(i, qet.IsBorder() ? true : false);
                triangle.AddEdge(edgeA);
                triangle.AddEdge(edgeB);
                triangle.AddEdge(edgeC);

                edgeA.AddTriangle(triangle);
                edgeB.AddTriangle(triangle);
                edgeC.AddTriangle(triangle);

                this.triangles[i] = triangle;
                i++;
            } // Next qet 


            // add triangle neighbourood
            foreach (var edge in this.edges.Values)
            {
                if (edge.Triangles.Count != 1)
                {
                    var tA = edge.Triangles[0];
                    var tB = edge.Triangles[1];
                    tA.AddNeighbour(tB);
                    tB.AddNeighbour(tA);
                }
            }

            // concave hull algorithm
            int index = 0;
            while (index != -1)
            {
                index = -1;

                Edge e = null;

                // find the max length (smallest id so first entry)
                int si = this.lengths.Count;

                if (si != 0)
                {
                    var entry = this.lengths.First();

                    int ind = entry.Key;
                    if (entry.Value.Geometry.Length > this.threshold)
                    {
                        index = ind;
                        e = entry.Value;
                    }

                } // End if (si != 0) 

                if (index != -1)
                {
                    var triangle = e.Triangles[0];
                    var neighbours = triangle.Neighbours;

                    // irregular triangle test
                    if (neighbours.Count == 1)
                    {
                        this.shortLengths[e.Id] = e;
                        this.lengths.Remove(e.Id);
                    }
                    else
                    {
                        var e0 = triangle.Edges[0];
                        var e1 = triangle.Edges[1];

                        // test if all the vertices are on the border
                        if (e0.OV.IsBorder && e0.EV.IsBorder
                            && e1.OV.IsBorder && e1.EV.IsBorder)
                        {
                            this.shortLengths[e.Id] = e;
                            this.lengths.Remove(e.Id);
                        }
                        else
                        {
                            // management of triangles
                            var tA = neighbours[0];
                            var tB = neighbours[1];
                            tA.Border = true; // FIXME not necessarily useful
                            tB.Border = true; // FIXME not necessarily useful
                            this.triangles.Remove(triangle.Id);
                            tA.RemoveNeighbour(triangle);
                            tB.RemoveNeighbour(triangle);

                            // new edges
                            var ee = triangle.Edges;
                            var eA = ee[0];
                            var eB = ee[1];
                            var eC = ee[2];

                            if (eA.Border)
                            {
                                this.edges.Remove(eA.Id);
                                eB.Border = true;

                                eB.OV.IsBorder = true;
                                eB.EV.IsBorder = true;

                                eC.Border = true;

                                eC.OV.IsBorder = true;
                                eC.EV.IsBorder = true;

                                // clean the relationships with the triangle
                                eB.RemoveTriangle(triangle);
                                eC.RemoveTriangle(triangle);

                                if (eB.Geometry.Length < this.threshold)
                                {
                                    this.shortLengths[eB.Id] = eB;
                                }
                                else
                                {
                                    this.lengths[eB.Id] = eB;
                                }

                                if (eC.Geometry.Length < this.threshold)
                                {
                                    this.shortLengths[eC.Id] = eC;
                                }
                                else
                                {
                                    this.lengths[eC.Id] = eC;
                                }

                                this.lengths.Remove(eA.Id);
                            } // End if (eA.Border) 
                            else if (eB.Border)
                            {
                                this.edges.Remove(eB.Id);
                                eA.Border = true;
                                eA.OV.IsBorder = true;
                                eA.EV.IsBorder = true;
                                eC.Border = true;
                                eC.OV.IsBorder = true;
                                eC.EV.IsBorder = true;

                                // clean the relationships with the triangle
                                eA.RemoveTriangle(triangle);
                                eC.RemoveTriangle(triangle);

                                if (eA.Geometry.Length < this.threshold)
                                {
                                    this.shortLengths[eA.Id] = eA;
                                }
                                else
                                {
                                    this.lengths[eA.Id] = eA;
                                }
                                if (eC.Geometry.Length < this.threshold)
                                {
                                    this.shortLengths[eC.Id] = eC;
                                }
                                else
                                {
                                    this.lengths[eC.Id] = eC;
                                }

                                this.lengths.Remove(eB.Id);

                            } // End else if (eB.Border)
                            else
                            {
                                this.edges.Remove(eC.Id);
                                eA.Border = true;

                                eA.OV.IsBorder = true;
                                eA.EV.IsBorder = true;
                                eB.Border = true;
                                eB.OV.IsBorder = true;
                                eB.EV.IsBorder = true;

                                // clean the relationships with the triangle
                                eA.RemoveTriangle(triangle);
                                eB.RemoveTriangle(triangle);

                                if (eA.Geometry.Length < this.threshold)
                                {
                                    this.shortLengths[eA.Id] = eA;
                                }
                                else
                                {
                                    this.lengths[eA.Id] = eA;
                                }

                                if (eB.Geometry.Length < this.threshold)
                                {
                                    this.shortLengths[eB.Id] = eB;
                                }
                                else
                                {
                                    this.lengths[eB.Id] = eB;
                                }

                                this.lengths.Remove(eC.Id);
                            } // End Else of if (e0.OV.Border && e0.EV.Border && e1.OV.Border && e1.EV.Border)

                        } // End Else of if 

                    } // End Else of if (neighbours.Count == 1)

                } // End if (index != -1) 

            } // Whend 

            // concave hull creation
            var edges = new List<LineString>();
            foreach (var e in this.lengths.Values)
            {
                var l = e.Geometry.ToGeometry(this.geomFactory);
                edges.Add(l);
            }

            foreach (var e in this.shortLengths.Values)
            {
                var l = e.Geometry.ToGeometry(this.geomFactory);
                edges.Add(l);
            }

            // merge
            var lineMerger = new Operation.Linemerge.LineMerger();
            lineMerger.Add(edges);

            LineString merge = null;

            using (var en = lineMerger.GetMergedLineStrings().GetEnumerator())
            {
                en.MoveNext();
                merge = (LineString)en.Current;
            }

            if (merge.IsRing)
            {
                var lr = new LinearRing(merge.CoordinateSequence, this.geomFactory);
                var concaveHull = new Polygon(lr, null, this.geomFactory);
                return concaveHull;
            }

            return merge;
        }


    }


}
