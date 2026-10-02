// The corpus cases that fail today.
//
// They are excluded from CI through the FailureCase category, the same one the rest of the suite
// uses, so that the test story can land ahead of the investigation into why each of them fails.
// An entry is not a permanent exception: each should be traced back to either a defect in the
// library or a case that never applied to this port, and removed once that is settled.
//
// The index is the position of the case within the collection loaded for that file, which is the
// number the test name carries. The description is stored so that an entry cannot go on pointing
// at a case that has since moved: KnownFailuresTests compares it against the case actually sitting
// at that index and fails if a file has drifted underneath the list.
//
using NetTopologySuite.Tests.XUnit;

[assembly: KnownCorpusFailure(
    "robust/overlay/TestOverlay-pg-list.xml", 1,
    "http://postgis.refractions.net/pipermail/postgis-users/2006-March/011332.html 2",
    "the result is correct within the declared tolerance, but the runner ignores tolerance unless the expected value is a GeometryCollection",
    "#893")]
