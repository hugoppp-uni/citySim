using CitySim.Backend.Entity;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Util;
using Mars.Common.Core.Random;
using Mars.Components.Environments;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using NesScripts.Controls.PathFind;

namespace CitySim.Backend.World;

public delegate void TwoPersonEventHandler(Person personA, Person personB);

public class WorldLayer : AbstractLayer, ISteppedActiveLayer
{
    public SpatialHashEnvironment<IPositionableEntity> GridEnvironment { get; private set; } =
        new(20, 20, true) { IsDiscretizePosition = true };

    public const int XSize = 20;
    public const int YSize = 20;
    public readonly BuildPositionEvaluator BuildPositionEvaluator;

    public readonly StructureCollection Structures = new(XSize, YSize);
    private readonly float[,] _pathFindingTileMap = new float[XSize, YSize];
    private readonly PathFindingGrid _pathFindingGrid;
    
    public static WorldLayer Instance { get; private set; } = null!; //Ctor

    public event TwoPersonEventHandler? PersonCellDivision;

    public WorldLayer()
    {
        Instance = this;
        
        for (int i = 0; i < XSize; i++)
        for (int j = 0; j < YSize; j++)
            _pathFindingTileMap[i, j] = 1; //walkable
        _pathFindingGrid = new PathFindingGrid(_pathFindingTileMap);
        BuildPositionEvaluator = new BuildPositionEvaluator(Structures);
    }

    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

        var agentManager = layerInitData.Container.Resolve<IAgentManager>();

        SpawnBuildings();

        agentManager.Spawn<Person, WorldLayer>().ToList();


        //todo this should be moved 
        BuildPositionEvaluator.EvaluateHousingScore();

        return true;
    }


    public void Kill(Person person)
    {
        GridEnvironment.Remove(person);
        UnregisterAgent.Invoke(this, person);
    }

    public Position RandomPosition()
    {
        var random = RandomHelper.Random;
        return Position.CreatePosition(random.Next(XSize - 1), random.Next(YSize - 1));
    }

    private void SpawnBuildings()
    {
        var csv = File.ReadAllLines("Resources/Map.csv");
        for (var x = 0; x < csv.Length; x++)
        {
            var strings = csv[x].Split(";");
            for (var y = 0; y < strings.Length; y++)
            {
                char c = strings[y][0];
                if (c == ' ') continue;

                InsertStructure(c switch
                {
                    'R' => new Restaurant { Position = new(x, y) },
                    'H' => new House { Position = new(x, y) },
                    '+' => new Street { Position = new(x, y) },
                    _ => throw new Exception()
                });
            }
        }
    }

    public void InsertStructure(Structure structure)
    {
        int x = (int)structure.Position.X;
        int y = (int)structure.Position.Y;
        if (structure.GetType() == typeof(Street))
            _pathFindingTileMap[x, y] = 0.1f;
        else
            _pathFindingTileMap[x, y] = 100000;
        lock (_pathFindingGrid)
            _pathFindingGrid.UpdateGrid(_pathFindingTileMap);
        GridEnvironment.Insert(structure);
        Structures.Add(structure);
    }

    public PathFindingRoute FindRoute(Position position, Position plannedActionTargetPosition)
    {
        lock (_pathFindingGrid)
            return new PathFindingRoute(_pathFindingGrid.FindPath(
                new PathFindingPoint((int)position.X, (int)position.Y),
                new PathFindingPoint((int)plannedActionTargetPosition.X, (int)plannedActionTargetPosition.Y),
                Pathfinding.DistanceType.Manhattan));
    }


    public GlobalState GetGlobalState()
    {
        return new GlobalState(
            GridEnvironment.Entities.Count((it) => it is Person),
            Structures.OfType<House>().Count(),
            Structures.OfType<Restaurant>().Count()
        );
    }

    public void Tick()
    {
    }

    public void PreTick()
    {
    }

    Random _rng = new Random();

    public void PostTick()
    {
        foreach (var structure in Structures)
        {
            structure.PostTick();
        }

        //just for testing, please remove once splitting is implemented in the backend
        if (_rng.Next() < int.MaxValue / 4)
        {
            var persons = GridEnvironment.Entities.OfType<Person>().ToList();

            PersonCellDivision?.Invoke(persons[_rng.Next(persons.Count - 1)], persons[_rng.Next(persons.Count - 1)]);
        }
    }
}