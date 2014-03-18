using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Handlers;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// This class represnts an ESRI Shapefile.
    /// </summary>
    public partial class ShapefileReader : IEnumerable
    {
        private readonly string _filename;
        private readonly IGeometryFactory _geometryFactory;
        private readonly ShapefileHeader _mainHeader;


        /// <summary>
        /// Initializes a new instance of the Shapefile class with the given parameter 
        /// and a standard GeometryFactory.
        /// </summary>
        /// <param name="filename">The filename of the shape file to read (with .shp).</param>        
        public ShapefileReader(string filename) :
            this(filename, new GeometryFactory()) { }


        /// <summary>
        /// Gets the bounds of the shape file.
        /// </summary>
        public ShapefileHeader Header
        {
            get { return _mainHeader; }
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object 
        /// that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return new ShapefileEnumerator(this);
        }

        #endregion

        /// <summary>
        /// Reads the shapefile and returns a GeometryCollection representing all the records in the shapefile.
        /// </summary>
        /// <returns>GeometryCollection representing every record in the shapefile.</returns>
        public IGeometryCollection ReadAll()
        {
            var list = new List<IGeometry>();
            ShapeGeometryType type = _mainHeader.ShapeType;
            ShapeHandler handler = Shapefile.GetShapeHandler(type);
            if (handler == null)
                throw new NotSupportedException("Unsupported shape type:" + type);

            int i = 0;
            foreach (IGeometry geometry in this)
            {
                list.Add(geometry);
                i++;
            }

            IGeometry[] geomArray = GeometryFactory.ToGeometryArray(list);
            return _geometryFactory.CreateGeometryCollection(geomArray);
        }

        #region Nested type: ShapefileEnumerator

        /// <summary>
        /// Summary description for ShapefileEnumerator.
        /// </summary>
        private partial class ShapefileEnumerator : IEnumerator, IDisposable
        {
            private readonly ShapeHandler _handler;
            private readonly ShapefileReader _parent;
            private readonly BigEndianBinaryReader _shpBinaryReader;
            private IGeometry _geometry;

            #region IDisposable Members



            #endregion

            #region IEnumerator Members

            /// <summary>
            /// Sets the enumerator to its initial position, which is 
            /// before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created. 
            /// </exception>
            public void Reset()
            {
                _shpBinaryReader.BaseStream.Seek(100, SeekOrigin.Begin);
                //throw new InvalidOperationException();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; 
            /// false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public bool MoveNext()
            {
                if (_shpBinaryReader.BaseStream.Position < _shpBinaryReader.BaseStream.Length)
                {
                    // Mark Jacquin: add a try catch when some shapefile have extra char at the end but no record
                    try
                    {
                        int recordNumber = _shpBinaryReader.ReadInt32BE();
                        int contentLength = _shpBinaryReader.ReadInt32BE();
                        _geometry = _handler.Read(_shpBinaryReader, contentLength, _parent._geometryFactory);
                    }
                    catch (Exception ex)
                    {
                        // actually we should remove this crap...
                        Trace.WriteLine(ex.Message);
                        return false;
                    }
                    return true;
                }
                
                // Reached end of file, so close the reader.
                //_shpBinaryReader.Close();
                return false;
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <value></value>
            /// <returns>The current element in the collection.</returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The enumerator is positioned before the first element 
            /// of the collection or after the last element. 
            /// </exception>
            public object Current
            {
                get { return _geometry; }
            }

            #endregion
        }

        #endregion
    }
}