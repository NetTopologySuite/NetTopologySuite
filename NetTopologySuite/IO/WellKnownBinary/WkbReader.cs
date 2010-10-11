// Portions copyright 2005 - 2007: Diego Guidi
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of GeoAPI.Net.
// GeoAPI.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeoAPI.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

#region Namespace Imports

using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using NPack.Interfaces;

#endregion

namespace NetTopologySuite.IO.WellKnownBinary
{
    /// <summary>
    /// Converts data encoded in Well-Known Binary format to corresponding 
    /// <see cref="IGeometry{TCoordinate}"/> instances.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of coordinate to use.</typeparam>
    public class WkbReader<TCoordinate> : IWkbReader<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        /// <summary>
        /// Converts the given data encoded in Well-Known Binary format to the 
        /// <see cref="IGeometry{TCoordinate}"/> it represents.
        /// </summary>
        /// <param name="wkbData">
        /// The data to decode and represent as an <see cref="IGeometry{TCoordinate}"/>.
        /// </param>
        /// <param name="geoFactory">
        /// A <see cref="IGeometryFactory{TCoordinate}"/> used to create
        /// <see cref="IGeometry{TCoordinate}"/> instances.
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry{TCoordinate}"/> represented by <paramref name="wkbData"/>.
        /// </returns>
        public static IGeometry<TCoordinate> ToGeometry(Byte[] wkbData, 
                                                        IGeometryFactory<TCoordinate> geoFactory)
        {
            return GeometryFromWkb.Parse(wkbData, geoFactory);
        }

        /// <summary>
        /// Converts the given data encoded in Well-Known Binary format to the 
        /// <see cref="IGeometry{TCoordinate}"/> it represents.
        /// </summary>
        /// <param name="wkbData">
        /// A stream of data to decode and represent as an <see cref="IGeometry{TCoordinate}"/>.
        /// </param>
        /// <param name="geoFactory">
        /// A <see cref="IGeometryFactory{TCoordinate}"/> used to create
        /// <see cref="IGeometry{TCoordinate}"/> instances.
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry{TCoordinate}"/> represented by the data obtained from
        /// <paramref name="wkbData"/>.
        /// </returns>
        public static IGeometry<TCoordinate> ToGeometry(Stream wkbData, 
                                                        IGeometryFactory<TCoordinate> geoFactory)
        {
            return GeometryFromWkb.Parse(wkbData, geoFactory);
        }

        /// <summary>
        /// Converts the given data encoded in Well-Known Binary format to the 
        /// <see cref="IGeometry{TCoordinate}"/> it represents.
        /// </summary>
        /// <param name="wkbData">
        /// A <see cref="BinaryReader"/> which accesses data to decode 
        /// and represent as an <see cref="IGeometry{TCoordinate}"/>.
        /// </param>
        /// <param name="geoFactory">
        /// A <see cref="IGeometryFactory{TCoordinate}"/> used to create
        /// <see cref="IGeometry{TCoordinate}"/> instances.
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry{TCoordinate}"/> represented by the data obtained from
        /// <paramref name="wkbData"/>.
        /// </returns>
        public static IGeometry<TCoordinate> ToGeometry(BinaryReader wkbData, 
                                                        IGeometryFactory<TCoordinate> geoFactory)
        {
            return GeometryFromWkb.Parse(wkbData, geoFactory);
        }

        #region Private instance fields

        private IGeometryFactory<TCoordinate> _geoFactory; 
        #endregion

        #region Object construction / disposal
        /// <summary>
        /// Creates a new instance of a <see cref="WkbReader{TCoordinate}"/> with
        /// the given <see cref="IGeometryFactory{TCoordinate}"/> used to create
        /// <see cref="IGeometry{TCoordinate}"/> instances.
        /// </summary>
        /// <param name="geoFactory">
        /// The <see cref="IGeometryFactory{TCoordinate}"/> used to create instances
        /// of <see cref="IGeometry{TCoordinate}"/> from the Well-Known Binary data.
        /// </param>
        public WkbReader(IGeometryFactory<TCoordinate> geoFactory)
        {
            _geoFactory = geoFactory;
        }
        #endregion

        #region IWkbDecoder Members

        /// <summary>
        /// Gets the <see cref="IGeometryFactory{TCoordinate}"/> used to create instances
        /// of <see cref="IGeometry{TCoordinate}"/> from the Well-Known Binary data.
        /// </summary>
        public IGeometryFactory<TCoordinate> GeometryFactory
        {
            set { _geoFactory = value; }
        }

        /// <summary>
        /// Converts the given data encoded in Well-Known Binary format to the 
        /// <see cref="IGeometry{TCoordinate}"/> it represents.
        /// </summary>
        /// <param name="wkb">
        /// The data to decode and represent as an <see cref="IGeometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry{TCoordinate}"/> represented by <paramref name="wkb"/>.
        /// </returns>
        public IGeometry<TCoordinate> Read(Byte[] wkb)
        {
            return ToGeometry(wkb, _geoFactory);
        }

