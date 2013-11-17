using System;
using System.Diagnostics;
using System.IO;
using System.Text;
#if SILVERLIGHT
using NetTopologySuite.Encodings;
#endif

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Class for holding the information assicated with a dbase header.
    /// </summary>
    public class DbaseFileHeader
    {
        public const int FieldNameMaxLength = 11;

        // Constant for the size of a record
        private int FileDescriptorSize = 32;

        // type of the file, must be 03h
        private int _fileType = 0x03;

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

        private Encoding _encoding;

        // collection of header records.
        private DbaseFieldDescriptor[] _fieldDescriptions;

        /// <summary>
        /// Initializes a new instance of the DbaseFileHeader class.
        /// </summary>
        public DbaseFileHeader() 
#if !SILVERLIGHT
            : this(Encoding.GetEncoding(1252)) { }
#else
            : this(CP1252.Instance) { }
#endif

        /// <summary>
        /// Initializes a new instance of the DbaseFileHeader class.
        /// </summary>
        public DbaseFileHeader(Encoding enc)
        {
            if (enc == null)
                throw new ArgumentNullException("enc");
            _encoding = enc;
        }

        /// <summary>
        /// Return the date this file was last updated.
        /// </summary>
        /// <returns></returns>
        public DateTime LastUpdateDate
        {
            get { return _updateDate; }
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

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
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
            get { return _recordLength; }
        }

        /// <summary>
        /// Return the length of the header.
        /// </summary>
        /// <returns></returns>
        public int HeaderLength
        {
            get { return _headerLength; }
        }

        /// <summary>
        ///  Add a column to this DbaseFileHeader.
        /// </summary>
        /// <param name="fieldName">The name of the field to add.</param>
        /// <param name="fieldType">The type is one of (C N L or D) character, number, logical(true/false), or date.</param>
        /// <param name="fieldLength"> The Field length is the total length in bytes reserved for this column.</param>
        /// <param name="decimalCount">The decimal count only applies to numbers(N), and floating point values (F), and refers to the number of characters to reserve after the decimal point.</param>
        public void AddColumn(string fieldName, char fieldType, int fieldLength, int decimalCount)
        {
            if (fieldLength <= 0) fieldLength = 1;
            if (_fieldDescriptions == null) _fieldDescriptions = new DbaseFieldDescriptor[0];
            int tempLength = 1;  // the length is used for the offset, and there is a * for deleted as the first byte
            DbaseFieldDescriptor[] tempFieldDescriptors = new DbaseFieldDescriptor[_fieldDescriptions.Length + 1];
            for (int i = 0; i < _fieldDescriptions.Length; i++)
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
            if (tempFieldName.Length > FieldNameMaxLength)
            {
                string s = String.Format("FieldName {0} is longer than {1} characters", fieldName, FieldNameMaxLength);
                throw new ArgumentException(s);
            }
            tempFieldDescriptors[_fieldDescriptions.Length].Name = tempFieldName;

            // the field type
            if ((fieldType == 'C') || (fieldType == 'c'))
            {
                tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'C';
                if (fieldLength > 254) Trace.WriteLine("Field Length for " + fieldName + " set to " + fieldLength + " Which is longer than 254, not consistent with dbase III");
            }
            else if ((fieldType == 'S') || (fieldType == 's'))
            {
                tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'C';
                Trace.WriteLine("Field type for " + fieldName + " set to S which is flat out wrong people!, I am setting this to C, in the hopes you meant character.");
                if (fieldLength > 254) Trace.WriteLine("Field Length for " + fieldName + " set to " + fieldLength + " Which is longer than 254, not consistent with dbase III");
                tempFieldDescriptors[_fieldDescriptions.Length].Length = 8;
            }
            else if ((fieldType == 'D') || (fieldType == 'd'))
            {
                tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'D';
                if (fieldLength != 8) Trace.WriteLine("Field Length for " + fieldName + " set to " + fieldLength + " Setting to 8 digets YYYYMMDD");
                tempFieldDescriptors[_fieldDescriptions.Length].Length = 8;
            }
            else if ((fieldType == 'F') || (fieldType == 'f'))
            {
                tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'F';
                if (fieldLength > 20) Trace.WriteLine("Field Length for " + fieldName + " set to " + fieldLength + " Preserving length, but should be set to Max of 20 not valid for dbase IV, and UP specification, not present in dbaseIII.");
            }
            else if ((fieldType == 'N') || (fieldType == 'n'))
            {
                tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'N';
                if (fieldLength > 18) Trace.WriteLine("Field Length for " + fieldName + " set to " + fieldLength + " Preserving length, but should be set to Max of 18 for dbase III specification.");
                if (decimalCount < 0)
                {
                    Trace.WriteLine("Field Decimal Position for " + fieldName + " set to " + decimalCount + " Setting to 0 no decimal data will be saved.");
                    tempFieldDescriptors[_fieldDescriptions.Length].DecimalCount = 0;
                }
                if (decimalCount > fieldLength - 1)
                {
                    Trace.WriteLine("Field Decimal Position for " + fieldName + " set to " + decimalCount + " Setting to " + (fieldLength - 1) + " no non decimal data will be saved.");
                    tempFieldDescriptors[_fieldDescriptions.Length].DecimalCount = fieldLength - 1;
                }
            }
            else if ((fieldType == 'L') || (fieldType == 'l'))
            {
                tempFieldDescriptors[_fieldDescriptions.Length].DbaseType = 'L';
                if (fieldLength != 1) Trace.WriteLine("Field Length for " + fieldName + " set to " + fieldLength + " Setting to length of 1 for logical fields.");
                tempFieldDescriptors[_fieldDescriptions.Length].Length = 1;
            }
            else
            {
                throw new NotSupportedException("Unsupported field type " + fieldType + " For column " + fieldName);
            }
            // the length of a record
            tempLength = tempLength + tempFieldDescriptors[_fieldDescriptions.Length].Length;

            // set the new fields.
            _fieldDescriptions = tempFieldDescriptors;
            _headerLength = 33 + 32 * _fieldDescriptions.Length;
            _numFields = _fieldDescriptions.Length;
            _recordLength = tempLength;
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
                if (fieldName.ToLower() != (_fieldDescriptions[i].Name.Trim().ToLower()))
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
                else retCol = i;
            }

            // set the new fields.
            _fieldDescriptions = tempFieldDescriptors;
            _headerLength = 33 + 32 * _fieldDescriptions.Length;
            _numFields = _fieldDescriptions.Length;
            _recordLength = tempLength;

            return retCol;
        }

        /// <summary>
        /// Read the header data from the DBF file.
        /// </summary>
        /// <param name="reader">BinaryReader containing the header.</param>
        /// <param name="filename">Filename </param>
        public void ReadHeader(BinaryReader reader, string filename)
        {
            // type of reader.
            _fileType = reader.ReadByte();
            if (_fileType != 0x03)
                throw new NotSupportedException("Unsupported DBF reader Type " + _fileType);

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
            byte[] data = reader.ReadBytes(20);
            int lcid = data[29 - 12]; //get the 29th byte in the file... we've first to read into arry was no 12
            _encoding = DetectEncodingFromMark(lcid, filename);

            //Replace reader with one with correct encoding..
            reader = new BinaryReader(reader.BaseStream, _encoding);
            // calculate the number of Fields in the header
            _numFields = (_headerLength - FileDescriptorSize - 1) / FileDescriptorSize;

            // read all of the header records
            _fieldDescriptions = new DbaseFieldDescriptor[_numFields];
            for (int i = 0; i < _numFields; i++)
            {
                _fieldDescriptions[i] = new DbaseFieldDescriptor();

                // read the field name				
                byte[] buffer = new byte[11];
                buffer = reader.ReadBytes(11);
                // NOTE: only this _encoding.GetString method is available in Silverlight
                String name = _encoding.GetString(buffer, 0, buffer.Length);
                int nullPoint = name.IndexOf((char)0);
                if (nullPoint != -1)
                    name = name.Substring(0, nullPoint);
                _fieldDescriptions[i].Name = name;

                // read the field type
                _fieldDescriptions[i].DbaseType = (char)reader.ReadByte();

                // read the field data address, offset from the start of the record.
                _fieldDescriptions[i].DataAddress = reader.ReadInt32();

                // read the field length in bytes
                int tempLength = (int)reader.ReadByte();
                if (tempLength < 0) tempLength = tempLength + 256;
                _fieldDescriptions[i].Length = tempLength;

                // read the field decimal count in bytes
                _fieldDescriptions[i].DecimalCount = (int)reader.ReadByte();

                // read the reserved bytes.
                //reader.skipBytes(14);
                reader.ReadBytes(14);
            }

            // Last byte is a marker for the end of the field definitions.
            // Trond Benum: This fails for some presumeably valid test shapefiles, so I have commented it out. 
            var lastByte = reader.ReadBytes(1)[0];
            // if (lastByte != 0x0d)
            //   throw new ShapefileException("DBase Header is not terminated");

            // Assure we are at the end of the header!
            if (reader.BaseStream.Position != _headerLength)
                reader.BaseStream.Seek(_headerLength, SeekOrigin.Begin);
        }

        /// <summary>
        /// See if we have a dbf file and make a guess on its encoding, based on
        /// code pages listed in the ArcGIS v9, ArcPad Reference Guide
        /// http://downloads.esri.com/support/documentation/pad_/ArcPad_RefGuide_1105.pdf
        /// </summary>
        /// <param name="lcid">Language driver id</param>
        /// <param name="cpgFileName">Filename of code page file</param>
        /// <returns></returns>
        private Encoding DetectEncodingFromMark(int lcid, string cpgFileName)
        {
#if !SILVERLIGHT
            var enc = Encoding.GetEncoding(1252);
#else
            var enc = CP1252.Instance;
#endif
            string lc = "";
            foreach (var it in _dbfCodePages)
            {
                if ((int)it[0] == lcid)
                {
                    lc = it[1] as string;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(lc))
            {
                try
                {
                    var winCodePage = int.Parse(lc.Replace("CP", ""));
#if !SILVERLIGHT
                    enc = Encoding.GetEncoding("windows-" + winCodePage);
#else
                    enc = new EncodingRegistry().GetEncoding(winCodePage);
#endif
                }
                catch (Exception ee)
                {
                    //Could not get encoding..
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(cpgFileName))
                {
                    cpgFileName = Path.ChangeExtension(cpgFileName, "cpg");
                    if (!File.Exists(cpgFileName))
                        cpgFileName = Path.ChangeExtension(cpgFileName, "cst");
                    if (File.Exists(cpgFileName))
                    {
                        var encoding = File.ReadAllText(cpgFileName).Trim();
                        try
                        {
                            return Encoding.GetEncoding(encoding);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    enc = Encoding.UTF8;
                }
            }


            return enc;
        }

        private int GetLCIDFromEncoding(Encoding enc)
        {
#if !SILVERLIGHT
            int lcid = enc.WindowsCodePage;
#else
            int lcid = enc.CodePage();
#endif
            int cpId = 0x03;
            foreach (var cp in _dbfCodePages)
            {
                if (cp[1] as string == "CP" + lcid)
                {
                    cpId = (int)cp[0];
                    break;
                }
            }


            return cpId;
        }

        private readonly object[][] _dbfCodePages = new[]
                                                        {
        new object[] {0x01 , "CP437",	}, // U.S. MS–DOS
		new object[] { 0x02 , "CP850"	}, // International MS–DOS
		new object[] { 0x03 , "CP1252"	}, // Windows ANSI
		new object[] { 0x08 , "CP865"	}, // Danish OEM
		new object[] { 0x09 , "CP437"	}, // Dutch OEM
		new object[] { 0x0A , "CP850"	}, // Dutch OEM*
		new object[] { 0x0B , "CP437"	}, // Finnish OEM
		new object[] { 0x0D , "CP437"	}, // French OEM
		new object[] { 0x0E , "CP850"	}, // French OEM*
		new object[] { 0x0F , "CP437"	}, // German OEM
		new object[] { 0x10 , "CP850"	}, // German OEM*
		new object[] { 0x11 , "CP437"	}, // Italian OEM
		new object[] { 0x12 , "CP850"	}, // Italian OEM*
		new object[] { 0x13 , "CP932"	}, // Japanese Shift-JIS
		new object[] { 0x14 , "CP850"	}, // Spanish OEM*
		new object[] { 0x15 , "CP437"	}, // Swedish OEM
		new object[] { 0x16 , "CP850"	}, // Swedish OEM*
		new object[] { 0x17 , "CP865"	}, // Norwegian OEM
		new object[] { 0x18 , "CP437"	}, // Spanish OEM
		new object[] { 0x19 , "CP437"	}, // English OEM (Britain)
		new object[] { 0x1A , "CP850"	}, // English OEM (Britain)*
		new object[] { 0x1B , "CP437"	}, // English OEM (U.S.)
		new object[] { 0x1C , "CP863"	}, // French OEM (Canada)
		new object[] { 0x1D , "CP850"	}, // French OEM*
		new object[] { 0x1F , "CP852"	}, // Czech OEM
		new object[] { 0x22 , "CP852"	}, // Hungarian OEM
		new object[] { 0x23 , "CP852"	}, // Polish OEM
		new object[] { 0x24 , "CP860"	}, // Portuguese OEM
		new object[] { 0x25 , "CP850"	}, // Portuguese OEM*
		new object[] { 0x26 , "CP866"	}, // Russian OEM
		new object[] { 0x37 , "CP850"	}, // English OEM (U.S.)*
		new object[] { 0x40 , "CP852"	}, // Romanian OEM
		new object[] { 0x4D , "CP936"	}, // Chinese GBK (PRC)
		new object[] { 0x4E , "CP949"	}, // Korean (ANSI/OEM)
		new object[] { 0x4F , "CP950"	}, // Chinese Big5 (Taiwan)
		new object[] { 0x50 , "CP874"	}, // Thai (ANSI/OEM)
		new object[] { 0x57 , "CP1252"	}, // ANSI
		new object[] { 0x58 , "CP1252"	}, // Western European ANSI
		new object[] { 0x59 , "CP1252"	}, // Spanish ANSI
		new object[] { 0x64 , "CP852"	}, // Eastern European MS–DOS
		new object[] { 0x65 , "CP866"	}, // Russian MS–DOS
		new object[] { 0x66 , "CP865"	}, // Nordic MS–DOS
		new object[] { 0x67 , "CP861"	}, // Icelandic MS–DOS
		new object[] { 0x6A , "CP737"	}, // Greek MS–DOS (437G)
		new object[] { 0x6B , "CP857"	}, // Turkish MS–DOS
		new object[] { 0x6C , "CP863"	}, // French–Canadian MS–DOS
		new object[] { 0x78 , "CP950"	}, // Taiwan Big 5
		new object[] { 0x79 , "CP949"	}, // Hangul (Wansung)
		new object[] { 0x7A , "CP936"	}, // PRC GBK
		new object[] { 0x7B , "CP932"	}, // Japanese Shift-JIS
		new object[] { 0x7C , "CP874"	}, // Thai Windows/MS–DOS
		new object[] { 0x86 , "CP737"	}, // Greek OEM
		new object[] { 0x87 , "CP852"	}, // Slovenian OEM
		new object[] { 0x88 , "CP857"	}, // Turkish OEM
		new object[] { 0xC8 , "CP1250"	}, // Eastern European Windows
		new object[] { 0xC9 , "CP1251"	}, // Russian Windows
		new object[] { 0xCA , "CP1254"	}, // Turkish Windows
		new object[] { 0xCB , "CP1253"	}, // Greek Windows
		new object[] { 0xCC , "CP1257"	}, // Baltic Windows
        };
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
            byte[] data = new byte[20];
            for (int i = 0; i < 20; i++)
            {
                data[i] = (byte)0;
            }
            data[29 - 12] = (byte)GetLCIDFromEncoding(_encoding);
            writer.Write(data);

            // write all of the header records
            int tempOffset = 0;
            for (int i = 0; i < _fieldDescriptions.Length; i++)
            {
                // write the field name
                DbaseFieldDescriptor descriptor = _fieldDescriptions[i];
                byte[] buffer = _encoding.GetBytes(descriptor.Name);
                if (buffer.Length != FieldNameMaxLength)
                {
                    byte[] temp = new byte[FieldNameMaxLength];
                    Array.Copy(buffer, temp, buffer.Length);    
                    writer.Write(temp);
                }
                else writer.Write(buffer);                

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
                for (int j = 0; j < 14; j++) writer.Write((byte)0);
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
