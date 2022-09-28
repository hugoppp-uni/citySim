using CitySim.Agents;
using CitySim.World;
using Mars.Components.Starter;
using Mars.Interfaces.Model;

var desc = new ModelDescription();
desc.AddLayer<GridLayer>();
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

var task = SimulationStarter.Start(desc, config);
var loopResults = task.Run();
// Feedback to user that simulation run was successful
Console.WriteLine($"Simulation execution finished after {loopResults.Iterations} steps");