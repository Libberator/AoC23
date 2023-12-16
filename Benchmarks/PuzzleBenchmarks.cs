using AoC;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class PuzzleBenchmarks
{
    private readonly ILogger _logger = new BenchmarkLogger();
    private readonly string _path = Utils.FullPath(16);

    //[GlobalSetup]
    //public void Setup()
    //{
    //}

    [Benchmark]
    public void MyPuzzle()
    {
        var puzzle = new Day16(_logger, _path);
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