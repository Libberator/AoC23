namespace System.Numerics;

public struct Vector3Long(long x, long y, long z)
{
    public long X = x;
    public long Y = y;
    public long Z = z;

    public static readonly Vector3Long Zero = new(0, 0, 0);
    public static readonly Vector3Long Up = new(0, 1, 0);
    public static readonly Vector3Long Down = new(0, -1, 0);
    public static readonly Vector3Long Right = new(1, 0, 0);
    public static readonly Vector3Long Left = new(-1, 0, 0);
    public static readonly Vector3Long Forward = new(0, 0, 1);
    public static readonly Vector3Long Backward = new(0, 0, -1);

    public readonly bool Equals(Vector3Long other) => this == other;
    public override readonly bool Equals(object? obj) => obj is Vector3Long other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    public static Vector3Long operator +(Vector3Long left, Vector3Long right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Vector3Long operator -(Vector3Long value) => new(-value.X, -value.Y, -value.Z);
    public static Vector3Long operator -(Vector3Long left, Vector3Long right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    public static Vector3Long operator *(long left, Vector3Long right) => new(left * right.X, left * right.Y, left * right.Z);
    public static Vector3Long operator *(Vector3Long left, long right) => new(left.X * right, left.Y * right, left.Z * right);
    public static bool operator ==(Vector3Long left, Vector3Long right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;
    public static bool operator !=(Vector3Long left, Vector3Long right) => left.X != right.X || left.Y != right.Y || left.Z != right.Z;
}
