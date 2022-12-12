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
    internal static CitySim Instance { get; private set; }

    public static IReadOnlyList<Type> MindImplementations = FindMindImplementations();


    private readonly Type _mindType;

    public IMind GetMind()
    {
        return IMind.Create(_mindType);
    }

    /// <param name="maxTick">The maximum ticks to be simulated</param>
    /// <param name="personCount">The number of persons to be spawned at the beginning of the simulation</param>
    /// <param name="training">Whether the evaluation of the persons' actions should be used to train the model</param>
    /// <param name="personMindWeightsFileToLoad">The name of the file to load the weights for the PersonMind from.
    /// Can be null to start with an untrained model.</param>
    /// <param name="newSaveLocationForPersonMindWeights">The name to the file to save the trained weights to.
    /// If null, the trained model does not gets saved</param>
    /// <param name="personMindBatchSize">The batch size of a PersonMind training.</param>
    /// <param name="personActionExplorationRate">The exploration rate in percent used for the action of the persons.</param>
    /// <param name="personMindLearningRate">The learning rate used for the model for the actions of the persons.</param>
    /// <param name="generateInsightInterval">If set, data will be generated to visualize the outputs of the neural networks based on all inputs every x epochs.</param>
    /// <param name="mindImplementationType">Implementation of <see cref="IMind"/> to use, choose from <see cref="MindImplementations"/></param>
    public CitySim(int maxTick = int.MaxValue, int personCount = 30, bool training = true,
        string? personMindWeightsFileToLoad = null,
        string? newSaveLocationForPersonMindWeights = null,
        int personMindBatchSize = 15,
        int personActionExplorationRate = 7,
        float personMindLearningRate = 0.01f,
        int? generateInsightInterval = null,
        Type? mindImplementationType = null
    )
    {
        _mindType = mindImplementationType ?? typeof(PersonMind);
        Instance = this;
        var desc = new ModelDescription();
        desc.AddLayer<WorldLayer>();
        desc.AddLayer<FixedUpdateLayer>();
        desc.AddAgent<Person, WorldLayer>();

        PersonMind.ExplorationRate = personActionExplorationRate;
        ModelWorker.RegisterInstance(new ModelWorker(new ModelWorkerConfiguration(useCase: ModelUseCase.PersonAction)
            {
                BatchSize = personMindBatchSize,
                WeightsFileToLoad = personMindWeightsFileToLoad,
                WeightsFileToSave = training ? newSaveLocationForPersonMindWeights : null,
                Training = training,
                LearningRate = personMindLearningRate,
                GenerateInsightsInterval = generateInsightInterval
            }),
            PersonMind.ModelWorkerKey);
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
                }
            }
        };
        Binding.tf.enable_eager_execution();
        Application = SimulationStarter.BuildApplication(desc, config);
        Simulation = Application.Resolve<ISimulation>();
        var fixedUpdateLayer = (FixedUpdateLayer)Model.Layers[new LayerType(typeof(FixedUpdateLayer))];
        fixedUpdateLayer.SimulationController = SimulationController;
        WorldLayer = (WorldLayer)Model.Layers[new LayerType(typeof(WorldLayer))];
        WorldLayer.citySim = this;
    }


    public Task<SimulationWorkflowState> StartAsync()
    {
        ModelWorker.StartAll();
        var watch = new Stopwatch();
        var task = Task.Run(() => Simulation.StartSimulation());
        task = task.ContinueWith(state =>
        {
            ModelWorker.TerminateAll();
            return state.Result;
        });
        watch.Start();
        return task;
    }

    public void Pause()
    {
        Simulation.PauseSimulation();
    }

    public void Abort()
    {
        Simulation.AbortSimulation();
        ModelWorker.TerminateAll();
    }
    private static List<Type> FindMindImplementations()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(s =>
            {
                try
                {
                    s.GetTypes();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            })
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IMind).IsAssignableFrom(p))
            .Where(p => p != typeof(MindMock) && p != typeof(IMind))
            .ToList();
    }
}