using CitySim.Backend.Agents;
using CitySim.Backend.World;
using Mars.Components.Starter;
using Mars.Core.Simulation;
using Mars.Interfaces;
using Mars.Interfaces.Model;

namespace CitySim.Backend;

public class CitySim
{
    public GridLayer GridLayer => (GridLayer)Simulation.WorkflowState.Model.Layers[new LayerType(typeof(GridLayer))];
    private ISimulationContainer Application { get; }
    private ISimulation Simulation { get; }

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
            }
        };
        Application = SimulationStarter.BuildApplication(desc, config);
        Simulation = Application.Resolve<ISimulation>();
    }


    public Task StartAsync()
    {
        return Task.Factory.StartNew(() =>
        {
            var loopResults = Simulation.StartSimulation();
            Console.WriteLine($"Simulation execution finished after {loopResults.Iterations} steps");
        });
    }

    public void Pause()
    {
        Simulation.PauseSimulation();
    }
}