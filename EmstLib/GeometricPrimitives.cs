using System.Numerics;

namespace EmstLib;
public class Point : IEquatable<Point>
{
    public double X { get; }
    public double Y { get; }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

     public Vector2 ToVector2() => new((float)X, (float)Y);

    public double DistanceSquaredTo(Point other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return dx * dx + dy * dy;
    }

    public bool Equals(Point other) => X == other.X && Y == other.Y;
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"{X}-{Y}";
}

public class Edge : IComparable<Edge>
{
    public Point A { get; }
    public Point B { get; }
    public double LengthSquared { get; }
    
    public Edge(Point a, Point b)
    {
        A = a;
        B = b;
        LengthSquared = a.DistanceSquaredTo(b);
    }
    
    public int CompareTo(Edge other) => LengthSquared.CompareTo(other.LengthSquared);
    public override bool Equals(object obj) => obj is Edge e && 
        ((e.A.Equals(A) && e.B.Equals(B)) || (e.A.Equals(B) && e.B.Equals(A)));
    public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode();
}