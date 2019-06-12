using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts all the 1-dimensional (<see cref="LineString"/>) components from a <see cref="Geometry"/>.
    /// For polygonal geometries, this will extract all the component <see cref="LinearRing"/>s.
    /// If desired, <see cref="LinearRing"/>s can be forced to be returned as <see cref="LineString"/>s.
    /// </summary>
    public class LinearComponentExtracter : IGeometryComponentFilter
    {
        /// <summary>
        /// Extracts the linear components from a <see cref="IEnumerable{Geometry}"/>
        /// and adds them to the provided <see cref="ICollection{LineString}"/>.
        /// </summary>
        /// <param name="geoms">The geometry from which to extract linear components</param>
        /// <param name="lines">The Collection to add the extracted linear components to</param>
        /// <returns>The Collection of linear components (LineStrings or LinearRings)</returns>
        public static TCollection GetLines<TCollection>(IEnumerable<Geometry> geoms, TCollection lines)
            where TCollection : ICollection<Geometry>
        {
            foreach (var g in geoms)
            {
                GetLines(g, lines);
            }
            return lines;
        }

        /// <summary>
        /// Extracts the linear components from a <see cref="IEnumerable{Geometry}"/>
        /// and adds them to the provided <see cref="ICollection{LineString}"/>.
        /// </summary>
        /// <param name="geoms">The geometry from which to extract linear components</param>
        /// <param name="lines">The Collection to add the extracted linear components to</param>
        /// <param name="forceToLineString"></param>
        /// <returns>The Collection of linear components (LineStrings or LinearRings)</returns>
        public static TCollection GetLines<TCollection>(IEnumerable<Geometry> geoms, TCollection lines, bool forceToLineString)
            where TCollection : ICollection<Geometry>
        {
            foreach (var g in geoms)
            {
                GetLines(g, lines, forceToLineString);
            }
            return lines;
        }

        /// <summary>
        /// Extracts the linear components from a single <see cref="Geometry"/>
        /// and adds them to the provided <see cref="ICollection{LineString}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract linear components</param>
        /// <param name="lines">The Collection to add the extracted linear components to</param>
        /// <returns>The Collection of linear components (LineStrings or LinearRings)</returns>
        public static TCollection GetLines<TCollection>(Geometry geom, TCollection lines)
            where TCollection : ICollection<Geometry>
        {
            if (geom is LineString)
            {
                lines.Add(geom);
            }
            else
            {
                geom.Apply(new LinearComponentExtracter(lines));
            }
            return lines;
        }

        /// <summary>
        /// Extracts the linear components from a single <see cref="Geometry"/>
        /// and adds them to the provided <see cref="ICollection{LineString}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract linear components</param>
        /// <param name="lines">The Collection to add the extracted linear components to</param>
        /// <param name="forceToLineString"></param>
        /// <returns>The Collection of linear components (LineStrings or LinearRings)</returns>
        public static TCollection GetLines<TCollection>(Geometry geom, TCollection lines, bool forceToLineString)
            where TCollection : ICollection<Geometry>
        {
            geom.Apply(new LinearComponentExtracter(lines, forceToLineString));
            return lines;
        }

        /// <summary>
        /// Extracts the linear components from a single point.
        /// If more than one point is to be processed, it is more
        /// efficient to create a single <c>LineExtracterFilter</c> instance
        /// and pass it to multiple geometries.
        /// </summary>
        /// <param name="geom">The point from which to extract linear components.</param>
        /// <returns>The list of linear components.</returns>
        public static ReadOnlyCollection<Geometry> GetLines(Geometry geom)
        {
            var lines = new List<Geometry>();
            geom.Apply(new LinearComponentExtracter(lines));
            return lines.AsReadOnly();
        }

        /// <summary>
        /// Extracts the linear components from a single geometry.
        /// If more than one geometry is to be processed, it is more
        /// efficient to create a single <see cref="LinearComponentExtracter"/> instance
        /// and pass it to multiple geometries.
        /// </summary>
        /// <param name="geom">The geometry from which to extract linear components</param>
        /// <param name="forceToLineString"><c>true</c> if <see cref="LinearRing"/>s should be converted to <see cref="LineString"/>s</param>
        /// <returns>The list of linear components</returns>
        public static ReadOnlyCollection<Geometry> GetLines(Geometry geom, bool forceToLineString)
        {
            var lines = new List<Geometry>();
            geom.Apply(new LinearComponentExtracter(lines, forceToLineString));
            return lines.AsReadOnly();
        }

        /// <summary>
        /// Extracts the linear components from a single <see cref="Geometry"/>
        /// and returns them as either a <see cref="LineString"/> or <see cref="MultiLineString"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <returns>A linear geometry</returns>
        public static Geometry GetGeometry(Geometry geom)
        {
            var list = GetLines(geom);
            return geom.Factory.BuildGeometry(list);
        }

        /// <summary>
        /// Extracts the linear components from a single <see cref="Geometry"/>
        /// and returns them as either a <see cref="LineString"/> or <see cref="MultiLineString"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="forceToLineString"><c>true</c> if <see cref="LinearRing"/>s should be converted to <see cref="LineString"/>s</param>
        /// <returns>A linear geometry</returns>
        public static Geometry GetGeometry(Geometry geom, bool forceToLineString)
        {
            var lines = GetLines(geom, forceToLineString);
            return geom.Factory.BuildGeometry(lines);
        }

        private readonly ICollection<Geometry> _lines;

        /// <summary>
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        /// </summary>
        /// <param name="lines"></param>
        public LinearComponentExtracter(ICollection<Geometry> lines) : this(lines, false) { }

        /// <summary>
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="isForcedToLineString"></param>
        public LinearComponentExtracter(ICollection<Geometry> lines, bool isForcedToLineString)
        {
            _lines = lines;
            IsForcedToLineString = isForcedToLineString;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsForcedToLineString { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(Geometry geom)
        {
            if (IsForcedToLineString && geom is LinearRing ring)
            {
                var line = geom.Factory.CreateLineString(ring.CoordinateSequence);
                _lines.Add(line);
                return;
            }
            // if not being forced, and this is a linear component
            if (geom is LineString)
                _lines.Add(geom);

            // else this is not a linear component, so skip it
        }
    }
}
