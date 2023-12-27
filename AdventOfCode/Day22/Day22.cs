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
        SlideBricksDown(); // also applies any connections
    }

    public override void SolvePart1() => _logger.Log(_bricks.Count(b => b.CanBeDisintegrated()));

    public override void SolvePart2() => _logger.Log(_bricks.Sum(b => b.SupportingTotal()));

    private void SlideBricksDown()
    {
        _bricks.Sort((a, b) => a.Bottom.CompareTo(b.Bottom)); // sort by the bottoms of each brick

        for (int i = 0; i < _bricks.Count; i++)
        {
            var brick = _bricks[i];
            var zMin = brick.Bottom;

            for (int z = zMin - 1; z >= 1; z--)
            {
                bool keepGoingDown = true;
                for (int j = 0; j < i; j++)
                {
                    var brickBelow = _bricks[j];
                    if (brickBelow.Top != z) continue;
                    if (!OverlapsXY(brickBelow, brick)) continue;
                    brick.Below.Add(brickBelow); // add connections if they'll be touching
                    brickBelow.Above.Add(brick);
                    keepGoingDown = false;
                }
                if (!keepGoingDown) break;
                zMin = z;
            }

            brick.MoveDownBy(brick.Bottom - zMin); // finally, move brick down
        }

        static bool OverlapsXY(Brick a, Brick b) =>
            a.Bounds.XMin <= b.Bounds.XMax && b.Bounds.XMin <= a.Bounds.XMax
            && a.Bounds.YMin <= b.Bounds.YMax && b.Bounds.YMin <= a.Bounds.YMax;
    }

    private class Brick(Bounds3D bounds)
    {
        public Bounds3D Bounds = bounds;
        public int Top => Bounds.ZMax;
        public int Bottom => Bounds.ZMin;

        public readonly HashSet<Brick> Above = [];
        public readonly HashSet<Brick> Below = [];

        public bool CanBeDisintegrated() => Above.All(a => a.Below.Count > 1);

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