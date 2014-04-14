namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    internal static class TopoData
    {
        internal const string Reference = @"{
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
                                ""this"": ""that"",
                            }                            
                        },
                        ""arcs"": [[-2]]
                    },
                    ]
                }
            },
            ""arcs"": [
                [[102, 0], [103, 1], [104, 0], [105, 1]],
                [[100, 0], [101, 0], [101, 1], [100, 1], [100, 0]]
            ]
        }";
        internal const string Quantized = @"{
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
                  ""arcs"": [0]
                },
                {
                  ""type"": ""Polygon"",
                  ""properties"": {
                    ""prop0"": ""value0"",
                    ""prop1"": {
                        ""this"": ""that"",
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

        internal const string MultiReference = @"{
            ""type"": ""Topology"",
            ""objects"": {
                ""example"": {
                    ""type"": ""GeometryCollection"",
                    ""geometries"": [
                    {
                        ""type"": ""MultiPoint"",
                        ""coordinates"": [[102, 0.5]]
                    },
                    {
                        ""type"": ""MultiLineString"",
                        ""arcs"": [[0]]
                    },
                    {
                        ""type"": ""MultiPolygon"",
                        ""arcs"": [[[-2]]]
                    }
                    ]
                }
            },
            ""arcs"": [
                [[102, 0], [103, 1], [104, 0], [105, 1]],
                [[100, 0], [101, 0], [101, 1], [100, 1], [100, 0]]
            ]
        }";
        /*,
                   */

        internal const string MultiQuantized = @"{
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
                  ""type"": ""MultiPoint"",
                  ""coordinates"": [[4000, 5000]]
                },
                {
                  ""type"": ""MultiLineString"",
                  ""arcs"": [[0]]
                },
                {
                  ""type"": ""MultiPolygon"",
                  ""arcs"": [[[1]]]
                }
              ]
            }
          },
          ""arcs"": [
            [[4000, 0], [1999, 9999], [2000, -9999], [2000, 9999]],
            [[0, 0], [0, 9999], [2000, 0], [0, -9999], [-2000, 0]]
          ]
        }";

        internal const string Aruba = @"{
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

        internal const string Airports = @"{
            ""type"":""Topology"",
            ""transform"":{
                ""scale"":[0.032229964456445645,0.006392461796179619],
                ""translate"":[-176.6460306,7.367222]
            },
            ""objects"":{
                ""airports"":{
                    ""type"":""MultiPoint"",
                    ""coordinates"":[[2712,3846],[2532,3647],[2236,4939],[3059,5533],[2939,3648]]
                }
            },
            ""arcs"":[]
        }";

        internal const string Counties = @"{
	        ""type"":""Topology"",
            ""transform"":{
		        ""scale"":[0.0003589261789261791,0.0000537148685138684],
		        ""translate"":[-179.1473399999999,17.67439566600018]
	        },
	        ""objects"": {
		        ""counties"": {
			        ""type"":""GeometryCollection"",
			        ""geometries"":[
				        {
					        ""type"":""MultiPolygon"",
					        ""id"":53073,
					        ""arcs"":[[[0,1,2]]]
				        },
				        {
					        ""type"":""Polygon"",
					        ""id"":30105,
					        ""arcs"":[[3,4,5,6,7,8]]
				        }
			        ]
		        }
	        },	        
            ""arcs"":[
                [[162416,583189],[235,-863],[96,-3199],[218,-1080],[-270,-1240]],
                [[162695,576807],[-442,-309],[-4397,83]],
                [[157856,576581],[-6,1800],[-436,605],[-476,3063],[168,1173],[2781,-89],[2529,56]],
                [[203483,583172],[-111,-3267],[363,0],[1,-4839],[604,-13]],
                [[204340,575053],[0,-6407],[-109,-3],[-2,-3885]],
                [[204229,564758],[-1073,300],[-477,661],[-51,-1946]],
                [[202628,563773],[-410,-2654],[-860,-2419],[-1043,-435],[-462,561]],
                [[199853,558826],[28,9804],[99,1617],[303,-7],[-114,3279],[301,528],[-35,7509],[73,1627]],
                [[200508,583183],[2975,-11]]
            ]
        }";
    }
}
