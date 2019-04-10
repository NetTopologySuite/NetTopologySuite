using System;
using System.Globalization;
using System.IO;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Outputs the textual representation of a <see cref="Geometry" />.
    /// The <see cref="WKTWriter" /> outputs coordinates rounded to the precision
    /// model. No more than the maximum number of necessary decimal places will be
    /// output.
    /// The Well-known Text format is defined in the <A
    /// HREF="http://www.opengis.org/techno/specs.htm">OpenGIS Simple Features
    /// Specification for SQL</A>.
    /// A non-standard "LINEARRING" tag is used for LinearRings. The WKT spec does
    /// not define a special tag for LinearRings. The standard tag to use is
    /// "LINESTRING".
    /// </summary>
    public class WKTWriter : ITextGeometryWriter
    {
        /// <summary>
        /// Generates the WKT for a <c>Point</c> specified by a <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="p0">The point coordinate.</param>
        /// <returns>The WKT</returns>
        public static string ToPoint(Coordinate p0)
        {
            // legacy note: JTS's version never checks Z or M, so the things that call this aren't
            // expecting to see them.  the "actual" code to write points handles Z / M just fine.
            return "POINT ( " + p0.X.ToString("G17", CultureInfo.InvariantCulture) + " " + p0.Y.ToString("G17", CultureInfo.InvariantCulture) + " )";
        }

        /// <summary>
        /// Generates the WKT for a N-point <c>LineString</c> specified by a <see cref="ICoordinateSequence"/>.
        /// </summary>
        /// <param name="seq">The sequence to write.</param>
        /// <returns>The WKT</returns>
        public static string ToLineString(ICoordinateSequence seq)
        {
            // legacy note: JTS's version never checks Z or M, so the things that call this aren't
            // expecting to see them.  the "actual" code to write lines handles Z / M just fine.
            var buf = new StringBuilder();
            buf.Append("LINESTRING");
            if (seq.Count == 0)
                buf.Append(" EMPTY");
            else
            {
                buf.Append("(");
                for (int i = 0; i < seq.Count; i++)
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(seq.GetX(i).ToString("G17", CultureInfo.InvariantCulture) + " " + seq.GetY(i).ToString("G17", CultureInfo.InvariantCulture));
              }
              buf.Append(")");
            }
            return buf.ToString();
        }

        /**
         * Generates the WKT for a <tt>LINESTRING</tt>
         * specified by a {@link CoordinateSequence}.
         *
         * @param seq the sequence to write
         *
         * @return the WKT string
         */
        public static string ToLineString(Coordinate[] coord)
        {
            // legacy note: JTS's version never checks Z or M, so the things that call this aren't
            // expecting to see them.  the "actual" code to write lines handles Z / M just fine.
            var buf = new StringBuilder();
            buf.Append("LINESTRING ");
            if (coord.Length == 0)
                buf.Append(" EMPTY");
            else
            {
                buf.Append("(");
                for (int i = 0; i < coord.Length; i++)
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(coord[i].X.ToString("G17", CultureInfo.InvariantCulture) + " " + coord[i].Y.ToString("G17", CultureInfo.InvariantCulture));
                }
                buf.Append(")");
            }
            return buf.ToString();
        }

        /// <summary>
        /// Generates the WKT for a <c>LineString</c> specified by two <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="p0">The first coordinate.</param>
        /// <param name="p1">The second coordinate.</param>
        /// <returns>The WKT</returns>
        public static string ToLineString(Coordinate p0, Coordinate p1)
        {
            // legacy note: JTS's version never checks Z or M, so the things that call this aren't
            // expecting to see them.  the "actual" code to write lines handles Z / M just fine.
            return "LINESTRING ( " + p0.X.ToString("G17", CultureInfo.InvariantCulture) + " " + p0.Y.ToString("G17", CultureInfo.InvariantCulture) + ", " + p1.X.ToString("G17", CultureInfo.InvariantCulture) + " " + p1.Y.ToString("G17", CultureInfo.InvariantCulture) + " )";
        }

        /// <summary>
        /// Creates the <c>NumberFormatInfo</c> used to write <c>double</c>s
        /// with a sufficient number of decimal places.
        /// </summary>
        /// <param name="precisionModel">
        /// The <c>PrecisionModel</c> used to determine
        /// the number of decimal places to write.
        /// </param>
        /// <returns>
        /// A <c>NumberFormatInfo</c> that write <c>double</c>s
        /// without scientific notation.
        /// </returns>
        internal static NumberFormatInfo CreateFormatter(PrecisionModel precisionModel)
        {
            // the default number of decimal places is 16, which is sufficient
            // to accomodate the maximum precision of a double.
            int digits = precisionModel.MaximumSignificantDigits;
            int decimalPlaces = Math.Max(0, digits); // negative values not allowed

            // specify decimal separator explicitly to avoid problems in other locales
            var nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = decimalPlaces,
                NumberGroupSeparator = string.Empty,
                NumberGroupSizes = new int[] {}
            };
            return nfi;
        }

        /// <summary>
        /// Returns a <c>String</c> of repeated characters.
        /// </summary>
        /// <param name="ch">The character to repeat.</param>
        /// <param name="count">The number of times to repeat the character.</param>
        /// <returns>A <c>string</c> of characters.</returns>
        private static string StringOfChar(char ch, int count)
        {
            var buf = new StringBuilder();
            for (int i = 0; i < count; i++)
                buf.Append(ch);
            return buf.ToString();
        }

        /// <summary>
        /// A filter implementation to test if a coordinate sequence actually has meaningful values
        /// for an ordinate bit-pattern
        /// </summary>
        private class CheckOrdinatesFilter : ICoordinateSequenceFilter
        {
            private readonly Ordinates _checkOrdinateFlags;
            private Ordinates _outputOrdinates;

            /// <summary>
            /// Initializes a new instance of the <see cref="CheckOrdinatesFilter"/> flag.
            /// </summary>
            /// <param name="checkOrdinateFlags">
            /// The index for the ordinates to test.
            /// </param>
            public CheckOrdinatesFilter(Ordinates checkOrdinateFlags)
            {
                _outputOrdinates = Ordinates.XY;
                _checkOrdinateFlags = checkOrdinateFlags;
            }

            /// <inheritdoc />
            public void Filter(ICoordinateSequence seq, int i)
            {
                if (_checkOrdinateFlags.HasFlag(Ordinates.Z) && !_outputOrdinates.HasFlag(Ordinates.Z))
                {
                    if (!double.IsNaN(seq.GetZ(i)))
                    {
                        _outputOrdinates |= Ordinates.Z;
                    }
                }

                if (_checkOrdinateFlags.HasFlag(Ordinates.M) && !_outputOrdinates.HasFlag(Ordinates.M))
                {
                    if (!double.IsNaN(seq.GetM(i)))
                    {
                        _outputOrdinates |= Ordinates.M;
                    }
                }
            }

            /// <inheritdoc />
            public bool GeometryChanged => false;

            /// <inheritdoc />
            public bool Done => _outputOrdinates == _checkOrdinateFlags;

            /// <summary>
            /// Gets the evaluated ordinate bit-pattern of ordinates with valid values masked by
            /// <see cref="_checkOrdinateFlags"/>.
            /// </summary>
            public Ordinates OutputOrdinates => _outputOrdinates;
        }

        private Ordinates _outputOrdinates;
        private readonly int _outputDimension;

        private PrecisionModel _precisionModel;
        private bool _isFormatted;
        private int _coordsPerLine = -1;
        private string _indentTabStr;
        //private bool _zIsMeasure;

        public WKTWriter() : this(2) { }

        public WKTWriter(int outputDimension)
        {
            this.Tab = 2;
            if (outputDimension < 2 || outputDimension > 4)
                throw new ArgumentException("Output dimension must be in the range [2, 4]", "outputDimension");
            _outputDimension = outputDimension;

            switch (outputDimension)
            {
                case 2:
                    _outputOrdinates = Ordinates.XY;
                    break;

                case 3:
                    _outputOrdinates = Ordinates.XY | Ordinates.Ordinate2;
                    break;

                case 4:
                    _outputOrdinates = Ordinates.XYZM;
                    break;
            }
        }

        /// <summary>
        /// Gets/sets whther the output woll be formatted
        /// </summary>
        public bool Formatted
        {
            get => _isFormatted;
            set => _isFormatted = value;
        }

        /// <summary>
        /// Gets/sets the maximum number of coordinates per line written in formatted output.
        /// </summary>
        /// <remarks>If the provided coordinate number is &lt; 0, coordinates will be written all on one line.</remarks>
        public int MaxCoordinatesPerLine
        {
            get => _coordsPerLine;
            set => _coordsPerLine = value;
        }

        /// <summary>Gets/sets the tab size to use for indenting.</summary>
        /// <exception cref="ArgumentException">If the size is non-positive</exception>
        public int Tab
        {
            get => _indentTabStr.Length;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Tab count must be positive", "value");
                _indentTabStr = StringOfChar(' ', value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Ordinates"/> to be written.  Possible members are:
        /// <list type="bullet">
        /// <item><description><see cref="Ordinates.X"/></description></item>
        /// <item><description><see cref="Ordinates.Y"/></description></item>
        /// <item><description><see cref="Ordinates.Z"/></description></item>
        /// <item><description><see cref="Ordinates.M"/></description></item>
        /// </list>
        /// Values of <see cref="Ordinates.X"/> and <see cref="Ordinates.Y"/> are always assumed and
        /// not particularly checked for.
        /// </summary>
        public Ordinates OutputOrdinates
        {
            get => _outputOrdinates;
            set => _outputOrdinates = Ordinates.XY | (value & Ordinates.XYZM);
        }

        /// <summary>
        /// Gets or sets a <see cref="PrecisionModel"/> that should be used on the ordinates written.
        /// <para>
        /// If none/<see langword="null"/> is assigned, the precision model of the
        /// <see cref="Geometry.Factory"/> is used.
        /// </para>
        /// <para>
        /// Note: The precision model is applied to all ordinate values, not just x and y.
        /// </para>
        /// </summary>
        public PrecisionModel PrecisionModel
        {
            get => _precisionModel;
            set => _precisionModel = value;
        }

        public bool EmitSRID { get; set; }

        public bool EmitZ{ get; set; }

        public bool EmitM{ get; set; }

        /// <summary>
        /// Converts a <c>Geometry</c> to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process.</param>
        /// <returns>A Geometry Tagged Text string (see the OpenGIS Simple Features Specification).</returns>
        public virtual string Write(Geometry geometry)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            // determine the precision model
            var pm = _precisionModel ?? geometry.Factory.PrecisionModel;

            try
            {
                WriteFormatted(geometry, false, sw, pm);
            }
            catch (IOException)
            {
                Assert.ShouldNeverReachHere();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a <c>Geometry</c> to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process.</param>
        /// <param name="stream">A <c>Stream</c> to write into</param>
        public void Write(Geometry geometry, Stream stream)
        {
            var sw = new StreamWriter(stream);

            // determine the precision model
            var pm = _precisionModel ?? geometry.Factory.PrecisionModel;

            try
            {
                WriteFormatted(geometry, _isFormatted, sw, pm);
            }
            catch (IOException)
            {
                Assert.ShouldNeverReachHere();
            }
        }

        /// <summary>
        /// Converts a <c>Geometry</c> to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process.</param>
        /// <param name="writer"></param>
        /// <returns>A "Geometry Tagged Text" string (see the OpenGIS Simple Features Specification)</returns>
        public virtual void Write(Geometry geometry, TextWriter writer)
        {
            // determine the precision model
            var pm = _precisionModel ?? geometry.Factory.PrecisionModel;

            WriteFormatted(geometry, _isFormatted, writer, pm);
        }

        /// <summary>
        /// Same as <c>write</c>, but with newlines and spaces to make the
        /// well-known text more readable.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process</param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces.
        /// </returns>
        public virtual string WriteFormatted(Geometry geometry)
        {
            var sw = new StringWriter();
            try
            {
                WriteFormatted(geometry, true, sw, _precisionModel);
            }
            catch (IOException)
            {
                Assert.ShouldNeverReachHere();
            }
            return sw.ToString();
        }

        /// <summary>
        /// Same as <c>write</c>, but with newlines and spaces to make the
        /// well-known text more readable.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process</param>
        /// <param name="writer"></param>
        /// <returns>
        /// A Geometry Tagged Text string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces.
        /// </returns>
        public virtual void WriteFormatted(Geometry geometry, TextWriter writer)
        {
            WriteFormatted(geometry, true, writer, _precisionModel);
        }

        /// <summary>
        /// Converts a <c>Geometry</c> to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process</param>
        /// <param name="useFormatting"></param>
        /// <param name="writer"></param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification).
        /// </returns>
        private void WriteFormatted(Geometry geometry, bool useFormatting, TextWriter writer, PrecisionModel precisionModel)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            // ensure we have a precision model
            precisionModel = precisionModel ?? geometry.PrecisionModel;

            // create the formatter
            bool useMaxPrecision = precisionModel.PrecisionModelType == PrecisionModels.Floating;
            var formatter = CreateFormatter(precisionModel);
            string format = "0." + StringOfChar('#', formatter.NumberDecimalDigits);

            // append the WKT
            AppendGeometryTaggedText(geometry, useFormatting, writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <see cref="Geometry"/> to &lt;Geometry Tagged Text&gt; format, then appends
        /// it to the writer.
        /// </summary>
        /// <param name="geometry">the <see cref="Geometry"/> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <param name="formatter">the <see cref="IFormatProvider"/> to use to convert from a precise coordinate to an external coordinate.</param>
        private void AppendGeometryTaggedText(Geometry geometry, bool useFormatting, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            // evaluate the ordinates actually present in the geometry
            var cof = new CheckOrdinatesFilter(_outputOrdinates);
            geometry.Apply(cof);

            // Append the WKT
            AppendGeometryTaggedText(geometry, cof.OutputOrdinates, useFormatting, 0, writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <see cref="Geometry"/> to &lt;Geometry Tagged Text&gt; format, then appends
        /// it to the writer.
        /// </summary>
        /// <param name="geometry">the <see cref="Geometry"/> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <param name="formatter">the <see cref="IFormatProvider"/> to use to convert from a precise coordinate to an external coordinate.</param>
        private void AppendGeometryTaggedText(Geometry geometry, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            Indent(useFormatting, level, writer);

            switch (geometry)
            {
                case Point point:
                    AppendPointTaggedText(point, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
                    break;

                case LinearRing linearRing:
                    AppendLinearRingTaggedText(linearRing, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
                    break;

                case LineString lineString:
                    AppendLineStringTaggedText(lineString, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
                    break;

                case Polygon polygon:
                    AppendPolygonTaggedText(polygon, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
                    break;

                case MultiPoint multiPoint:
                    AppendMultiPointTaggedText(multiPoint, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
                    break;

                case MultiLineString multiLineString:
                    AppendMultiLineStringTaggedText(multiLineString, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
                    break;

                case MultiPolygon multiPolygon:
                    AppendMultiPolygonTaggedText(multiPolygon, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
                    break;

                case GeometryCollection geometryCollection:
                    AppendGeometryCollectionTaggedText(geometryCollection, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
                    break;

                default:
                    Assert.ShouldNeverReachHere("Unsupported Geometry implementation:" + geometry.GetType());
                    break;
            }
        }

        /// <summary>
        /// Converts a <c>Coordinate</c> to Point Tagged Text format,
        /// then appends it to the writer.
        /// </summary>
        /// <param name="point">The <c>Point</c> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted</param>
        /// <param name="level">the indentation level</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="formatter">the formatter to use when writing numbers</param>
        private void AppendPointTaggedText(Point point, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write("POINT ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendSequenceText(point.CoordinateSequence, outputOrdinates, useFormatting, level, false, writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <c>LineString</c> to &lt;LineString Tagged Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="lineString">The <c>LineString</c> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted</param>
        /// <param name="level">the indentation level</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="formatter">the formatter to use when writing numbers</param>
        private void AppendLineStringTaggedText(LineString lineString, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write("LINESTRING ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendSequenceText(lineString.CoordinateSequence, outputOrdinates, useFormatting, level, false, writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <c>LinearRing</c> to &lt;LinearRing Tagged Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="linearRing">The <c>LinearRing</c> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted</param>
        /// <param name="level">the indentation level</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="formatter">the formatter to use when writing numbers</param>
        private void AppendLinearRingTaggedText(LinearRing linearRing, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write("LINEARRING ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendSequenceText(linearRing.CoordinateSequence, outputOrdinates, useFormatting, level, false, writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <c>Polygon</c> to Polygon Tagged Text format,
        /// then appends it to the writer.
        /// </summary>
        /// <param name="polygon">The <c>Polygon</c> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted</param>
        /// <param name="level">the indentation level</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="formatter">the formatter to use when writing numbers</param>
        private void AppendPolygonTaggedText(Polygon polygon, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write("POLYGON ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendPolygonText(polygon, outputOrdinates, useFormatting, level, false, writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <c>MultiPoint</c> to &lt;MultiPoint Tagged Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="multipoint">The <c>MultiPoint</c> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted</param>
        /// <param name="level">the indentation level</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="formatter">the formatter to use when writing numbers</param>
        private void AppendMultiPointTaggedText(MultiPoint multipoint, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write("MULTIPOINT ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendMultiPointText(multipoint, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <c>MultiLineString</c> to MultiLineString Tagged
        /// Text format, then appends it to the writer.
        /// </summary>
        /// <param name="multiLineString">The <c>MultiLineString</c> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted</param>
        /// <param name="level">the indentation level</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="formatter">the formatter to use when writing numbers</param>
        private void AppendMultiLineStringTaggedText(MultiLineString multiLineString, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write("MULTILINESTRING ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendMultiLineStringText(multiLineString, outputOrdinates, useFormatting, level, /*false, */writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <c>MultiPolygon</c> to MultiPolygon Tagged Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="multiPolygon">The <c>MultiPolygon</c> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted</param>
        /// <param name="level">the indentation level</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="formatter">the formatter to use when writing numbers</param>
        private void AppendMultiPolygonTaggedText(MultiPolygon multiPolygon, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write("MULTIPOLYGON ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendMultiPolygonText(multiPolygon, outputOrdinates, useFormatting, level, /*false, */writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Converts a <c>GeometryCollection</c> to GeometryCollection
        /// Tagged Text format, then appends it to the writer.
        /// </summary>
        /// <param name="geometryCollection">The <c>GeometryCollection</c> to process.</param>
        /// <param name="useFormatting">flag indicating that the output should be formatted</param>
        /// <param name="level">the indentation level</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="formatter">the formatter to use when writing numbers</param>
        private void AppendGeometryCollectionTaggedText(GeometryCollection geometryCollection, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write("GEOMETRYCOLLECTION ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendGeometryCollectionText(geometryCollection, outputOrdinates, useFormatting, level, writer, formatter, format, useMaxPrecision);
        }

        /// <summary>
        /// Appends the i'th coordinate from the sequence to the writer
        /// <para>
        /// If the <paramref name="seq"/> has coordinates that are <see cref="double.IsNaN">NaN</see>,
        /// these are not written, even though <see cref="_outputDimension"/> suggests this.
        /// </para>
        /// </summary>
        /// <param name="seq">the <see cref="ICoordinateSequence"/> to process</param>
        /// <param name="i">the index of the coordinate to write</param>
        /// <param name="writer">writer the output writer to append to</param>
        /// <param name="formatter">the formatter to use for writing ordinate values</param>
        /// <exception cref="IOException"></exception>
        private void AppendCoordinate(ICoordinateSequence seq, Ordinates outputOrdinates, int i, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            writer.Write(WriteNumber(seq.GetX(i), formatter, format, useMaxPrecision) + " " + WriteNumber(seq.GetY(i), formatter, format, useMaxPrecision));

            if (outputOrdinates.HasFlag(Ordinates.Z))
            {
                double z = seq.GetZ(i);
                if (!double.IsNaN(z))
                {
                    writer.Write(" ");
                    writer.Write(WriteNumber(seq.GetZ(i), formatter, format, useMaxPrecision));
                }
                else
                {
                    writer.Write(" NaN");
                }
            }

            if (outputOrdinates.HasFlag(Ordinates.M))
            {
                writer.Write(" ");
                writer.Write(WriteNumber(seq.GetM(i), formatter, format, useMaxPrecision));
            }
        }

        /// <summary>
        /// Converts a <see cref="double" /> to a <see cref="string" />.
        /// </summary>
        /// <param name="d">The <see cref="double" /> to convert.</param>
        /// <returns>
        /// The <see cref="double" /> as a <see cref="string" />.
        /// </returns>
        private static string WriteNumber(double d, IFormatProvider formatProvider, string format, bool useMaxPrecision)
        {
            string standard = d.ToString(format, formatProvider);
            if (!useMaxPrecision)
            {
                return standard;
            }
            try
            {
                double converted = Convert.ToDouble(standard, formatProvider);
                // check if some precision is lost during text conversion: if so, use G17
                if (converted == d)
                    return standard;
            }
            catch (OverflowException ex)
            {
            }

            return d.ToString("G17", formatProvider);
        }

        /// <summary>
        /// Appends additional ordinate information. This function may
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// append 'Z' if in <paramref name="outputOrdinates"/> the <see cref="Ordinates.Z"/> value is included.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// append 'M' if in <paramref name="outputOrdinates"/> the <see cref="Ordinates.M"/> value is included.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// append 'ZM' if in <paramref name="outputOrdinates"/> the <see cref="Ordinates.Z"/> and
        /// <see cref="Ordinates.M"/> values are included.
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="outputOrdinates">a bit-pattern of ordinates to write.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <exception cref="IOException">if an error occurs while using the writer.</exception>
        private void AppendOrdinateText(Ordinates outputOrdinates, TextWriter writer)
        {
            if (outputOrdinates.HasFlag(Ordinates.Z))
            {
                writer.Write('Z');
            }

            if (outputOrdinates.HasFlag(Ordinates.M))
            {
                writer.Write('M');
            }
        }

        /// <summary>
        /// Appends all members of a <see cref="ICoordinateSequence"/> to the stream. Each
        /// <see cref="Coordinate"/> is separated from another using a colon, the ordinates of a
        /// <see cref="Coordinate"/> are separated by a space.
        /// </summary>
        /// <param name="seq">the <see cref="ICoordinateSequence"/> to process.</param>
        /// <param name="useFormatting">flag indicating that.</param>
        /// <param name="level">the indentation level.</param>
        /// <param name="indentFirst">flag indicating that the first <see cref="Coordinate"/> of the sequence should be indented for better visibility.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <param name="formatter">the formatter to use for writing ordinate values.</param>
        private void AppendSequenceText(ICoordinateSequence seq, Ordinates outputOrdinates, bool useFormatting, int level, bool indentFirst, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            if (seq.Count == 0)
            {
                writer.Write("EMPTY");
            }
            else
            {
                if (indentFirst) Indent(useFormatting, level, writer);
                writer.Write("(");
                for (int i = 0; i < seq.Count; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                        if (_coordsPerLine > 0
                            && i%_coordsPerLine == 0)
                        {
                            Indent(useFormatting, level + 1, writer);
                        }
                    }
                    AppendCoordinate(seq, outputOrdinates, i, writer, formatter, format, useMaxPrecision);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>Polygon</c> to Polygon Text format, then
        /// appends it to the writer.
        /// </summary>
        /// <param name="polygon">The <c>Polygon</c> to process.</param>
        /// <param name="useFormatting">flag indicating that.</param>
        /// <param name="level">the indentation level.</param>
        /// <param name="indentFirst">flag indicating that the first <see cref="Coordinate"/> of the sequence should be indented for better visibility.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <param name="formatter">the formatter to use for writing ordinate values.</param>
        private void AppendPolygonText(Polygon polygon, Ordinates outputOrdinates, bool useFormatting, int level, bool indentFirst, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            if (polygon.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                if (indentFirst) Indent(useFormatting, level, writer);
                writer.Write("(");
                AppendSequenceText(polygon.ExteriorRing.CoordinateSequence, outputOrdinates,
                    useFormatting, level, false, writer, formatter, format, useMaxPrecision);
                for (int i = 0; i < polygon.NumInteriorRings; i++)
                {
                    writer.Write(", ");
                    AppendSequenceText(polygon.GetInteriorRingN(i).CoordinateSequence, outputOrdinates,
                        useFormatting, level + 1, true, writer, formatter, format, useMaxPrecision);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>MultiPoint</c> to &lt;MultiPoint Text format, then
        /// appends it to the writer.
        /// </summary>
        /// <param name="multiPoint">The <c>MultiPoint</c> to process.</param>
        /// <param name="useFormatting">flag indicating that.</param>
        /// <param name="level">the indentation level.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <param name="formatter">the formatter to use for writing ordinate values.</param>
        private void AppendMultiPointText(MultiPoint multiPoint, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            if (multiPoint.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (int i = 0; i < multiPoint.NumGeometries; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                        IndentCoords(useFormatting, i, level + 1, writer);
                    }
                    AppendSequenceText(((Point)multiPoint.GetGeometryN(i)).CoordinateSequence,
                        outputOrdinates, useFormatting, level, false, writer, formatter, format, useMaxPrecision);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>MultiLineString</c> to &lt;MultiLineString Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="multiLineString">The <c>MultiLineString</c> to process.</param>
        /// <param name="useFormatting">flag indicating that.</param>
        /// <param name="level">the indentation level.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <param name="formatter">the formatter to use for writing ordinate values.</param>
        private void AppendMultiLineStringText(MultiLineString multiLineString, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            if (multiLineString.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                int level2 = level;
                bool doIndent = false;
                writer.Write("(");
                for (int i = 0; i < multiLineString.NumGeometries; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                        level2 = level + 1;
                        doIndent = true;
                    }
                    AppendSequenceText(((LineString) multiLineString.GetGeometryN(i)).CoordinateSequence,
                        outputOrdinates, useFormatting, level2, doIndent, writer, formatter, format, useMaxPrecision);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>MultiPolygon</c> to &lt;MultiPolygon Text format,
        /// then appends it to the writer.
        /// </summary>
        /// <param name="multiPolygon">The <c>MultiPolygon</c> to process.</param>
        /// <param name="useFormatting">flag indicating that.</param>
        /// <param name="level">the indentation level.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <param name="formatter">the formatter to use for writing ordinate values.</param>
        private void AppendMultiPolygonText(MultiPolygon multiPolygon, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            if (multiPolygon.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                int level2 = level;
                bool doIndent = false;
                writer.Write("(");
                for (int i = 0; i < multiPolygon.NumGeometries; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                        level2 = level + 1;
                        doIndent = true;
                    }
                    AppendPolygonText((Polygon) multiPolygon.GetGeometryN(i), outputOrdinates, useFormatting, level2, doIndent, writer, formatter, format, useMaxPrecision);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>GeometryCollection</c> to GeometryCollectionText
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="geometryCollection">The <c>GeometryCollection</c> to process.</param>
        /// <param name="useFormatting">flag indicating that.</param>
        /// <param name="level">the indentation level.</param>
        /// <param name="indentFirst">flag indicating that the first <see cref="Coordinate"/> of the sequence should be indented for better visibility.</param>
        /// <param name="writer">the output writer to append to.</param>
        /// <param name="formatter">the formatter to use for writing ordinate values.</param>
        private void AppendGeometryCollectionText(GeometryCollection geometryCollection, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, IFormatProvider formatter, string format, bool useMaxPrecision)
        {
            if (geometryCollection.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                int level2 = level;
                writer.Write("(");
                for (int i = 0; i < geometryCollection.NumGeometries; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                        level2 = level + 1;
                    }
                    AppendGeometryTaggedText(geometryCollection.GetGeometryN(i), outputOrdinates, useFormatting, level2, writer, formatter, format, useMaxPrecision);
                }
                writer.Write(")");
            }
        }

        private void IndentCoords(bool useFormatting, int coordIndex, int level, TextWriter writer)
        {
            if (_coordsPerLine <= 0 || coordIndex % _coordsPerLine != 0)
                return;
            Indent(useFormatting, level, writer);
        }

        private void Indent(bool useFormatting, int level, TextWriter writer)
        {
            if (!useFormatting || level <= 0) return;
            writer.Write("\n");
            for (int i = 0; i < level; i++)
                writer.Write(_indentTabStr);
        }

        #region Implementation of IGeometryIOSettings

        public bool HandleSRID
        {
            get => EmitSRID;
            set => EmitSRID = value;
        }

        public Ordinates AllowedOrdinates => Ordinates.XYZM;

        public Ordinates HandleOrdinates
        {
            get
            {
                var ret = Ordinates.XY;
                if (EmitZ) ret |= Ordinates.Z;
                if (EmitM) ret |= Ordinates.M;
                return ret;
            }
            set
            {
                value &= AllowedOrdinates;
                if ((value & Ordinates.Z) != 0) EmitZ = true;
                if ((value & Ordinates.M) != 0) EmitM = true;
            }
        }

        #endregion
    }
}
