namespace CitySim.Backend.Util.Learning;

/// <summary>
/// The configuration of a <see cref="ModelWorker"/>
/// </summary>
public class ModelWorkerConfiguration
{
    /// <summary>
    /// Created a ModelWorker for the given useCase
    /// </summary>
    public ModelWorkerConfiguration(ModelUseCase useCase)
    {
        UseCase = useCase;
    }
    /// <summary>
    /// The model to be used by the worker
    /// </summary>
    public ModelUseCase UseCase { get; }

    /// <summary>
    /// The filename to load a weights file for the model
    /// </summary>
    public string? WeightsFileToLoad
    {
        get => weightsFileToLoad;
        set => weightsFileToLoad = value == null ? value : Path.GetFullPath(value);
    }

    /// <summary>
    /// The file to save the model to. If set, the model gets saved even if <see cref="Training"/> is set to false
    /// </summary>
    public string? WeightsFileToSave
    {
        get => weightsFileToSave;
        set => weightsFileToSave = value == null ? value : Path.GetFullPath(value);
    }

    /// <summary>
    /// The batch size of a training. Training Tasks are collected until enough data was collected 
    /// </summary>
    public Func<int, int> BatchSize { get; set; } = x => x / 2;

    /// <summary>
    /// if set to false, all training tasks are ignored
    /// </summary>
    public bool Training = true;

    /// <summary>
    /// The Learning rate of the model
    /// </summary>
    public float LearningRate = 0.01f;

    /// <summary>
    /// If set, data will be generated to visualize the outputs based on all inputs every x epoch
    /// </summary>
    public int? GenerateInsightsInterval = null;

    private string? weightsFileToLoad = null;
    private string? weightsFileToSave = null;
}