using CitySim.Backend.World;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity;

//should be an IEntity
public abstract class Structure : IPositionableEntity
{
    public virtual void Init(WorldLayer worldLayer)
    {
        WorldLayer = worldLayer;
        if (Position is null)
            throw new Exception("Position not set");

        worldLayer.InsertStructure(this);
    }

    protected WorldLayer WorldLayer { get; set; } = null!; //Init()

    public Position Position { get; set; }

    public Guid ID { get; set; }


    public void Tick()
    {
    }
}