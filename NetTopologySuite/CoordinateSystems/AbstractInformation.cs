using System;

using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// A class interface for metadata applicable to coordinate system objects.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The metadata items ‘Abbreviation’, ‘Alias’, ‘Authority’, ‘AuthorityCode’, ‘Name’ and ‘Remarks’
	/// were specified in the Simple Features interfaces, so they have been kept here.
	///	This specification does not dictate what the contents of these items should be. However, the
	///	following guidelines are suggested:
	///	</para>
	///	<para>
	///	When ICoordinateSystemAuthorityFactory is used to create an object, the ‘Authority’ and
	///	‘AuthorityCode’ values should be set to the authority name of the factory object, and the authority
	///	code supplied by the client, respectively. The other values may or may not be set. (If the
	///	authority is EPSG, the implementer may consider using the corresponding metadata values in the
	///	EPSG tables.)
	///	</para>
	///	<para>
	///	When ICoordinateSystemFactory creates an object, the ‘Name’ should be set to the value
	///	supplied by the client. All of the other metadata items should be left empty.
	///	</para>
	/// </remarks>
	public class AbstractInformation
	{
		private string _remarks;
		private string _authorityCode;
		private string _authority;
        private string _alias;
        private string _abbreviation;

        /// <summary>
        /// 
        /// </summary>
		protected string _name;	
	
		/// <summary>
		/// Initializes a new instance of the AbstractInformation class.
		/// </summary>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>
		internal AbstractInformation(string remarks, string authority, string authorityCode,
						                string name, string alias, string abbreviation)
		{
			_remarks = remarks;
			_authorityCode = authorityCode;
			_authority = authority;
			_name =  name;
			_alias = alias;
			_abbreviation = abbreviation;
		}
	
		/// <summary>
		/// Gets the provider-supplied remarks.
		/// </summary>
		public string Remarks
		{
			get
			{
				return _remarks;
			}
		}

		/// <summary>
		/// Gets the authority-specific identification code.
		/// </summary>
		public string AuthorityCode
		{
			get
			{
				return _authorityCode;
			}
		}

		/// <summary>
		/// Gets a Well-Known text representation of this object.
		/// </summary>
		public string WKT
		{
			get
			{
				return CoordinateSystemWktWriter.Write(this);
			}
		}

		/// <summary>
		/// Gets the authority name.
		/// </summary>
		/// <remarks>
		/// An Authority is an organization that maintains definitions of Authority Codes. For example the
		/// European Petroleum Survey Group (EPSG) maintains a database of coordinate systems, and
		/// other spatial referencing objects, where each object has a code number ID. For example, the
		/// EPSG code for a WGS84 Lat/Lon coordinate system is ‘4326’.
		/// </remarks>
		public string Authority
		{
			get
			{
				return _authority;
			}
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// Gets the alias.
		/// </summary>
		public string Alias
		{
			get
			{
				return _alias;
			}
		}

		/// <summary>
		/// Gets an XML representation of this object.
		/// </summary>
		public string XML
		{
			get
			{
				return CoordinateSystemXmlWriter.Write(this);
			}
		}

		/// <summary>
		/// Gets the abbreviation.
		/// </summary>
		public string Abbreviation
		{
			get
			{
				return _abbreviation;
			}
		}
	}
}
