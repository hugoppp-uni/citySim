using CitySim.Backend.Agents;
using CitySim.Backend.Entity;
using Mars.Common.Core.Random;
using Mars.Components.Environments.Cartesian;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;

namespace CitySim.Backend.World;

public class GridLayer : AbstractLayer
{
    public CollisionEnvironment<Person, IObstacle> CollisionEnvironment { get; set; } = new();
    private IAgentManager AgentManager { get; set; }
    public List<Structure> Structures = new();

    const int MAX_X = 10;
    const int MAX_Y = 10;

    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

        AgentManager = layerInitData.Container.Resolve<IAgentManager>();
        CollisionEnvironment.BoundingBox = new BoundingBox(0, 0, MAX_X, MAX_Y);

        // Create and register objects of type MyAgentType.
        AgentManager.Spawn<Person, GridLayer>().Take(1).ToList();

        Structures.Add( AgentManager.Spawn<House, GridLayer>(assignment: house => house.Position = new Position(5, 3)).First());
        Structures.Add( AgentManager.Spawn<House, GridLayer>(assignment: house => house.Position = new Position(4, 3)).First());
        Structures.Add( AgentManager.Spawn<House, GridLayer>(assignment: house => house.Position = new Position(3, 3)).First());
        Structures.Add( AgentManager.Spawn<House, GridLayer>(assignment: house => house.Position = new Position(3, 4)).First());
        Structures.Add( AgentManager.Spawn<House, GridLayer>(assignment: house => house.Position = new Position(3, 5)).First());

        return true;
    }

    public void Kill(Person person)
    {
        CollisionEnvironment.Remove(person);
        UnregisterAgent.Invoke(this, person);
    }

    public void RemoveObstacle(IObstacle obstacle)
    {
        CollisionEnvironment.Remove(obstacle);
    }


    public Position RandomPosition()
    {
        var random = RandomHelper.Random;
        return Position.CreatePosition(random.Next(MAX_X - 1), random.Next(MAX_Y - 1));
    }
}