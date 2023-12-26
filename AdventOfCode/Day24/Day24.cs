using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AoC;

public class Day24(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Hailstone> _hailstones = [];

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
        long min = 200000000000000; // 7
        long max = 400000000000000; // 27

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
        var startingPos = GetStartingPosition(_hailstones);
        _logger.Log(startingPos.X + startingPos.Y + startingPos.Z);
    }

    // solving for y = mx + b for both.
    private static bool WillCollideBetween(Hailstone a, Hailstone b, long min, long max)
    {
        // this is the "m", or slope, in the equation
        var slopeA = (double)a.VelY / a.VelX;
        var slopeB = (double)b.VelY / b.VelX;

        if (slopeA == slopeB) return false; // parallel lines

        // the "plus b" in the equation
        var plusB_A = a.PosY - slopeA * a.PosX;
        var plusB_B = b.PosY - slopeB * b.PosX;

        var xIntercept = (plusB_A - plusB_B) / (slopeB - slopeA);
        if (xIntercept < min || xIntercept > max) return false; // outside range for x value

        if (a.VelX > 0 ^ xIntercept > a.PosX || b.VelX > 0 ^ xIntercept > b.PosX) return false; // in the past

        var yIntercept = xIntercept * slopeA + plusB_A;
        if (yIntercept < min || yIntercept > max) return false; // outside range for y value
        return true;
    }

    private Vector3Long GetStartingPosition(List<Hailstone> hailstones)
    {
        var startingPos = Vector3Long.Zero;
        var startingVel = Vector3Long.Zero;





        return startingPos;
    }

    private bool PathIntersects(Vector3Long pos,  Hailstone other)
    {



        return false;
    }

    private record struct Hailstone(long PosX, long PosY, long PosZ, int VelX, int VelY, int VelZ);


}



// consider only X and Y axis
// y1 = mx1 + b1
// y2 = mx2 + b2

// 19, 13, 30 @ -2, 1, -2
// 18, 19, 22 @ -1, -1, -2
// x=14.333, y=15.333

// y = -0.5 x + 22.5
// y = x + 1
// 1.5x = 21.5
// x =  (slope a - slope b)
// x = 14 1/3
// y = 15 1/3

// 19, 13, 30 @ -2, 1, -2
// 20, 25, 34 @ -2, -2, -4
// x=11.667, y=16.667).

// y = -0.5x + 22.5
// y = 1x + 5
// 1.5x = 17.5
// x = 11.667, y = 16.667