using AoC;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class PuzzleBenchmarks
{
    private readonly ILogger _logger = new BenchmarkLogger();
    private string _path = Utils.FullPath(15);
    private Day15 _puzzle;

    [GlobalSetup]
    public void Setup()
    {
        _puzzle = new Day15(_logger, _path);
        _puzzle.Setup();
    }

    [Benchmark]
    public void Part1()
    {
        //_puzzle.Setup();
        _puzzle.SolvePart1();
    }

    [Benchmark]
    public void Part2()
    {
        //_puzzle.Setup();
        _puzzle.SolvePart2();
    }
}

public class BenchmarkLogger : ILogger
{
    public string? LastMessage => string.Empty;
    public void Log(string? msg) { }
}