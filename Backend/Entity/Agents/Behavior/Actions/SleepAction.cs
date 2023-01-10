using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior.Actions;

internal record SleepAction(ActionType Type, Position TargetPosition, Person Person) : PersonAction(Type, TargetPosition, Person)
{
    public override ActionResult Execute()
    {
        Person.Needs.Sleepiness = 1;
        return ActionResult.Executed;
    }
}