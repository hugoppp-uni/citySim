
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Util.Learning;
using NLog;
using Tensorflow;

public class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
    

    public static async Task Main(string[] args)
    {
        const string PersonMindFileName = "./ModelWeights/personMind.hdf5";
        _logger.Debug(Binding.tf.config.list_physical_devices("GPU"));
        var g = Binding.tf.config.list_physical_devices("GPU");
        var cancellationTokenSource = new CancellationTokenSource();
        var iterationCount = 5;
        CitySim.Backend.CitySim? citySim = null;
        if (args.Length > 0)
        {
            iterationCount = int.Parse(args[0]);
        }

        var iteration = 1;
        void OnConsoleOnCancelKeyPress(object? _, ConsoleCancelEventArgs e)
        {
            citySim.Abort();
            iteration = iterationCount + 1;
            cancellationTokenSource.Cancel();
        }
        Binding.tf_output_redirect = TextWriter.Null;
        for (; iteration <= iterationCount; iteration++)
        {
            await Console.Out.WriteLineAsync($"Prepare Iteration {iteration}");
            _logger.Trace($"Prepare Iteration {iteration}");
            citySim = new CitySim.Backend.CitySim(
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
            var task = citySim.StartAsync();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            try
            {
                await task.WaitAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Training canceled");
            }
            //await task;
            Console.CancelKeyPress -= OnConsoleOnCancelKeyPress;
            _logger.Debug($"The training took in average {ModelWorker.GetInstance(nameof(Person)).AverageFitDuration}");
            Console.WriteLine($"Iteration {iteration} finished");
        }
    }
}