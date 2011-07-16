// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Copyright 2007, 2008 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of Proj.Net.
// Proj.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Proj.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Proj.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Runtime.Serialization;

namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// Exception thrown when a computation fails.
    /// </summary>
    [Serializable]
    public class ComputationException : Exception
    {
        /// <summary>
        /// Creates a new instance of a <see cref="ComputationException"/>.
        /// </summary>
        public ComputationException() {}

        /// <summary>
        /// Creates a new <see cref="ComputationException"/> instance with the given 
        /// <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Information about the exception.</param>
        public ComputationException(String message) : base(message) {}

        /// <summary>
        /// Creates a new <see cref="ComputationException"/> instance with the given 
        /// <paramref name="message"/> and causal <paramref name="inner"/> <see cref="Exception"/>.
        /// </summary>
        /// <param name="message">Information about the exception.</param>
        /// <param name="inner">The <see cref="Exception"/> which caused this exception.</param>
        public ComputationException(String message, Exception inner) : base(message, inner) {}

        protected ComputationException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}