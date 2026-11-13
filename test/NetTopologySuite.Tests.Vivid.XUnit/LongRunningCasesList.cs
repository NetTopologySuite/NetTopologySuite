// The corpus cases held back from the runs CI waits for.
//
// They carry the LongRunning category, the same one the rest of the suite uses, so a run without
// the CI filter still executes them. An entry is a statement about how long a case takes, not
// about whether it passes: a case listed here is expected to pass when it is run.
//
// The index is the position of the case within the collection loaded for that file, which is the
// number the test name carries. The description is stored so that an entry cannot go on pointing
// at a case that has since moved: KnownFailuresTests compares it against the case actually
// sitting at that index and fails if a file has drifted underneath the list.
//
using NetTopologySuite.Tests.XUnit;

[assembly: LongRunningCorpusCase(
    "robust/overlay/TestOverlay-geos-997-union-slow.xml", 0,
    "GEOS #997 - Snapping heuristic triggered by noding error takes a very long time",
    196,
    "overlayAreaTest runs five overlays over 4.3 MB of geometry, on an input whose noding error triggers the snapping heuristic the case was filed for")]
