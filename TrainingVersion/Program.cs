
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Util.Learning;
using NLog;
using Tensorflow;

public class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
    

    public static void Main(string[] args)
    {
        const string PersonMindFileName = "./ModelWeights/personMind.hdf5";
        _logger.Debug(Binding.tf.config.list_physical_devices("GPU"));
        var g = Binding.tf.config.list_physical_devices("GPU");
        var iterationCount = 1;
        if (args.Length > 0)
        {
            iterationCount = int.Parse(args[0]);
        }
        Binding.tf_output_redirect = TextWriter.Null;
        for (var iteration = 1; iteration <= iterationCount; iteration++)
        {
           Console.Out.WriteLine($"Prepare Iteration {iteration}");
            _logger.Trace($"Prepare Iteration {iteration}");
            var citySim = new CitySim.Backend.CitySim(
                personMindWeightsFileToLoad: PersonMindFileName,
                newSaveLocationForPersonMindWeights: PersonMindFileName,
                personCount: 45,
                maxTick: 250,
                personMindBatchSize: 50,
                personActionExplorationRate: 15,
                training: true,
                generateInsightData: false
            )
            {
                SimulationController =
                {
                    TicksPerSecond = int.MaxValue
                }
            };
            _logger.Trace($"Prepare Iteration {iteration}");
            citySim.StartAsync().Wait();
            _logger.Debug($"The training took in average {ModelWorker.GetInstance(nameof(Person)).AverageFitDuration}");
        }
    }
}