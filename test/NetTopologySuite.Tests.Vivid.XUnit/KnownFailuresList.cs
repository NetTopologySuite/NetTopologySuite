// The corpus cases that fail today, grouped by why.
//
// They are excluded from CI through the FailureCase category, the same one the rest of the suite
// uses, so that the test story can land ahead of the investigation into each group. An entry is
// not a permanent exception: each should be traced back to either a defect in the library or a
// case that never applied to this port, and removed once that is settled.
//
// Most of these have never run before: the runner reached only a fraction of the corpus, so this
// is the first time anyone has seen them. They are not regressions - no case that passed before
// fails now. 134 of the 164 are a difference between the legacy implementations this library
// installs by default and the NG ones, not a defect in either; the two groups below say which.
//
// The index is the position of the case within the collection loaded for that file, which is the
// number the test name carries. The description is stored so that an entry cannot go on pointing
// at a case that has since moved: KnownFailuresTests compares it against the case actually sitting
// at that index and fails if a file has drifted underneath the list.
//
using NetTopologySuite.Tests.XUnit;

// ---- 108 cases: the relate implementation decides them -------------------------------------
//
// Every one of these passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy
// the runner installs, which is this library's own default. All 108 are in one file,
// misc/TestRelateGC.xml, which had never been executed here at all - no fixture was ever written
// for it. Under the legacy relate they fail in three different ways, which is worth knowing when
// reading them one by one but does not make them three different problems: 58 are rejected with
// "Operation does not support GeometryCollection arguments", 29 throw a side location conflict,
// and 21 simply answer differently.

