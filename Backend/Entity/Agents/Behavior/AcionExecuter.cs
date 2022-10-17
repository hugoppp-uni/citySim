using Mars.Interfaces.Environments;
using MQTTnet.Server.Internal;

namespace CitySim.Backend.Entity.Agents.Behavior;

public static class ActionExecuter
{
    public enum Result
    {
        Executed,
        NotExeceuted,
    }

    public static Result Execute(this PersonAction personAction)
    {
        return personAction.Type switch
        {
            ActionType.Eat => Eat(personAction.Person, personAction.TargetPosition),
            ActionType.Sleep => Sleep(personAction.Person, personAction.TargetPosition),
            ActionType.BuildHouse => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static Result Sleep(Person personActionPerson, Position personActionTargetPosition)
    {
        personActionPerson.Needs.Sleepiness = 1;
        return Result.Executed;
    }

    private static Result Eat(Person person, Position position)
    {
        person.Needs.Hunger = 1;
        return Result.Executed;
    }
}