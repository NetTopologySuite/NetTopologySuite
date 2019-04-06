using System;
using GeoAPI.Geometries;

namespace GeoAPI.IO
{
    /// <summary>
    /// Lightweight class that handles OGC Geometry type declaration
    /// </summary>
    [CLSCompliant(false)]
    public struct GeometryType
    {
        /// <summary>
        /// Initializes this instance
        /// </summary>
        /// <param name="geometryType">The value describing the <see cref="GeometryType"/></param>
        public GeometryType(uint geometryType)
        {
            _geometrytype = geometryType;
        }

        /// <summary>
        /// Inititalizes this instance based on a geometry and an Ordinates flag.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="ordinates">The ordinates flag.</param>
        public GeometryType(IGeometry geometry, Ordinates ordinates)
            : this(geometry.OgcGeometryType, ordinates, geometry.SRID >= 0)
        {
        }

        /// <summary>
        /// Inititalizes this instance based on an <see cref="OgcGeometryType"/>
        /// </summary>
        /// <param name="ogcGeometryType">The OGC geometry type</param>
        public GeometryType(OgcGeometryType ogcGeometryType)
            : this(ogcGeometryType, Ordinates.XY, false)
        {

        }

        /// <summary>
        /// Inititalizes this instance based on an <see cref="OgcGeometryType"/> and an SRID indicator
        /// </summary>
        /// <param name="ogcGeometryType">The OGC geometry type</param>
        /// <param name="hasSrid">Indicator if a SRID is supplied.</param>
        public GeometryType(OgcGeometryType ogcGeometryType, bool hasSrid)
            :this(ogcGeometryType, Ordinates.XY, hasSrid)
        {
        }

        /// <summary>
        /// Inititalizes this instance based on an <see cref="OgcGeometryType"/> and an SRID indicator
        /// </summary>
        /// <param name="ogcGeometryType">The OGC geometry type</param>
        /// <param name="ordinates">The ordinates flag.</param>
        /// <param name="hasSrid">Indicator if a SRID is supplied.</param>
        public GeometryType(OgcGeometryType ogcGeometryType, Ordinates ordinates, bool hasSrid)
        {
            _geometrytype = (uint) ogcGeometryType;
            
            if ((ordinates & Ordinates.Z) != 0)
            {
                HasWkbZ = true;
                HasEwkbM = true;
            }

            if ((ordinates & Ordinates.M) != 0)
            {
                HasWkbZ = true;
                HasEwkbM = true;
            }

            HasEwkbSrid = hasSrid;
        }

        private uint _geometrytype;

        /// <summary>
        /// Gets or sets the base geometry type
        /// </summary>
        public OgcGeometryType BaseGeometryType
        {
            get
            {
                //Leave out Ewkb flags
                var val = _geometrytype & 0xffffff;
                if (val > 2000) val -= 2000;
                if (val > 1000) val -= 1000;
                return (OgcGeometryType) val;
            }
            set
            {
                var ewkbFlags = _geometrytype & EwkbFlags;
                var newGeometryType = (uint) value;
                if (HasWkbZ) newGeometryType += 1000;
                if (HasWkbM) newGeometryType += 2000;
                _geometrytype = ewkbFlags | newGeometryType;
            }
        }

        /// <summary>
        /// Gets the OGC Well-Known-Binary type code
        /// </summary>
        public int WkbGeometryType
        {
            get { return (int) (_geometrytype & 0x1ffffff); }
        }

        /// <summary>
        /// Gets the PostGIS Enhanced Well-Known-Binary type code
        /// </summary>
        public int EwkbWkbGeometryType
        {
            get
            {
                return (int) ((uint) BaseGeometryType | (_geometrytype & 0xfe000000));
            }
        }

        /// <summary>
        /// Gets or sets whether z-ordinate values are stored along with the geometry.
        /// </summary>
        public bool HasZ { get { return HasWkbZ | HasEwkbZ; } }
        
        /// <summary>
        /// Gets or sets whether m-ordinate values are stored along with the geometry.
        /// </summary>
        public bool HasM { get { return HasWkbM | HasEwkbM; } }

        /// <summary>
        /// Gets whether SRID value is stored along with the geometry.
        /// </summary>
        public bool HasSrid { get { return HasEwkbSrid; } }

        /// <summary>
        /// Gets or sets whether z-ordinate values are stored along with the geometry.
        /// </summary>
        public bool HasWkbZ
        {
            get { return (_geometrytype/1000) == 1; }
            set
            {
                if (value == HasWkbZ)
                    return;
                if (HasWkbZ)
                    _geometrytype -= 1000;
                else
                    _geometrytype += 1000;

            }
        }

        /// <summary>
        /// Gets or sets whether m-ordinate values are stored along with the geometry.
        /// </summary>
        public bool HasWkbM
        {
            get { return (_geometrytype/2000) == 2; }
            set
            {
                if (value == HasWkbM)
                    return;
                if (HasWkbM)
                    _geometrytype -= 2000;
                else
                    _geometrytype += 2000;

            }
        }

        #region PostGis EWKB/EWKT
        
        private const uint EwkbZFlag = 0x8000000;
        private const uint EwkbMFlag = 0x4000000;
        private const uint EwkbSridFlag = 0x2000000;

        private const uint EwkbFlags = EwkbZFlag | EwkbMFlag | EwkbSridFlag;

        /// <summary>
        /// Gets or sets whether z-ordinates are stored along with the geometry.
        /// <para>PostGis EWKB format.</para>
        /// </summary>
        public bool HasEwkbZ
        {
            get { return (_geometrytype & EwkbZFlag) == EwkbZFlag; }
            set
            {
                var gt = _geometrytype & (~EwkbZFlag);
                if (value)
                    gt = _geometrytype | EwkbZFlag;
                _geometrytype = gt;
            }
        }

        /// <summary>
        /// Gets or sets whether z-ordinates are stored along with the geometry.
        /// <para>PostGis EWKB format.</para>
        /// </summary>
        public bool HasEwkbM
        {
            get { return (_geometrytype & EwkbMFlag) == EwkbMFlag; }
            set
            {
                var gt = _geometrytype & (~EwkbMFlag);
                if (value)
                    gt = _geometrytype | EwkbMFlag;
                _geometrytype = gt;
            }
        }

        /// <summary>
        /// Gets or sets whether z-ordinates are stored along with the geometry.
        /// <para>PostGis EWKB format.</para>
        /// </summary>
        public bool HasEwkbSrid
        {
            get { return (_geometrytype & EwkbSridFlag) == EwkbSridFlag; }
            set { 
                var gt = _geometrytype & (~EwkbSridFlag);
                if (value)
                    gt = _geometrytype | EwkbSridFlag;
                _geometrytype = gt;
            }
        }

        #endregion
    }
}