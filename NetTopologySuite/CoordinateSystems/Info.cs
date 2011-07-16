// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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
using System.Text;
using GeoAPI.CoordinateSystems;

namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// The <see cref="Info"/> class implements the 
    /// <see cref="IInfo"/> interface and provides common implementation
    /// for spatial reference objects.
    /// </summary>
    public abstract class Info : IInfo
    {
        private readonly String _abbreviation;
        private readonly String _alias;
        private readonly String _authority;
        private readonly String _authorityCode;
        private readonly String _name;
        private readonly String _remarks;

        /// <summary>
        /// A base interface for metadata applicable to coordinate system objects.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The metadata items 'Abbreviation', 'Alias', 'Authority', 'AuthorityCode', 
        /// 'Name' and 'Remarks' were specified in the Simple Features interfaces, 
        /// so they have been kept here.
        /// </para>
        /// <para>
        /// This specification does not dictate what the contents of these 
        /// items should be. However, the following guidelines are suggested:
        /// </para>
        /// <para>
        /// When <see cref="ICoordinateSystemAuthorityFactory{TCoordinate}"/> 
        /// is used to create an object, the 'Authority' and 'AuthorityCode' values 
        /// should be set to the authority name of the factory object, and the 
        /// authority code supplied by the client, respectively. The other values 
        /// may or may not be set. (If the authority is EPSG, the implementer may 
        /// consider using the corresponding metadata values in the EPSG tables.)
        /// </para>
        /// <para>
        /// When <see cref="CoordinateSystemFactory{TCoordinate}"/> creates an 
        /// object, the 'Name' should be set to the value supplied by the client. 
        /// All of the other metadata items should be left empty.
        /// </para>
        /// </remarks>
        /// <param name="name">Spatial reference object name.</param>
        /// <param name="authority">
        /// Spatial reference object authority name.
        /// </param>
        /// <param name="authorityCode">
        /// Spatial reference object authority-specific identification code.
        /// </param>
        /// <param name="alias">Spatial reference object alternative name.</param>
        /// <param name="abbreviation">Spatial reference object abbreviation.</param>
        /// <param name="remarks">Provider-supplied remarks</param>
        internal Info(String name, 
                      String authority, 
                      String authorityCode, 
                      String alias,
                      String abbreviation, 
                      String remarks)
        {
            _name = name;
            _authority = authority;
            _authorityCode = authorityCode;
            _alias = alias;
            _abbreviation = abbreviation;
            _remarks = remarks;
        }

        #region ISpatialReferenceInfo Members

        public String Name
        {
            get { return _name; }
        }

        public String Authority
        {
            get { return _authority; }
        }

        public String AuthorityCode
        {
            get { return _authorityCode; }
        }

        public String Alias
        {
            get { return _alias; }
        }

        public String Abbreviation
        {
            get { return _abbreviation; }
        }

        public String Remarks
        {
            get { return _remarks; }
        }

        public override String ToString()
        {
            return Wkt;
        }

        public abstract String Wkt { get; }

        public abstract String Xml { get; }

        /// <summary>
        /// Returns an XML String of the info object
        /// </summary>
        internal String InfoXml
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<CS_Info");

                if (String.IsNullOrEmpty(AuthorityCode))
                {
                    sb.AppendFormat(" AuthorityCode=\"{0}\"", AuthorityCode);
                }

                if (!String.IsNullOrEmpty(Abbreviation))
                {
                    sb.AppendFormat(" Abbreviation=\"{0}\"", Abbreviation);
                }

                if (!String.IsNullOrEmpty(Authority))
                {
                    sb.AppendFormat(" Authority=\"{0}\"", Authority);
                }

                if (!String.IsNullOrEmpty(Name))
                {
                    sb.AppendFormat(" Name=\"{0}\"", Name);
                }

                sb.Append("/>");
                return sb.ToString();
            }
        }

        /// <summary>
        /// Checks whether the values of this instance is equal to the values 
        /// of another instance. Only parameters used for coordinate system 
        /// are used for comparison. <see cref="Name"/>, <see cref="Abbreviation"/>, 
        /// <see cref="Authority"/>, <see cref="Alias"/> and 
        /// <see cref="Remarks"/> are ignored in the comparison.
        /// </summary>
        /// <param name="other">
        /// The <see cref="IInfo"/> object to compare parameters with.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the parameters are equal between the 
        /// <see cref="IInfo"/> objects, <see langword="false"/> otherwise.
        /// </returns>
        public abstract Boolean EqualParams(IInfo other);

        #endregion
    }
}