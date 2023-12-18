using AoC;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class PuzzleBenchmarks
{
    private readonly ILogger _logger;
    private readonly string _path;
    private readonly Puzzle _puzzle;

    public PuzzleBenchmarks()
    {
        _logger = new BenchmarkLogger();
        _path = Utils.FullPath(18); // Update here
        _puzzle = new Day18(_logger, _path); // and here
    }

    [GlobalSetup]
    public void GlobalSetup() => _puzzle.Setup();

    //[Benchmark]
    //public void Setup() => _puzzle.Setup(); // verify this won't mess up any _puzzle internal state

    [Benchmark]
    public void Part1() => _puzzle.SolvePart1();

    [Benchmark]
    public void Part2() => _puzzle.SolvePart2();

    [Benchmark]
    public void FullPuzzle()
    {
        var puzzle = new Day18(_logger, _path); // Update Here
        puzzle.Setup();
        puzzle.SolvePart1();
        puzzle.SolvePart2();
    }
}

public class BenchmarkLogger : ILogger
{
    public string? LastMessage => string.Empty;
    public void Log(string? msg) { }
}