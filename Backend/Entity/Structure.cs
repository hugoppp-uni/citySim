using CitySim.Backend.World;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity;

//should be an IEntity
public abstract class Structure : IPositionableEntity
{
    public Position Position { get; set; }

    public Guid ID { get; set; }

    public virtual void PostTick()
    {
        
    }

}