using System;

namespace NetTopologySuite.IO
{
	/// <summary>
	/// Class for holding the information assicated with a dbase field.
	/// </summary>
	public class DbaseFieldDescriptor
	{
		// Field Name

	    // Field Type (C N L D or M)

	    // Field Data Address offset from the start of the record.

	    // Length of the data in bytes

	    // Field decimal count in Binary, indicating where the decimal is

	    /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public static char GetDbaseType(Type type)
		{
            if (type == typeof(Char))
                return 'C';
            if (type == typeof(string))
                return 'C';
            if (type == typeof(Double))
                return 'N';
            if (type == typeof(Single))
                return 'N';
            if (type == typeof(Int16))
                return 'N';
            if (type == typeof(Int32))
                return 'N';
            if (type == typeof(Int64))
                return 'N';
            if (type == typeof(UInt16))
                return 'N';
            if (type == typeof(UInt32))
                return 'N';
            if (type == typeof(UInt64))
                return 'N';
            if (type == typeof(Decimal))
                return 'N';
            if (type == typeof(Boolean))
                return 'L';
            if (type == typeof(DateTime))
                return 'D';

			throw new NotSupportedException(String.Format("{0} does not have a corresponding dbase type.", type.Name));
		}

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
		public static DbaseFieldDescriptor ShapeField()
		{
			DbaseFieldDescriptor shpfield = new DbaseFieldDescriptor();
			shpfield.Name="Geometry";
			shpfield.DbaseType='B';
			return shpfield;
		}

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
		public static DbaseFieldDescriptor IdField()
		{
			DbaseFieldDescriptor shpfield = new DbaseFieldDescriptor();
			shpfield.Name="Row";
			shpfield.DbaseType='I';
			return shpfield;
		}

        /// <summary>
        /// Field Name.
        /// </summary>
		public string Name { get; set; }

	    /// <summary>
        /// Field Type (C N L D or M).
        /// </summary>
		public char DbaseType { get; set; }

	    /// <summary>
        /// Field Data Address offset from the start of the record.
        /// </summary>
		public int DataAddress { get; set; }

	    /// <summary>
        /// Length of the data in bytes.
        /// </summary>
		public int Length { get; set; }

	    /// <summary>
        /// Field decimal count in Binary, indicating where the decimal is.
        /// </summary>
		public int DecimalCount { get; set; }

	    /// <summary>
		/// Returns the equivalent CLR type for this field.
		/// </summary>
		public Type Type
		{
			get
			{
				switch (DbaseType)
				{
					case 'L': // logical data type, one character (T,t,F,f,Y,y,N,n)
						return typeof(bool);
					case 'C': // char or string
						return typeof(string);
					case 'D': // date
						return typeof(DateTime);
					case 'N': // numeric
				        if (DecimalCount == 0)
				        {
				            if (Length < 10)
				                return typeof (int);
				            return typeof (long);
				        }

						return typeof(double);
					case 'F': // double
						return typeof(float);
                    case 'I':
				        return typeof (int);
					case 'B': // BLOB - not a dbase but this will hold the WKB for a geometry object.
						return typeof(byte[]);
					default:
						throw new NotSupportedException("Do not know how to parse Field type "+DbaseType);
				}
			}
		}

	    public override string ToString()
	    {
	        return string.Format("[ Field '{0}' ('{1}', {2}) ]", Name.TrimEnd(), DbaseType, Length);
	    }
	}
}
