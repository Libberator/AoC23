using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace AoC;

public class Day16(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char EMPTY = '.', FORWARD_SLASH = '/', BACKSLASH = '\\', VERT_SPLITTER = '|', HORIZ_SPLITTER = '-';

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
            var score = GetTotalEnergizedFrom(new Vector2Int(i, -1), Vector2Int.N); // Left side, firing East
            score = Math.Max(score, GetTotalEnergizedFrom(new Vector2Int(i, _cols), Vector2Int.S)); // West
            score = Math.Max(score, GetTotalEnergizedFrom(new Vector2Int(_rows, i), Vector2Int.W)); // North
            score = Math.Max(score, GetTotalEnergizedFrom(new Vector2Int(-1, i), Vector2Int.E)); // South

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
                    case FORWARD_SLASH:
                        beam.Dir = new Vector2Int(-beam.Dir.Y, -beam.Dir.X);
                        break;
                    case BACKSLASH:
                        beam.Dir = new Vector2Int(beam.Dir.Y, beam.Dir.X);
                        break;
                    case VERT_SPLITTER when beam.Dir.X == 0: // Moving Horizontally
                        beam.Dir = Vector2Int.W; // Split Upwards
                        beams.Add(new Beam(beam.Pos, Vector2Int.E)); // Split Downwards
                        break;
                    case HORIZ_SPLITTER when beam.Dir.Y == 0: // Moving Vertically
                        beam.Dir = Vector2Int.S; // Split Leftwards
                        beams.Add(new Beam(beam.Pos, Vector2Int.N)); // Split Rightwards
                        break;
                }
            }
        }

        return energized.Count;
    }

    private bool IsInGrid(Vector2Int pos) => pos.X >= 0 && pos.Y >= 0 && pos.X < _rows && pos.Y < _cols;

    private record struct BeamSnapshot(Vector2Int Pos, Vector2Int Dir);
    private class Beam(Vector2Int pos, Vector2Int dir)
    {
        public Vector2Int Pos = pos;
        public Vector2Int Dir = dir;
        public BeamSnapshot Snapshot() => new(Pos, Dir);
    }
}