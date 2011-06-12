// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Proj.Net.
// Proj.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Proj.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Proj.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GeoAPI.CoordinateSystems;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// The Projection class defines the standard information stored with a projection
    /// objects. A projection object implements a coordinate transformation from a geographic
    /// coordinate system to a projected coordinate system, given the ellipsoid for the
    /// geographic coordinate system. It is expected that each coordinate transformation of
    /// interest, e.g., Transverse Mercator, Lambert, will be implemented as a class of
    /// type Projection, supporting the IProjection interface.
    /// </summary>
    public class Projection : Info, IProjection
    {
        private readonly List<ProjectionParameter> _parameters;
        private readonly String _className;

        internal Projection(String className, IEnumerable<ProjectionParameter> parameters,
                            String name, String authority, String authorityCode, String alias,
                            String abbreviation, String remarks)
            : base(name, authority, authorityCode, alias, abbreviation, remarks)
        {
            _parameters = new List<ProjectionParameter>(parameters);
            _className = className;
        }

        #region Predefined projections

        #endregion

        #region IProjection Members

        /// <summary>
        /// Gets the number of parameters of the projection.
        /// </summary>
        public Int32 ParameterCount
        {
            get { return _parameters.Count; }
        }

        /// <summary>
        /// Gets or sets the parameters of the projection
        /// </summary>
        internal IList<ProjectionParameter> Parameters
        {
            get { return _parameters.AsReadOnly(); }
            set
            {
                _parameters.Clear();
                _parameters.AddRange(value);
            }
        }

        /// <summary>
        /// Gets an indexed parameter of the projection.
        /// </summary>
        /// <param name="index">Index of parameter.</param>
        /// <returns>The parameter at <paramref name="index"/>.</returns>
        public ProjectionParameter this[Int32 index]
        {
            get { return _parameters[index]; }
        }

        /// <summary>
        /// Gets an named parameter of the projection.
        /// </summary>
        /// <remarks>The parameter name is case insensitive</remarks>
        /// <param name="name">Name of parameter</param>
        /// <returns>parameter or null if not found</returns>
        public ProjectionParameter this[String name]
        {
            get
            {
                return
                    _parameters.Find(
                        delegate(ProjectionParameter par) { return par.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
            }
        }

        /// <summary>
        /// Gets the projection classification name (e.g. "Transverse_Mercator").
        /// </summary>
        public String ProjectionClassName
        {
            get { return _className; }
        }

        /// <summary>
        /// Returns the Well-Known Text for this object
        /// as defined in the simple features specification.
        /// </summary>
        public override String Wkt
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("PROJECTION[\"{0}\"", Name);

                if (!String.IsNullOrEmpty(Authority) && String.IsNullOrEmpty(AuthorityCode))
                {
                    sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);
                }

                sb.Append("]");
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets an XML representation of this object
        /// </summary>
        public override String Xml
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat,
                                "<CS_Projection Classname=\"{0}\">{1}", ProjectionClassName, InfoXml);

                foreach (ProjectionParameter param in Parameters)
                {
                    sb.Append(param.Xml);
                }

                sb.Append("</CS_Projection>");
                return sb.ToString();
            }
        }

        public override Boolean EqualParams(IInfo other)
        {
            Projection p = other as Projection;

            if (ReferenceEquals(p, null))
            {
                return false;
            }

            if (p.ParameterCount != ParameterCount)
            {
                return false;
            }

            foreach (ProjectionParameter parameter in _parameters)
            {
                ProjectionParameter found = p._parameters.Find(
                    delegate(ProjectionParameter seek) { return seek.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase); });

                if (found == null)
                {
                    return false;
                }

                if (found.Value != parameter.Value)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IEnumerable<ProjectionParameter> Members

        public IEnumerator<ProjectionParameter> GetEnumerator()
        {
            foreach (ProjectionParameter parameter in _parameters)
            {
                yield return parameter;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}