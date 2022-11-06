namespace CitySim.Backend.Util.Learning;

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

    public bool Training = true;
}