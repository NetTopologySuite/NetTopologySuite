using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Windows.Forms
{

    ///<summary>
    /// Provides methods to read <see cref="System.Drawing.Font"/> glyphs for strings 
    /// into <see cref="IPolygonal"/> geometry.
    ///</summary>
    /// <remarks>
    /// <para>
    /// It is suggested to use larger point sizes to render fonts glyphs, to reduce the effects of scale-dependent hints.</para>
    /// <para>The resulting geometry are in the base coordinate system of the font.</para>
    /// <para>The geometry can be further transformed as necessary using <see cref="AffineTransformation"/>s</para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class FontGlyphReader
    {
        public const string FontSerif = "Serif";
        public const string FontSanserif = "SanSerif";
        public const string FontMonospaced = "Monospaced";

        // a flatness factor empirically determined to provide good results
        private const float FlatnessFactor = 400f;

        ///<summary>
        /// Converts text rendered in the given <see cref="Font"/> and pointsize to a <see cref="IGeometry"/> using a standard flatness factor.
        /// </summary>
        /// <param name="text">The text to render</param>
        /// <param name="font">The <see cref="FontFamily"/></param>
        /// <param name="pointSize">The pointSize to render at</param>
        /// <param name="geomFact">The geometry factory to use to create the result</param>
        /// <returns>A polygonal geometry representing the rendered text</returns>
        public static IGeometry Read(string text, FontFamily font, int pointSize, IGeometryFactory geomFact)
        {
            return Read(text, font, FontStyle.Regular, pointSize, new PointF(0,0),  geomFact);
        }

        ///<summary>
        /// Converts text rendered in the given <see cref="Font"/> to a <see cref="IGeometry"/> using a standard flatness factor.
        /// </summary>
        /// <param name="text">The text to render</param>
        /// <param name="font">The <see cref="FontFamily"/></param>
        /// <param name="geomFact">The geometry factory to use to create the result</param>
        /// <returns>A polygonal geometry representing the rendered text</returns>
        public static IGeometry Read(string text, FontFamily font, IGeometryFactory geomFact)
        {
            return Read(text, font, FontStyle.Regular, 12, new PointF(0,0), geomFact);
        }

        public static IGeometry Read(string text, FontFamily font, FontStyle style, float size, PointF origin, IGeometryFactory geomFact)

        {
            return Read(text, font, style, size, origin, StringFormat.GenericTypographic, size / FlatnessFactor, geomFact);
        }

        ///<summary>
        /// Converts text rendered in the given <see cref="Font"/> and pointsize to a <see cref="IGeometry"/> using a standard flatness factor.
        /// </summary>
        /// <param name="text">The text to render</param>
        /// <param name="font">The <see cref="FontFamily"/></param>
        /// <param name="size">The size to render at</param>
        /// <param name="style">The style to use</param>
        /// <param name="flatness">The flatness to use</param>
        /// <param name="origin">The point to start</param>
        /// <param name="stringFormat">The string format to use</param>
        /// <param name="geomFact">The geometry factory to use to create the result</param>
        /// <returns>A polygonal geometry representing the rendered text</returns>
        public static IGeometry Read(string text, FontFamily font, FontStyle style, float size, PointF origin, StringFormat stringFormat, double flatness, IGeometryFactory geomFact)
        {
            var path = new GraphicsPath();
            path.AddString(text, font, (int)style, size, origin, stringFormat);
            return GraphicsPathReader.Read(path, flatness, geomFact);
        }

        public static IGeometry Read(string text, Font font, IGeometryFactory geomFact)
        {
            return Read(text, font.FontFamily, font.Style, font.Size, new PointF(0, 0), StringFormat.GenericTypographic,
                        FlatnessFactor, geomFact);
        }
    }
}