namespace System.Numerics;

public struct Vector2Long(long x, long y)
{
    public long X = x;
    public long Y = y;

    public static readonly Vector2Long Zero = new(0, 0);
    public static readonly Vector2Long N = new(0, 1);
    public static readonly Vector2Long E = new(1, 0);
    public static readonly Vector2Long S = new(0, -1);
    public static readonly Vector2Long W = new(-1, 0);

    public readonly bool Equals(Vector2Long other) => this == other;
    public override readonly bool Equals(object? obj) => obj is Vector2Long other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    public static Vector2Long operator +(Vector2Long left, Vector2Long right) => new(left.X + right.X, left.Y + right.Y);
    public static Vector2Long operator -(Vector2Long value) => new(-value.X, -value.Y);
    public static Vector2Long operator -(Vector2Long left, Vector2Long right) => new(left.X - right.X, left.Y - right.Y);
    public static Vector2Long operator *(long left, Vector2Long right) => new(left * right.X, left * right.Y);
    public static Vector2Long operator *(Vector2Long left, long right) => new(left.X * right, left.Y * right);
    public static bool operator ==(Vector2Long left, Vector2Long right) => left.X == right.X && left.Y == right.Y;
    public static bool operator !=(Vector2Long left, Vector2Long right) => left.X != right.X || left.Y != right.Y;
}
