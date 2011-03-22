// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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

namespace GisSharpBlog.NetTopologySuite.Shapefile
{
	/// <summary>
	/// Represents invariant shapefile values, offsets and lengths
	/// derived from the shapefile specification.
	/// </summary>
	internal class ShapeFileConstants
	{
		/// <summary>
		/// Size, in bytes, of the shapefile header region.
		/// </summary>
		public const Int32 HeaderSizeBytes = 100;

		/// <summary>
		/// The first value in any shapefile.
		/// </summary>
		public const Int32 HeaderStartCode = 9994;

		/// <summary>
		/// The version of any valid shapefile.
		/// </summary>
		public const Int32 VersionCode = 1000;

		/// <summary>
		/// The number of bytes in a shapefile record header 
		/// (per-record preamble).
		/// </summary>
        public const Int32 ShapeRecordHeaderByteLength = 8;

        /// <summary>
        /// The number of bytes in a shapefile record header 
        /// (per-record preamble) Content Length field.
        /// </summary>
        public const Int32 ShapeRecordContentLengthByteLength = 4;

		/// <summary>
		/// The number of bytes in a record header (per-record preamble)
		/// in a shapefile's index file.
		/// </summary>
		public const Int32 IndexRecordByteLength = 8;

		/// <summary>
		/// The number of bytes used to store the bounding box, or 
		/// extents, for the shapefile.
		/// </summary>
        public const Int32 BoundingBoxFieldByteLength = 32;

		/// <summary>
		/// The name given to the row identifier in a ShapeFileProvider.
		/// </summary>
		public static readonly String IdColumnName = "OID";

        /// <summary>
        /// The value assigned to null in shape records
        /// </summary>
	    public const double NullDoubleValue = -10E40;
	    
	}
}
