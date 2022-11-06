using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.World;
using MQTTnet.Internal;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;

namespace CitySim.Backend.Util.Learning;
using static KerasApi;

public class ModelWorker
{
    private const float LearningRate = 0.2f;// the learning rate is high because the expected value is not changed directly to 0 or 1
    private readonly BlockingQueue<ModelTask> _taskQueue = new();
    private Model? _model;
    private readonly List<NDArray> _trainingBatchInput = new();
    private readonly List<NDArray> _trainingBatchExpected = new();
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;
    
    public  void Queue(ModelTask task)
    {
        _taskQueue.Enqueue(task);
    }
    
    private  Thread? _thread;
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

    public void Start()
    {
        _thread = new Thread(WorkOnModel);
        _thread.Start();
    }
    private void WorkOnModel()
    {
        _model = BuildModel(_configuration.Type, _configuration.WeightsFileToLoad);
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                ModelTask task = _taskQueue.Dequeue(_cancellationToken);
                Monitor.Enter(task);
                if (task.Output.size != 0)
                {
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
                    task.Output = _model.predict(task.Input)[0].numpy()[0];
                }
                Monitor.Pulse(task);
                Monitor.Exit(task);
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

    private static Model BuildModel(ModelType type, string? weightsFile)
    {
        Model model = null!;
        if (type == ModelType.PersonAction)
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
    public static ModelWorker GetInstance(string key)
    {
        return Instances[key];
    }

    public static void TerminateAll()
    {
        foreach (var (_, modelWorker) in Instances)
        {
            modelWorker.End();
        }
    }
    
    public static void StartAll()
    {
        foreach (var (_, modelWorker) in Instances)
        {
            modelWorker.Start();
        }
    }

    public static void RegisterInstance(ModelWorker worker, string key)
    {
        Instances[key] = worker;
    }
}