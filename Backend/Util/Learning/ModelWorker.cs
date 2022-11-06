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
    private const float LearningRate = 0.2f;// the learning rate is high because the expected value is not changed directly to 0 or 1
    private readonly BlockingQueue<ModelTask> _taskQueue = new();
    private Model? _model;
    private readonly List<NDArray> _trainingBatchInput = new();
    private readonly List<NDArray> _trainingBatchExpected = new();
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
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
        _thread?.Join();
    }
    private readonly  ModelWorkerConfiguration _configuration;
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
    private void WorkOnModel()
    {
        _model = BuildModel(_configuration.UseCase, _configuration.WeightsFileToLoad);
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                ModelTask task = _taskQueue.Dequeue(_cancellationToken);
                if (task.Output.size != 0)
                {
                    // there is no need to wa
                    if (_configuration.Training)
                    {
                        _trainingBatchInput.Add(task.Input);
                        _trainingBatchExpected.Add(task.Output);
                        if (_trainingBatchInput.Count ==  _configuration.BatchSize)
                        {
                            var input = np.stack(_trainingBatchInput.ToArray());
                            var expected = np.stack(_trainingBatchExpected.ToArray());
                            _model.fit(input, expected, batch_size: _configuration.BatchSize);
                            _trainingBatchInput.Clear();
                            _trainingBatchExpected.Clear();
                        }
                    }
                }
                else
                {
                    Monitor.Enter(task);
                    task.Output = _model.predict(task.Input)[0].numpy()[0];
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

    private static Model BuildModel(ModelUseCase useCase, string? weightsFile)
    {
        Model model = null!;
        if (useCase == ModelUseCase.PersonAction)
        {
            model = BuildPersonActionModel();
        }
        if (weightsFile != null)
        {
            model.load_weights(weightsFile);
        }
        return model;
    }
    
    private static Model BuildPersonActionModel()
    {
        var layers = new LayersApi();
        var cLength = new GlobalState(0, 0, 0).AsNormalizedArray().Length;
        var iLength = new PersonNeeds().AsNormalizedArray().Length;
        var actions = new ActionType[Enum.GetValues(typeof(ActionType)).Length];
        var inLayer = keras.Input((iLength+cLength), name:"status");
        var dense = layers.Dense(iLength+cLength, activation: "relu").Apply(inLayer);
        var output = layers.Dense(actions.Length, activation: "softmax").Apply(dense);
        var model = keras.Model(inLayer, output);
        model.summary();
        model.compile(
            optimizer: keras.optimizers.SGD(LearningRate),
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