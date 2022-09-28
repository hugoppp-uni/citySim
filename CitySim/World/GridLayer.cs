using Mars.Components.Environments;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;

namespace CitySim.World;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agents;
using Mars.Components.Layers;
using Mars.Components.Services;
using Mars.Core.Data;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Data;
using Mars.Interfaces.Layers;

public class GridLayer : AbstractLayer
{
    public SpatialHashEnvironment<Person> GridEnvironment { get; private set; } = new (10, 10, true);
    

    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle, UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

        var agentManager = layerInitData.Container.Resolve<IAgentManager>();

        // Create and register objects of type MyAgentType.
        var agents = agentManager.Spawn<Person, GridLayer>().Take(3).ToList();
        
        return true;
    }

    public void RemoveAgent(ITickClient agent)
    {
        this.UnregisterAgent(this, agent);
    }
    

    public Position RandomPosition()
    {
        var random = RandomHelper.Random;
        return Position.CreatePosition(random.Next(GridEnvironment.DimensionX - 1), random.Next(GridEnvironment.DimensionY - 1));
    }
}