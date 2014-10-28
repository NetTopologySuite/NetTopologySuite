namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    internal static class TopoWriterData
    {
        internal const string SimplePoint = @"{""type"":""Topology"",""objects"":{""data"":{""type"":""Point"",""coordinates"":[23.4,56.7],""properties"":{""prop0"":""value0"",""prop1"":""value1""}}}}";

        internal const string SimpleLineString = @"{""type"":""Topology"",""objects"":{""data"":{""type"":""LineString"",""arcs"":[0],""properties"":{""prop0"":""value0"",""prop1"":""value1""}}},""arcs"":[[[10.1,10.0],[20.2,20.0],[30.3,30.0]]]}";

        internal const string SimplePolygon = @"{""type"":""Topology"",""objects"":{""data"":{""type"":""Polygon"",""arcs"":[[0]],""properties"":{""prop0"":""value0"",""prop1"":""value1""}}},""arcs"":[[[10.1,10.0],[20.2,20.0],[30.3,30.0],[10.1,10.0]]]}";
        internal const string PolygonWithHole = @"{""type"":""Topology"",""objects"":{""data"":{""type"":""Polygon"",""arcs"":[[0,1]],""properties"":{""prop0"":""value0"",""prop1"":""value1""}}},""arcs"":[[[10.1,10.0],[20.2,20.0],[30.3,30.0],[10.1,10.0]],[[15.0,15.0],[17.0,15.0],[15.0,17.0],[15.0,15.0]]]}";
    }
}