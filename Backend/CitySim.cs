using System.Diagnostics;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Util;
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

    public CitySim(int maxTick = int.MaxValue)
    {
        var desc = new ModelDescription();
        desc.AddLayer<WorldLayer>();
        desc.AddLayer<FixedUpdateLayer>();
        desc.AddAgent<Person, WorldLayer>();
        ModelWorker.RegisterInstance(new ModelWorker(new ModelWorkerConfiguration(type: ModelType.PersonAction)
            {
                BatchSize = 10
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
                    InstanceCount = 30,
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
        task.ContinueWith(_ =>
        {
            ModelWorker.TerminateAll();
        });
        watch.Start();
        return task;
    }

    public void Pause()
    {
        Simulation.PauseSimulation();
    }
}