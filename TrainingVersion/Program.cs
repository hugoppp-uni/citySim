
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Util.Learning;
using NLog;
using Tensorflow;

public class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
    

    public static async Task Main(string[] args)
    {
        const string PersonMindFileName = "./ModelWeights/personMindXXX.hdf5";
        _logger.Debug(Binding.tf.config.list_physical_devices("GPU"));
        var g = Binding.tf.config.list_physical_devices("GPU");
        var cancellationTokenSource = new CancellationTokenSource();
        var iterationCount = 300;
        var startNumber = 370;
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
                personMindWeightsFileToLoad: startNumber == 0 ? null :
                    PersonMindFileName.Replace("XXX", (startNumber + iteration - 1).ToString()),
                newSaveLocationForPersonMindWeights: PersonMindFileName.Replace("XXX", (startNumber + iteration).ToString()),
                personCount: iteration < 100 ? 24 : 20,
                maxTick: iteration < 200 && iteration % 2 != 0 ? 350 : 700,
                personMindBatchSize: (x)=> x / 2,
                personActionExplorationRate:iteration % 2 == 0 ? 0 : iteration < 100 ? 35 : iteration < 200 ? 15 : 5,
                personMindLearningRate: iteration < 100 ? 0.03f : iteration < 200 ? 0.01f : 0.005f,
                training: true,
                generateInsightInterval: null
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
            _logger.Debug($"The training took in average {ModelWorker.GetInstance(PersonMind.ModelWorkerKey).AverageFitDuration}");
            Console.WriteLine($"Iteration {iteration} finished after step {citySim.WorldLayer.GetCurrentTick()} with a maximum of {citySim.WorldLayer.MaxPersonCount} persons");
        }
    }
}