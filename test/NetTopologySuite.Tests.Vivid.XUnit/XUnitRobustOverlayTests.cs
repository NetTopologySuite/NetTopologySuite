using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.XUnit
{
    class TestRobustOverlayTests : GeneralXUnitRunner
    {
        protected override string TestLocation => $"robust{System.IO.Path.DirectorySeparatorChar}overlay";


    }
}
