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


using System.Collections.Generic;
using NetTopologySuite.Triangulate.QuadEdge;


namespace NetTopologySuite.Hull
{



    // Comparator of a map containing QuadEdge as key
    // and Double as value (Double comparator).
    // Author: Eric Grosso
    // modified for use in C# List Stefan Steiger 
    internal class DoubleComparator 
        : IComparer<KeyValuePair<QuadEdge, double>>
    {
        Dictionary<QuadEdge, double> map;


        public DoubleComparator(Dictionary<QuadEdge, double> map)
        {
            this.map = map;
        }


        // Method of comparison.
        // qeA: quad edge to compare
        // qeB: quad edge to compare
        // return
        // 		1 if double value associated to qeA  < double
        // 		value associated to qeB,
        // 		0 if values are equals,
        // 		-1 otherwise
        public int Compare(KeyValuePair<QuadEdge, double> qeA, KeyValuePair<QuadEdge, double> qeB)
        {
            if (this.map[qeA.Key] < this.map[qeB.Key])
                return 1;
            else if (this.map[qeA.Key] == this.map[qeB.Key])
                return 0;

            return -1;
        }


    }


}
