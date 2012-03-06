using System.Collections.Generic;
using DotSpatial.Topology;

namespace DotSpatial.Data
{
    /// <summary>
    /// Spatial operations
    /// </summary>
    public enum SpatialPredicate
    {
        Contains,
        ContainsProperly,
        Covers,
        CoveredBy,
        Crosses,
        Disjoint,
        Intersects,
        Overlaps,
        Touches,
        Within,
    }

    public static class FeatureSetEx
    {
        public static List<IFeature> Select(this IFeatureSet self, Shape shape, SpatialPredicate predicate = SpatialPredicate.Intersects)
        {
            var result = new List<IFeature>();
            var pg = new NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory().Create(shape.ToGeoAPI());

            foreach (var feat in self.Features)
            {
                var valid = false;
                var tryShape = feat.ToShape().ToGeoAPI();
                switch (predicate)
                {
                    case SpatialPredicate.Intersects:
                        valid = pg.Intersects(tryShape);
                        break;
                    case SpatialPredicate.Overlaps:
                        valid = pg.Overlaps(tryShape);
                        break;
                    case SpatialPredicate.Touches:
                        valid = pg.Touches(tryShape);
                        break;
                    case SpatialPredicate.Within:
                        valid = pg.Within(tryShape);
                        break;
                    case SpatialPredicate.Contains:
                        valid = pg.Contains(tryShape);
                        break;
                    case SpatialPredicate.ContainsProperly:
                        valid = pg.ContainsProperly(tryShape);
                        break;
                    case SpatialPredicate.Covers:
                        valid = pg.Covers(tryShape);
                        break;
                    case SpatialPredicate.CoveredBy:
                        valid = pg.CoveredBy(tryShape);
                        break;
                    case SpatialPredicate.Disjoint:
                        valid = pg.Disjoint(tryShape);
                        break;
                }
                if (valid)
                    result.Add(feat);
            }
            return result;
        }

        /*
        public static void Join(this IFeatureSet self, IFeatureSet join, SpatialPredicate predicate = SpatialPredicate.Intersects)
        {
            if (self.DataTable == null)
                throw new InvalidOperationException("Cannot join without associated DataTable (self)");
            if (join.DataTable == null)
                throw new InvalidOperationException("Cannot join without associated DataTable (join)");

            var dataset = self.DataTable.DataSet ?? join.DataTable.DataSet;
            if (dataset == null)
            {
                dataset = new System.Data.DataSet(string.Format("{0}-{1}-Join",
                                                                self.DataTable.TableName,
                                                                join.DataTable.TableName));
                var joinTable = new System.Data.DataTable(string.Format("Join-{0}-{1}",
                                                                        self.DataTable.TableName,
                                                                        join.DataTable.TableName));
                joinTable.Columns.AddRange(new[]
                                               {
                                                   new DataColumn("master", typeof (Int32)),
                                                   new DataColumn("slave", typeof (Int32))
                                               });

                dataset.Tables.AddRange(new []{ self.DataTable, join.DataTable, joinTable });
            }

            var pg = new NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory().Create(shape.ToGeoAPI());

            foreach (var feat in self.Features)
            {
                var valid = false;
                var tryShape = feat.ToShape().ToGeoAPI();
                switch (predicate)
                {
                    case SpatialPredicate.Intersects:
                        valid = pg.Intersects(tryShape);
                        break;
                    case SpatialPredicate.Overlaps:
                        valid = pg.Overlaps(tryShape);
                        break;
                    case SpatialPredicate.Touches:
                        valid = pg.Touches(tryShape);
                        break;
                    case SpatialPredicate.Within:
                        valid = pg.Within(tryShape);
                        break;
                    case SpatialPredicate.Contains:
                        valid = pg.Contains(tryShape);
                        break;
                    case SpatialPredicate.ContainsProperly:
                        valid = pg.ContainsProperly(tryShape);
                        break;
                    case SpatialPredicate.Covers:
                        valid = pg.Covers(tryShape);
                        break;
                    case SpatialPredicate.CoveredBy:
                        valid = pg.CoveredBy(tryShape);
                        break;
                    case SpatialPredicate.Disjoint:
                        valid = pg.Disjoint(tryShape);
                        break;
                }
                if (valid)
                    result.Add(feat);
            }
            return result;
        }

         */
    }
}