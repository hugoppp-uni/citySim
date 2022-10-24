using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


namespace Benchmark;

public class Benchmark
{
    private CitySim.Backend.CitySim _citySim = CreateCitySim();

    private static CitySim.Backend.CitySim CreateCitySim()
    {
        var citySim = new CitySim.Backend.CitySim(10);
        citySim.SimulationController.TicksPerSecond = Int32.MaxValue;
        return citySim;
    }

    [Benchmark]
    public void Bench()
    {
        _citySim.StartAsync().Wait();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

    }
}