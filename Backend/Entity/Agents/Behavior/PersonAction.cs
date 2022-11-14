using CitySim.Backend.Entity.Structures;
using CitySim.Backend.World;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior;

public enum ActionType
{
    Sleep,
    Eat,
    BuildHouse
}
    public enum ActionResult
    {
        Executed,
        WaitingInQueue,
    }

public record PersonAction(ActionType Type, Position TargetPosition, Person Person)
{
    public override string ToString()
    {
        return $"{Type} at ({TargetPosition})";
    }
    
    public ActionResult Execute()
    {
        return Type switch
        {
            ActionType.Eat => Eat(Person, TargetPosition),
            ActionType.Sleep => Sleep(Person, TargetPosition),
            ActionType.BuildHouse => BuildHouse(Person, TargetPosition),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private ActionResult Sleep(Person personActionPerson, Position personActionTargetPosition)
    {
        personActionPerson.Needs.Sleepiness = 1;
        return ActionResult.Executed;
    }

    private ActionResult Eat(Person person, Position position)
    {
        if (WorldLayer.Instance.Structures[position] is Restaurant restaurant)
        {
            if (restaurant.TryEat(Person))
            {
                person.Needs.Hunger = 1;
                return ActionResult.Executed;
            }
        }

        return ActionResult.WaitingInQueue;
    }

    private ActionResult BuildHouse(Person person, Position targetPosition)
    {
        WorldLayer.Instance.InsertStructure(new House{Position = targetPosition});
        return ActionResult.Executed;
    }
}