using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    public class PostGis2GeometryHeader
    {
        /// <summary>
        /// Three unused bits, followed by ReadOnly, Geodetic, HasBBox, HasM and HasZ flags.
        /// [---RGBMZ]
        /// </summary>
        [Flags]
        private enum PostGis2Flags : uint
        {
            HasZ = 1 << 24,
            HasM = 1 << 25,
            HasZM = HasZ | HasM,
            HasBoundingBox = 1 << 26,
            IsGeodetic = 1 << 27,
            IsReadonly = 1 << 28
        }
///**
//* Macros for manipulating the 'flags' byte. A uint8_t used as follows: 
//* ---RGBMZ
//* Three unused bits, followed by ReadOnly, Geodetic, HasBBox, HasM and HasZ flags.
//*/
//#define FLAGS_GET_Z(flags) ((flags) & 0x01)
//#define FLAGS_GET_M(flags) (((flags) & 0x02)>>1)
//#define FLAGS_GET_BBOX(flags) (((flags) & 0x04)>>2)
//#define FLAGS_GET_GEODETIC(flags) (((flags) & 0x08)>>3)
//#define FLAGS_GET_READONLY(flags) (((flags) & 0x10)>>4)
//#define FLAGS_GET_SOLID(flags) (((flags) & 0x20)>>5)
//#define FLAGS_SET_Z(flags, value) ((flags) = (value) ? ((flags) | 0x01) : ((flags) & 0xFE))
//#define FLAGS_SET_M(flags, value) ((flags) = (value) ? ((flags) | 0x02) : ((flags) & 0xFD))
//#define FLAGS_SET_BBOX(flags, value) ((flags) = (value) ? ((flags) | 0x04) : ((flags) & 0xFB))
//#define FLAGS_SET_GEODETIC(flags, value) ((flags) = (value) ? ((flags) | 0x08) : ((flags) & 0xF7))
//#define FLAGS_SET_READONLY(flags, value) ((flags) = (value) ? ((flags) | 0x10) : ((flags) & 0xEF))
//#define FLAGS_SET_SOLID(flags, value) ((flags) = (value) ? ((flags) | 0x20) : ((flags) & 0xDF))
//#define FLAGS_NDIMS(flags) (2 + FLAGS_GET_Z(flags) + FLAGS_GET_M(flags))
//#define FLAGS_GET_ZM(flags) (FLAGS_GET_M(flags) + FLAGS_GET_Z(flags) * 2)
//#define FLAGS_NDIMS_BOX(flags) (FLAGS_GET_GEODETIC(flags) ? 3 : FLAGS_NDIMS(flags))

        
        #region fields

        public PostGis2GeometryHeader(BinaryReader reader)
{
    _size = reader.ReadUInt32();
    _sridFlags = reader.ReadUInt32();

    if (!HasBoundingBox) return;
    _envelope = new Envelope(reader.ReadDouble(), reader.ReadDouble(),
                             reader.ReadDouble(), reader.ReadDouble());
    if (HasZ || IsGeodetic)
        _zInterval = new[] {reader.ReadDouble(), reader.ReadDouble()};

    if (HasM)
        _mInterval = new[] {reader.ReadDouble(), reader.ReadDouble()};
}

        public PostGis2GeometryHeader(IGeometry geometry)
            : this(geometry, Ordinates.XY)
        { }

        public PostGis2GeometryHeader(IGeometry geometry, Ordinates handleOrdinates)
            : this(geometry, handleOrdinates, false)
        {}

        public PostGis2GeometryHeader(IGeometry geometry, Ordinates handleOrdinates, bool isGeodetic)
        {
            Srid = geometry.SRID;

            HasM = (handleOrdinates & Ordinates.Z) == Ordinates.Z; // geometry.HasM();
            HasZ = (handleOrdinates & Ordinates.M) == Ordinates.M; // geometry.HasZ();

            if (geometry.OgcGeometryType != OgcGeometryType.Point)
            {
                HasBoundingBox = true;
                _envelope = geometry.EnvelopeInternal;
                if (HasM)
                    _mInterval = geometry.GetMRange();
                if (HasZ | isGeodetic)
                    _zInterval = geometry.GetZRange();
            }

            ComputeSize(geometry);
            _factory = geometry.Factory;
        }

        private uint ComputeSize(IGeometry geometry)
        {
            // We start with header size
            uint geometrySize = 8;

            if (HasBoundingBox)
                geometrySize += 2*Dimension*8;
            
            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    geometrySize += 4 + 4 + Dimension*8;
                    break;
                case OgcGeometryType.LineString:
                case OgcGeometryType.CircularString:
                    geometrySize += 4 + 4 + Dimension * 8 * (uint)geometry.NumPoints;
                    break;
                case OgcGeometryType.Polygon:
                    geometrySize += 4 + 4 + ComputePolygonSize((IPolygon) geometry);
                    break;
                default:
                    for (var i = 0; i < geometry.NumGeometries; i++)
                        geometrySize += ComputeSize(geometry.GetGeometryN(i));
                    break;
            }
            return geometrySize;
        }

        private uint ComputePolygonSize(IPolygon polygon)
        {
            if (polygon.NumPoints == 0)
                return 0;

            var rings = 1;
            var points = polygon.Shell.NumPoints;

            for (var i = 0; i < polygon.NumInteriorRings; i++)
            {
                rings += 1;
                points += polygon.GetInteriorRingN(i).NumPoints;
            }

            if ((rings % 2) != 0) rings++;
            return (uint)(4*rings + points*Dimension*8);
        }

        private readonly uint _size;
        private uint _sridFlags;

        private Envelope _envelope;
        private double[] _zInterval;
        private double[] _mInterval;

        private IGeometryFactory _factory;
        
        public Ordinates Ordinates
        {
            get
            {
                var res = Ordinates.XY;
                if (HasZ) res |= Ordinates.Z;
                if (HasM) res |= Ordinates.M;
                
                return res;
            }
        }

        public Ordinate[] OrdinateIndices
        {
            get 
            { 
                var lst = new List<Ordinate>(new[] {Ordinate.X, Ordinate.Y,});
                
                if (HasZ) lst.Add(Ordinate.Z);
                if (HasM) lst.Add(Ordinate.M);

                return lst.ToArray();
            }
        }
        
        #endregion
        
        public uint Dimension
        {
            get
            {
                return IsGeodetic
                           ? 3
                           : (uint)(2 + (HasZ ? 1 : 0) + (HasM ? 1 : 0));
            }
        }

        /// <summary>
        /// Gets or sets the spatial reference id
        /// </summary>
        public int Srid
        {
            get { return (int) (_sridFlags & 0xffffff); }
            set
            {
                if (value != Srid)
                    _sridFlags = (uint) value | (uint)Flags; 
            }
        }

        private PostGis2Flags Flags { get { return (PostGis2Flags)(_sridFlags & 0xff000000); } }

        public void Lock()
        {
            _sridFlags = _sridFlags | (uint) PostGis2Flags.IsReadonly;
        }

        public void UnLock()
        {
            _sridFlags = _sridFlags & ~(uint)PostGis2Flags.IsReadonly;
        }

        public bool IsReadonly
        {
            get { return (Flags | PostGis2Flags.IsReadonly) == PostGis2Flags.IsReadonly; }
        }

        public bool HasBoundingBox
        {
            get { return (Flags | PostGis2Flags.HasBoundingBox) == PostGis2Flags.HasBoundingBox; }
            set 
            {
                if (IsReadonly)
                    throw new InvalidOperationException("IsReadonly");

                _sridFlags = _sridFlags & ~(uint)PostGis2Flags.HasBoundingBox;
                if (value)
                    _sridFlags = _sridFlags | (uint)PostGis2Flags.HasBoundingBox;
            }
        }

        /// <summary>
        /// Gets or sets whether M ordinates are present
        /// </summary>
        public bool HasM
        {
            get { return (Flags | PostGis2Flags.HasM) == PostGis2Flags.HasM; }
            set
            {
                if (IsReadonly)
                    throw new InvalidOperationException("IsReadonly");

                _sridFlags = _sridFlags & ~(uint)PostGis2Flags.HasM;
                if (value)
                    _sridFlags = _sridFlags | (uint)PostGis2Flags.HasM;
            }
        }

        /// <summary>
        /// Gets or sets whether Z ordinates are present
        /// </summary>
        public bool HasZ
        {
            get { return (Flags | PostGis2Flags.HasZ) == PostGis2Flags.HasZ; }
            set
            {
                if (IsReadonly)
                    throw new InvalidOperationException("IsReadonly");

                _sridFlags = _sridFlags & ~(uint)PostGis2Flags.HasZ;
                if (value)
                    _sridFlags = _sridFlags | (uint)PostGis2Flags.HasZ;
            }
        }

        /// <summary>
        /// Gets or sets whether Z and M ordinates are present
        /// </summary>
        public bool HasZM
        {
            get { return (Flags | PostGis2Flags.HasZM) == PostGis2Flags.HasZM; }
            set
            {
                if (IsReadonly)
                    throw new InvalidOperationException("IsReadonly");

                _sridFlags = _sridFlags & ~(uint)PostGis2Flags.HasZM;
                if (value)
                    _sridFlags = _sridFlags | (uint)PostGis2Flags.HasZM;
            }
        }

        /// <summary>
        /// Gets or sets whether geometry is geodetic
        /// </summary>
        public bool IsGeodetic
        {
            get { return (Flags | PostGis2Flags.IsGeodetic) == PostGis2Flags.IsGeodetic; }
            set
            {
                if (IsReadonly)
                    throw new InvalidOperationException("IsReadonly");

                _sridFlags = _sridFlags & ~(uint)PostGis2Flags.HasZM;
                if (value)
                    _sridFlags = _sridFlags | (uint)PostGis2Flags.HasZM;
            }
        }

        //private void Read(BinaryReader reader)
        //{
        //    _size = reader.ReadUInt32();
        //    _sridFlags = reader.ReadUInt32();

        //    if (HasBoundingBox)
        //    {
        //        _envelope = new Envelope(reader.ReadDouble(), reader.ReadDouble(), 
        //                                 reader.ReadDouble(), reader.ReadDouble());
        //        if (HasZ || IsGeodetic)
        //            _zInterval = new [] { reader.ReadDouble(), reader.ReadDouble() };

        //        if (HasM)
        //            _mInterval = new [] { reader.ReadDouble(), reader.ReadDouble() };
        //    }
        //}

        public void Write(BinaryWriter writer)
        {
            writer.Write(_size);
            writer.Write(_sridFlags);
            if (!HasBoundingBox) return;
            
            // xy
            writer.Write(_envelope.MinX);
            writer.Write(_envelope.MaxX);
            writer.Write(_envelope.MinY);
            writer.Write(_envelope.MaxY);

            // z
            if (HasZ || IsGeodetic)
            {
                writer.Write(_zInterval[0]);
                writer.Write(_zInterval[0]);
            }

            // m
            if (HasM)
            {
                writer.Write(_mInterval[0]);
                writer.Write(_mInterval[0]);
            }
        }

        private static PostGis2Flags Not(PostGis2Flags flags)
        {
            return (PostGis2Flags) ~((uint) flags);
        }

        public IGeometryFactory Factory
        {
            get { return _factory ?? (_factory = GeometryServiceProvider.Instance.CreateGeometryFactory(Srid)); }
        }
    }
}
