using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior.Actions;

internal record WorkAction(ActionType Type, Position TargetPosition, Person Person) : PersonAction(Type, TargetPosition, Person)
{
    private int _leftDuration = 4;
    public override ActionResult Execute()
    {
        _leftDuration--;
        if (_leftDuration == 0)
        {
            Person.Needs.Money += 10;
        }
        return _leftDuration <= 0 ? ActionResult.Executed : ActionResult.InProgress;
    }
}