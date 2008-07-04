using System;
using System.Diagnostics;
using System.IO;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Class for holding the information assicated with a dbase header.
	/// </summary>
	public class DbaseFileHeader
	{
		// Constant for the size of a record
		private int FileDescriptorSize = 32;
    
		// type of the file, must be 03h
		private int _fileType=0x03;
    
		// Date the file was last updated.
		private DateTime _updateDate; 
    
		// Number of records in the datafile
		private int _numRecords = 0;
    
		// Length of the header structure
		private int _headerLength;
    
		// Length of the records
		private int _recordLength;
    
		// Number of fields in the record.
		private int _numFields;
   
		// collection of header records.
		private DbaseFieldDescriptor[] _fieldDescriptions;

		/// <summary>
		/// Initializes a new instance of the DbaseFileHeader class.
		/// </summary>
		public DbaseFileHeader() { }

		/// <summary>
		/// Return the date this file was last updated.
		/// </summary>
		/// <returns></returns>
		public DateTime LastUpdateDate
		{
			get
			{
				return _updateDate;
			}
		}    

		/// <summary>
		/// Return the number of fields in the records.
		/// </summary>
		/// <returns></returns>
		public int NumFields
		{
			get { return _numFields; }
            set { _numFields = value; }
		}
    
		/// <summary>
		/// Return the number of records in the file.
		/// </summary>
		/// <returns></returns>
		public int NumRecords
		{
            get { return _numRecords; }
            set { _numRecords = value; }
		}
    		
		/// <summary>
		/// Return the length of the records in bytes.
		/// </summary>
		/// <returns></returns>
		public int RecordLength
		{
			get
			{
				return _recordLength;
			}
		}

		/// <summary>
		/// Return the length of the header.
		/// </summary>
		/// <returns></returns>
		public int HeaderLength
		{
			get
			{
				return _headerLength;
			}
		}   

		/// <summary>
		///  Add a column to this DbaseFileHeader.
		/// </summary>
		/// <param name="fieldName">The name of the field to add.</param>
		/// <param name="fieldType">The type is one of (C N L or D) character, number, logical(true/false), or date.</param>
		/// <param name="fieldLength"> The Field length is the total length in bytes reserved for this column.</param>
		/// <param name="decimalCount">The decimal count only applies to numbers(N), and floating point values (F), and refers to the number of characters to reserve after the decimal point.</param>
		public void AddColumn (string fieldName, char fieldType, int fieldLength, int decimalCount)
		{
			if (fieldLength <=0) fieldLength = 1;
			if (_fieldDescriptions == null) _fieldDescriptions = new DbaseFieldDescriptor[0];
			int tempLength = 1;  // the length is used for the offset, and there is a * for deleted as the first byte
			DbaseFieldDescriptor[] tempFieldDescriptors = new DbaseFieldDescriptor[_fieldDescriptions.Length+1];
			for (int i=0; i<_fieldDescriptions.Length; i++)
			{
				_fieldDescriptions[i].DataAddress = tempLength;
				tempLength = tempLength + _fieldDescriptions[i].Length;
				tempFieldDescriptors[i] = _fieldDescriptions[i];
			}
			tempFieldDescriptors[_fieldDescriptions.Length] = new DbaseFieldDescriptor();
			tempFieldDescriptors[_fieldDescriptions.Length].Length = fieldLength;
			tempFieldDescriptors[_fieldDescriptions.Length].DecimalCount = decimalCount;
			tempFieldDescriptors[_fieldDescriptions.Length].DataAddress = tempLength;
        
			// set the field name
			string tempFieldName = fieldName;
			if (tempFieldName == null) tempFieldName = "NoName";
			if (tempFieldName.Length > 11) 
			{
				tempFieldName = tempFieldName.Substring(0,11);
				Trace.Write("FieldName "+fieldName+" is longer than 11 characters, truncating to "+tempFieldName);
			}        
			tempFieldDescriptors[_fieldDescriptions.Length].Name = tempFieldName;
        
			// the field type
			if ((fieldType == 'C') || (fieldType == 'c'))
			{
				tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'C';
				if (fieldLength > 254) Trace.WriteLine("Field Length for "+fieldName+" set to "+fieldLength+" Which is longer than 254, not consistent with dbase III");            
			}
			else if ((fieldType == 'S') || (fieldType == 's'))
			{
				tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'C';
				Trace.WriteLine("Field type for "+fieldName+" set to S which is flat out wrong people!, I am setting this to C, in the hopes you meant character.");
				if (fieldLength >254) Trace.WriteLine("Field Length for "+fieldName+" set to "+fieldLength+" Which is longer than 254, not consistent with dbase III");
				tempFieldDescriptors[_fieldDescriptions.Length].Length = 8;
			}
			else if ((fieldType == 'D') || (fieldType == 'd'))
			{
				tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'D';
				if (fieldLength != 8) Trace.WriteLine("Field Length for "+fieldName+" set to "+fieldLength+" Setting to 8 digets YYYYMMDD");            
				tempFieldDescriptors[_fieldDescriptions.Length].Length = 8;
			}
			else if ((fieldType == 'F') || (fieldType == 'f'))
			{
				tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'F';
				if (fieldLength > 20) Trace.WriteLine("Field Length for "+fieldName+" set to "+fieldLength+" Preserving length, but should be set to Max of 20 not valid for dbase IV, and UP specification, not present in dbaseIII.");
			}
			else if ((fieldType == 'N') || (fieldType == 'n'))
			{
				tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'N';
				if (fieldLength > 18) Trace.WriteLine("Field Length for "+fieldName+" set to "+fieldLength+" Preserving length, but should be set to Max of 18 for dbase III specification.");
				if (decimalCount < 0)
				{
					Trace.WriteLine("Field Decimal Position for "+fieldName+" set to "+decimalCount+" Setting to 0 no decimal data will be saved.");
					tempFieldDescriptors[_fieldDescriptions.Length].DecimalCount = 0;                
				}
				if(decimalCount>fieldLength-1)
				{
					Trace.WriteLine("Field Decimal Position for "+fieldName+" set to "+decimalCount+" Setting to "+(fieldLength-1)+" no non decimal data will be saved.");
					tempFieldDescriptors[_fieldDescriptions.Length].DecimalCount = fieldLength-1;
				}
			}
			else if ((fieldType == 'L') || (fieldType == 'l'))
			{
				tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'L';
				if (fieldLength != 1) Trace.WriteLine("Field Length for "+fieldName+" set to "+fieldLength+" Setting to length of 1 for logical fields.");
				tempFieldDescriptors[_fieldDescriptions.Length].Length = 1;
			}
			else 
			{
				throw new NotSupportedException("Unsupported field type "+fieldType + " For column "+fieldName);
			}
			// the length of a record
			tempLength = tempLength + tempFieldDescriptors[_fieldDescriptions.Length].Length;
        
			// set the new fields.
			_fieldDescriptions = tempFieldDescriptors;
			_headerLength = 33+32*_fieldDescriptions.Length;
			_numFields = _fieldDescriptions.Length;
			_recordLength=tempLength;
		}
	
		/// <summary>
		/// Remove a column from this DbaseFileHeader.
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns>return index of the removed column, -1 if no found.</returns>
		public int RemoveColumn(string fieldName) 
		{
			int retCol = -1;
			int tempLength = 1;
			DbaseFieldDescriptor[] tempFieldDescriptors =
				new DbaseFieldDescriptor[_fieldDescriptions.Length - 1];
			for (int i = 0, j = 0; i < _fieldDescriptions.Length; i++) 
			{
				if (fieldName.ToLower()!=(_fieldDescriptions[i].Name.Trim().ToLower())) 
				{
					// if this is the last field and we still haven't found the
					// named field
					if (i == j && i == _fieldDescriptions.Length - 1) 										
						return retCol;
					tempFieldDescriptors[j] = _fieldDescriptions[i];
					tempFieldDescriptors[j].DataAddress = tempLength;
					tempLength += tempFieldDescriptors[j].Length;
					// only increment j on non-matching fields
					j++;
				}
				else  retCol = i;
			}

			// set the new fields.
			_fieldDescriptions = tempFieldDescriptors;
			_headerLength = 33+32*_fieldDescriptions.Length;
			_numFields = _fieldDescriptions.Length;
			_recordLength = tempLength;

			return retCol;
		}
				
		/// <summary>
		/// Read the header data from the DBF file.
		/// </summary>
		/// <param name="reader">BinaryReader containing the header.</param>
		public void ReadHeader(BinaryReader reader)
		{
			// type of reader.
			_fileType = reader.ReadByte();
			if (_fileType != 0x03)
				throw new NotSupportedException("Unsupported DBF reader Type "+_fileType);
        
			// parse the update date information.
			int year = (int)reader.ReadByte();
			int month = (int)reader.ReadByte();
			int day = (int)reader.ReadByte();
			_updateDate = new DateTime(year + 1900, month, day);
        
			// read the number of records.
			_numRecords = reader.ReadInt32();
        
			// read the length of the header structure.
			_headerLength = reader.ReadInt16();
        
			// read the length of a record
			_recordLength = reader.ReadInt16();
        		
			// skip the reserved bytes in the header.
			//in.skipBytes(20);
			reader.ReadBytes(20);
			// calculate the number of Fields in the header
			_numFields = (_headerLength - FileDescriptorSize -1)/FileDescriptorSize;
        
			// read all of the header records
			_fieldDescriptions = new DbaseFieldDescriptor[_numFields];
			for (int i=0; i<_numFields; i++)
			{
				_fieldDescriptions[i] = new DbaseFieldDescriptor();
            
				// read the field name				
				char[] buffer = new char[11];
				buffer = reader.ReadChars(11);
				string name = new string(buffer);
				int nullPoint = name.IndexOf((char)0);
				if(nullPoint != -1)
					name = name.Substring(0,nullPoint);
				_fieldDescriptions[i].Name = name;
            
				// read the field type
				_fieldDescriptions[i].DbaseType = (char) reader.ReadByte();
            
				// read the field data address, offset from the start of the record.
				_fieldDescriptions[i].DataAddress = reader.ReadInt32();
            
				// read the field length in bytes
				int tempLength = (int) reader.ReadByte();
				if (tempLength < 0) tempLength = tempLength + 256;
				_fieldDescriptions[i].Length = tempLength;
            
				// read the field decimal count in bytes
				_fieldDescriptions[i].DecimalCount = (int) reader.ReadByte();
            
				// read the reserved bytes.
				//reader.skipBytes(14);
				reader.ReadBytes(14);
			}
        
			// Last byte is a marker for the end of the field definitions.
			reader.ReadBytes(1);        
		}
    
		/// <summary>
		/// Set the number of records in the file
		/// </summary>
		/// <param name="inNumRecords"></param>
		protected void SetNumRecords(int inNumRecords)
		{
			_numRecords = inNumRecords;
		}
   
		/// <summary>
		/// Write the header data to the DBF file.
		/// </summary>
		/// <param name="writer"></param>
		public void WriteHeader(BinaryWriter writer)
		{
			// write the output file type.
			writer.Write((byte)_fileType);
       
			writer.Write((byte)(_updateDate.Year - 1900));
			writer.Write((byte)_updateDate.Month);
			writer.Write((byte)_updateDate.Day);
        
			// write the number of records in the datafile.
			writer.Write(_numRecords);
        
			// write the length of the header structure.
			writer.Write((short)_headerLength);
        
			// write the length of a record
			writer.Write((short)_recordLength);
        
			// write the reserved bytes in the header
			for (int i=0; i<20; i++) 
                writer.Write((byte)0);
        
			// write all of the header records
			int tempOffset = 0;
			for (int i=0; i<_fieldDescriptions.Length; i++)
			{            
				// write the field name
				for (int j=0; j<11; j++)
				{
					if (_fieldDescriptions[i].Name.Length > j)			
						 writer.Write((byte) _fieldDescriptions[i].Name[j]);
					else writer.Write((byte)0);
				}
            
				// write the field type
				writer.Write((char)_fieldDescriptions[i].DbaseType);
            
				// write the field data address, offset from the start of the record.
				writer.Write(0);
				tempOffset += _fieldDescriptions[i].Length;
            
				// write the length of the field.
				writer.Write((byte)_fieldDescriptions[i].Length);
            
				// write the decimal count.
				writer.Write((byte)_fieldDescriptions[i].DecimalCount);
            
				// write the reserved bytes.
				for (int j=0; j<14; j++) writer.Write((byte)0);
			}
        
			// write the end of the field definitions marker
			writer.Write((byte)0x0D);
		}

		/// <summary>
		/// Returns the fields in the dbase file.
		/// </summary>
		public DbaseFieldDescriptor[] Fields
		{
			get
			{
				return _fieldDescriptions;
			}
		}
	}
}
