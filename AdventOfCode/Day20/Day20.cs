using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day20(ILogger logger, string path) : Puzzle(logger, path)
{
    private const string START = "broadcaster", END = "rx";
    private const char FLIP_FLOP = '%', CONJUNCTION = '&';

    private readonly Dictionary<string, Module> _modules = [];
    private string[] _gateKeepers = [];

    public override void Setup()
    {
        _modules.Clear();
        foreach (var line in ReadFromFile())
        {
            var split = line.Split(" -> ");
            var source = split[0];
            var id = source[1..];
            var dest = split[1].Split(',', StringSplitOptions.TrimEntries);
            if (source[0] == FLIP_FLOP)
                _modules.Add(id, new FlipFlop(id, dest));
            else if (source[0] == CONJUNCTION)
                _modules.Add(id, new Conjunction(id, dest));
            else
                _modules.Add(START, new Broadcaster(START, dest));
        }

        foreach (var module in _modules.Values)
        {
            var inputs = _modules
                .Where(kvp => kvp.Value.Outputs.Contains(module.Id))
                .Select(kvp => kvp.Key).ToArray();
            module.AddInputConnections(inputs);
        }

        _gateKeepers = _modules.Values.First(mod => mod.Outputs.Contains(END)).Inputs;
    }

    public override void SolvePart1()
    {
        Setup(); // Reset State for benchmarks

        for (int i = 0; i < 1000; i++)
            PressButton();

        long lowPulseCount = 1000 + _modules.Values.Sum(m => m.LowPulsesSent);
        long highPulseCount = _modules.Values.Sum(m => m.HighPulsesSent);
        _logger.Log(lowPulseCount * highPulseCount);
    }

    public override void SolvePart2()
    {
        Setup(); // Reset State for benchmarks
        long[] periods = new long[_gateKeepers.Length];
        int i = 0;

        long buttonPresses = 0;
        while (periods.Any(p => p == 0))
        {
            buttonPresses++;
            if (PressButton())
                periods[i++] = buttonPresses;
        }

        _logger.Log(periods.Product()); // Least Common Multiple
    }

    private bool PressButton()
    {
        bool hitNewEndState = false;
        Queue<string> processing = new();
        processing.Enqueue(START);

        while (processing.Count > 0)
        {
            var source = processing.Dequeue();
            var module = _modules[source];
            var outputState = module.State;

            if (outputState)
            {
                module.HighPulsesSent += module.Outputs.Length;

                if (_gateKeepers.Contains(source))
                    hitNewEndState = true;
            }
            else
                module.LowPulsesSent += module.Outputs.Length;

            foreach (var nextID in module.Outputs)
            {
                if (!_modules.TryGetValue(nextID, out var nextModule))
                    continue;

                nextModule.ProcessPulse(source, outputState);
                if (!outputState || nextModule is Conjunction)
                    processing.Enqueue(nextID);
            }
        }
        return hitNewEndState;
    }

    private class Broadcaster(string id, string[] outputs) : Module(id, outputs) { }

    private class FlipFlop(string id, string[] outputs) : Module(id, outputs)
    {
        public override void ProcessPulse(string source, bool pulse)
        {
            if (!pulse) State = !State; // toggle on low pulses
        }
    }

    private class Conjunction(string id, string[] outputs) : Module(id, outputs)
    {
        public override bool State => !_inputStates.All(p => p);
        private bool[] _inputStates = []; // defaults to all false
        public override void AddInputConnections(string[] inputs)
        {
            base.AddInputConnections(inputs);
            _inputStates = new bool[inputs.Length];
        }
        public override void ProcessPulse(string source, bool pulse) =>
            _inputStates[Array.IndexOf(Inputs, source)] = pulse;
    }

    private abstract class Module(string id, string[] outputs)
    {
        public string Id { get; } = id;
        public string[] Outputs { get; } = outputs;
        public string[] Inputs { get; protected set; } = [];
        public long HighPulsesSent { get; set; }
        public long LowPulsesSent { get; set; }
        public virtual bool State { get; protected set; }
        public virtual void AddInputConnections(string[] inputs) => Inputs = inputs;
        public virtual void ProcessPulse(string source, bool pulse) { }
    }
}