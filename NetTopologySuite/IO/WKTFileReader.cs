using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using RTools_NTS.Util;

namespace NetTopologySuite.IO
{
    ///<summary>
    /// Reads a sequence of <see cref="IGeometry"/>s in WKT format from a text file.
    ///</summary>
    /// <remarks>The geometries in the file may be separated by any amount of whitespace and newlines.</remarks>
    /// <author>
    /// Martin Davis
    /// </author>
    public class WKTFileReader
    {
#if FEATURE_FILE_IO
        private const int MaxLookahead = 2048;
        private readonly FileInfo _file;
#endif

        private TextReader _reader;
        private readonly WKTReader _wktReader;
        private int _count;

        private WKTFileReader(WKTReader wktReader)
        {
            _wktReader = wktReader;
            Limit = -1;
        }

#if FEATURE_FILE_IO
        ///<summary>
        /// Creates a new <see cref="WKTFileReader" /> given the <paramref name="file" /> to read from and a <see cref="WKTReader" /> to use to parse the geometries.
        ///</summary>
        /// <param name="file"> the <see cref="FileInfo" /> to read from</param>
        /// <param name="wktReader">the geometry reader to use</param>
        public WKTFileReader(FileInfo file, WKTReader wktReader)
            :this(wktReader)
        {
            _file = file;
        }

        ///<summary>
        /// Creates a new <see cref="WKTFileReader" />, given the name of the file to read from.
        ///</summary>
        /// <param name="filename">The name of the file to read from</param>
        /// <param name="wktReader">The geometry reader to use</param>
        public WKTFileReader(string filename, WKTReader wktReader)
            : this(new FileInfo(filename), wktReader)
        {
        }
#endif
        ///<summary>
        /// Creates a new <see cref="WKTFileReader" />, given a <see cref="Stream"/> to read from.
        ///</summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="wktReader">The geometry reader to use</param>
        public WKTFileReader(Stream stream, WKTReader wktReader)
            : this(new StreamReader(stream), wktReader)
        {
        }

        ///<summary>
        /// Creates a new <see cref="WKTFileReader" />, given a <see cref="TextReader"/> to read with.
        ///</summary>
        /// <param name="reader">The stream reader of the file to read from</param>
        /// <param name="wktReader">The geometry reader to use</param>
        public WKTFileReader(TextReader reader, WKTReader wktReader)
            :this(wktReader)
        {
            _reader = reader;
        }

        ///<summary>
        /// Gets/Sets the maximum number of geometries to read.
        ///</summary>
        public int Limit { get; set; }

        ///<summary>
        /// Gets/Sets the number of geometries to skip before reading.
        ///</summary>
        public int Offset { get; set; }

        ///<summary>
        /// Reads a sequence of geometries.
        ///</summary>
        /// <remarks>
        /// <para>
        /// If an offset is specified, geometries read up to the offset count are skipped.</para>
        /// <para>If a limit is specified, no more than <see cref="Limit" /> geometries are read.</para>
        /// </remarks>
        /// <exception cref="IOException">Thrown if an I/O exception was encountered</exception>
        /// <exception cref="GeoAPI.IO.ParseException">Thrown if an error occurred reading a geometry</exception>
        /// <returns>The list of geometries read</returns>
        public IList<IGeometry> Read()
        {
            _count = 0;

#if FEATURE_FILE_IO
            if (_file != null)
                _reader =  new StreamReader(new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, MaxLookahead));
#endif
            try
            {
                return Read(_reader);
            }
            finally
            {
#if FEATURE_FILE_IO
                _reader.Dispose();
#endif
            }
        }

        private IList<IGeometry> Read(TextReader bufferedReader)
        {
            var geoms = new List<IGeometry>();
            var tokens = _wktReader.Tokenizer(bufferedReader);
            tokens.MoveNext();
            while (!IsAtEndOfTokens(tokens.Current) && !IsAtLimit(geoms))
            {
                var g = _wktReader.ReadGeometryTaggedText(tokens);
                if (_count >= Offset)
                    geoms.Add(g);
                _count++;
            }

            /*
            while (!IsAtEndOfFile(bufferedReader) && !IsAtLimit(geoms))
            {
                IGeometry g = _wktReader.Read(bufferedReader);
                if (_count >= Offset)
                    geoms.Add(g);
                _count++;
            }
             */
            return geoms;
        }

        private bool IsAtLimit(IList<IGeometry> geoms)
        {
            if (Limit < 0) return false;
            if (geoms.Count < Limit) return false;
            return true;
        }

        //private bool IsAtEndOfTokens(IList<Token> tokens)
        //{
        //    return !(_wktReader.Index < tokens.Count);
        //}

        private static bool IsAtEndOfTokens(Token token)
        {
            return token is EofToken;
        }

        ///<summary>
        /// Tests if reader is at EOF.
        ///</summary>
        private bool IsAtEndOfFile(StreamReader bufferedReader)
        {
            return bufferedReader.EndOfStream;
            /*
            bufferedReader.mark(1000);

            StreamTokenizer tokenizer = new StreamTokenizer(bufferedReader);
            int type = tokenizer.nextToken();

            if (type == StreamTokenizer.TT_EOF) {
                return true;
            }
            bufferedReader.reset();
            return false;
             */
        }
    }
}