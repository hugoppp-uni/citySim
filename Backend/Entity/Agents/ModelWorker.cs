using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.World;
using MQTTnet.Internal;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;

namespace CitySim.Backend.Entity.Agents;
using static KerasApi;

public class ModelWorker
{
    private const float LearningRate = 0.2f;// the learning rate is high because the expected value is not changed directly to 0 or 1
    private readonly BlockingQueue<ModelTask> _taskQueue = new();
    private bool _running = true;
    private Model? _model;
    private readonly List<NDArray> _trainingBatchInput = new();
    private readonly List<NDArray> _trainingBatchExpected = new();
    
    public  void Queue(ModelTask task)
    {
        _taskQueue.Enqueue(task);
    }
    
    private  Thread? _thread;
    public void End()
    {
        _running = false;
        _thread?.Join();
    }
    private readonly  ModelWorkerConfiguration _configuration;
    public ModelWorker(ModelWorkerConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Start()
    {
        _thread = new Thread(WorkOnModel);
        _thread.Start();
    }
    private void WorkOnModel()
    {
        _model = BuildModel(_configuration.Type, _configuration.WeightsFileToLoad);
        while (_running)
        {
            ModelTask task = _taskQueue.Dequeue();
            Monitor.Enter(task);
            if (task.Output.size != 0)
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
            else
            {
               task.Output = _model.predict(task.Input)[0].numpy()[0];
            }
            Monitor.Pulse(task);
            Monitor.Exit(task);
        }

        if (_configuration.WeightsFileToSave != null)
        {
            _model.save_weights(_configuration.WeightsFileToSave);
        }
        
    }

    private static Model BuildModel(ModelType type, string? weightsFile)
    {
        if (type == ModelType.PersonAction)
        {
            return BuildPersonActionModel(weightsFile);
        }

        return null;
    }
    
    private static Model BuildPersonActionModel(string? weightsFile)
    {
        var layers = new LayersApi();
        var cLength = new GlobalState(0, 0, 0).AsNormalizedArray().Length;
        var iLength = new PersonNeeds().AsNormalizedArray().Length;
        var actions = new ActionType[Enum.GetValues(typeof(ActionType)).Length];
        var inLayer = keras.Input((iLength+cLength));
        var dense = layers.Dense(iLength+cLength, activation: "relu").Apply(inLayer);
        var output = layers.Dense(actions.Length, activation: "softmax").Apply(dense);
        var model = keras.Model(inLayer, output);
        model.summary();
        model.compile(
            optimizer: keras.optimizers.SGD(LearningRate),
            loss: keras.losses.CategoricalCrossentropy(from_logits: true),
            metrics: Array.Empty<string>()
        );
        
        if (weightsFile != null && File.Exists(weightsFile))
        {
            model.load_weights(weightsFile);
        }

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

public class ModelTask
{
    public NDArray Input { get;}
    public NDArray Output { get; set; }
    
    public ModelTask(NDArray input, NDArray output)
    {
        Input = input;
        Output = output;
    }
    public ModelTask(NDArray input):this(input, new NDArray(Array.Empty<double>()))
    { }
}

public class ModelWorkerConfiguration
{
    public ModelWorkerConfiguration(ModelType type)
    {
        Type = type;
    }
    public ModelType Type { get; }
    public string? WeightsFileToLoad { get; set; } = null;
    public string? WeightsFileToSave { get; set; } = null;
    public int BatchSize { get; set; } = 5;
}

public enum ModelType
{
    PersonAction
}
    
    