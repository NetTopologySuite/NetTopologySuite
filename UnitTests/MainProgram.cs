//******************************
// Written by Peter Golde
// Copyright (c) 2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************

using System;
using Wintellect.PowerCollections.Tests;

[assembly: CLSCompliant(true)]

namespace Wintellect.PowerCollections.Tests
{
    class MainProgram
    {
        public static void Main()
        {
            new OrderedMultiDictionaryTests().SerializeStrings();
        }
    }
}
