#define LikeJTS

using System;
using System.Globalization;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using GeoAPI.IO;
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
        /// Generates the WKT for a <c>Point</c>.
        /// </summary>
        /// <param name="p0">The point coordinate.</param>
        /// <returns></returns>
        public static String ToPoint(Coordinate p0)
        {
#if LikeJTS
			return "POINT(" + p0.X + " " + p0.Y + ")";
#else
            if (double.IsNaN(p0.Z))
				return "POINT(" + p0.X + " " + p0.Y + ")";
            return "POINT(" + p0.X + " " + p0.Y + " " + p0.Z + ")";
#endif
        }

        /// <summary>
        /// Generates the WKT for a N-point <c>LineString</c>.
        /// </summary>
        /// <param name="seq">The sequence to output.</param>
        /// <returns></returns>
        public static String ToLineString(ICoordinateSequence seq)
        {
            var buf = new StringBuilder();
            buf.Append("LINESTRING");
            if (seq.Count == 0)
                buf.Append(" EMPTY");
            else 
            {
                buf.Append("(");
                for (var i = 0; i < seq.Count; i++) 
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(seq.GetX(i) + " " + seq.GetY(i));
              }
              buf.Append(")");
            }
            return buf.ToString();
        }

        /// <summary>
        /// Generates the WKT for a 2-point <c>LineString</c>.
        /// </summary>
        /// <param name="p0">The first coordinate.</param>
        /// <param name="p1">The second coordinate.</param>
        /// <returns></returns>
        public static String ToLineString(Coordinate p0, Coordinate p1)
        {
#if LikeJTS
			return "LINESTRING(" + p0.X + " " + p0.Y + ", " + p1.X + " " + p1.Y + ")";
#else
            if (double.IsNaN(p0.Z))
				return "LINESTRING(" + p0.X + " " + p0.Y + ", " + p1.X + " " + p1.Y + ")";
            return "LINESTRING(" + p0.X + " " + p0.Y + " " + p0.Z + ", " + p1.X + " " + p1.Y + " " + p1.Z + ")";
#endif
        }

        // NOTE: modified for "safe" assembly in Sql 2005
        // const added!
        private const int WKTWriterIndent = 2;

        /// <summary>  
        /// Creates the <c>NumberFormatInfo</c> used to write <c>double</c>s
        /// with a sufficient number of decimal places.
        /// </summary>
        /// <param name="precisionModel"> 
        /// The <c>PrecisionModel</c> used to determine
        /// the number of decimal places to write.
        /// </param>
        /// <returns>
        /// A <c>NumberFormatInfo</c> that write <c>double</c>
        /// s without scientific notation.
        /// </returns>
        private static NumberFormatInfo CreateFormatter(IPrecisionModel precisionModel) 
        {
            // the default number of decimal places is 16, which is sufficient
            // to accomodate the maximum precision of a double.
            var decimalPlaces = precisionModel.MaximumSignificantDigits;

            // specify decimal separator explicitly to avoid problems in other locales
            var nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = decimalPlaces,
                NumberGroupSeparator = String.Empty,
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
        public static string StringOfChar(char ch, int count) 
        {
            var buf = new StringBuilder();
            for (var i = 0; i < count; i++) 
                buf.Append(ch);            
            return buf.ToString();
        }

        private readonly int _outputDimension = 2;

        private readonly string MaxPrecisionFormat = "{0:R}";
        private NumberFormatInfo _formatter;
        private string _format;
        private bool _isFormatted;
        private bool _useFormating = false;
        private bool _useMaxPrecision;
        private int _level;
        private int _coordsPerLine = -1;
        private String _indentTabStr = "  ";

        public WKTWriter()
        {
        }

        public WKTWriter(int outputDimension)
        {
            if (outputDimension < 2 || outputDimension > 3)
                throw new ArgumentException("Output dimension must be in the range [2, 3]", "outputDimension");
            _outputDimension = outputDimension;
        }

        ///<summary>
        /// Gets/sets whther the output woll be formatted
        ///</summary>
        public bool Formatted
        {
            get { return _isFormatted; }
            set { _isFormatted = value; }
        }

        ///<summary>
        /// Gets/sets the maximum number of coordinates per line written in formatted output.
        ///</summary>
        /// <remarks>If the provided coordinate number is &lt; 0, coordinates will be written all on one line.</remarks>
        public int MaxCoordinatesPerLine
        {
            get { return _coordsPerLine; }
            set { _coordsPerLine = value; }
        }

        ///<summary>Gets/sets the tab size to use for indenting.</summary>
        /// <exception cref="ArgumentException">If the size is non-positive</exception>
        public int Tab
        {
            get { return _indentTabStr.Length; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Tab count must be positive", "value");
                _indentTabStr = StringOfChar(' ', value);
            }
        }

        public bool EmitSRID { get; set; }

        public bool EmitZ{ get; set; }

        public bool EmitM{ get; set; }

        /// <summary>
        /// Converts a <c>Geometry</c> to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process.</param>
        /// <returns>A Geometry Tagged Text string (see the OpenGIS Simple Features Specification).</returns>
        public virtual string Write(IGeometry geometry)
        {
            TextWriter sw = new StringWriter();
            try 
            {
                WriteFormatted(geometry, _isFormatted, sw);
            }
            catch (IOException)
            {
                Assert.ShouldNeverReachHere();
            }
            return sw.ToString();
        }

        public void Write(IGeometry geometry, Stream stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts a <c>Geometry</c> to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process.</param>
        /// <param name="writer"></param>
        /// <returns>A "Geometry Tagged Text" string (see the OpenGIS Simple Features Specification)</returns>
        public virtual void Write(IGeometry geometry, TextWriter writer)
        {
            WriteFormatted(geometry, _isFormatted, writer);
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
        public virtual string WriteFormatted(IGeometry geometry)
        {
            TextWriter sw = new StringWriter();
            try 
            {
                WriteFormatted(geometry, true, sw);
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
        public virtual void WriteFormatted(IGeometry geometry, TextWriter writer)
        {
            WriteFormatted(geometry, true, writer);
        }

        /// <summary>
        /// Converts a <c>Geometry</c> to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A <c>Geometry</c> to process</param>
        /// <param name="useFormating"></param>
        /// <param name="writer"></param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification).
        /// </returns>
        private void WriteFormatted(IGeometry geometry, bool useFormating, TextWriter writer)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            _useFormating = useFormating;
            // Enable maxPrecision (via {0:R} formatter) in WriteNumber method
            _useMaxPrecision = geometry.Factory.PrecisionModel.PrecisionModelType == PrecisionModels.Floating;

            _formatter = CreateFormatter(geometry.PrecisionModel);           
            _format = "0." + StringOfChar('#', _formatter.NumberDecimalDigits);
            AppendGeometryTaggedText(geometry, 0, writer);

            // Disable maxPrecision as default setting
            _useMaxPrecision = false;
        }

        /// <summary>
        /// Converts a <c>Geometry</c> to &lt;Geometry Tagged Text format,
        /// then appends it to the writer.
        /// </summary>
        /// <param name="geometry">/he <c>Geometry</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">/he output writer to append to.</param>
        private void AppendGeometryTaggedText(IGeometry geometry, int level, TextWriter writer)
        {
            Indent(level, writer);

            if (geometry is IPoint) 
            {
                var point = (IPoint)geometry;
                AppendPointTaggedText(point.Coordinate, level, writer, point.PrecisionModel);
            }
            else if (geometry is ILinearRing)
                AppendLinearRingTaggedText((ILinearRing) geometry, level, writer);            
            else if (geometry is ILineString)
                AppendLineStringTaggedText((ILineString) geometry, level, writer);            
            else if (geometry is IPolygon) 
                AppendPolygonTaggedText((IPolygon) geometry, level, writer);
            else if (geometry is IMultiPoint) 
                AppendMultiPointTaggedText((IMultiPoint) geometry, level, writer);            
            else if (geometry is IMultiLineString) 
                AppendMultiLineStringTaggedText((IMultiLineString) geometry, level, writer);            
            else if (geometry is IMultiPolygon) 
                AppendMultiPolygonTaggedText((IMultiPolygon) geometry, level, writer);            
            else if (geometry is IGeometryCollection) 
                AppendGeometryCollectionTaggedText((IGeometryCollection) geometry, level, writer);
            else Assert.ShouldNeverReachHere("Unsupported Geometry implementation:" + geometry.GetType());
        }

        /// <summary>
        /// Converts a <c>Coordinate</c> to Point Tagged Text format,
        /// then appends it to the writer.
        /// </summary>
        /// <param name="coordinate">The <c>Coordinate</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="precisionModel"> 
        /// The <c>PrecisionModel</c> to use to convert
        /// from a precise coordinate to an external coordinate.
        /// </param>
        private void AppendPointTaggedText(Coordinate coordinate, int level, TextWriter writer, IPrecisionModel precisionModel)
        {
            writer.Write("POINT ");
            AppendPointText(coordinate, level, writer, precisionModel);
        }

        /// <summary>
        /// Converts a <c>LineString</c> to &lt;LineString Tagged Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="lineString">The <c>LineString</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendLineStringTaggedText(ILineString lineString, int level, TextWriter writer)
        {
            writer.Write("LINESTRING ");
            AppendLineStringText(lineString, level, false, writer);
        }

        /// <summary>
        /// Converts a <c>LinearRing</c> to &lt;LinearRing Tagged Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="linearRing">The <c>LinearRing</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendLinearRingTaggedText(ILinearRing linearRing, int level, TextWriter writer)
        {
            writer.Write("LINEARRING ");
            AppendLineStringText(linearRing, level, false, writer);
        }

        /// <summary>
        /// Converts a <c>Polygon</c> to Polygon Tagged Text format,
        /// then appends it to the writer.
        /// </summary>
        /// <param name="polygon">The <c>Polygon</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendPolygonTaggedText(IPolygon polygon, int level, TextWriter writer)
        {
            writer.Write("POLYGON ");
            AppendPolygonText(polygon, level, false, writer);
        }

        /// <summary>
        /// Converts a <c>MultiPoint</c> to &lt;MultiPoint Tagged Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="multipoint">The <c>MultiPoint</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendMultiPointTaggedText(IMultiPoint multipoint, int level, TextWriter writer)
        {
            writer.Write("MULTIPOINT ");
            AppendMultiPointText(multipoint, level, writer);
        }

        /// <summary>
        /// Converts a <c>MultiLineString</c> to MultiLineString Tagged
        /// Text format, then appends it to the writer.
        /// </summary>
        /// <param name="multiLineString">The <c>MultiLineString</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendMultiLineStringTaggedText(IMultiLineString multiLineString, int level, TextWriter writer)
        {
            writer.Write("MULTILINESTRING ");
            AppendMultiLineStringText(multiLineString, level, false, writer);
        }

        /// <summary>
        /// Converts a <c>MultiPolygon</c> to MultiPolygon Tagged Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="multiPolygon">The <c>MultiPolygon</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendMultiPolygonTaggedText(IMultiPolygon multiPolygon, int level, TextWriter writer)
        {
            writer.Write("MULTIPOLYGON ");
            AppendMultiPolygonText(multiPolygon, level, writer);
        }

        /// <summary>
        /// Converts a <c>GeometryCollection</c> to GeometryCollection
        /// Tagged Text format, then appends it to the writer.
        /// </summary>
        /// <param name="geometryCollection">The <c>GeometryCollection</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendGeometryCollectionTaggedText(IGeometryCollection geometryCollection, int level, TextWriter writer)
        {
            writer.Write("GEOMETRYCOLLECTION ");
            AppendGeometryCollectionText(geometryCollection, level, writer);
        }

        /// <summary>
        /// Converts a <c>Coordinate</c> to Point Text format, then
        /// appends it to the writer.
        /// </summary>
        /// <param name="coordinate">The <c>Coordinate</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="precisionModel">
        /// The <c>PrecisionModel</c> to use to convert
        /// from a precise coordinate to an external coordinate.
        /// </param>
        private void AppendPointText(Coordinate coordinate, int level, TextWriter writer, IPrecisionModel precisionModel)
        {
            if (coordinate == null) 
                writer.Write("EMPTY");
            else 
            {
                writer.Write("(");
                AppendCoordinate(coordinate, writer, precisionModel);
                writer.Write(")");
            }
        }

        ///<summary>Appends the i'th coordinate from the sequence to the writer</summary>
        /// <param name="seq">the <see cref="ICoordinateSequence"/> to process</param>
        /// <param name="i">the index of the coordinate to write</param>
        /// <param name="writer">writer the output writer to append to</param>
        ///<exception cref="IOException"></exception>
        private void AppendCoordinate(ICoordinateSequence seq, int i, TextWriter writer)
        {
            writer.Write(WriteNumber(seq.GetX(i)) + " " + WriteNumber(seq.GetY(i)));
            if (_outputDimension >= 3 && seq.Dimension >= 3)
            {
                double z = seq.GetOrdinate(i, Ordinate.Z);
                if (!Double.IsNaN(z))
                {
                    writer.Write(" ");
                    writer.Write(WriteNumber(z));
                }
            }
        }

        /// <summary>
        /// Converts a <c>Coordinate</c> to Point format, then appends
        /// it to the writer.
        /// </summary>
        /// <param name="coordinate">The <c>Coordinate</c> to process.</param>
        /// <param name="writer">The output writer to append to.</param>
        /// <param name="precisionModel">
        /// The <c>PrecisionModel</c> to use to convert
        /// from a precise coordinate to an external coordinate.
        /// </param>
        private void AppendCoordinate(Coordinate coordinate, TextWriter writer, IPrecisionModel precisionModel)
        {
			writer.Write(WriteNumber(coordinate.X) + " " + WriteNumber(coordinate.Y));
            if (_outputDimension >= 3 && !double.IsNaN(coordinate.Z))
			{
				writer.Write(" " + WriteNumber(coordinate.Z));
			}
        }

        /// <summary>
        /// Converts a <see cref="double" /> to a <see cref="string" />, 
        /// not in scientific notation.
        /// </summary>
        /// <param name="d">The <see cref="double" /> to convert.</param>
        /// <returns>
        /// The <see cref="double" /> as a <see cref="string" />, 
        /// not in scientific notation.
        /// </returns>
        private string WriteNumber(double d)
        {            
            var standard = d.ToString(_format, _formatter);
            if (!_useMaxPrecision)
                return standard;

            try
            {
                var converted = Convert.ToDouble(standard, _formatter);
                // Check if some precision is lost during text conversion: if so, use {0:R} formatter
                if (converted != d)
                    return String.Format(_formatter, MaxPrecisionFormat, d);
                return standard;
            }
            catch (OverflowException ex)
            {
                // http://groups.google.com/group/microsoft.public.dotnet.framework/browse_thread/thread/ed77db2f3fcb5a4a
                // Use MaxPrecisionFormat
                return String.Format(_formatter, MaxPrecisionFormat, d);
            }
        }

        ///<summary>Converts a <see cref="ICoordinateSequence"/> to &lt;LineString Text&gt; format, then appends it to the writer</summary>
        ///<exception cref="IOException"></exception>
        private void AppendSequenceText(ICoordinateSequence seq, int level, bool doIndent, TextWriter writer)
        {
            if (seq.Count == 0)
            {
                writer.Write("EMPTY");
            }
            else
            {
                if (doIndent) Indent(level, writer);
                writer.Write("(");
                for (int i = 0; i < seq.Count; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                        if (_coordsPerLine > 0
                            && i%_coordsPerLine == 0)
                        {
                            Indent(level + 1, writer);
                        }
                    }
                    AppendCoordinate(seq, i, writer);
                }
                writer.Write(")");
            }
        }


        /// <summary>
        /// Converts a <c>LineString</c> to &lt;LineString Text format, then
        /// appends it to the writer.
        /// </summary>
        /// <param name="lineString">The <c>LineString</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="doIndent"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendLineStringText(ILineString lineString, int level, bool doIndent, TextWriter writer)
        {
            if (lineString.IsEmpty)
                writer.Write("EMPTY");            
            else 
            {
                if (doIndent) Indent(level, writer);
                writer.Write("(");
                for (var i = 0; i < lineString.NumPoints; i++) 
                {
                    if (i > 0) 
                    {
                        writer.Write(", ");
                        if (_coordsPerLine > 0
                            && i % _coordsPerLine == 0)
                        {
                            Indent(level + 1, writer);
                        }
                    }
                    AppendCoordinate(lineString.GetCoordinateN(i), writer, lineString.PrecisionModel);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>Polygon</c> to Polygon Text format, then
        /// appends it to the writer.
        /// </summary>
        /// <param name="polygon">The <c>Polygon</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="indentFirst"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendPolygonText(IPolygon polygon, int level, bool indentFirst, TextWriter writer)
        {
            if (polygon.IsEmpty) 
                writer.Write("EMPTY");            
            else 
            {
                if (indentFirst) Indent(level, writer);
                writer.Write("(");
                AppendLineStringText(polygon.ExteriorRing, level, false, writer);
                for (var i = 0; i < polygon.NumInteriorRings; i++) 
                {
                    writer.Write(", ");
                    AppendLineStringText(polygon.GetInteriorRingN(i), level + 1, true, writer);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>MultiPoint</c> to &lt;MultiPoint Text format, then
        /// appends it to the writer.
        /// </summary>
        /// <param name="multiPoint">The <c>MultiPoint</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendMultiPointText(IMultiPoint multiPoint, int level, TextWriter writer)
        {
            if (multiPoint.IsEmpty) 
                writer.Write("EMPTY");
            else 
            {
                writer.Write("(");
                for (var i = 0; i < multiPoint.NumGeometries; i++) 
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                        IndentCoords(i, level + 1, writer);
                    }
                    writer.Write("(");
                    AppendCoordinate(((IPoint)multiPoint.GetGeometryN(i)).Coordinate, writer, multiPoint.PrecisionModel);
                    writer.Write(")");
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>MultiLineString</c> to &lt;MultiLineString Text
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="multiLineString">The <c>MultiLineString</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="indentFirst"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendMultiLineStringText(IMultiLineString multiLineString, int level, bool indentFirst, TextWriter writer)
        {
            if (multiLineString.IsEmpty) 
                writer.Write("EMPTY");            
            else 
            {
                var level2 = level;
                var doIndent = indentFirst;
                writer.Write("(");
                for (var i = 0; i < multiLineString.NumGeometries; i++) 
                {
                    if (i > 0) 
                    {
                        writer.Write(", ");
                        level2 = level + 1;
                        doIndent = true;
                    }
                    AppendLineStringText((ILineString) multiLineString.GetGeometryN(i), level2, doIndent, writer);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>MultiPolygon</c> to &lt;MultiPolygon Text format,
        /// then appends it to the writer.
        /// </summary>
        /// <param name="multiPolygon">The <c>MultiPolygon</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendMultiPolygonText(IMultiPolygon multiPolygon, int level, TextWriter writer)            
        {
            if (multiPolygon.IsEmpty) 
                writer.Write("EMPTY");            
            else 
            {
                var level2 = level;
                var doIndent = false;
                writer.Write("(");
                for (var i = 0; i < multiPolygon.NumGeometries; i++) 
                {
                    if (i > 0) 
                    {
                        writer.Write(", ");
                        level2 = level + 1;
                        doIndent = true;
                    }
                    AppendPolygonText((IPolygon) multiPolygon.GetGeometryN(i), level2, doIndent, writer);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a <c>GeometryCollection</c> to GeometryCollectionText
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="geometryCollection">The <c>GeometryCollection</c> to process.</param>
        /// <param name="level"></param>
        /// <param name="writer">The output writer to append to.</param>
        private void AppendGeometryCollectionText(IGeometryCollection geometryCollection, int level, TextWriter writer)            
        {
            if (geometryCollection.IsEmpty)
                writer.Write("EMPTY");            
            else 
            {
                var level2 = level;
                writer.Write("(");
                for (var i = 0; i < geometryCollection.NumGeometries; i++) 
                {
                    if (i > 0) 
                    {
                        writer.Write(", ");
                        level2 = level + 1;
                    }
                    AppendGeometryTaggedText(geometryCollection.GetGeometryN(i), level2, writer);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordIndex"></param>
        /// <param name="level"></param>
        /// <param name="writer"></param>
        /// <exception cref="IOException"></exception>
        private void IndentCoords(int coordIndex,  int level, TextWriter writer)
        {
            if (_coordsPerLine <= 0 || coordIndex % _coordsPerLine != 0)
                return;
            Indent(level, writer);
  }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="writer"></param>
        private void Indent(int level, TextWriter writer)
        {
            if (!_useFormating || level <= 0) return;
            writer.Write("\n");
            for (int i = 0; i < level; i++)
                writer.Write(_indentTabStr);
        }

        #region Implementation of IGeometryIOSettings

        public bool HandleSRID
        {
            get { return EmitSRID; }
            set { EmitSRID = value; }
        }

        public Ordinates AllowedOrdinates
        {
            get { return Ordinates.XYZM; }
        }

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
