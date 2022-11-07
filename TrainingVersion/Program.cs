using NLog;

public class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    public static void Main(string[] args)
    {
        const string PersonMindFileName = "./ModelWeights/personMind.hdf5";
        var iterationCount = 1;
        if (args.Length > 0)
        {
            iterationCount = int.Parse(args[0]);
        }

        for (var iteration = 1; iteration <= iterationCount; iteration++)
        {
            _logger.Trace($"Prepare Iteration {iteration}");
            var citySim = new CitySim.Backend.CitySim(personMindWeightsFileToLoad: PersonMindFileName,
                newSaveLocationForPersonMindWeights: PersonMindFileName, personCount: 30, maxTick: 1000,
                personMindBatchSize: 15, personActionExplorationRate: 25)
            {
                SimulationController =
                {
                    TicksPerSecond = int.MaxValue
                }
            };
            _logger.Trace($"Prepare Iteration {iteration}");
            citySim.StartAsync().Wait();
        }
    }
}
