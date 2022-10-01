using CitySim.Backend.Agents;
using Mars.Components.Environments;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using Mars.Numerics.Statistics;

namespace CitySim.Backend.World;

public class GridLayer : AbstractLayer
{
    public SpatialHashEnvironment<Person> GridEnvironment { get; private set; } = new (10, 10, true);


    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle, UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

        var agentManager = layerInitData.Container.Resolve<IAgentManager>();

        // Create and register objects of type MyAgentType.
        var agents = agentManager.Spawn<Person, GridLayer>().Take(1).ToList();

        return true;
    }
    

    public Position RandomPosition()
    {
        var random = RandomHelper.Random;
        return Position.CreatePosition(random.Next(GridEnvironment.DimensionX - 1), random.Next(GridEnvironment.DimensionY - 1));
    }
}