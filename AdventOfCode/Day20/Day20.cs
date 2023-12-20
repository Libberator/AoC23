using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC;

public class Day20(ILogger logger, string path) : Puzzle(logger, path)
{
    private const string START = "broadcaster", END = "rx";
    private const char FLIP_FLOP = '%', CONJUNCTION = '&';

    private readonly List<Module> _modules = [];
    private readonly Module _start = new(START), _end = new(END);
    private Module[] _gateKeepers = [];

    public override void Setup()
    {
        _modules.Clear();
        _modules.Add(_end);
        Dictionary<Module, string[]> moduleToOutputs = [];

        foreach (var line in ReadFromFile())
        {
            var split = line.Split(" -> ");
            var source = split[0];
            var outputs = split[1].Split(", ");
            var id = source[1..];

            var module = source[0] switch
            {
                FLIP_FLOP => new FlipFlop(id),
                CONJUNCTION => new Conjunction(id),
                _ => _start
            };

            _modules.Add(module);
            moduleToOutputs.Add(module, outputs);
        }

        // assign outputs
        foreach (var kvp in moduleToOutputs)
            kvp.Key.Outputs = kvp.Value.Select(id => _modules.First(m => m.Id == id)).ToArray();

        // assign inputs
        foreach (var module in _modules)
            module.Inputs = _modules.Where(m => m.Outputs.Contains(module)).ToArray();

        // grab the inputs of the next-to-last module as "gatekeepers"
        _gateKeepers = _end.Inputs[0].Inputs;
    }

    private void Reset() => _modules.ForEach(m => m.Reset());

    public override void SolvePart1()
    {
        Reset(); // Reset State for benchmarks

        for (int i = 0; i < 1000; i++)
            PressButton();

        long lowPulseCount = 1000 + _modules.Sum(m => m.LowPulsesSent);
        long highPulseCount = _modules.Sum(m => m.HighPulsesSent);
        _logger.Log(lowPulseCount * highPulseCount);
    }

    public override void SolvePart2()
    {
        Reset(); // Reset State for benchmarks

        long[] periods = new long[_gateKeepers.Length];
        long buttonPresses = 0, i = 0;

        while (periods.Any(p => p == 0))
        {
            buttonPresses++;
            if (PressButton()) // if it lights up any of the gatekeepers
                periods[i++] = buttonPresses;
        }

        _logger.Log(periods.Product()); // Least Common Multiple
    }

    private bool PressButton()
    {
        bool hitNewEndState = false;
        Queue<Module> processing = new();
        processing.Enqueue(_start);

        while (processing.Count > 0)
        {
            var module = processing.Dequeue();
            var isHighPulse = module.State;

            if (isHighPulse)
            {
                module.HighPulsesSent += module.Outputs.LongLength;

                if (_gateKeepers.Contains(module))
                    hitNewEndState = true;
            }
            else
                module.LowPulsesSent += module.Outputs.LongLength;

            foreach (var nextModule in module.Outputs)
            {
                nextModule.ProcessPulse(module, isHighPulse);

                if (!isHighPulse || nextModule is Conjunction)
                    processing.Enqueue(nextModule);
            }
        }
        return hitNewEndState;
    }

    private class FlipFlop(string id) : Module(id)
    {
        public override void ProcessPulse(Module _, bool pulse) => State ^= !pulse; // toggle on low pulses
    }

    private class Conjunction(string id) : Module(id)
    {
        private bool[] _inputStates = [];

        public override Module[] Inputs
        {
            set
            {
                base.Inputs = value;
                _inputStates = new bool[value.Length]; // defaults to all false
            }
        }

        public override bool State => !_inputStates.All(p => p);

        public override void ProcessPulse(Module source, bool pulse) =>
            _inputStates[Array.IndexOf(Inputs, source)] = pulse;

        public override void Reset()
        {
            base.Reset();
            _inputStates = new bool[Inputs.Length];
        }
    }

    private class Module(string id)
    {
        public string Id { get; } = id;
        public Module[] Outputs { get; set; } = [];
        public virtual Module[] Inputs { get; set; } = [];
        public virtual bool State { get; protected set; }
        public long HighPulsesSent { get; set; }
        public long LowPulsesSent { get; set; }
        public virtual void ProcessPulse(Module source, bool pulse) { }
        public virtual void Reset()
        {
            HighPulsesSent = LowPulsesSent = 0;
            State = false;
        }
    }
}