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
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Data;

namespace GisSharpBlog.NetTopologySuite.Shapefile
{
    internal static class DbaseSchema
    {
        internal static readonly String OidColumnName = "OID";

        internal static ICollection<DbaseField> GetFields(ISchema schema, DbaseHeader header)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }

            List<DbaseField> fields = new List<DbaseField>();

            Int32 offset = 1;
            Int32 index = 0;
            foreach (IPropertyInfo rowView in schema.Properties)
            {


                String colName = rowView.Name;
                Type dataType = rowView.PropertyType;
                Int16 length = rowView is IStringPropertyInfo ? (short)((IStringPropertyInfo)rowView).MaxLength : (short)0;
                Byte decimals = rowView is IDecimalPropertyInfo ? Convert.ToByte(((IDecimalPropertyInfo)rowView).Precision) : (byte)0;
                Int32 ordinal = index;

                DbaseField field = new DbaseField(header, colName, dataType, length, decimals, ordinal, offset);

                fields.Add(field);

                offset += field.Length;
                index++;
            }

            return fields;
        }

        internal static ISchema GetFeatureTableForFields(ISchemaFactory schemaFactory, IEnumerable<DbaseField> dbaseColumns, IGeometryFactory factory)
        {
            List<IPropertyInfo> propertyInfos = new List<IPropertyInfo>();
            propertyInfos.Add(schemaFactory.PropertyFactory.Create<uint>("OID"));
            propertyInfos.Add(schemaFactory.PropertyFactory.Create<IGeometry>("Geom"));
            foreach (DbaseField dbf in dbaseColumns)
            {

                IPropertyInfo propertyInfo = schemaFactory.PropertyFactory.Create(dbf.DataType, dbf.ColumnName);
                propertyInfos.Add(propertyInfo);


                if (dbf.DataType == typeof(String))
                {
                    ((IStringPropertyInfo)propertyInfo).MaxLength = dbf.Length;
                }

                if (dbf.Decimals > 0)
                    ((IDecimalPropertyInfo)propertyInfo).Precision = dbf.Decimals;
            }

            return schemaFactory.Create(propertyInfos, propertyInfos.First(a => a.Name == "OID"));
        }

        internal static Char GetFieldTypeCode(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return 'L';
                case TypeCode.DateTime:
                    return 'D';
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return 'N';
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    return 'F';
                case TypeCode.Char:
                case TypeCode.String:
                    return 'C';
                case TypeCode.Object: //jd: added to handle Guid
                    {
                        if (type == typeof(Guid))
                            return 'C';
                        throw new NotSupportedException("Type is not supported");
                    }
                default:
                    throw new NotSupportedException("Type is not supported");
            }
        }

        internal static ISchema DeriveSchemaTable(IRecordSource model)
        {
            // UNDONE: the precision computation delegate should not be null
            return model.Schema;
        }

        internal static ISchema DeriveSchemaTable(ISchema model)
        {
            // UNDONE: the precision computation delegate should not be null
            return model;
        }

        private static Int32 getLengthByHeuristic(IPropertyInfo column)
        {
            switch (Type.GetTypeCode(column.PropertyType))
            {
                case TypeCode.Boolean:
                    return 1;
                case TypeCode.DateTime:
                    return 8;
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return 18;
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    return 20;
                case TypeCode.Char:
                case TypeCode.String:
                    return 254;
                case TypeCode.Object: //added by JD to handle Guid
                    {
                        if (column.PropertyType == typeof(Guid))
                            return 40;
                        throw new NotSupportedException("Type is not supported");
                    }
                default:
                    throw new NotSupportedException("Type is not supported");
            }
        }
    }
}
