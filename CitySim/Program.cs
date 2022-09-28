using System.Diagnostics;
using CitySim.Agents;
using CitySim.World;
using Mars.Components.Starter;
using Mars.Core.Simulation;
using Mars.Interfaces.Model;
using Mars.Interfaces.Model.Options;

var desc = new ModelDescription();
desc.AddLayer<GridLayer>();
desc.AddAgent<Person, GridLayer>();

var config = new SimulationConfig
{
    SimulationIdentifier = "CitySim",
    ModelDescription = desc,
    Globals = new Globals
    {
        StartPoint = DateTime.Today,
        EndPoint = DateTime.Today.AddDays(1),
        DeltaTUnit = TimeSpanUnit.Hours,
        ShowConsoleProgress = false,
        OutputTarget = OutputTargetType.Csv,
        CsvOptions = new CsvOptions
        {
            OutputPath = "Results",
            
        }
    },
    AgentMappings = new List<AgentMapping>
    {
        new()
        {
            Name = nameof(Person),
            IndividualMapping = new List<IndividualMapping>
            {
                new ()
                {
                    Name = nameof(Person.Hunger),
                    Value = 1 // Each person starts with 1 hunger, just to see that it works
                }
            }
        }
    }
};

var watch = new Stopwatch();

var task = SimulationStarter.Start(desc, config);

watch.Start();
var loopResults = task.Run();
watch.Stop();

// Feedback to user that simulation run was successful
Console.WriteLine($"Simulation execution finished after {loopResults.Iterations} steps and took {watch.ElapsedMilliseconds}ms");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
