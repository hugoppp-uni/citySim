using Mars.Interfaces.Environments;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.World;

namespace CitySim.Backend.Entity.Agents.Behavior.Actions;

internal record BuildHouseAction(ActionType Type, Position TargetPosition, Person Person) : PersonAction(Type, TargetPosition, Person)
{
    public override ActionResult Execute()
    {
        WorldLayer.Instance.InsertStructure(new House{Position = TargetPosition});
        return ActionResult.Executed;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        WorldLayer.Instance.BuildPositionEvaluator.ResetHousingScore(TargetPosition);
    }
}