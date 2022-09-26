using CitySim.Agents;
using CitySim.World;
using Mars.Components.Starter;
using Mars.Core.Simulation;
using Mars.Interfaces.Model;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var desc = new ModelDescription();
desc.AddLayer<GridWorld>();
desc.AddAgent<Person, GridWorld>();

var config = new SimulationConfig
{
    SimulationIdentifier = "CitySim",
    ModelDescription = desc,
    Globals = new Globals
    {
        StartPoint = DateTime.Now,
        EndPoint = DateTime.Now.AddHours(1),
        DeltaTUnit = TimeSpanUnit.Seconds,
        ShowConsoleProgress = true,
        OutputTarget = OutputTargetType.Csv,
    }
};

var app = SimulationStarter.BuildApplication(desc, config);

var simulation = app.Resolve<ISimulation>();

await simulation.StartSimulationAsync(desc, config);

Console.ReadKey();
