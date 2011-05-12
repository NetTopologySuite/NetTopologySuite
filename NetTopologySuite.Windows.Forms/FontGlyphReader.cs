using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Windows.Forms
{

    /**
     * Provides methods to read {@link Font} glyphs for strings 
     * into {@link Polygonal} geometry.
     * <p>
     * It is suggested to use larger point sizes to render fonts glyphs,
     * to reduce the effects of scale-dependent hints.
     * The resulting geometry are in the base coordinate system 
     * of the font.  
     * The geometry can be further transformed as necessary using
     * {@link AffineTransformation}s.
     * 
     * @author Martin Davis
     *
     */
    public class FontGlyphReader
    {
        public const string FontSerif = "Serif";
        public const string FontSanserif = "SanSerif";
        public const string FontMonospaced = "Monospaced";

        // a flatness factor empirically determined to provide good results
        private const float FlatnessFactor = 400f;

        /**
         * Converts text rendered in the given font and pointsize to a {@link Geometry}
         * using a standard flatness factor.
         *  
         * @param text the text to render
         * @param fontName the name of the font
         * @param pointSize the pointSize to render at
         * @param geomFact the geometryFactory to use to create the result
         * @return a polygonal geometry representing the rendered text
         */
        public static IGeometry Read(string text, FontFamily font, int pointSize, IGeometryFactory geomFact)
        {
            return Read(text, font, FontStyle.Regular, pointSize, new PointF(0,0),  geomFact);
        }

        /**
         * Converts text rendered in the given {@link Font} to a {@link Geometry}
         * using a standard flatness factor.
         * 
         * @param text the text to render
         * @param font  the font to render with
         * @param geomFact the geometryFactory to use to create the result
         * @return a polygonal geometry representing the rendered text
         */
        public static IGeometry Read(string text, FontFamily font, IGeometryFactory geomFact)
        {
            return Read(text, font, FontStyle.Regular, 12, new PointF(0,0), geomFact);
        }

        public static IGeometry Read(string text, FontFamily font, FontStyle style, float size, PointF origin, IGeometryFactory geomFact)

        {
            return Read(text, font, style, size, origin, StringFormat.GenericTypographic, size / FlatnessFactor, geomFact);
        }

        /**
         * Converts text rendered in the given {@link Font} to a {@link Geometry}
         * 
         * @param text the text to render
         * @param font  the font to render with
         * @param flatness the flatness to use
         * @param geomFact the geometryFactory to use to create the result
         * @return a polygonal geometry representing the rendered text
         */
        public static IGeometry Read(string text, FontFamily font, FontStyle style, float size, PointF origin, StringFormat stringFormat, double flatness, IGeometryFactory geomFact)
        {
            var path = new GraphicsPath();
            path.AddString(text, font, (int)style, size, origin, stringFormat);
            return GraphicsPathReader.Read(path, flatness, geomFact);
        }

    }
}