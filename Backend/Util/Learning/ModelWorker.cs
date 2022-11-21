using System.Diagnostics;
using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.World;
using MQTTnet.Internal;
using NLog;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;

namespace CitySim.Backend.Util.Learning;
using static KerasApi;

/// <summary>
/// Worker to handle <see cref="ModelTask"/>s on a model.
/// This class is required because all calls to the same Tensorflow model must come from the same thread.
/// A worker instance can be register and then fetched by a static methode.
/// Call <see cref="Start"/> or <see cref="StartAll"/> to start the worker thread,
/// <see cref="Queue"/> to queue a task and <see cref="End"/> or <see cref="TerminateAll"/> to terminate the worker
/// </summary>
public class ModelWorker
{
    private readonly BlockingQueue<ModelTask> _taskQueue = new();
    private Model? _model;
    private readonly List<NDArray> _trainingBatchInput = new();
    private readonly List<NDArray> _trainingBatchExpected = new();
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly  ModelWorkerConfiguration _configuration;
    private int _fitCalls = 0;
    public long AverageFitDuration { get; private set; }
    private readonly Stopwatch _stopwatch = new();
    
    /// <summary>
    /// Queues an <see cref="ModelTask"/>. Use <code>Monitor.Wait(task)</code> to proceed with the result
    /// as soon as the prediction task was completed.
    /// It is needed to wait for a prediction task, but it is recommended not to wait for a training task.
    /// </summary>
    /// <param name="task">The <see cref="ModelTask"/> to be handled</param>
    public void Queue(ModelTask task)
    {
        if (task.Output.size != 0)
        {
            if (Monitor.IsEntered(task))
            {
                _logger.Warn("Training should not be waited for");
            }
        }
        else
        {
            if (!Monitor.IsEntered(task))
            {
                throw new SynchronizationLockException("If the task is not locked while queuing, the task may " +
                                                       "already be completed before the caller waits for the notify");
            }
        }
        _taskQueue.Enqueue(task);
    }
    
    private  Thread? _thread;
    
    /// <summary>
    /// Terminates the worker (thread)
    /// </summary>
    public void End()
    {
        _cancellationTokenSource.Cancel();
    }
    
    public ModelWorker(ModelWorkerConfiguration configuration)
    {
        _configuration = configuration;
        _cancellationTokenSource = new();
        _cancellationToken = _cancellationTokenSource.Token;
    }

    /// <summary>
    /// Starts the worker (thread)
    /// </summary>
    public void Start()
    {
        _thread = new Thread(WorkOnModel);
        _thread.Start();
    }

    private int _epoch = -1;
    private async void WorkOnModel()
    {
        _model = BuildModel(_configuration.UseCase, _configuration.WeightsFileToLoad, _configuration.LearningRate);
        while (!_cancellationToken.IsCancellationRequested || _taskQueue.Count != 0)
        {
            try
            {
                ModelTask task;
                task = _cancellationToken.IsCancellationRequested ?
                    _taskQueue.RemoveFirst() : _taskQueue.Dequeue(_cancellationToken);
                if (task.Output.size != 0)
                {
                    // there is no need to wait
                    if (_configuration.Training && !_cancellationToken.IsCancellationRequested)
                    {
                        _trainingBatchInput.Add(task.Input);
                        _trainingBatchExpected.Add(task.Output);
                        if (_trainingBatchInput.Count ==  _configuration.BatchSize)
                        {
                            var input = np.stack(_trainingBatchInput.ToArray());
                            var expected = np.stack(_trainingBatchExpected.ToArray());
                            _stopwatch.Start();
                            _model.fit(input, expected, batch_size: _configuration.BatchSize);
                            _stopwatch.Stop();
                            AverageFitDuration = (AverageFitDuration * _fitCalls++ + _stopwatch.ElapsedMilliseconds) /
                                                 _fitCalls;
                            _stopwatch.Reset();
                            _trainingBatchInput.Clear();
                            _trainingBatchExpected.Clear();
                            _epoch++;
                            if (_configuration.GenerateInsightsInterval != null && 
                                _epoch % _configuration.GenerateInsightsInterval == 0)
                            {
                                await ModelVisualisation.SaveInsight(_model, 0.08m,
                                    $"{_configuration.UseCase.ToString()}-Epoch {_epoch}", _cancellationToken);
                            }
                            
                        }
                    }
                }
                else
                {
                    Monitor.Enter(task);
                    task.Output = _model.predict(task.Input)[0].numpy()[0];
                    _logger.Trace($"The input {task.Input.JoinDataToString()} generated the" +
                                  $" prediction {task.Output.JoinDataToString()} in epoch {_epoch}");
                    Monitor.Pulse(task);
                    Monitor.Exit(task);
                }
                
            }
            catch (OperationCanceledException _)
            {
                //ignore    
            }
        }
        
        

        if (_configuration.WeightsFileToSave != null)
        {
            _model.save_weights(_configuration.WeightsFileToSave);
        }
        
    }

    private static Model BuildModel(ModelUseCase useCase, string? weightsFile, float learningRate)
    {
        Model model = null!;
        if (useCase == ModelUseCase.PersonAction)
        {
            model = BuildPersonActionModel(learningRate);
        }
        if (weightsFile != null)
        {
            if (File.Exists(weightsFile))
            {
                model.load_weights(weightsFile);
            }
            else
            {
                Console.WriteLine("No weights file found, beginning with new weights");
            }
        }
        return model;
    }
    
    private static Model BuildPersonActionModel(float learningRate)
    {
        var layers = new LayersApi();
        var lenght = new GlobalState(0, 0, 0).AsNormalizedArray().Length;
        lenght += new PersonNeeds().AsNormalizedArray().Length;
        lenght += Distances.Count;
        var actions = new ActionType[Enum.GetValues(typeof(ActionType)).Length];
        var inLayer = keras.Input((lenght), name:"status");
        var dense = layers.Dense(lenght, activation: "relu").Apply(inLayer);
        var output = layers.Dense(actions.Length, activation: "softmax").Apply(dense);
        var model = keras.Model(inLayer, output);
        model.summary();
        model.compile(
            optimizer: keras.optimizers.SGD(learningRate),
            loss: keras.losses.CategoricalCrossentropy(from_logits: true),
            metrics: Array.Empty<string>()
        );
        
        return model;
    }
    
    private static readonly Dictionary<string, ModelWorker> Instances = new();
    
    /// <returns> the registered instance for the given key</returns>
    public static ModelWorker GetInstance(string key)
    {
        return Instances[key];
    }

    /// <summary>
    /// Terminates all with <see cref="RegisterInstance"/> registered workers 
    /// </summary>
    public static void TerminateAll()
    {
        foreach (var (_, modelWorker) in Instances)
        {
            modelWorker.End();
        }
    }
    
    /// <summary>
    /// Starts all with <see cref="RegisterInstance"/> registered workers 
    /// </summary>
    public static void StartAll()
    {
        foreach (var (_, modelWorker) in Instances)
        {
            modelWorker.Start();
        }
    }

    /// <summary>
    /// Registers an instance toi be fetched by via <see cref="GetInstance"/>.
    /// A worker can be used without even if it wasn't registered. 
    /// </summary>
    /// <param name="worker">The instance to be registered</param>
    /// <param name="key">the key to associate the instance with</param>
    public static void RegisterInstance(ModelWorker worker, string key)
    {
        Instances[key] = worker;
    }
}