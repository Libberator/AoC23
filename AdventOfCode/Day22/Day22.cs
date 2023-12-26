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
        _bricks.Sort((a, b) => a.Bounds.ZMin.CompareTo(b.Bounds.ZMin)); // sort by the bottoms of each brick

        foreach (var brick in _bricks)
        {
            var zMin = brick.Bounds.ZMin;

            for (int z = zMin - 1; z >= 1; z--)
            {
                var spaceBelow = brick.Bounds with { ZMin = z, ZMax = z }; // one small slice of 3D space

                bool keepGoingDown = true;
                foreach (var brickBelow in _bricks)
                {
                    if (!brickBelow.Bounds.Overlaps(spaceBelow)) continue; // check the tops of each other brick
                    brick.Below.Add(brickBelow); // add connections if they'll be touching
                    brickBelow.Above.Add(brick);
                    keepGoingDown = false;
                }
                if (!keepGoingDown) break;

                zMin = z;
            }
            brick.MoveDownBy(brick.Bounds.ZMin - zMin); // finally, move brick down
        }
    }

    private class Brick(Bounds3D bounds)
    {
        public Bounds3D Bounds = bounds;
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