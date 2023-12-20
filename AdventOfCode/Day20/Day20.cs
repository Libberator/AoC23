using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day20(ILogger logger, string path) : Puzzle(logger, path)
{
    private const string START = "broadcaster", END = "rx";
    private const char FLIP_FLOP = '%', CONJUNCTION = '&';

    private readonly Dictionary<string, IModule> _modules = [];
    private readonly HashSet<string> _stateCache = [];
    private string[] _nextToLast = [];

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

        foreach (var kvp in _modules)
        {
            var id = kvp.Key;
            var inputs = _modules
                .Where(kvp => kvp.Value.Outputs.Contains(id))
                .Select(kvp => kvp.Key).ToArray();
            kvp.Value.AddInputConnections(inputs);
        }

        _nextToLast = _modules.Values.FirstOrDefault(mod => mod.Outputs.Contains(END))?.Inputs ?? [];
    }

    public override void SolvePart1()
    {
        for (int i = 0; i < 1000; i++)
            PressButton();

        long lowPulseCount = 1000 + _modules.Values.Sum(m => m.LowPulsesSent);
        long highPulseCount = _modules.Values.Sum(m => m.HighPulsesSent);

        _logger.Log(lowPulseCount * highPulseCount);
    }

    public override void SolvePart2()
    {
        Setup(); // Reset State
        var nextToRxModule = _modules.Values.First(m => m.Outputs.Contains(END)).Inputs.Select(i => _modules[i]).ToArray();
        long[] periods = new long[nextToRxModule.Length];
        int i = 0;

        long buttonPresses = 0;
        while (periods.Any(p => p == 0))
        {
            buttonPresses++;
            if (PressButton())
                periods[i++] = buttonPresses;
        }

        _logger.Log(periods.Product());
    }

    private bool PressButton()
    {
        bool newEndStateEncountered = false;
        List<string> processBuffer = [START];
        while (processBuffer.Count > 0)
        {
            Queue<string> processing = new(processBuffer);
            processBuffer.Clear();

            while (processing.Count > 0)
            {
                var source = processing.Dequeue();
                var module = _modules[source];
                var outputState = module.State;

                if (outputState && _nextToLast.Contains(source))
                    newEndStateEncountered = true;

                foreach (var next in module.Outputs)
                {
                    if (outputState)
                        module.HighPulsesSent++;
                    else
                        module.LowPulsesSent++;

                    if (!_modules.TryGetValue(next, out var nextModule))
                        continue;

                    bool shouldContinue = nextModule.ProcessPulse(source, outputState);

                    if (shouldContinue == true)
                        processBuffer.Add(next);
                }
            }
        }
        return newEndStateEncountered;
    }

    private class Broadcaster(string id, string[] destinations) : IModule
    {
        public string Id { get; } = id;
        public string[] Inputs { get; } = []; // empty
        public string[] Outputs { get; } = destinations;
        public long HighPulsesSent { get; set; }
        public long LowPulsesSent { get; set; }
        public bool State { get; private set; } = false; // default low
        public void AddInputConnections(string[] inputs) { } // this will be empty
        public bool ProcessPulse(string source, bool pulse) => true;
    }

    private class FlipFlop(string id, string[] destinations) : IModule
    {
        public string Id { get; } = id;
        public string[] Inputs { get; private set; } = [];
        public string[] Outputs { get; } = destinations;
        public long HighPulsesSent { get; set; }
        public long LowPulsesSent { get; set; }
        public bool State { get; private set; } = false; // default low

        public void AddInputConnections(string[] inputs) => Inputs = inputs;
        public bool ProcessPulse(string source, bool pulse)
        {
            if (pulse) return false; // ignore high pulses
            State = !State; // toggle on low pulses
            return true;
        }
    }

    private class Conjunction(string id, string[] destinations) : IModule
    {
        public string Id { get; } = id;
        public string[] Inputs { get; private set; } = [];
        public string[] Outputs { get; } = destinations;
        public long HighPulsesSent { get; set; }
        public long LowPulsesSent { get; set; }
        public bool State => !InputStates.All(p => p);
        public bool[] InputStates = [];
        public void AddInputConnections(string[] inputs)
        {
            Inputs = inputs;
            InputStates = new bool[inputs.Length]; // defaults to false
        }
        public bool ProcessPulse(string source, bool pulse)
        {
            InputStates[Array.IndexOf(Inputs, source)] = pulse;
            return true;
        }
    }

    private interface IModule
    {
        string Id { get; }
        string[] Inputs { get; }
        string[] Outputs { get; }
        long HighPulsesSent { get; set; }
        long LowPulsesSent { get; set; }
        public bool State { get; }
        void AddInputConnections(string[] inputs);
        bool ProcessPulse(string source, bool pulse);
    }
}