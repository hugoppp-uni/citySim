using Autofac.Builder;
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

public class WorldLayer : AbstractLayer
{
    public SpatialHashEnvironment<IPositionableEntity> GridEnvironment { get; private set; } =
        new(20, 20, true) { IsDiscretizePosition = true };

    public int XSize { get; }
    public int YSize { get; }
    public BuildPositionEvaluator BuildPositionEvaluator;

    public Grid2D<Structure> Structures;
    private PathFindingGrid _pathFindingGrid;
    public readonly EventLog EventLog = new();
    public readonly Names Names = new();

    public static WorldLayer Instance { get; private set; } = null!; //Ctor
    public static long CurrentTick => Instance.Context.CurrentTick;

    public event TwoPersonEventHandler? ReproduceEventHandler;

    public CitySim citySim { set; get; }
    private string[] csv_map;

    public WorldLayer()
    {
        Instance = this;

        csv_map = File.ReadAllLines("Resources/Map.csv");
        XSize = csv_map[0].Length;
        YSize = csv_map.Length;
        float[,] pathFindingTileMap = new float[XSize, YSize];
        for (int i = 0; i < XSize; i++)
        for (int j = 0; j < YSize; j++)
            pathFindingTileMap[i, j] = 1; //walkable
        _pathFindingGrid = new PathFindingGrid(pathFindingTileMap);
        Structures = new Grid2D<Structure>(XSize, YSize);
    }

    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

        var agentManager = layerInitData.Container.Resolve<IAgentManager>();

        SpawnBuildings(csv_map);
        BuildPositionEvaluator = new BuildPositionEvaluator(Structures);
        BuildPositionEvaluator.EvaluateHousingScore();

        agentManager.Spawn<Person, WorldLayer>().ToList();

        return true;
    }


    public void Kill(Person person)
    {
        GridEnvironment.Remove(person);
        UnregisterAgent.Invoke(this, person);

        if (!GridEnvironment.Entities.OfType<Person>().Any())
        {
            citySim.Abort();
        }
    }

    public Position RandomPosition()
    {
        var random = RandomHelper.Random;
        return Position.CreatePosition(random.Next(XSize - 1), random.Next(YSize - 1));
    }

    public Position RandomBuildingPosition()
    {
        return Structures.Skip(Random.Shared.Next(Structures.Count - 1)).First().Position.Copy();
    }

    private void SpawnBuildings(string[] csv)
    {
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
        lock (_pathFindingGrid)
        {
            if (structure.GetType() == typeof(Street))
                _pathFindingGrid.nodes[x, y].price = 0.1f;
            else
                _pathFindingGrid.nodes[x, y].price = 100;
        }

        GridEnvironment.Insert(structure);
        Structures.Add(structure);
    }

    public PathFindingRoute FindRoute(Position position, Position destination)
    {
        return FindRoute(
            (int)position.X, (int)position.Y,
            (int)destination.X, (int)destination.Y
        );
    }

    public PathFindingRoute FindRoute(int x, int y, int x1, int y1)
    {
        lock (_pathFindingGrid)
            return _pathFindingGrid.FindPath(new PathFindingPoint(x, y), new PathFindingPoint(x1, y1));
    }


    public GlobalState GetGlobalState()
    {
        return new GlobalState(
            GridEnvironment.Entities.Count((it) => it is Person),
            Structures.OfType<House>().Sum(house => house.MaxSpaces),
            Structures.OfType<Restaurant>().Sum(restaurant => restaurant.MaxCapacityPerTick)
        );
    }

    public void InvokePersonReproduceHandler(Person p1, Person p2)
    {
        ReproduceEventHandler?.Invoke(p1, p2);
    }
}