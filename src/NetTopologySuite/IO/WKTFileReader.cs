using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using NetTopologySuite.Geometries;
using RTools_NTS.Util;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Reads a sequence of <see cref="Geometry"/>s in WKT format from a text file.
    /// </summary>
    /// <remarks>The geometries in the file may be separated by any amount of whitespace and newlines.</remarks>
    /// <author>
    /// Martin Davis
    /// </author>
    public class WKTFileReader
    {
        private const int MaxLookahead = 2048;
        private readonly FileInfo _file;

        private StreamReader _reader;
        private readonly WKTReader _wktReader;
        private int _count;

        private WKTFileReader(WKTReader wktReader)
        {
            _wktReader = wktReader;
            Limit = -1;
        }

        /// <summary>
        /// Creates a new <see cref="WKTFileReader" /> given the <paramref name="file" /> to read from and a <see cref="WKTReader" /> to use to parse the geometries.
        /// </summary>
        /// <param name="file"> the <see cref="FileInfo" /> to read from</param>
        /// <param name="wktReader">the geometry reader to use</param>
        public WKTFileReader(FileInfo file, WKTReader wktReader)
            :this(wktReader)
        {
            _file = file;
        }

        /// <summary>
        /// Creates a new <see cref="WKTFileReader" />, given the name of the file to read from.
        /// </summary>
        /// <param name="filename">The name of the file to read from</param>
        /// <param name="wktReader">The geometry reader to use</param>
        public WKTFileReader(string filename, WKTReader wktReader)
            : this(new FileInfo(filename), wktReader)
        {
        }

        /// <summary>
        /// Creates a new <see cref="WKTFileReader" />, given a <see cref="Stream"/> to read from.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="wktReader">The geometry reader to use</param>
        public WKTFileReader(Stream stream, WKTReader wktReader)
            : this(new StreamReader(stream), wktReader)
        {
        }

        /// <summary>
        /// Creates a new <see cref="WKTFileReader" />, given a <see cref="TextReader"/> to read with.
        /// </summary>
        /// <param name="reader">The stream reader of the file to read from</param>
        /// <param name="wktReader">The geometry reader to use</param>
        public WKTFileReader(TextReader reader, WKTReader wktReader)
            :this(wktReader)
        {
            _reader = (StreamReader)reader;
        }

        /// <summary>
        /// Gets/Sets the maximum number of geometries to read.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets/Sets allow ignoring WKT parse errors
        /// after at least one geometry has been read,
        /// to return a partial result.
        /// </summary>
        public bool StrictParsing { get; set; }

        /// <summary>
        /// Gets/Sets the number of geometries to skip before reading.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Reads a sequence of geometries.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If an offset is specified, geometries read up to the offset count are skipped.</para>
        /// <para>If a limit is specified, no more than <see cref="Limit" /> geometries are read.</para>
        /// </remarks>
        /// <exception cref="IOException">Thrown if an I/O exception was encountered</exception>
        /// <exception cref="ParseException">Thrown if an error occurred reading a geometry</exception>
        /// <returns>The list of geometries read</returns>
        public IList<Geometry> Read()
        {
            _count = 0;

            if (_file != null)
                _reader =  new StreamReader(new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, MaxLookahead));

            using (_reader)
            {
                return Read(_reader);
            }
        }

        private IList<Geometry> Read(StreamReader bufferedReader)
        {
            var geoms = new List<Geometry>();
            try
            {
                Read(bufferedReader, geoms);
            }
            catch (ParseException ex)
            {
                // throw if strict or error is on first geometry
                if (StrictParsing || geoms.Count == 0)
                    throw ex;
            }
            return geoms;
        }

        private void Read(StreamReader bufferedReader, ICollection<Geometry> geoms)
        {
            var tokenizer = _wktReader.Tokenizer(bufferedReader);
            while (!IsAtEndOfFile(bufferedReader) && !IsAtLimit(geoms)) {
                var g = _wktReader.ReadGeometryTaggedText(tokenizer);
                if (_count >= Offset )
                    geoms.Add(g);
                _count++;
            }
        }

        private bool IsAtLimit(ICollection<Geometry> geoms)
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

        /// <summary>
        /// Tests if reader is at EOF.
        /// </summary>
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
