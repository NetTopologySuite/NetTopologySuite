namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    internal static class TopoData
    {
        internal const string ReferenceData = @"{
            ""type"": ""Topology"",
            ""objects"": {
                ""example"": {
                    ""type"": ""GeometryCollection"",
                    ""geometries"": [
                    {
                        ""type"": ""Point"",
                        ""properties"": {
                            ""prop0"": ""value0""
                        },
                        ""coordinates"": [102, 0.5]
                    },
                    {
                        ""type"": ""LineString"",
                        ""arcs"": [0]
                    },
                    {
                        ""type"": ""Polygon"",
                        ""properties"": {
                            ""prop0"": ""value0"",
                            ""prop1"": {
                                ""this"": ""that""
                            }
                        },
                        ""arcs"": [[-2]]
                    }
                    ]
                }
            },
            ""arcs"": [
                [[102, 0], [103, 1], [104, 0], [105, 1]],
                [[100, 0], [101, 0], [101, 1], [100, 1], [100, 0]]
            ]
        }";

        internal static readonly dynamic ReferenceDef = new
        {
            type = "",
            objects = new
            {
                example = new
                {
                    type = "",
                    geometries = new object[]
                    {
                        new 
                        {
                            type = "",
                            properties = new
                            {
                                prop0 = "" 
                            },
                            coordinates = new double[0]                              
                        },
                        new 
                        {
                            type = "",                                
                            arcs = new int[0]                              
                        },
                        new 
                        {
                            type = "",
                            properties = new
                            {
                                prop0 = "",
                                prop1 = new
                                {
                                     @this= ""       
                                }
                            },
                            arcs = new int[0][],                          
                        }
                    }
                }
            },
            arcs = new int[0][][]
        };

        internal const string QuantizedData = @"{
          ""type"": ""Topology"",
          ""transform"": {
            ""scale"": [0.0005000500050005, 0.00010001000100010001],
            ""translate"": [100, 0]
          },
          ""objects"": {
            ""example"": {
              ""type"": ""GeometryCollection"",
              ""geometries"": [
                {
                  ""type"": ""Point"",
                  ""properties"": {
                    ""prop0"": ""value0""
                  },
                  ""coordinates"": [4000, 5000]
                },
                {
                  ""type"": ""LineString"",
                  ""properties"": {
                    ""prop0"": ""value0"",
                    ""prop1"": 0
                  },
                  ""arcs"": [0]
                },
                {
                  ""type"": ""Polygon"",
                  ""properties"": {
                    ""prop0"": ""value0"",
                    ""prop1"": {
                      ""this"": ""that""
                    }
                  },
                  ""arcs"": [[1]]
                }
              ]
            }
          },
          ""arcs"": [
            [[4000, 0], [1999, 9999], [2000, -9999], [2000, 9999]],
            [[0, 0], [0, 9999], [2000, 0], [0, -9999], [-2000, 0]]
          ]
        }";

        internal static readonly dynamic QuantizedDef = new
        {
            type = "",
            transform = new
            {
                scale = new double[] { },
                translate = new double[] { }
            },
            objects = new
            {
                example = new
                {
                    type = "",
                    geometries = new object[]
                    {
                        new 
                        {
                            type = "",
                            properties = new
                            {
                                prop0 = "" 
                            },
                            coordinates = new double[0]                              
                        },
                        new 
                        {
                            type = "",                                
                            arcs = new int[0]                              
                        },
                        new 
                        {
                            type = "",
                            properties = new
                            {
                                prop0 = "",
                                prop1 = new
                                {
                                     @this= ""       
                                }
                            },
                            arcs = new int[0][],                          
                        }
                    }
                }
            },
            arcs = new int[0][][]
        };

        internal const string ArubaData = @"{
            ""type"": ""Topology"",
            ""transform"": {
                ""scale"": [0.036003600360036005, 0.017361589674592462],
                ""translate"": [-180, -89.99892578124998]
            },
            ""objects"": {
                ""aruba"": {
                    ""type"": ""Polygon"",
                    ""arcs"": [[0]],
                    ""id"": 533
                }
            },
            ""arcs"": [
                [[3058, 5901], [0, -2], [-2, 1], [-1, 3], [-2, 3], [0, 3], [1, 1], [1, -3], [2, -5], [1, -1]]
            ]
        }";

        internal static readonly dynamic ArubaDef = new
        {
            type = "",
            transform = new
            {
                scale = new double[] { },
                translate = new double[] { }
            },
            objects = new
            {
                aruba = new
                {
                    type = "",
                    arcs = new int[0][],
                    id = ""
                }
            },
            arcs = new int[0][][]
        };
    }
}
