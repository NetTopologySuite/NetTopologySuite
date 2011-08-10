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
        private const int MaxLookahead = 2048;
        
        private readonly FileInfo _file;
        private TextReader _reader;
        private readonly WKTReader _wktReader;
        private int _count;

        ///<summary>
        /// Creates a new <see cref="WKTFileReader" /> given the <paramref name="file" /> to read from and a <see cref="WKTReader" /> to use to parse the geometries.
        ///</summary>
        /// <param name="file"> the <see cref="FileInfo" /> to read from</param>
        /// <param name="wktReader">the geometry reader to use</param>
        public WKTFileReader(FileInfo file, WKTReader wktReader)
        {
            _file = file;
            _wktReader = wktReader;
            Limit = -1;
        }

        ///<summary>
        /// Creates a new <see cref="WKTFileReader" />, given the name of the file to read from.
        ///</summary>
        /// <param name="filename">The name of the file to read from</param>
        /// <param name="wktReader">The geometry reader to use</param>
        public WKTFileReader(String filename, WKTReader wktReader)
            : this(new FileInfo(filename), wktReader)
        {
        }

        ///<summary>
        /// Creates a new <see cref="WKTFileReader" />, given a <see cref="StreamReader"/> of the file to read from.
        ///</summary>
        /// <param name="reader">The stream reader of the file to read from</param>
        /// <param name="wktReader">The geometry reader to use</param>
        public WKTFileReader(TextReader reader, WKTReader wktReader)
        {
            _reader = reader;
            _wktReader = wktReader;
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
        /// <returns>The list of geometries read</returns>
        public IList<IGeometry> Read()
        {
            _count = 0;

            if (_file != null)
                _reader =  new StreamReader(new BufferedStream(_file.OpenRead(), MaxLookahead)); 

            try
            {
                //BufferedReader bufferedReader = new BufferedReader(fileReader);
                //try {
                return Read(_reader/*bufferedReader*/);
                //} finally {
                //    bufferedReader.close();
                //}
            }
            finally
            {
                _reader.Close();
            }
        }

        private IList<IGeometry> Read(TextReader bufferedReader)
        {
            IList<IGeometry> geoms = new List<IGeometry>();
            var tokens = _wktReader.Tokenize(bufferedReader);
            
            if (tokens[tokens.Count-1] is EofToken)
                tokens.RemoveAt(tokens.Count-1);
            
            _wktReader.Index = 0;
            while (!IsAtEndOfTokens(tokens) && !IsAtLimit(geoms))
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

        private bool IsAtEndOfTokens(IList<Token> tokens)
        {
            return !(_wktReader.Index < tokens.Count);
        }

        ///<summary>
        /// Tests if reader is at EOF.
        ///</summary>
        private bool IsAtEndOfFile(/*BufferedReader*/StreamReader bufferedReader)
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