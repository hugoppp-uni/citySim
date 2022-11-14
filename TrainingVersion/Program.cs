
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
                personCount: 40,
                maxTick: 500,
                personMindBatchSize: 25,
                personActionExplorationRate: 20,
                personMindLearningRate: 0.01f,
                training: true,
                generateInsightInterval: 50
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