[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 0,
    "GC:L/GC:PL - a line with the same line in a collection with an empty polygon - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 1,
    "GC:L/GC:PL - a line with the same line in a collection with an empty polygon - 2",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 3,
    "GC:L/GC:PL - a line with the same line in a collection with an empty polygon - 4",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 4,
    "GC:L/GC:PL - a line with the same line in a collection with an empty polygon - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 6,
    "GC:L/GC:PL - a line with the same line in a collection with an empty polygon - 7",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 11,
    "A/GC:mP - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 15,
    "A/GC:mP - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 22,
    "A/GC:mP with empty MultiPoint elements - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 26,
    "A/GC:mP with empty MultiPoint elements - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 33,
    "mA/GC:PL - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 34,
    "mA/GC:PL - 2",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 36,
    "mA/GC:PL - 4",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 37,
    "mA/GC:PL - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 44,
    "GC:PL/A - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 48,
    "GC:PL/A - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 53,
    "GC:PL/A - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 55,
    "P/GC:PL - point on boundary of GC with line and point - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 59,
    "P/GC:PL - point on boundary of GC with line and point - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 66,
    "L/GC:A - line in interior of GC of overlapping polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 68,
    "L/GC:A - line in interior of GC of overlapping polygons - 3",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 70,
    "L/GC:A - line in interior of GC of overlapping polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 74,
    "L/GC:A - line in interior of GC of overlapping polygons - 9",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 75,
    "L/GC:A - line in interior of GC of overlapping polygons - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 76,
    "L/GC:A - line in interior of GC of overlapping polygons - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 77,
    "P/GC:A - point on common boundaries of 2 adjacent polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 81,
    "P/GC:A - point on common boundaries of 2 adjacent polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 86,
    "P/GC:A - point on common boundaries of 2 adjacent polygons - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 87,
    "P/GC:A - point on common boundaries of 2 adjacent polygons - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 88,
    "P/GC:A - point on common node of 3 adjacent polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 92,
    "P/GC:A - point on common node of 3 adjacent polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 97,
    "P/GC:A - point on common node of 3 adjacent polygons - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 98,
    "P/GC:A - point on common node of 3 adjacent polygons - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 99,
    "P/GC:A - point on common node of 6 adjacent polygons with holes at node - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 103,
    "P/GC:A - point on common node of 6 adjacent polygons with holes at node - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 108,
    "P/GC:A - point on common node of 6 adjacent polygons with holes at node - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 109,
    "P/GC:A - point on common node of 6 adjacent polygons with holes at node - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 110,
    "P/GC:A - point on common node of 5 adjacent polygons with holes at node and one",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 114,
    "P/GC:A - point on common node of 5 adjacent polygons with holes at node and one",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 121,
    "L/GC:A - line on common boundaries of adjacent polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 125,
    "L/GC:A - line on common boundaries of adjacent polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 132,
    "L/GC:A - line on exterior boundaries of GC of overlapping polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 134,
    "L/GC:A - line on exterior boundaries of GC of overlapping polygons - 3",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 136,
    "L/GC:A - line on exterior boundaries of GC of overlapping polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 140,
    "L/GC:A - line on exterior boundaries of GC of overlapping polygons - 9",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 141,
    "L/GC:A - line on exterior boundaries of GC of overlapping polygons - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 142,
    "L/GC:A - line on exterior boundaries of GC of overlapping polygons - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 143,
    "GC:L/GC:A - lines covers boundaries of overlapping polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 145,
    "GC:L/GC:A - lines covers boundaries of overlapping polygons - 3",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 147,
    "GC:L/GC:A - lines covers boundaries of overlapping polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 149,
    "GC:L/GC:A - lines covers boundaries of overlapping polygons - 7",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 151,
    "GC:L/GC:A - lines covers boundaries of overlapping polygons - 9",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 152,
    "GC:L/GC:A - lines covers boundaries of overlapping polygons - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 153,
    "GC:L/GC:A - lines covers boundaries of overlapping polygons - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 154,
    "GC:A/GC:A - adjacent polygons contained by adjacent polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 158,
    "GC:A/GC:A - adjacent polygons contained by adjacent polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 165,
    "GC:A/P - adjacent polygons contain point at interior node - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 166,
    "GC:A/P - adjacent polygons contain point at interior node - 2",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 169,
    "GC:A/P - adjacent polygons contain point at interior node - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 174,
    "GC:A/P - adjacent polygons contain point at interior node - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 176,
    "GC:A/P - adjacent polygons contain point on interior edge - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 177,
    "GC:A/P - adjacent polygons contain point on interior edge - 2",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 180,
    "GC:A/P - adjacent polygons contain point on interior edge - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 185,
    "GC:A/P - adjacent polygons contain point on interior edge - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 187,
    "GC:A/P - adjacent polygons cover point on exterior node - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 191,
    "GC:A/P - adjacent polygons cover point on exterior node - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 198,
    "GC:A/L - adjacent polygons contain line touching interior node - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 202,
    "GC:A/L - adjacent polygons contain line touching interior node - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 209,
    "GC:A/L - adjacent polygons contain line along interior edge to boundary - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 213,
    "GC:A/L - adjacent polygons contain line along interior edge to boundary - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 220,
    "GC:A/GC:PL - adjacent polygons contain line and point - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 221,
    "GC:A/GC:PL - adjacent polygons contain line and point - 2",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 223,
    "GC:A/GC:PL - adjacent polygons contain line and point - 4",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 224,
    "GC:A/GC:PL - adjacent polygons contain line and point - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 231,
    "GC:A/A - adjacent polygons containing polygon with endpoint inside - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 232,
    "GC:A/A - adjacent polygons containing polygon with endpoint inside - 2",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 234,
    "GC:A/A - adjacent polygons containing polygon with endpoint inside - 4",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 235,
    "GC:A/A - adjacent polygons containing polygon with endpoint inside - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 239,
    "GC:A/A - adjacent polygons containing polygon with endpoint inside - 9",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 242,
    "GC:A/A - adjacent polygons overlapping polygon with shell outside and hole insid",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 246,
    "GC:A/A - adjacent polygons overlapping polygon with shell outside and hole insid",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 253,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 254,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 2",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 255,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 3",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 256,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 4",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 257,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 259,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 7",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 261,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 9",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 262,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 263,
    "GC:A/GC:A - overlapping polygons equal to overlapping polygons - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 264,
    "GC:A/GC:A - overlapping polygons contained by overlapping polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 266,
    "GC:A/GC:A - overlapping polygons contained by overlapping polygons - 3",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 268,
    "GC:A/GC:A - overlapping polygons contained by overlapping polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 272,
    "GC:A/GC:A - overlapping polygons contained by overlapping polygons - 9",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 273,
    "GC:A/GC:A - overlapping polygons contained by overlapping polygons - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 274,
    "GC:A/GC:A - overlapping polygons contained by overlapping polygons - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 275,
    "A/GC:A - polygon equal to nested overlapping polygons - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 277,
    "A/GC:A - polygon equal to nested overlapping polygons - 3",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 279,
    "A/GC:A - polygon equal to nested overlapping polygons - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 281,
    "A/GC:A - polygon equal to nested overlapping polygons - 7",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 283,
    "A/GC:A - polygon equal to nested overlapping polygons - 9",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 284,
    "A/GC:A - polygon equal to nested overlapping polygons - 10",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 285,
    "A/GC:A - polygon equal to nested overlapping polygons - 11",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 286,
    "GC:AmP/A - polygon with overlapping points equal to polygon - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 290,
    "GC:AmP/A - polygon with overlapping points equal to polygon - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 300,
    "GC:AmP/A - polygon with overlapping points equal to polygon - 15",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 307,
    "GC:AL/A - polygon with line in boundary and interior equal to polygon - 1",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 311,
    "GC:AL/A - polygon with line in boundary and interior equal to polygon - 5",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "misc/TestRelateGC.xml", 321,
    "GC:AL/A - polygon with line in boundary and interior equal to polygon - 15",
    "passes under GeometryRelate.NG and fails under the GeometryRelate.Legacy the runner installs",
    "#903")]

