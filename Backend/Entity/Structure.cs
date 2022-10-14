using System.Drawing;
using CitySim.Backend.World;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.HPRtree;
using Position = Mars.Interfaces.Environments.Position;

namespace CitySim.Backend.Entity;

public abstract class Structure : IAgent<GridLayer>, IPositionable, IObstacle
{
    public virtual void Init(GridLayer landscape)
    {
        Landscape = landscape;
        if (Position is null)
            throw new Exception("Position not set");
        InsertIntoEnv();

        Landscape = landscape;
    }

    protected GridLayer Landscape { get; set; }

    public Position Position { get; set; }

    public Guid ID { get; set; }


    public virtual void Tick()
    {
        // do nothing
    }
    public bool IsRoutable(ICharacter character) => false;


    public virtual CollisionKind? HandleCollision(ICharacter character)
    {
        return CollisionKind.Pass;
    }

    public virtual VisibilityKind? HandleExploration(ICharacter explorer)
    {
        return VisibilityKind.Opaque;
    }

    protected virtual void InsertIntoEnv()
    {
        Landscape.CollisionEnvironment.Insert(this, new Polygon(new LinearRing(new[]
        {
            new Coordinate(Position.X + 0.5, Position.Y + 0.5),
            new Coordinate(Position.X - 0.5, Position.Y + 0.5),
            new Coordinate(Position.X - 0.5, Position.Y - 0.5),
            new Coordinate(Position.X + 0.5, Position.Y - 0.5),
            new Coordinate(Position.X + 0.5, Position.Y + 0.5)
        })));
    }
}