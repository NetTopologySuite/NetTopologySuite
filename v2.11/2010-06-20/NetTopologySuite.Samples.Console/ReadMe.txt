We have two group of tests:

Some simple tests are used by me (Diego Guidi) for C# porting and are started by root/program.cs Main entry point.
Other tests are in *Example.cs classes and are created for JTS team.
Search for methods named "main" in those classes and rename as Main the tests to start.

If the console samples don't start, assure that in the project properties, Debug tab,
Start Program is activated!