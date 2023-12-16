using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace AoC;

public class Day16(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char EMPTY = '.', BACKSLASH = '\\', FORWARD_SLASH = '/', VERT_SPLITTER = '|', HORIZ_SPLITTER = '-';

    private string[] _grid = [];
    private int _rows, _cols;

    public override void Setup()
    {
        _grid = ReadAllLines();
        _rows = _grid.Length;
        _cols = _grid[0].Length;
    }

    public override void SolvePart1() => logger.Log(GetTotalEnergizedFrom(new(0, -1), new(0, 1)));

    public override void SolvePart2()
    {
        int highScore = 0;

        // this loop assumes a Square Grid, where _rows == _cols
        Parallel.For(0, _rows, i =>
        {
            var leftPos = new Vector2Int(i, -1);
            var score = GetTotalEnergizedFrom(leftPos, Vector2Int.N); // East

            var rightPos = new Vector2Int(i, _cols);
            score = Math.Max(score, GetTotalEnergizedFrom(rightPos, Vector2Int.S)); // West

            var botPos = new Vector2Int(_rows, i);
            score = Math.Max(score, GetTotalEnergizedFrom(botPos, Vector2Int.W)); // North

            var topPos = new Vector2Int(-1, i);
            score = Math.Max(score, GetTotalEnergizedFrom(topPos, Vector2Int.E)); // South

            if (score > highScore) Interlocked.Exchange(ref highScore, score);
        });

        _logger.Log(highScore);
    }

    private int GetTotalEnergizedFrom(Vector2Int startPos, Vector2Int startDir)
    {
        List<Beam> beams = [new(startPos, startDir)];
        HashSet<BeamSnapshot> snapshots = [];
        HashSet<Vector2Int> energized = [];

        while (beams.Count > 0)
        {
            for (int i = beams.Count - 1; i >= 0; i--)
            {
                var beam = beams[i];

                beam.Pos += beam.Dir; // Move
                var snapshot = beam.Snapshot(); // Capture pos and dir in a struct
                if (!IsInGrid(beam.Pos) || !snapshots.Add(snapshot))
                {
                    beams.Remove(beam); // left the grid or already been here before
                    continue;
                }

                energized.Add(beam.Pos);

                switch (_grid[beam.Pos.X][beam.Pos.Y])
                {
                    case EMPTY: break;
                    case VERT_SPLITTER:
                        if (ShouldSplitVertically(beam.Dir))
                        {
                            beam.Dir = Vector2Int.W; // Split Upwards
                            var newBeam = new Beam(beam.Pos, Vector2Int.E); // Split Downwards
                            beams.Add(newBeam);
                        }
                        break;
                    case HORIZ_SPLITTER:
                        if (ShouldSplitHorizontally(beam.Dir))
                        {
                            beam.Dir = Vector2Int.S; // Split Leftwards
                            var newBeam = new Beam(beam.Pos, Vector2Int.N); // Split Rightwards
                            beams.Add(newBeam);
                        }
                        break;
                    case BACKSLASH: beam.Dir = ReflectBackslash(beam.Dir); break;
                    case FORWARD_SLASH: beam.Dir = ReflectForwardSlash(beam.Dir); break;
                }
            }
        }

        return energized.Count;
    }

    private bool IsInGrid(Vector2Int pos) => pos.X >= 0 && pos.Y >= 0 && pos.X < _rows && pos.Y < _cols;
    private static bool ShouldSplitVertically(Vector2Int dir) => dir == Vector2Int.N || dir == Vector2Int.S;
    private static bool ShouldSplitHorizontally(Vector2Int dir) => dir == Vector2Int.E || dir == Vector2Int.W;

    private static Vector2Int ReflectBackslash(Vector2Int dir) // hit '\'
    {
        if (dir == Vector2Int.N) return Vector2Int.E; // east -> south
        else if (dir == Vector2Int.E) return Vector2Int.N; // south -> east
        else if (dir == Vector2Int.S) return Vector2Int.W; // west -> north
        else return Vector2Int.S; // north -> west
    }

    private static Vector2Int ReflectForwardSlash(Vector2Int dir) // hit '/'
    {
        if (dir == Vector2Int.N) return Vector2Int.W; // east -> north
        else if (dir == Vector2Int.E) return Vector2Int.S; // south -> west
        else if (dir == Vector2Int.S) return Vector2Int.E; // west -> south
        else return Vector2Int.N; // north -> east
    }

    public class Beam(Vector2Int pos, Vector2Int dir)
    {
        public Vector2Int Pos = pos;
        public Vector2Int Dir = dir;
        public BeamSnapshot Snapshot() => new(Pos, Dir);
    }

    public record struct BeamSnapshot(Vector2Int Pos, Vector2Int Dir);
}