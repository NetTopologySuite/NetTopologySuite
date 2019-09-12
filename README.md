If you can read this, then I merged something I shouldn't have merged.

[![Build Status](https://travis-ci.org/NetTopologySuite/NetTopologySuite.svg?branch=master)](https://travis-ci.org/NetTopologySuite/NetTopologySuite)
NetTopologySuite
================

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/NetTopologySuite/NetTopologySuite?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A .NET GIS solution that is fast and reliable for the .NET platform.
**NetTopologySuite** is a direct-port of all the functionalities offered by JTS Topology Suite: _NTS expose JTS in a '.NET way'_, as example using Properties, Indexers etc...

An excerpt from [JTS website](http://sourceforge.net/projects/jts-topo-suite) explains the capabilities of NTS too:
_"The JTS Topology Suite is an API for modelling and manipulating 2-dimensional linear geometry. It provides numerous geometric predicates and functions. JTS conforms to the Simple Features Specification for SQL published by the Open GIS Consortium."_

Enjoy using them!

## Upgrading to 2.x from 1.x
A [wiki page](https://github.com/NetTopologySuite/NetTopologySuite/wiki/Upgrading-to-2.0-from-1.x) has been started to try to document the breaking changes in 2.0 when coming from 1.x.

## Documentation
A documentation of the NetTopologySuite API based on code xml comments is accessible on [github-pages](http://nettopologysuite.github.io/NetTopologySuite/api/NetTopologySuite.html)

## Install with NuGet package manager
### Stable [![NuGet Status](http://img.shields.io/nuget/v/NetTopologySuite.svg?style=flat)](http://www.nuget.org/packages/NetTopologySuite/)
Stable releases are hosted on the default NuGet feed. You can install them using the following command on the package manager command line
```
PM> Install-Package NetTopologySuite
```

### Pre release [![MyGet PreRelease Status](http://img.shields.io/myget/nettopologysuite/vpre/NetTopologySuite.svg?style=flat)](https://www.myget.org/feed/nettopologysuite/package/nuget/NetTopologySuite)
Pre-Release versions of NetTopologySuite are hosted on MyGet. The sources for the NetTopologySuite feed are as follows:

Version | URL
--- |---
NuGet v3 | https://www.myget.org/F/nettopologysuite/api/v3/index.json
NuGet v2 | https://www.myget.org/F/nettopologysuite/api/v2

You can install the latest pre-release package using the following command on the package manager command line
```
PM> Install-Package NetTopologySuite -pre -source "<Nuget v3 or NuGet v2 source>"
```
