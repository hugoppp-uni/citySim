using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior;

public class PersonRecollection
{
    private readonly Dictionary<ActionType, List<Position>> _actionPosition = new();

    public PersonRecollection()
    {
        foreach (var actionType in Enum.GetValues<ActionType>())
        {
            _actionPosition[actionType] = new();
        }
    }

    public IEnumerable<Position> ResolvePosition(ActionType nextActionType)
    {
        return _actionPosition.TryGetValue(nextActionType, out var positions)
            ? positions
            : Enumerable.Empty<Position>();
    }

    public void Add(ActionType actionType, Position position)
    {
        _actionPosition[actionType].Add(position);
    }
}