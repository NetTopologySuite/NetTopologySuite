#!/usr/bin/env bash

#exit if any command fails
set -e

# builds only the netstandard projects.
dotnet restore NetTopologySuite.sln
dotnet build ./NetTopologySuite/NetTopologySuite.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.Lab/NetTopologySuite.Lab.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.IO/NetTopologySuite.IO/NetTopologySuite.IO.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.IO/NetTopologySuite.IO.GDB/NetTopologySuite.IO.GDB.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.IO/NetTopologySuite.IO.GeoJSON/NetTopologySuite.IO.GeoJSON.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.IO/NetTopologySuite.IO.MsSqlSpatial/NetTopologySuite.IO.MsSqlSpatial.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.IO/NetTopologySuite.IO.PostGis/NetTopologySuite.IO.PostGis.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.IO/NetTopologySuite.IO.ShapeFile/NetTopologySuite.IO.ShapeFile.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.IO/NetTopologySuite.IO.SpatiaLite/NetTopologySuite.IO.SpatiaLite.csproj -f netstandard1.0 -c release
dotnet build ./NetTopologySuite.IO/NetTopologySuite.IO.TopoJSON/NetTopologySuite.IO.TopoJSON.csproj -f netstandard1.0 -c release

# TODO: replace this with the Mono 5.0 mbuild.
#
# Build the project
#
# xbuild /p:Configuration=Release /t:"Build" NetTopologySuite.sln /v:minimal
#
# Run unit tests
#
# mono .testRunner/NUnit.ConsoleRunner.3.6.0/tools/nunit3-console.exe ./Release/v4.5/AnyCPU/GeoAPI.Tests.dll
