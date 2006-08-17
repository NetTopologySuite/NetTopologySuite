// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Converters.WellKnownText
{
	/// <summary>
	/// Converts spatial reference IDs to a Well-Known Text representation.
	/// </summary>
	public class SpatialReference
	{
		/// <summary>
		/// Converts a Spatial Reference ID to a Well-known Text representation
		/// </summary>
		/// <param name="srid">Spatial Reference ID</param>
		/// <returns>Well-known text</returns>
		public static string SridToWkt(int srid)
		{
			
			System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();
			
			string file = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase) + "\\SpatialRefSys.xml";		
			xmldoc.Load(file);			
			System.Xml.XmlNode node = xmldoc.DocumentElement.SelectSingleNode("/SpatialReference/ReferenceSystem[SRID='" + srid.ToString() + "']");
			if(node!=null)
				return node.LastChild.InnerText;
			else
				return "";
		}
	}
}
