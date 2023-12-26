using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day22(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Brick> _bricks = [];

    public override void Setup()
    {
        _bricks.Clear();
        foreach (var line in ReadFromFile())
        {
            var split = line.Split('~');
            var left = split[0].Split(',').ToIntArray();
            var right = split[1].Split(',').ToIntArray();
            var bounds = new Bounds3D(left[0], right[0], left[1], right[1], left[2], right[2]);
            _bricks.Add(new Brick(bounds));
        }
        SlideAllBricksDown(); // also applies any connections
    }

    public override void SolvePart1() => _logger.Log(_bricks.Count(b => b.CanBeDisintegrated()));

    public override void SolvePart2() => _logger.Log(_bricks.Sum(b => b.SupportingTotal()));

    private static Bounds3D BoundsWithZ(Bounds3D b, int z) => new(b.XMin, b.XMax, b.YMin, b.YMax, z, z);

    private void SlideAllBricksDown()
    {
        _bricks.Sort((a, b) => a.Bounds.ZMin.CompareTo(b.Bounds.ZMin));

        foreach (var brick in _bricks)
        {
            var zMin = brick.Bounds.ZMin;

            for (int z = zMin - 1; z >= 1; z--)
            {
                var space = BoundsWithZ(brick.Bounds, z);
                var touchingBelow = _bricks.Where(b => b.Bounds.Overlaps(space)).ToList();
                // TODO: replace Where check with a for-loop that breaks early once we get past the possible ranges of bricks that could be touching
                if (touchingBelow.Count > 0)
                {
                    foreach (var below in touchingBelow)
                    {
                        brick.Below.Add(below);
                        below.Above.Add(brick);
                    }
                    break;
                }

                zMin = z;
            }
            brick.MoveDownBy(brick.Bounds.ZMin - zMin);
        }
    }

    private class Brick(Bounds3D bounds)
    {
        public Bounds3D Bounds = bounds;
        public readonly HashSet<Brick> Above = [];
        public readonly HashSet<Brick> Below = [];

        public bool CanBeDisintegrated()
        {
            foreach (var above in Above)
            {
                if (above.Below.Count < 2)
                    return false;
            }
            return true;
        }

        public int SupportingTotal() => FallBrick(this, []);

        public void MoveDownBy(int zDistance)
        {
            Bounds.ZMin -= zDistance;
            Bounds.ZMax -= zDistance;
        }

        private int FallBrick(Brick disintegrated, HashSet<Brick> moved)
        {
            int total = 0;
            moved.Add(disintegrated);
            List<Brick> toFallNext = [];

            foreach (var above in disintegrated.Above)
            {
                if (moved.IsSupersetOf(above.Below))
                {
                    total++;
                    toFallNext.Add(above);
                }
            }

            foreach (var next in toFallNext)
                total += FallBrick(next, moved);

            return total;
        }
    }
}