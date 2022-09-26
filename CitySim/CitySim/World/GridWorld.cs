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
using Mars.Interfaces.Data;
using Mars.Interfaces.Layers;

public class GridWorld: AbstractLayer
{
    private long currentTick;

    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle = null,
        UnregisterAgent unregisterAgentHandle = null)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);
        
        var am = layerInitData.Container.Resolve<IAgentManager>();
        var agents = am.Spawn<Person>()!.First();

        return true;
    }

    public override long GetCurrentTick()
    {
        return currentTick;
    }

    public override void SetCurrentTick(long currentStep)
    {
        currentTick = currentStep;
    }
}
