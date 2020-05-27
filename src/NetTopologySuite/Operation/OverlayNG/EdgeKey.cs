using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * A key for sorting and comparing edges in a noded arrangement.
     * Relies on the fact that in a correctly noded arrangement
     * edges are identical (up to direction) 
     * iff they have their first segment in common. 
     * 
     * @author mdavis
     *
     */
    class EdgeKey : IComparable<EdgeKey> {


    public static EdgeKey Create(Edge edge)
    {
        return new EdgeKey(edge);
    }

    private double p0x;
    private double p0y;
    private double p1x;
    private double p1y;

    private EdgeKey(Edge edge)
    {
        initPoints(edge);
    }

    private void initPoints(Edge edge)
    {
        bool direction = edge.direction();
        if (direction)
        {
            init(edge.getCoordinate(0),
                edge.getCoordinate(1));
        }
        else
        {
            int len = edge.Count;
            init(edge.getCoordinate(len - 1),
                edge.getCoordinate(len - 2));
        }
    }

    private void init(Coordinate p0, Coordinate p1)
    {
        p0x = p0.X;
        p0y = p0.Y;
        p1x = p1.X;
        p1y = p1.Y;
    }

  public int CompareTo(EdgeKey ek)
    {
        if (p0x < ek.p0x) return -1;
        if (p0x > ek.p0x) return 1;
        if (p0y < ek.p0y) return -1;
        if (p0y > ek.p0y) return 1;
        // first points are equal, compare second
        if (p1x < ek.p1x) return -1;
        if (p1x > ek.p1x) return 1;
        if (p1y < ek.p1y) return -1;
        if (p1y > ek.p1y) return 1;
        return 0;
    }

    public override bool Equals(object o)
    {
        if (!(o is EdgeKey)) {
            return false;
        }
        var ek = (EdgeKey)o;
        return p0x == ek.p0x
            && p0y == ek.p0y
            && p1x == ek.p1x
            && p1y == ek.p1y;
    }

    /**
     * Gets a hashcode for this object.
     * 
     * @return a hashcode for this object
     */
    public override int GetHashCode()
    {
        //Algorithm from Effective Java by Joshua Bloch
        int result = 17;
        result = 37 * result + GetHashCode(p0x);
        result = 37 * result + GetHashCode(p0y);
        result = 37 * result + GetHashCode(p1x);
        result = 37 * result + GetHashCode(p1y);
        return result;
    }

    /**
     * Computes a hash code for a double value, using the algorithm from
     * Joshua Bloch's book <i>Effective Java"</i>
     * 
     * @param x the value to compute for
     * @return a hashcode for x
     */
    public static int GetHashCode(double x)
    {
        long f = BitConverter.DoubleToInt64Bits(x);
        return (int)(f ^ (f >> 32));
    }

    public override string ToString()
    {
        return "EdgeKey(" + Format(p0x, p0y)
          + ", " + Format(p1x, p1y) + ")";
    }

    private static string Format(double x, double y)
    {
        return OrdinateFormat.Default.Format(x) + " " + OrdinateFormat.Default.Format(y);
    }
}
}