        /// <summary>
        /// Converts the given data encoded in Well-Known Binary format to the 
        /// <see cref="IGeometry{TCoordinate}"/> it represents, starting at 
        /// <paramref name="offset"/> in <paramref name="wkb"/>.
        /// </summary>
        /// <param name="wkb">
        /// The data to decode and represent as an <see cref="IGeometry{TCoordinate}"/>.
        /// </param>
        /// <param name="offset">
        /// The offset into <paramref name="wkb"/> at which to start decoding.
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry{TCoordinate}"/> represented by <paramref name="wkb"/>.
        /// </returns>
        public IGeometry<TCoordinate> Read(Byte[] wkb, Int32 offset)
        {
            Int32 inputLength = wkb.Length - offset;
            Byte[] wkbOffset = new Byte[inputLength];
            Buffer.BlockCopy(wkb, offset, wkbOffset, 0, inputLength);

            return ToGeometry(wkbOffset, _geoFactory);
        }

        /// <summary>
        /// Converts the given data encoded in Well-Known Binary format to the 
        /// <see cref="IGeometry{TCoordinate}"/> it represents.
        /// </summary>
        /// <param name="wkb">
        /// The data to decode and represent as an <see cref="IGeometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry{TCoordinate}"/> represented by <paramref name="wkb"/>.
        /// </returns>
        public IGeometry<TCoordinate> Read(IEnumerable<Byte> wkb)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts data accessed via the given <see cref="Stream"/>, encoded in Well-Known Binary 
        /// format to the <see cref="IGeometry{TCoordinate}"/> it represents.
        /// </summary>
        /// <param name="wkb">
        /// A <see cref="Stream"/> of data to decode and represent as 
        /// an <see cref="IGeometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry{TCoordinate}"/> represented by the data in
        /// <paramref name="wkb"/>.
        /// </returns>
        public IGeometry<TCoordinate> Read(Stream wkb)
        {
            return ToGeometry(wkb, _geoFactory);
        }

        /// <summary>
        /// Converts data accessed via the given <see cref="BinaryReader"/>, encoded in Well-Known Binary 
        /// format to the <see cref="IGeometry{TCoordinate}"/> it represents.
        /// </summary>
        /// <param name="wkb">
        /// A <see cref="BinaryReader"/> accessing data to decode and represent as 
        /// an <see cref="IGeometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry{TCoordinate}"/> represented by the data accessed through
        /// <paramref name="wkb"/>.
        /// </returns>
        public IGeometry<TCoordinate> Read(BinaryReader wkb)
        {
            return ToGeometry(wkb, _geoFactory);
        }

        public IEnumerable<IGeometry<TCoordinate>> ReadAll(Byte[] wkb)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IGeometry<TCoordinate>> ReadAll(Byte[] wkb, Int32 offset)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IGeometry<TCoordinate>> ReadAll(IEnumerable<Byte> wkb)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IGeometry<TCoordinate>> ReadAll(BinaryReader wkbData)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IWkbReader Members

        IGeometryFactory IWkbReader.GeometryFactory
        {
            set
            {
                IGeometryFactory<TCoordinate> factory = value as IGeometryFactory<TCoordinate>;

                if (factory == null)
                {
                    throw new ArgumentException("Value must be a non-null " +
                                                "IGeometryFactory<TCoordinate> instance.");
                }
            }
        }

        IGeometry IWkbReader.Read(Byte[] wkb)
        {
            return Read(wkb);
        }

        IGeometry IWkbReader.Read(Byte[] wkb, Int32 offset)
        {
            return Read(wkb, offset);
        }

        IGeometry IWkbReader.Read(IEnumerable<Byte> wkb)
        {
            return Read(wkb);
        }

        IGeometry IWkbReader.Read(Stream wkbData)
        {
            return Read(wkbData);
        }

        IGeometry IWkbReader.Read(BinaryReader wkbData)
        {
            return Read(wkbData);
        }

        IEnumerable<IGeometry> IWkbReader.ReadAll(Byte[] wkb)
        {
            foreach (IGeometry<TCoordinate> geometry in ReadAll(wkb))
            {
                yield return geometry;
            }
        }

        IEnumerable<IGeometry> IWkbReader.ReadAll(Byte[] wkb, Int32 offset)
        {
            foreach (IGeometry<TCoordinate> geometry in ReadAll(wkb, offset))
            {
                yield return geometry;
            }
        }

        IEnumerable<IGeometry> IWkbReader.ReadAll(IEnumerable<Byte> wkb)
        {
            foreach (IGeometry<TCoordinate> geometry in ReadAll(wkb))
            {
                yield return geometry;
            }
        }

        IEnumerable<IGeometry> IWkbReader.ReadAll(BinaryReader wkbData)
        {
            foreach (IGeometry<TCoordinate> geometry in ReadAll(wkbData))
            {
                yield return geometry;
            }
        }

        #endregion
    }
}