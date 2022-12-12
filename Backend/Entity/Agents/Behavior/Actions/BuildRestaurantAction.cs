using Mars.Interfaces.Environments;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.World;

namespace CitySim.Backend.Entity.Agents.Behavior.Actions;

public record BuildRestaurantAction(ActionType Type, Position TargetPosition, Person Person) : PersonAction(Type, TargetPosition, Person)
{
    public override ActionResult Execute()
    {
        WorldLayer.Instance.InsertStructure(new Restaurant(){Position = TargetPosition});
        return ActionResult.Executed;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        WorldLayer.Instance.BuildPositionEvaluator.ResetRestaurantScore(TargetPosition);
    }
}