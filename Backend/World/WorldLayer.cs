using CitySim.Backend.Entity;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Util;
using Mars.Common.Core.Random;
using Mars.Common.IO.Csv;
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
        new(10, 10, true) { IsDiscretizePosition = true };

    public readonly List<Structure> Structures = new();

    private const int MaxX = 20;
    private const int MaxY = 20;
    private readonly float[,] _pathFindingTileMap = new float[MaxX, MaxY];
    private readonly PathFindingGrid _pathFindingGrid;

    public WorldLayer()
    {
        for (int i = 0; i < MaxX; i++)
        for (int j = 0; j < MaxY; j++)
            _pathFindingTileMap[i, j] = 1; //walkable
        _pathFindingGrid = new PathFindingGrid(_pathFindingTileMap);
    }

    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

        var agentManager = layerInitData.Container.Resolve<IAgentManager>();

        SpawnBuildings();

        agentManager.Spawn<Person, WorldLayer>().Take(10).ToList();

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
        return Position.CreatePosition(random.Next(MaxX - 1), random.Next(MaxY - 1));
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
        if (Structures.GetType() == typeof(Street))
            _pathFindingTileMap[(int)structure.Position.X, (int)structure.Position.Y] = 4;
        else
            _pathFindingTileMap[(int)structure.Position.X, (int)structure.Position.Y] = 100000;
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
            this.GridEnvironment.Entities.Count((it) => it is Person),
            Structures.Count((it) => it is House),
            Structures.Count((it) => it is Restaurant)
        );
    }
}