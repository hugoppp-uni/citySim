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
        new(10, 10, true) { IsDiscretizePosition = true };

    public readonly List<Structure> Structures = new();

    private const int MaxX = 10;
    private const int MaxY = 10;
    private readonly float[,] _pathFindingTileMap = new float[MaxX, MaxY];
    private readonly PathFindingGrid _pathFindingGrid;

    public WorldLayer()
    {
        for (int i = 0; i < MaxX; i++)
        for (int j = 0; j < MaxY; j++)
            _pathFindingTileMap[i, j] = 1;
        _pathFindingGrid = new PathFindingGrid(_pathFindingTileMap);
    }

    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

        var agentManager = layerInitData.Container.Resolve<IAgentManager>();

        InsertStructure(new House { Position = new Position(6, 3) });
        InsertStructure(new House { Position = new Position(5, 3) });
        // InsertStructure(new House { Position = new Position(4, 3) });
        InsertStructure(new House { Position = new Position(3, 3) });
        InsertStructure(new House { Position = new Position(3, 4) });
        InsertStructure(new House { Position = new Position(3, 5) });
        InsertStructure(new House { Position = new Position(3, 6) });
        InsertStructure(new House { Position = new Position(4, 5) });
        InsertStructure(new House { Position = new Position(5, 5) });
        InsertStructure(new House { Position = new Position(6, 0) });
        InsertStructure(new House { Position = new Position(6, 1) });
        InsertStructure(new House { Position = new Position(6, 2) });
        
        InsertStructure(new House { Position = new Position(8, 9) });
        InsertStructure(new House { Position = new Position(8, 8) });
        InsertStructure(new House { Position = new Position(8, 7) });
        InsertStructure(new House { Position = new Position(8, 6) });
        InsertStructure(new House { Position = new Position(8, 5) });
        InsertStructure(new House { Position = new Position(8, 4) });
        InsertStructure(new House { Position = new Position(8, 3) });
        InsertStructure(new House { Position = new Position(8, 2) });
        InsertStructure(new House { Position = new Position(8, 1) });
        
        //last one will be sleep location for now
        InsertStructure(new House { Position = new Position(9, 9) });

        agentManager.Spawn<Person, WorldLayer>().Take(1).ToList();

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

    public void InsertStructure(Structure structure)
    {
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
}