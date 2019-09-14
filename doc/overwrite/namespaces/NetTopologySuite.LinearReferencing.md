---
uid: NetTopologySuite.LinearReferencing
summary: *content
---
Contains classes and interfaces implementing linear referencing on linear geometries

## Linear Referencing
Linear Referencing is a way of defining positions along linear geometries (LineStrings and MultiLineStrings). It is used extensively in linear network systems. There are numerous possible Linear Referencing Methods which can be used to define positions along linear geometry. This package supports two:

## Linear Location
A linear location is a triple (component index, segment index, segment fraction) which precisely specifies a point on a linear geometry. It allows for efficient mapping of the index value to actual coordinate values.

## Length
The natural concept of using the length along the geometry to specify a position.

# Package Specification
- Java Topology Suite Technical Specifications
