/*
PostGIS: http://svn.osgeo.org/postgis/trunk/doc/bnf-wkt.txt
*/
grammar WKT;

geom
 : geomXY
 | geomXYZ
 | geomXYM
 | geomXYZM
 ;

/* TODO: COMPOUNDCURVE, CIRCULARSTRING, CURVEPOLYGON, MULTICURVE, POLYHEDRALSURFACE, TIN, TRIANGLE */
/* Of those, only POLYHEDRALSURFACE, TIN, and TRIANGLE are OGC */
geomXY
 : geomPointXY
 | geomLineStringXY
 | geomPolygonXY
 | geomMultiPointXY
 | geomMultiLineStringXY
 | geomMultiPolygonXY
 | geomCollectionXY
 ;

geomXYZ
 : geomPointXYZ
 | geomLineStringXYZ
 | geomPolygonXYZ
 | geomMultiPointXYZ
 | geomMultiLineStringXYZ
 | geomMultiPolygonXYZ
 | geomCollectionXYZ
 ;

geomXYM
 : geomPointXYM
 | geomLineStringXYM
 | geomPolygonXYM
 | geomMultiPointXYM
 | geomMultiLineStringXYM
 | geomMultiPolygonXYM
 | geomCollectionXYM
 ;

geomXYZM
 : geomPointXYZM
 | geomLineStringXYZM
 | geomPolygonXYZM
 | geomMultiPointXYZM
 | geomMultiLineStringXYZM
 | geomMultiPolygonXYZM
 | geomCollectionXYZM
 ;

geomPointXY
 : POINT (EMPTY | coordXY)
 ;

geomPointXYZ
 : POINT Z (EMPTY | coordXYZ)
 ;

geomPointXYM
 : POINT M (EMPTY | coordXYM)
 ;

geomPointXYZM
 : POINT ZM (EMPTY | coordXYZM)
 ;

geomLineStringXY
 : LINESTRING (EMPTY | coordsXY)
 ;

geomLineStringXYZ
 : LINESTRING Z (EMPTY | coordsXYZ)
 ;

geomLineStringXYM
 : LINESTRING M (EMPTY | coordsXYM)
 ;

geomLineStringXYZM
 : LINESTRING ZM (EMPTY | coordsXYZM)
 ;

geomPolygonXY
 : POLYGON (EMPTY | polyXY)
 ;

geomPolygonXYZ
 : POLYGON Z (EMPTY | polyXYZ)
 ;

geomPolygonXYM
 : POLYGON M (EMPTY | polyXYM)
 ;

geomPolygonXYZM
 : POLYGON ZM (EMPTY | polyXYZM)
 ;

geomMultiPointXY
 : MULTIPOINT (EMPTY | '(' coordXY (',' coordXY)* ')')
 ;

geomMultiPointXYZ
 : MULTIPOINT Z (EMPTY | '(' coordXYZ (',' coordXYZ)* ')')
 ;

geomMultiPointXYM
 : MULTIPOINT M (EMPTY | '(' coordXYM (',' coordXYM)* ')')
 ;

geomMultiPointXYZM
 : MULTIPOINT ZM (EMPTY | '(' coordXYZM (',' coordXYZM)* ')')
 ;

geomMultiLineStringXY
 : MULTILINESTRING (EMPTY | '(' coordsXY (',' coordsXY)* ')')
 ;

geomMultiLineStringXYZ
 : MULTILINESTRING Z (EMPTY | '(' coordsXYZ (',' coordsXYZ)* ')')
 ;

geomMultiLineStringXYM
 : MULTILINESTRING M (EMPTY | '(' coordsXYM (',' coordsXYM)* ')')
 ;

geomMultiLineStringXYZM
 : MULTILINESTRING ZM (EMPTY | '(' coordsXYZM (',' coordsXYZM)* ')')
 ;

geomMultiPolygonXY
 : MULTIPOLYGON (EMPTY | '(' polyXY (',' polyXY)* ')')
 ;

geomMultiPolygonXYZ
 : MULTIPOLYGON Z (EMPTY | '(' polyXYZ (',' polyXYZ)* ')')
 ;

geomMultiPolygonXYM
 : MULTIPOLYGON M (EMPTY | '(' polyXYM (',' polyXYM)* ')')
 ;

geomMultiPolygonXYZM
 : MULTIPOLYGON ZM (EMPTY | '(' polyXYZM (',' polyXYZM)* ')')
 ;

geomCollectionXY
 : GEOMETRYCOLLECTION (EMPTY | '(' geomXY (',' geomXY)* ')')
 ;

geomCollectionXYZ
 : GEOMETRYCOLLECTION Z (EMPTY | '(' geomXYZ (',' geomXYZ)* ')')
 ;

geomCollectionXYM
 : GEOMETRYCOLLECTION M (EMPTY | '(' geomXYM (',' geomXYM)* ')')
 ;

geomCollectionXYZM
 : GEOMETRYCOLLECTION ZM (EMPTY | '(' geomXYZM ((',' geomXYZM)*) ')')
 ;

polyXY
 : '(' coordsXY (',' coordsXY)* ')'
 ;

polyXYZ
 : '(' coordsXYZ (',' coordsXYZ)* ')'
 ;

polyXYM
 : '(' coordsXYM (',' coordsXYM)* ')'
 ;

polyXYZM
 : '(' coordsXYZM (',' coordsXYZM)* ')'
 ;

coordsXY
 : '(' coordXY (',' coordXY)* ')'
 ;

coordsXYZ
 : '(' coordXYZ (',' coordXYZ)* ')'
 ;

coordsXYM
 : '(' coordXYM (',' coordXYM)* ')'
 ;

coordsXYZM
 : '(' coordXYZM (',' coordXYZM)* ')'
 ;

coordXY
 : x=FLOAT y=FLOAT
 ;

coordXYZ
 : x=FLOAT y=FLOAT z=FLOAT
 ;

coordXYM
 : x=FLOAT y=FLOAT m=FLOAT
 ;

coordXYZM
 : x=FLOAT y=FLOAT z=FLOAT m=FLOAT
 ;

POINT
 : [pP] [oO] [iI] [nN] [tT]
 ;

LINESTRING
 : [lL] [iI] [nN] [eE] [sS] [tT] [rR] [iI] [nN] [gG]
 ;

POLYGON
 : [pP] [oO] [lL] [yY] [gG] [oO] [nN]
 ;

MULTIPOINT
 : [mM] [uU] [lL] [tT] [iI] [pP] [oO] [iI] [nN] [tT]
 ;

MULTILINESTRING
 : [mM] [uU] [lL] [tT] [iI] [lL] [iI] [nN] [eE] [sS] [tT] [rR] [iI] [nN] [gG]
 ;

MULTIPOLYGON
 : [mM] [uU] [lL] [tT] [iI] [pP] [oO] [lL] [yY] [gG] [oO] [nN]
 ;

GEOMETRYCOLLECTION
 : [gG] [eE] [oO] [mM] [eE] [tT] [rR] [yY] [cC] [oO] [lL] [lL] [eE] [cC] [tT] [iI] [oO] [nN]
 ;

EMPTY
 : [eE] [mM] [pP] [tT] [yY]
 ;

FLOAT
 : SIGN? DIGIT* ('.' DIGIT*)? ([eE] SIGN? DIGIT+)?
 ;

Z
 : [zZ]
 ;

M
 : [mM]
 ;

ZM
 : [zZ] [mM]
 ;

fragment DIGIT
 : [0-9]
 ;

fragment SIGN
 : '+'
 | '-'
 ;

SPACES
 : [ \t\r\n]+ -> skip
 ;