// ---- 26 cases: the overlay engine decides them ----------------------------------------------
//
// These pass under GeometryOverlay.NG and fail under the GeometryOverlay.Legacy the runner
// installs. They are overlayAreaTest and unionLength cases, which reach the engine through the
// geometry's own methods, so they are in effect an assertion about whichever engine is installed.

[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-geos-1051.xml", 0,
    "https://trac.osgeo.org/geos/ticket/1051",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-geos-350.xml", 0,
    "http://trac.osgeo.org/geos/ticket/350 Original data fails at noding after snappi",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-geos-350.xml", 1,
    "http://trac.osgeo.org/geos/ticket/350 Original data fails at noding after snappi",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-geos-350.xml", 2,
    "http://trac.osgeo.org/geos/ticket/350 Pre-snapped version of the data.",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-geos-368.xml", 0,
    "Intersection of two valid polygons produces an error with classic overlay See ht",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-geos-488.xml", 0,
    "This used to be a TopologyException unioning two valid polygons. Cause was very",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-geos-522.xml", 0,
    "Overlay fails with classic overlay floating overlay http://trac.osgeo.org/geos/t",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-geos-586.xml", 0,
    "http://trac.osgeo.org/geos/ticket/586",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-isochrone.xml", 11,
    "",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-isochrone.xml", 64,
    "",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-isochrone.xml", 85,
    "",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-jts-300.xml", 0,
    "JTS #300 - union fails with simple noding.",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-jts-798.xml", 0,
    "JTS #798 : 1- intersection fails with original OverlayNG.",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-jts-798.xml", 1,
    "JTS #798 : 2 - intersection fails with original OverlayNG.",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-jts-798.xml", 2,
    "JTS #798 : 3 - intersection fails with original OverlayNG.",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-misc-4.xml", 0,
    "AA - causes failure due to snapping making input invalid JTS 1.10",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-misc-4.xml", 4,
    "Personal email - Nov 6 2012",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-2055.xml", 0,
    "http://trac.osgeo.org/postgis/ticket/2055 A triangle which has a segment crossin",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-2176.xml", 0,
    "https://trac.osgeo.org/postgis/ticket/2176 Also http://trac.osgeo.org/geos/ticke",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-4182-2.xml", 0,
    "https://trac.osgeo.org/postgis/ticket/4182 part",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-4538.xml", 0,
    "https://trac.osgeo.org/postgis/ticket/4538",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-4738.xml", 0,
    "https://trac.osgeo.org/postgis/ticket/4738",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-list.xml", 2,
    "https://lists.osgeo.org/pipermail/postgis-users/2006-November/013735.html Overla",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-list.xml", 3,
    "https://lists.osgeo.org/pipermail/postgis-users/2020-July/044303.html Overlay fa",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-list.xml", 4,
    "http://postgis.refractions.net/pipermail/postgis-users/2006-November/013743.html",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]
