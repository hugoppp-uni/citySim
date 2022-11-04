using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.World;
using MQTTnet.Internal;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;

namespace CitySim.Backend.Entity.Agents;
using static KerasApi;

public static class ModelWorker
{
    private const float LearningRate = 0.2f;// the learning rate is high because the expected value is not changed directly to 0 or 1
    private const int TrainingBatchSize = 5;
    private static readonly BlockingQueue<ModelTask> TaskQueue = new();
    private static bool _running = true;
    private static Model? _model;
    private static List<NDArray> _trainingBatchInput = new(TrainingBatchSize);
    private static List<NDArray> _trainingBatchExpected = new(TrainingBatchSize);

    public static void Queue(ModelTask task)
    {
        TaskQueue.Enqueue(task);
    }

    private static Thread? _thread;
    public static void End()
    {
        _running = false;
        _thread?.Join();
    }

    public static void Start(string? weightsFileToLoad, string? weightsFileToSave)
    {
        _thread = new Thread(WorkOnModel);
        _thread.Start((weightsFileToLoad, weightsFileToSave));
    }
    private static void WorkOnModel(object? weightsFiles)
    {
        string? weightsFileToLoad = null, weightsFileToSave = null;
        if (weightsFiles != null)
        {
            (weightsFileToLoad, weightsFileToSave) = ((string?, string?)) weightsFiles;
        }
        _model = BuildModel(weightsFileToLoad);
        while (_running)
        {
            ModelTask task = TaskQueue.Dequeue();
            Monitor.Enter(task);
            if (task.Output.size != 0)
            {
                _trainingBatchInput.Add(task.Input);
                _trainingBatchExpected.Add(task.Output);
                if (_trainingBatchInput.Count == TrainingBatchSize)
                {
                    var input = np.stack(_trainingBatchInput.ToArray());
                    var expected = np.stack(_trainingBatchExpected.ToArray());
                    _model.fit(input, expected, batch_size: TrainingBatchSize);
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

        if (weightsFileToSave != null)
        {
            _model.save_weights(weightsFileToSave);
        }
        
    }
    
    
    private static Model BuildModel(string? weightsFile)
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