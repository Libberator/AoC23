using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AoC;

public class Day24(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Hailstone> _hailstones = [];
    private record struct Hailstone(long PosX, long PosY, long PosZ, int VelX, int VelY, int VelZ)
    {
        public readonly long this[int i] => i switch
        {
            0 => PosX,
            1 => PosY,
            2 => PosZ,
            3 => VelX,
            4 => VelY,
            5 => VelZ,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            var matches = Regex.Matches(line, @"-?\d+");
            var hailstone = new Hailstone(long.Parse(matches[0].Value), long.Parse(matches[1].Value), long.Parse(matches[2].Value),
                int.Parse(matches[3].Value), int.Parse(matches[4].Value), int.Parse(matches[5].Value));
            _hailstones.Add(hailstone);
        }
    }

    public override void SolvePart1()
    {
        int total = 0;
        long min = 200000000000000; // for test, use 7;
        long max = 400000000000000; // for test, use 27;

        for (int i = 0; i < _hailstones.Count - 1; i++)
        {
            var a = _hailstones[i];
            for (int j = i + 1; j < _hailstones.Count; j++)
            {
                var b = _hailstones[j];
                if (WillCollideBetween(a, b, min, max))
                    total++;
            }
        }

        _logger.Log(total);
    }

    public override void SolvePart2()
    {
        var (Pos, Vel) = GetStartingCondition();
        _logger.Log(Pos.X + Pos.Y + Pos.Z);
    }

    private static bool WillCollideBetween(Hailstone a, Hailstone b, long min, long max)
    {
        long determinant = (a.VelY * b.VelX) - (a.VelX * b.VelY);
        if (determinant == 0) return false; // parallel lines

        var quotient = b.VelX * (b.PosY - a.PosY) - b.VelY * (b.PosX - a.PosX);
        var t = quotient / determinant;
        if (t < 0) return false; // in the past

        var xIntercept = a.PosX + a.VelX * t;
        if (xIntercept < min || xIntercept > max) return false; // outside window

        if (a.VelX > 0 != xIntercept > a.PosX || b.VelX > 0 != xIntercept > b.PosX) return false; // more reliable past check

        var yIntercept = a.PosY + a.VelY * t;
        return yIntercept >= min && yIntercept <= max; // true if inside window
    }

    private (Vector3Long Pos, Vector3Int Vel) GetStartingCondition()
    {
        var range = 500;

        // Brute force rock velocities in the XY plane
        foreach (var x in AlternatingRange(range))
        {
            foreach (var y in AlternatingRange(range))
            {
                Vector2Int velOffset = new(x, y);
                // Grab the intersection of the first few hailstone trajectories with their velocities modified by offset
                if (!TryIntersect(_hailstones[1], _hailstones[0], velOffset, out var pos1, out var t1))
                    continue;

                if (!TryIntersect(_hailstones[2], _hailstones[0], velOffset, out var pos2, out var t2))
                    continue;

                if (pos1 != pos2) continue;

                // Calculate the z interception
                var z1_before = _hailstones[1].PosZ + t1 * _hailstones[1].VelZ;
                var z2_before = _hailstones[2].PosZ + t2 * _hailstones[2].VelZ;

                var rockVelZ = (z2_before - z1_before) / (t1 - t2);

                var z1 = z1_before + rockVelZ * t1;
                var z2 = z2_before + rockVelZ * t2;

                if (z1 == z2)
                    return (new(pos1.X, pos1.Y, z1), new(x, y, (int)rockVelZ));
            }
        }

        return (Vector3Long.Zero, Vector3Int.Zero);
    }

    private static bool TryIntersect(Hailstone a, Hailstone b, Vector2Int offset, out Vector2Long pos, out long t)
    {
        pos = Vector2Long.Zero;
        t = 0;

        // adjust velocities with offset
        var (aVelX, aVelY) = (a.VelX + offset.X, a.VelY + offset.Y);
        var (bVelX, bVelY) = (b.VelX + offset.X, b.VelY + offset.Y);

        long determinant = (aVelY * bVelX) - (aVelX * bVelY);

        if (determinant == 0) return false;

        var quotient = bVelX * (b.PosY - a.PosY) - bVelY * (b.PosX - a.PosX);

        t = quotient / determinant;
        pos = new Vector2Long(a.PosX + (t * aVelX), a.PosY + (t * aVelY));

        return true;
    }

    // 0, -1, 1, -2, 2, -3, 3...
    private static IEnumerable<int> AlternatingRange(int max)
    {
        var i = 0;
        yield return i;
        while (i < max)
        {
            if (i >= 0)
                i++;
            i *= -1;
            yield return i;
        }
    }
}