using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using NetTopologySuite.Geometries;
using RTools_NTS.Util;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Reads a sequence of {@link Geometry}s in WKBHex format
    /// from a text file.
    /// Each WKBHex geometry must be on a single line
    /// The geometries in the file may be separated by any amount
    /// of whitespace and newlines.
    /// </summary>
    /// <author>Martin Davis</author>
    public class WKBHexFileReader
    {
        private readonly WKBReader _wkbReader;

        /// <summary>
        /// Creates a new <see cref="WKBHexFileReader"/> given the
        /// <see cref="WKBReader"/> to use to parse the geometries.
        /// </summary>
        /// <param name="wkbReader">The geometry reader to use</param>
        public WKBHexFileReader(WKBReader wkbReader)
        {
            if (wkbReader == null)
                throw new ArgumentNullException("wkbReader");

            Limit = -1;
            _wkbReader = wkbReader;
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum number of geometries to read
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the number of geometries to skip before storing.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Reads a sequence of geometries.<br/>
        /// If an <see cref="Offset"/> is specified, geometries read up to the offset count are skipped.
        /// If a <see cref="Limit"/> is specified, no more than <see cref="Limit"/> geometries are read.
        /// </summary>
        /// <param name="file">The path to the file</param>
        /// <exception cref="ArgumentNullException">Thrown if no filename was specified</exception>
        /// <exception cref="FileNotFoundException">Thrown if the filename specified does not exist</exception>
        /// <exception cref="IOException">Thrown if an I/O exception was encountered</exception>
        /// <exception cref="ParseException">Thrown if an error occurred reading a geometry</exception>
        public ReadOnlyCollection<Geometry> Read(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("file");

            // do this here so that constructors don't throw exceptions
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                return Read(stream);
            }
        }

        /// <summary>
        /// Reads a sequence of geometries.<br/>
        /// If an <see cref="Offset"/> is specified, geometries read up to the offset count are skipped.
        /// If a <see cref="Limit"/> is specified, no more than <see cref="Limit"/> geometries are read.
        /// </summary>
        /// <param name="stream">The path to the file</param>
        /// <exception cref="ArgumentNullException">Thrown if no stream was passed</exception>
        /// <exception cref="ArgumentException">Thrown if passed stream is not readable or seekable</exception>
        /// <exception cref="IOException">Thrown if an I/O exception was encountered</exception>
        /// <exception cref="ParseException">Thrown if an error occured reading a geometry</exception>
        public ReadOnlyCollection<Geometry> Read(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
                throw new Exception("Stream must be readable");
            if (!stream.CanSeek)
                throw new Exception("Stream must be seekable");

            using (var sr = new StreamReader(stream))
            {
                return Read(sr);
            }

        }

        /// <summary>
        /// Reads a sequence of geometries.<br/>
        /// If an <see cref="Offset"/> is specified, geometries read up to the offset count are skipped.
        /// If a <see cref="Limit"/> is specified, no more than <see cref="Limit"/> geometries are read.
        /// </summary>
        /// <param name="streamReader">The stream reader to use.</param>
        /// <exception cref="IOException">Thrown if an I/O exception was encountered</exception>
        /// <exception cref="ParseException">Thrown if an error occured reading a geometry</exception>
        private ReadOnlyCollection<Geometry> Read(StreamReader streamReader)
        {
            var geoms = new List<Geometry>();
            int count = 0;
            while (!IsAtEndOfFile(streamReader) && !IsAtLimit(geoms))
            {
                string line = streamReader.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                var g = _wkbReader.Read(WKBReader.HexToBytes(line));
                if (count >= Offset)
                    geoms.Add(g);
                count++;
            }
            return geoms.AsReadOnly();
        }

        /// <summary>
        /// Tests if reader has reached limit
        /// </summary>
        /// <param name="geoms">A collection of already read geometries</param>
        /// <returns><c>true</c> if <see cref="Limit"/> number of geometries has been read.</returns>
        private bool IsAtLimit(List<Geometry> geoms)
        {
            if (Limit < 0)
                return false;
            return geoms.Count >= Limit;
        }

        /// <summary>
        /// Tests if reader is at EOF.
        /// </summary>
        private static bool IsAtEndOfFile(StreamReader bufferedReader)
        {
            return bufferedReader.EndOfStream;
        }
    }
}