[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-stmlf.xml", 1863,
    "987002 - 5242358 abs areaA - differenceAB - intersectionAB = 30",
    "passes under GeometryOverlay.NG and fails under the GeometryOverlay.Legacy the runner installs",
    "#903")]

// ---- 30 cases: these fail under either implementation ---------------------------------------
//
// What is left once the two groups above are set aside, grouped by the exception each one
// actually raises.

// 22 case(s) - no exception - the operation answered, and the answer is not the one the case declares
[assembly: KnownCorpusFailure(
    "failure/TestBufferFailure.xml", 1,
    "http://lists.osgeo.org/pipermail/postgis-users/2013-January/036334.html Buffer r",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "failure/TestOverlayNGFailure.xml", 0,
    "TopologyException on union of valid geometries https://github.com/libgeos/geos/i",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "failure/TestOverlayNGFailure.xml", 1,
    "Incorrect intersection and difference results https://github.com/libgeos/geos/is",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "failure/TestOverlayNGFailure.xml", 2,
    "Incorrect result in union and intersection https://github.com/libgeos/geos/issue",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "failure/TestOverlayNGFailure.xml", 3,
    "Union produces incorrect result. https://github.com/shapely/shapely/issues/1871",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "failure/TestOverlayNGFailure.xml", 4,
    "Union and intersection produce incorrect result. https://github.com/JuliaGeo/Lib",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 20,
    "#21 Polygon - gid: 2521",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 33,
    "#34 Polygon - gid: 2534",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 38,
    "#39 Polygon - gid: 2539",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 40,
    "#41 Polygon - gid: 2541",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 41,
    "#42 Polygon - gid: 2542",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 54,
    "#55 Polygon - gid: 2555",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 76,
    "#77 Polygon - gid: 2577",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 77,
    "#78 Polygon - gid: 2578",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 81,
    "#82 Polygon - gid: 2582",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 86,
    "#87 Polygon - gid: 2587",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 91,
    "#92 Polygon - gid: 2592",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 93,
    "#94 Polygon - gid: 2594",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 94,
    "#95 Polygon - gid: 2595",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 97,
    "#98 Polygon - gid: 2598",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestBufferExternal2.xml", 98,
    "#99 Polygon - gid: 2599",
    "the result differs from the one the case declares",
    "#904")]
[assembly: KnownCorpusFailure(
    "robust/TestRobustRelateFloat.xml", 0,
    "A/P - Point is on boundary of polygon. Orientation algorithms including CGAlgori",
    "the result differs from the one the case declares",
    "#904")]

// 4 case(s) - ArgumentException: reduction failed, possible invalid input
[assembly: KnownCorpusFailure(
    "failure/TestReducePrecisionFailure.xml", 1,
    "",
    "ArgumentException: reduction failed, possible invalid input",
    "#904")]
[assembly: KnownCorpusFailure(
    "failure/TestReducePrecisionFailure.xml", 2,
    "simplified version of #2",
    "ArgumentException: reduction failed, possible invalid input",
    "#904")]
[assembly: KnownCorpusFailure(
    "failure/TestReducePrecisionFailure.xml", 3,
    "",
    "ArgumentException: reduction failed, possible invalid input",
    "#904")]
[assembly: KnownCorpusFailure(
    "failure/TestReducePrecisionFailure.xml", 4,
    "Canonical version of #2",
    "ArgumentException: reduction failed, possible invalid input",
    "#904")]

// 3 case(s) - TopologyException: found non-noded intersection
[assembly: KnownCorpusFailure(
    "misc/TestOverlay.xml", 2,
    "https://trac.osgeo.org/geos/ticket/368",
    "TopologyException: found non-noded intersection",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestOverlay.xml", 3,
    "https://trac.osgeo.org/geos/ticket/522",
    "TopologyException: found non-noded intersection",
    "#904")]
[assembly: KnownCorpusFailure(
    "misc/TestOverlay.xml", 4,
    "https://trac.osgeo.org/geos/ticket/737",
    "TopologyException: found non-noded intersection",
    "#904")]

// 1 case(s) - TopologyException: no outgoing dirEdge found
[assembly: KnownCorpusFailure(
    "misc/TestOverlay.xml", 1,
    "http://trac.osgeo.org/geos/ticket/488 TopologyException unioning two valid polyg",
    "TopologyException: no outgoing dirEdge found",
    "#904")]
