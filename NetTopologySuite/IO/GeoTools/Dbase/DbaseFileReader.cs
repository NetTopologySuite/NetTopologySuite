using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Class that allows records in a dbase file to be enumerated.
	/// </summary>
	public class DbaseFileReader  : IEnumerable
	{
        /// <summary>
        /// 
        /// </summary>
		private class DbaseFileEnumerator : IEnumerator, IDisposable
		{
			DbaseFileReader _parent;
			ArrayList _arrayList;
			int _iCurrentRecord=0;
			private BinaryReader _dbfStream = null;
			private int _readPosition = 0;
			private DbaseFileHeader _header = null;
			protected string[] _fieldNames = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="DbaseFileEnumerator"/> class.
            /// </summary>
            /// <param name="parent"></param>
            public DbaseFileEnumerator(DbaseFileReader parent)
			{
				_parent = parent;
				FileStream stream = new FileStream(parent._filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				_dbfStream = new BinaryReader(stream, Encoding.Default);
				ReadHeader();
			}            

			#region Implementation of IEnumerator

            /// <summary>
            /// Sets the enumerator to its initial position, which is 
            /// before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created. 
            /// </exception>
			public void Reset()
			{
                _dbfStream.BaseStream.Seek(_header.HeaderLength, SeekOrigin.Begin);
                _iCurrentRecord = 0;
				//throw new InvalidOperationException();
			}

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; 
            /// false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            ///  </exception>
			public bool MoveNext()
			{
				_iCurrentRecord++;
				if (_iCurrentRecord <= _header.NumRecords)				
					_arrayList = this.Read();				
				bool more= true;
				if (_iCurrentRecord > _header.NumRecords)
				{
					//this._dbfStream.Close();			
					more = false;
				}
				return more;
			}

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <value></value>
            /// <returns>The current element in the collection.</returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The enumerator is positioned before the first element of the collection
            /// or after the last element.
            /// </exception>
			public object Current
			{
				get
				{
					return _arrayList;
				}
			}

            /// <summary>
            /// 
            /// </summary>
			protected void ReadHeader()
			{
				_header = new DbaseFileHeader();
				// read the header
				_header.ReadHeader(_dbfStream);

				// how many records remain
				_readPosition = _header.HeaderLength;
			}

			/// <summary>
			/// Read a single dbase record
			/// </summary>
			/// <returns>
			/// The read shapefile record,
			///  or null if there are no more records.
			///  </returns>
			private ArrayList Read()  
			{
				ArrayList attrs = null;
       
				bool foundRecord = false;
				while (!foundRecord) 
				{
					// retrieve the record length
					int tempNumFields = _header.NumFields;
            
					// storage for the actual values
					attrs = new ArrayList(tempNumFields);
            
					// read the deleted flag
					char tempDeleted = (char)_dbfStream.ReadChar();
            
					// read the record length
					int tempRecordLength = 1; // for the deleted character just read.
            
					// read the Fields
					for (int j = 0; j < tempNumFields; j++)
					{                
						// find the length of the field.
						int tempFieldLength = _header.Fields[j].Length;
						tempRecordLength = tempRecordLength + tempFieldLength;
                
						// find the field type
						char tempFieldType = _header.Fields[j].DbaseType;
                
						// read the data.
						object tempObject = null;
						switch (tempFieldType)
						{
							case 'L':   // logical data type, one character (T,t,F,f,Y,y,N,n)
								char tempChar = (char)_dbfStream.ReadByte();
								if ((tempChar == 'T') || (tempChar == 't') || (tempChar == 'Y') || (tempChar == 'y'))
									 tempObject = true;
								else tempObject = false;
								break;

							case 'C':   // character record.
								char[] sbuffer = new char[tempFieldLength];
								sbuffer = _dbfStream.ReadChars(tempFieldLength);
								// use an encoding to ensure all 8 bits are loaded
								// tempObject = new string(sbuffer, "ISO-8859-1").Trim();								

								//HACK: this can be made more efficient
								tempObject = new string(sbuffer).Trim().Replace("\0",String.Empty);   //.ToCharArray();
								break;
                        
							case 'D':   // date data type.
								char[] ebuffer = new char[8];
								ebuffer = _dbfStream.ReadChars(8);
								string tempString = new string(ebuffer, 0, 4);
								int year = Int32.Parse(tempString, CultureInfo.InvariantCulture);
								tempString = new string(ebuffer, 4, 2);
                                int month = Int32.Parse(tempString, CultureInfo.InvariantCulture);
								tempString = new string(ebuffer, 6, 2);
								int day = Int32.Parse(tempString, CultureInfo.InvariantCulture);
								tempObject = new DateTime(year, month, day);
								break;
                        
							case 'N': // number
							case 'F': // floating point number
								char[] fbuffer = new char[tempFieldLength];
								fbuffer = _dbfStream.ReadChars(tempFieldLength);
								tempString = new string(fbuffer);
								try 
								{ 
									tempObject = Double.Parse(tempString.Trim(), CultureInfo.InvariantCulture);
								}
								catch (FormatException) 
								{
									// if we can't format the number, just save it as a string
									tempObject = tempString;
								}
								break;
                        
							default:
								throw new NotSupportedException("Do not know how to parse Field type "+tempFieldType);
						}
						attrs.Add(tempObject);
					}
            
					// ensure that the full record has been read.
					if (tempRecordLength < _header.RecordLength)
					{
						byte[] tempbuff = new byte[_header.RecordLength-tempRecordLength];
						tempbuff = _dbfStream.ReadBytes(_header.RecordLength-tempRecordLength);
					}
            
					// add the row if it is not deleted.
					if (tempDeleted != '*')
					{
						foundRecord = true;
					}
				}
				return attrs;
			}

			#endregion

            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _dbfStream.Close();
            }

            #endregion
        }

		private DbaseFileHeader _header = null;
		private string _filename;

		#region Constructors

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="filename"></param>
		public DbaseFileReader(string filename) 
		{
			if (filename==null)
			{
				throw new ArgumentNullException(filename);
			}
			// check for the file existing here, otherwise we will not get an error
			//until we read the first record or read the header.
			if (!File.Exists(filename))
			{
				throw new FileNotFoundException(String.Format("Could not find file \"{0}\"",filename));
			}
			_filename = filename;
			
		}
		
		#endregion

		#region Methods

		/// <summary>
		/// Gets the header information for the dbase file.
		/// </summary>
		/// <returns>DbaseFileHeader contain header and field information.</returns>
		public DbaseFileHeader GetHeader() 
		{
			if (_header==null)
			{
				FileStream stream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
				BinaryReader dbfStream = new BinaryReader(stream);

				_header = new DbaseFileHeader();
				// read the header
				_header.ReadHeader(dbfStream);

				dbfStream.Close();
				stream.Close();

			}
			return _header;
		}
		
		#endregion

		#region Implementation of IEnumerable

        /// <summary>
        /// Gets the object that allows iterating through the members of the collection.
        /// </summary>
        /// <returns>
        /// An object that implements the IEnumerator interface.
        /// </returns>
		public IEnumerator GetEnumerator()
		{
			return new DbaseFileEnumerator(this);
		}

		#endregion		
	}
}
