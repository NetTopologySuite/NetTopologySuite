Beware: this code is in a working status, and doesn't compile yet, so solution doesn't build the project by default)

To get a valid NetTopologySuite.IO.Oracle project, you need a reference to Oracle.DataAccess.dll
You can download the oracle reference (i.e. ODAC) from:
http://www.oracle.com/technetwork/database/windows/downloads

At the time of writing, you can grab 'ODAC 11.2 Release 3 (11.2.0.2.1)' from
http://www.oracle.com/technetwork/database/windows/downloads/index-101290.html

You can install the package and then reference 'Oracle.DataAccess.dll' from the GAC, 
or take the "Xcopy Deployment" package, unzip in a folder and take 'Oracle.DataAccess.dll' inside 'odp.net20\odp.net\bin\2.x' folder