using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior;

public class PersonRecollection
{
    private Dictionary<ActionType, List<Position>> _actionPosition = new();

    public PersonRecollection()
    {
        foreach (var actionType in Enum.GetValues<ActionType>())
        {
            _actionPosition[actionType] = new();
        }

        _actionPosition[ActionType.Eat].Add(new Position(1, 1));
        _actionPosition[ActionType.Sleep].Add(new Position(9, 9));
    }

    public IEnumerable<Position> ResolvePosition(ActionType nextActionType)
    {
        return _actionPosition.TryGetValue(nextActionType, out var positions)
            ? positions
            : Enumerable.Empty<Position>();
    }
}