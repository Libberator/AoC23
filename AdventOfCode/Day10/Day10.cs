using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC;

public class Day10(ILogger logger, string path) : Puzzle(logger, path)
{
    private const char VERT = '|', HORIZ = '-', LL = 'L', LR = 'J', UR = '7', UL = 'F', GROUND = '.', START = 'S';

    private string[] _data = [];
    private int _rows, _cols;
    private Vector2Int _startingPos;

    private readonly List<Vector2Int> _fullPath = [];
    private readonly List<Vector2Int> _enclosedPositions = [];

    public override void Setup()
    {
        _data = ReadAllLines();
        _rows = _data.Length;
        _cols = _data[0].Length;
        for (int row = 0; row < _rows; row++)
        {
            var line = _data[row];
            var col = line.IndexOf(START);
            if (col != -1)
            {
                _startingPos = new(row, col);
                break;
            }
        }
    }

    public override void SolvePart1() => _logger.Log(TraverseFrom(_startingPos) / 2);

    public override void SolvePart2()
    {
        // Re-walk the loops in both directions, mark enclosed positions on the INSIDE of the loop
        for (int i = 0; i < _fullPath.Count; i++)
        {
            var current = _fullPath[i];
            var inFront = _fullPath[(i + 1) % _fullPath.Count];
            var behind = _fullPath[(i - 1).Mod(_fullPath.Count)];

            var right = (inFront - current).RotateRight(); // if you get StackOverflow, swap the Rotate directions xD
            var left = (behind - current).RotateLeft();

            var insideForward = current + right;
            var insideBack = current + left;

            if (!_fullPath.Contains(insideForward) && !_enclosedPositions.Contains(insideForward))
                _enclosedPositions.Add(insideForward);

            if (!_fullPath.Contains(insideBack) && !_enclosedPositions.Contains(insideBack))
                _enclosedPositions.Add(insideBack);
        }

        // There are some deeper enclosed positions - do a floodfill to get the rest
        var seeds = _enclosedPositions.ToArray();
        foreach (var seed in seeds)
            Floodfill(seed, _enclosedPositions);

        _logger.Log(_enclosedPositions.Count);
    }

    private void Floodfill(Vector2Int seed, List<Vector2Int> enclosedPositions)
    {
        foreach (var neighbor in Vector2Int.CardinalDirections)
        {
            var pos = seed + neighbor;
            if (_fullPath.Contains(pos) || enclosedPositions.Contains(pos)) continue;
            enclosedPositions.Add(pos);
            Floodfill(pos, enclosedPositions);
        }
    }

    private int TraverseFrom(Vector2Int startingPos)
    {
        var prevPos = startingPos;
        var prevDir = Vector2Int.CardinalDirections.First(dir => IsValidMove(startingPos, dir));
        var currentPos = prevPos + prevDir;
        _fullPath.Add(currentPos);

        while (currentPos != startingPos)
        {
            prevDir = currentPos - prevPos;
            prevPos = currentPos;
            currentPos = NextPos(prevDir, currentPos);
            _fullPath.Add(currentPos);
        }

        return _fullPath.Count;
    }

    private Vector2Int NextPos(Vector2Int prevDir, Vector2Int currentPos)
    {
        foreach (var dir in Vector2Int.CardinalDirections)
        {
            if (dir == -prevDir) continue; // don't turn back where you just came from

            if (IsValidMove(currentPos, dir))
                return currentPos + dir;
        }
        return currentPos;
    }

    private bool IsValidMove(Vector2Int currentPos, Vector2Int dir)
    {
        var current = _data[currentPos.X][currentPos.Y];
        var nextPos = currentPos + dir;

        if (nextPos.X < 0 || nextPos.Y < 0 || nextPos.X >= _rows || nextPos.Y >= _cols) return false;

        var next = _data[nextPos.X][nextPos.Y];

        if (next is GROUND) return false;

        if (dir == Vector2Int.N) // visually East
        {
            if (current is VERT or LR or UR) return false;
            return next is HORIZ or LR or UR or START;
        }
        else if (dir == Vector2Int.E) // visually South
        {
            if (current is HORIZ or LR or LL) return false;
            return next is VERT or LR or LL or START;
        }
        else if (dir == Vector2Int.S) // visually West
        {
            if (current is VERT or UL or LL) return false;
            return next is HORIZ or UL or LL or START;
        }
        else if (dir == Vector2Int.W) // visually North
        {
            if (current is HORIZ or UL or UR) return false;
            return next is VERT or UL or UR or START;
        }
        return false;
    }
}