using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC;

public class Day16(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char EMPTY = '.', LEFT_MIRROR = '\\', RIGHT_MIRROR = '/', VERT_SPLITTER = '|', HORIZ_SPLITTER = '-';

    private readonly Vector2Int _beamStartPos = Vector2Int.S; // left of top-left corner
    private readonly Vector2Int _beamStartDir = Vector2Int.N; // actually east

    private string[] _data = [];

    public override void Setup()
    {
        _data = ReadAllLines();
    }

    public override void SolvePart1()
    {
        int total = GetEnergizedCountStartingFrom(_beamStartPos, _beamStartDir);
        _logger.Log(total);
    }

    public override void SolvePart2()
    {
        int total = 0;

        for (int row = 0; row < _data.Length; row++)
        {
            // East
            var leftPos = new Vector2Int(row, -1);
            total = Math.Max(total, GetEnergizedCountStartingFrom(leftPos, Vector2Int.N));

            // West
            var rightPos = new Vector2Int(row, _data[0].Length);
            total = Math.Max(total, GetEnergizedCountStartingFrom(rightPos, Vector2Int.S));
        }

        for (int col = 0; col < _data[0].Length; col++)
        {
            // North
            var botPos = new Vector2Int(_data.Length, col);
            total = Math.Max(total, GetEnergizedCountStartingFrom(botPos, Vector2Int.W));

            // South
            var topPos = new Vector2Int(-1, col);
            total = Math.Max(total, GetEnergizedCountStartingFrom(topPos, Vector2Int.E));
        }

        _logger.Log(total);
    }

    private int GetEnergizedCountStartingFrom(Vector2Int startPos, Vector2Int startDir)
    {
        List<Beam> beamList = [];
        var startingBeam = new Beam(startPos, startDir);
        beamList.Add(startingBeam);

        HashSet<Visited> visited = [];

        while (beamList.Count > 0)
        {
            // cycle through the beams
            for (int i = beamList.Count - 1; i >= 0; i--)
            {
                var beam = beamList[i];
                var nextPos = beam.Pos + beam.Dir; // we moved here

                if (nextPos.X < 0 || nextPos.Y < 0 || nextPos.X >= _data.Length || nextPos.Y >= _data[0].Length)
                {
                    beamList.Remove(beam);
                    continue;
                }
                var visit = new Visited(nextPos, beam.Dir);

                if (!visited.Add(visit))
                {
                    beamList.Remove(beam); // been here before. avoid endless loops
                    continue;
                }

                beam.Pos = nextPos;

                var c = _data[nextPos.X][nextPos.Y];
                switch (c)
                {
                    case LEFT_MIRROR:
                        beam.Dir = HitLeftMirror(beam.Dir);
                        break;
                    case RIGHT_MIRROR:
                        beam.Dir = HitRightMirror(beam.Dir);
                        break;
                    case VERT_SPLITTER:
                        if (HitVertSplitter(beam.Dir, out var up, out var down))
                        {
                            beam.Dir = up;
                            var newBeam = new Beam(nextPos, down);
                            beamList.Add(newBeam);
                        }
                        break;
                    case HORIZ_SPLITTER:
                        if (HitHorizSplitter(beam.Dir, out var left, out var right))
                        {
                            beam.Dir = left;
                            var newBeam = new Beam(nextPos, right);
                            beamList.Add(newBeam);
                        }
                        break;
                }
            }
        }

        return visited.Select(v => v.Pos).ToHashSet().Count;
    }

    public class Beam(Vector2Int pos, Vector2Int dir)
    {
        public Vector2Int Pos = pos;
        public Vector2Int Dir = dir;
    }

    public record struct Visited(Vector2Int Pos, Vector2Int Dir);

    private static Vector2Int HitLeftMirror(Vector2Int dir)
    {
        if (dir == Vector2Int.N) // east -> south
            return Vector2Int.E;
        else if (dir == Vector2Int.E) // south -> east
            return Vector2Int.N;
        else if (dir == Vector2Int.S) // west -> north
            return Vector2Int.W;
        else // north -> west
            return Vector2Int.S;
    }

    private static Vector2Int HitRightMirror(Vector2Int dir)
    {
        if (dir == Vector2Int.N) // east -> north
            return Vector2Int.W;
        else if (dir == Vector2Int.E) // south -> west
            return Vector2Int.S;
        else if (dir == Vector2Int.S) // west -> south
            return Vector2Int.E;
        else // north -> east
            return Vector2Int.N;
    }

    private static bool HitVertSplitter(Vector2Int dir, out Vector2Int up, out Vector2Int down)
    {
        up = Vector2Int.W;
        down = Vector2Int.E;

        return dir != up && dir != down;
    }

    private static bool HitHorizSplitter(Vector2Int dir, out Vector2Int left, out Vector2Int right)
    {
        left = Vector2Int.S;
        right = Vector2Int.N;

        return dir != left && dir != right; // only split if we were going up or down
    }
}