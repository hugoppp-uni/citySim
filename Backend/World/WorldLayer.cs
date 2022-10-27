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

public class WorldLayer : AbstractLayer
{
    public SpatialHashEnvironment<IPositionableEntity> GridEnvironment { get; private set; } =
        new(20, 20, true) { IsDiscretizePosition = true };

    public const int XSize = 20;
    public const int YSize = 20;
    public readonly BuildPositionEvaluator BuildPositionEvaluator;

    public readonly Grid2D<Structure> Structures = new(XSize, YSize);
    private readonly float[,] _pathFindingTileMap = new float[XSize, YSize];
    private readonly PathFindingGrid _pathFindingGrid;
    
    public static WorldLayer Instance { get; private set; } = null!; //Ctor
    public static long CurrentTick => Instance.Context.CurrentTick;

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
        BuildPositionEvaluator.EvaluateHousingScore();

        agentManager.Spawn<Person, WorldLayer>().Take(30).ToList();

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
            _pathFindingTileMap[x, y] = 100;
        lock (_pathFindingGrid)
            _pathFindingGrid.UpdateGrid(_pathFindingTileMap);
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
            Structures.OfType<House>().Count(),
            Structures.OfType<Restaurant>().Count()
        );
    }
}