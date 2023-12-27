using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day25(ILogger logger, string path) : Puzzle(logger, path)
{
    private readonly List<string> _nodes = [];
    private int[,] _adjacencyMatrix = new int[0, 0];

    public override void Setup()
    {
        Dictionary<string, string[]> connections = [];

        foreach (string line in ReadFromFile())
        {
            var split = line.Split(": ");
            string a = split[0];
            string[] bNodes = split[1].Split(' ');

            connections[a] = bNodes;

            if (!_nodes.Contains(a))
                _nodes.Add(a);

            _nodes.AddRange(bNodes.Where(b => !_nodes.Contains(b)));
        }

        // adjacencyMatrix[i,j] == 1 iff nodes i and j are connected (symmetric)
        _adjacencyMatrix = new int[_nodes.Count, _nodes.Count];
        foreach (var connection in connections)
        {
            int row = _nodes.IndexOf(connection.Key);
            foreach (var b in connection.Value)
            {
                int col = _nodes.IndexOf(b);
                _adjacencyMatrix[col, row] = 1;
                _adjacencyMatrix[row, col] = 1;
            }
        }
    }

    public override void SolvePart1()
    {
        var product = 0;
        for (int i = 1; i < _nodes.Count; i++)
        {
            (var flow, product) = MaxFlow(0, i);
            if (flow <= 3)
                break;
        }
        _logger.Log(product);
    }

    public override void SolvePart2() { } // Freebie

    private (int Flow, int Product) MaxFlow(int start, int end)
    {
        int[,] flow = new int[_nodes.Count, _nodes.Count];
        int flowVal = 0;
        while (true)
        {
            var componentSize = FindPath(flow, start, end);
            if (componentSize == 0) // augmenting path found
                flowVal++;
            else  // no more augmenting paths can be found
                return (flowVal, componentSize * (_nodes.Count - componentSize));
        }
    }

    private int FindPath(int[,] flow, int start, int end)
    {
        int[] from = Enumerable.Repeat(-1, _nodes.Count).ToArray(); // Will store the in-neighbors, for reconstructing the augmenting path
        from[start] = 0;

        int steps = 0;
        Stack<int> toSearch = new([start]);

        while (toSearch.Count > 0 && from[end] == -1)
        {
            int curr = toSearch.Pop();
            steps++;
            for (int i = 0; i < _nodes.Count; i++)
            {
                // Key insight: if we can *decrease* flow going in opposite direction, that's also okay:
                if (_adjacencyMatrix[curr, i] - flow[curr, i] > 0 && from[i] == -1)
                {
                    from[i] = curr;
                    toSearch.Push(i);
                }
            }
        }

        if (from[end] >= 0)
        {
            // Path found - increase flow along the path from start to end
            int i = end;
            while (i != start)
            {
                flow[from[i], i]++;
                flow[i, from[i]]--; // the flow matrix is also symmetric!
                i = from[i];
            }
            return 0;
        }
        return steps;
    }
}