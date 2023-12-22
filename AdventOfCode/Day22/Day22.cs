using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day22(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<Bounds3D> _bricks = [];

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            var split = line.Split('~');
            var left = split[0].Split(',').ToIntArray();
            var right = split[1].Split(',').ToIntArray();
            _bricks.Add(new Bounds3D(left[0], right[0], left[1], right[1], left[2], right[2]));
        }
        SlideAllBricksDown();
    }

    public override void SolvePart1() => _logger.Log(_bricks.Count(CanBeDisintegrated));

    public override void SolvePart2() => _logger.Log(_bricks.Sum(b => DisintegrateBrick(b, [])));

    private void SlideAllBricksDown()
    {
        _bricks.Sort((a, b) => a.ZMin.CompareTo(b.ZMin));

        for (int i = 0; i < _bricks.Count; i++)
        {
            var brick = _bricks[i];
            var zMin = brick.ZMin;
            // check below

            for (int z = zMin - 1; z >= 1; z--)
            {
                if (BricksOverlappingSpace(brick, z) > 0)
                {
                    break;
                }
                zMin = z; // can shift it down
            }
            if (zMin != brick.ZMin)
            {
                int movedDown = brick.ZMin - zMin;
                var newBrick = new Bounds3D(brick.XMin, brick.XMax, brick.YMin, brick.YMax, brick.ZMin - movedDown, brick.ZMax - movedDown);
                _bricks[i] = newBrick;
            }
        }
    }

    private bool CanBeDisintegrated(Bounds3D brick)
    {
        var spotAbove = new Bounds3D(brick.XMin, brick.XMax, brick.YMin, brick.YMax, brick.ZMax + 1, brick.ZMax + 1);

        foreach (var b in _bricks.Where(spotAbove.Overlaps))
        {
            if (BricksOverlappingSpace(b, b.ZMin - 1) < 2)
                return false;
        }

        return true;
    }

    private int BricksOverlappingSpace(Bounds3D brick, int z)
    {
        var space = BoundsWithZ(brick, z);
        return _bricks.Count(space.Overlaps);
    }

    private static Bounds3D BoundsWithZ(Bounds3D b, int z) => new(b.XMin, b.XMax, b.YMin, b.YMax, z, z);

    // TODO: start from top, give each brick a number of bricks that would move if it got disintegrated

    // breadth-first exploration of disintegrating and moving bricks
    private long DisintegrateBrick(Bounds3D brick, HashSet<Bounds3D> moved)
    {
        long total = 0;
        moved.Add(brick);
        List<Bounds3D> toMoveNext = [];

        var spaceAbove = BoundsWithZ(brick, brick.ZMax + 1);
        foreach (var above in _bricks.Where(spaceAbove.Overlaps))
        {
            var belowBrickAbove = BoundsWithZ(above, above.ZMin - 1);
            bool canMove = true;
            foreach (var below in _bricks.Where(belowBrickAbove.Overlaps))
            {
                if (!moved.Contains(below))
                {
                    canMove = false;
                    break;
                }
            }
            // can also be safely moved down
            if (canMove)
            {
                total++;
                toMoveNext.Add(above);
            }
        }

        foreach (var next in toMoveNext)
            total += DisintegrateBrick(next, moved);

        return total;
    }
}