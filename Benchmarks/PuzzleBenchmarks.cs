using AoC;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class PuzzleBenchmarks
{
    private readonly ILogger _logger = new BenchmarkLogger();
    private readonly string _path = Utils.FullPath(20); // Update here
    private readonly Puzzle _puzzle;
    
    public PuzzleBenchmarks() => _puzzle = new Day20(_logger, _path); // and here

    [GlobalSetup]
    public void GlobalSetup() => _puzzle.Setup();

    [Benchmark]
    public void Setup() => _puzzle.Setup(); // Be sure to reset the state at start of method

    [Benchmark]
    public void Part1() => _puzzle.SolvePart1();

    [Benchmark]
    public void Part2() => _puzzle.SolvePart2();

    //[Benchmark]
    //public void FullPuzzle()
    //{
    //    //_puzzle.Setup(); // the I/O slows down results significantly
    //    _puzzle.SolvePart1();
    //    _puzzle.SolvePart2();
    //}
}

public class BenchmarkLogger : ILogger
{
    public string? LastMessage => string.Empty;
    public void Log(string? msg) { }
}