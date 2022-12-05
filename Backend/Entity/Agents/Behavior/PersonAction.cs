using CitySim.Backend.Entity.Agents.Behavior.Actions;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.World;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior;

public enum ActionType
{
    Sleep,
    Eat,
    BuildHouse,
    BuildRestaurant
}
    public enum ActionResult
    {
        Executed,
        WaitingInQueue,
    }

public abstract record PersonAction(ActionType Type, Position TargetPosition, Person Person)
{
    public abstract ActionResult Execute();

    public virtual void CleanUp()
    {
        // do nothing
    }
    
    public override string ToString()
    {
        return $"{Type} at ({TargetPosition})";
    }
    
    public static PersonAction Create(ActionType type, Position targetPosition, Person person)
    {
        return type switch
        {
            ActionType.Eat => new EatAction(type, targetPosition, person),
            ActionType.Sleep => new SleepAction(type, targetPosition, person),
            ActionType.BuildHouse => new BuildHouseAction(type, targetPosition, person),
            ActionType.BuildRestaurant => new BuildRestaurantAction(type, targetPosition, person),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}