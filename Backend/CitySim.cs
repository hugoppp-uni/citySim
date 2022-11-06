using System.Diagnostics;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Util;
using CitySim.Backend.Util.Learning;
using CitySim.Backend.World;
using Mars.Components.Starter;
using Mars.Core.Executor;
using Mars.Core.Simulation;
using Mars.Interfaces;
using Mars.Interfaces.Model;
using Tensorflow;

namespace CitySim.Backend;

using Mars.Core.Simulation.Entities;

public class CitySim
{
    public WorldLayer WorldLayer { get; }
    public IRuntimeModel Model => Simulation.WorkflowState.Model;
    private ISimulationContainer Application { get; }
    private ISimulation Simulation { get; }
    public SimulationController SimulationController { get; } = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxTick">The maximum ticks to be simulated</param>
    /// <param name="personCount">The number of persons to be spawned at the beginning of the simulation</param>
    /// <param name="training">Whether the evaluation of the persons' actions should be used to train the model</param>
    /// <param name="personMindWeightsFileToLoad">The name of the file to load the weights for the PersonMind from.
    /// Can be null to start with an untrained model.</param>
    /// <param name="newSaveLocationForPersonMindWeights">The name to the file to save the trained weights to.
    /// If null, the trained model does not gets saved</param>
    /// <param name="personMindBatchSize">The batch size of a PersonMind training.</param>
    /// <param name="personActionExplorationRate">The exploration rate in percent used for the action of the persons.</param>
    public CitySim(int maxTick = int.MaxValue, int personCount = 30, bool training = true,
        string? personMindWeightsFileToLoad = null, 
        string? newSaveLocationForPersonMindWeights = null, 
        int personMindBatchSize = 15,
        int personActionExplorationRate = 7)
    {
        var desc = new ModelDescription();
        desc.AddLayer<WorldLayer>();
        desc.AddLayer<FixedUpdateLayer>();
        desc.AddAgent<Person, WorldLayer>();
        PersonMind.ExplorationRate = personActionExplorationRate;
        ModelWorker.RegisterInstance(new ModelWorker(new ModelWorkerConfiguration(type: ModelType.PersonAction)
            {
                BatchSize = personMindBatchSize,
                WeightsFileToLoad = personMindWeightsFileToLoad,
                WeightsFileToSave = training ? newSaveLocationForPersonMindWeights : null,
                Training = training
            }),
            nameof(Person));
        var config = new SimulationConfig
        {
            SimulationIdentifier = "CitySim",
            ModelDescription = desc,
            Globals = new Globals
            {
                ShowConsoleProgress = false,
                OutputTarget = OutputTargetType.Csv,
                Steps = maxTick
            },
            AgentMappings =
            {
                new AgentMapping
                {
                    Name = nameof(Person),
                    InstanceCount = personCount,
                    IndividualMapping =
                    {
                        new IndividualMapping
                        {
                            ParameterName = "ModelWorkerKey", Value = nameof(Person)
                        }
                    }
                }
            }
        };
        Binding.tf.enable_eager_execution();
        Application = SimulationStarter.BuildApplication(desc, config);
        Simulation = Application.Resolve<ISimulation>();
        var fixedUpdateLayer = (FixedUpdateLayer)Model.Layers[new LayerType(typeof(FixedUpdateLayer))];
        fixedUpdateLayer.SimulationController = SimulationController;
        WorldLayer = (WorldLayer)Model.Layers[new LayerType(typeof(WorldLayer))];
    }


    public Task<SimulationWorkflowState> StartAsync()
    {
        ModelWorker.StartAll();
        var watch = new Stopwatch();
        var task = Task.Run(() => Simulation.StartSimulation());
        task.ContinueWith(_ => { ModelWorker.TerminateAll(); });
        watch.Start();
        return task;
    }

    public void Pause()
    {
        Simulation.PauseSimulation();
    }
}