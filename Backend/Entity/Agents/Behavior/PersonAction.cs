using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior;

public enum ActionType
{
    Sleep,
    Eat,
    BuildHouse
}

public record PersonAction(ActionType Type, Position TargetPosition, Person Person)
{
    public override string ToString()
    {
        return $"{Type} at ({TargetPosition})";
    }
}