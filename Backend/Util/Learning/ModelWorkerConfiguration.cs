﻿namespace CitySim.Backend.Util.Learning;

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
    public string? WeightsFileToLoad { get; set; } = null;
    /// <summary>
    /// The file to save the model to. If set, the model gets saved even if <see cref="Training"/> is set to false
    /// </summary>
    public string? WeightsFileToSave { get; set; } = null;
    
    /// <summary>
    /// The batch size of a training. Training Tasks are collected until enough data was collected 
    /// </summary>
    public int BatchSize { get; set; } = 5;

    /// <summary>
    /// if set to false, all training tasks are ignored
    /// </summary>
    public bool Training = true;
}