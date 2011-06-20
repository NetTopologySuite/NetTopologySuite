using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Distance;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Operation.Distance;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer.Validate
{
    public class BufferDistanceValidator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
  private static Boolean Verbose = false;
	/**
	 * Maximum allowable fraction of buffer distance the 
	 * actual distance can differ by.
	 * 1% sometimes causes an error - 1.2% should be safe.
	 */
	private const double MaxDistanceDiffFrac = .012;

  private readonly IGeometry<TCoordinate> _input;
  private readonly double _bufDistance;
  private readonly IGeometry<TCoordinate> _result;
  
  private double _minValidDistance;
  private double _maxValidDistance;
  
  private double _minDistanceFound;
  private double _maxDistanceFound;
  
  private Boolean _isValid = true;
  private String _errMsg;
  private TCoordinate _errorLocation;
  
  public BufferDistanceValidator(IGeometry<TCoordinate> input, double bufDistance, IGeometry<TCoordinate> result)
  {
  	_input = input;
  	_bufDistance = bufDistance;
  	_result = result;
  }
  
  public Boolean IsValid()
  {
  	double posDistance = Math.Abs(_bufDistance);
  	double distDelta = MaxDistanceDiffFrac * posDistance;
  	_minValidDistance = posDistance - distDelta;
  	_maxValidDistance = posDistance + distDelta;
  	
  	// can't use this test if either is empty
  	if (_input.IsEmpty || _result.IsEmpty)
  		return true;
  	
  	if (_bufDistance > 0.0) {
  		checkPositiveValid();
  	}
  	else {
  		checkNegativeValid();
  	}
    if (Verbose) {
      Console.WriteLine("Min Dist= " + _minDistanceFound + "  err= " 
        + (1.0 - _minDistanceFound / _bufDistance) 
        + "  Max Dist= " + _maxDistanceFound + "  err= " 
        + (_maxDistanceFound / _bufDistance - 1.0)
        );
    }
  	return _isValid;
  }
  
  public String ErrorMessage
  { 
  	get {return _errMsg;}
  }
  
  public TCoordinate ErrorLocation
  {
      get { return _errorLocation; }
  }
  
  private void checkPositiveValid()
  {
  	IGeometry<TCoordinate> bufCurve = _result.Boundary;
  	CheckMinimumDistance(_input, bufCurve, _minValidDistance);
  	if (! _isValid) return;
  	
  	CheckMaximumDistance(_input, bufCurve, _maxValidDistance);
  }
  
  private void checkNegativeValid()
  {
  	// Assert: only polygonal inputs can be checked for negative buffers
  	
  	// MD - could generalize this to handle GCs too
  	if (! (_input is IPolygon<TCoordinate> 
  			|| _input is IMultiPolygon<TCoordinate>
  			|| _input is IGeometryCollection<TCoordinate>
  			)) {
  		return;
  	}
  	IGeometry<TCoordinate> inputCurve = getPolygonLines(_input);
  	CheckMinimumDistance(inputCurve, _result, _minValidDistance);
  	if (! _isValid) return;
  	
  	CheckMaximumDistance(inputCurve, _result, _maxValidDistance);
  }
  
  private IGeometry<TCoordinate> getPolygonLines(IGeometry<TCoordinate> g)
  {
  	List<ILineString<TCoordinate>> lines = new List<ILineString<TCoordinate>>(
        GeometryFilter.Extract<ILineString<TCoordinate>, TCoordinate>(g));

  	/*
      LinearComponentExtracter<TCoordinate> lineExtracter = new LinearComponentExtracter<TCoordinate>(lines);
  	List<IPolygon<TCoordinate>> polys = PolygonExtracter.getPolygons(g);
  	for (Iterator i = polys.iterator(); i.hasNext(); ) {
  		IPolygon<TCoordinate> poly = (IPolygon<TCoordinate>) i.next();
  		poly.apply(lineExtracter);
  	}
     */ 
  	return g.Factory.BuildGeometry((IEnumerable<IGeometry<TCoordinate>>)lines);
  }
  
  /**
   * Checks that two geometries are at least a minumum distance apart.
   * 
   * @param g1 a geometry
   * @param g2 a geometry
   * @param minDist the minimum distance the geometries should be separated by
   */
  private void CheckMinimumDistance(IGeometry<TCoordinate> g1, IGeometry<TCoordinate> g2, double minDist)
  {
  	DistanceOp<TCoordinate> distOp = new DistanceOp<TCoordinate>(g1, g2, minDist);
  	_minDistanceFound = distOp.Distance;
    
    
  	if (_minDistanceFound < minDist) 
    {
  		_isValid = false;
  		Pair<TCoordinate>? pts = distOp.ClosestPoints();
        if (pts.HasValue)
        {
            _errorLocation = pts.Value.Second;
            _errMsg = "Distance between buffer curve and input is too small "
                     + "(" + _minDistanceFound
                     + " at " + g1.Factory.CreateLineString(new TCoordinate[] { pts.Value.First, pts.Value.Second }) + " )";
        }
    }
  }
  
  /**
   * Checks that the furthest distance from the buffer curve to the _input
   * is less than the given maximum distance.
   * This uses the Oriented Hausdorff distance metric.
   * It corresponds to finding
   * the point on the buffer curve which is furthest from <i>some</i> point on the _input.
   * 
   * @param _input a geometry
   * @param bufCurve a geometry
   * @param maxDist the maximum distance that a buffer _result can be from the _input
   */
  private void CheckMaximumDistance(IGeometry<TCoordinate> input, IGeometry<TCoordinate> bufCurve, double maxDist)
  {
//    BufferCurveMaximumDistanceFinder maxDistFinder = new BufferCurveMaximumDistanceFinder(_input);
//    _maxDistanceFound = maxDistFinder.findDistance(bufCurve);
    
    DiscreteHausdorffDistance<TCoordinate> haus = new DiscreteHausdorffDistance<TCoordinate>(bufCurve, input);
    haus.DensifyFraction = 0.25;
    _maxDistanceFound = haus.OrientedDistance();
    
    if (_maxDistanceFound > maxDist)
    {
      _isValid = false;
      Pair<TCoordinate> pts = haus.Coordinates;
      _errorLocation = pts[1];
      _errMsg = "Distance between buffer curve and _input is too large "
        + "(" + _maxDistanceFound
        + " at " + input.Factory.CreateLineString( new TCoordinate[] {pts[0], pts[1]} ) +")";
    }
  }
  
  /*
  private void OLDcheckMaximumDistance(Geometry _input, Geometry bufCurve, double maxDist)
  {
    BufferCurveMaximumDistanceFinder maxDistFinder = new BufferCurveMaximumDistanceFinder(_input);
    _maxDistanceFound = maxDistFinder.findDistance(bufCurve);
    
    
    if (_maxDistanceFound > maxDist) {
      isValid = false;
      PointPairDistance ptPairDist = maxDistFinder.getDistancePoints();
      _errorLocation = ptPairDist.getCoordinate(1);
      _errMsg = "Distance between buffer curve and _input is too large "
        + "(" + ptPairDist.getDistance()
        + " at " + ptPairDist.toString() +")";
    }
  }
  */
        
    }
}