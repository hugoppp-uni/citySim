using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using NLog;


namespace Benchmark;

public class Benchmark
{
    private const string PersonMindFileName = "./ModelWeights/personMind.hdf5";
    private CitySim.Backend.CitySim _citySim = null!;


    [Params(10, 20, 30)]
#pragma warning disable CS0649
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnassignedField.Global
    public int PersonCount;
#pragma warning restore CS0649

    [GlobalSetup]
    public void GlobalSetup()
    {
        LogManager.Configuration = new NLog.Config.LoggingConfiguration();
        LogManager.Shutdown();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _citySim = new CitySim.Backend.CitySim(15, personMindWeightsFileToLoad: PersonMindFileName,
            personCount: PersonCount, personMindBatchSize: PersonCount / 2)
        {
            SimulationController =
            {
                TicksPerSecond = int.MaxValue
            }
        };
    }

    [Benchmark]
    [IterationCount(5)]
    public void Bench()
    {
        _citySim.StartAsync().Wait();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var config = new ManualConfig();
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}