using System.Diagnostics;
using CitySim.Backend.Agents;
using CitySim.Backend.World;
using Mars.Components.Starter;
using Mars.Core.Executor;
using Mars.Core.Simulation;
using Mars.Interfaces;
using Mars.Interfaces.Model;

namespace CitySim.Backend;

using Mars.Core.Simulation.Entities;

public class CitySim
{
    public GridLayer GridLayer { get; }
    public IRuntimeModel Model => Simulation.WorkflowState.Model;
    private ISimulationContainer Application { get; }
    private ISimulation Simulation { get; }
    public SimulationController SimulationController { get; } = new();

    public CitySim()
    {
        var desc = new ModelDescription();
        desc.AddLayer<GridLayer>();
        desc.AddLayer<FixedUpdateLayer>();
        desc.AddAgent<Person, GridLayer>();

        var config = new SimulationConfig
        {
            SimulationIdentifier = "CitySim",
            ModelDescription = desc,
            Globals = new Globals
            {
                StartPoint = DateTime.Now,
                EndPoint = DateTime.Now.AddSeconds(30),
                DeltaTUnit = TimeSpanUnit.Seconds,
                ShowConsoleProgress = false,
                OutputTarget = OutputTargetType.Csv,
                
            },
            
        };
        Application = SimulationStarter.BuildApplication(desc, config);
        Simulation = Application.Resolve<ISimulation>();
        var fixedUpdateLayer = (FixedUpdateLayer)Model.Layers[new LayerType(typeof(FixedUpdateLayer))];
        fixedUpdateLayer.SimulationController = SimulationController;
        GridLayer = (GridLayer)Model.Layers[new LayerType(typeof(GridLayer))];
    }


    public Task<SimulationWorkflowState> StartAsync()
    {
        var watch = new Stopwatch();
        var task = Task.Run(() => Simulation.StartSimulation());
        watch.Start();
        return task;
    }

    public void Pause()
    {
        Simulation.PauseSimulation();
    }
}