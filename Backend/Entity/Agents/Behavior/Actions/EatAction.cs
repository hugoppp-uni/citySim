using Mars.Interfaces.Environments;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.World;

namespace CitySim.Backend.Entity.Agents.Behavior.Actions;

public record EatAction(ActionType Type, Position TargetPosition, Person Person) : PersonAction(Type, TargetPosition, Person)
{
    public override ActionResult Execute()
    {
        if (WorldLayer.Instance.Structures[TargetPosition] is Restaurant restaurant)
        {
            if (restaurant.TryEat(Person))
            {
                Person.Needs.Hunger = 1;
                return ActionResult.Executed;
            }
        }

        return ActionResult.WaitingInQueue;
    }


}