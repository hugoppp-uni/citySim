using CitySim.Backend.Entity.Agents.Behavior.Actions;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior;

public enum ActionType
{
    Sleep,
    Eat,
    BuildHouse,
    BuildRestaurant,
    Work
}
    public enum ActionResult
    {
        Executed,
        WaitingInQueue,
        InProgress,
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
            ActionType.Work => new WorkAction(type, targetPosition, person),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}