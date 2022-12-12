using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.World;

namespace Plugins;

// This is an example for implementing IMind.
// Duplicate this file and change the name of the class to your group name.
public class ExampleUserMind : IMind
{
    public ExampleUserMind()
    {
        throw new NotImplementedException();
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState, Distances distances, double wellBeing)
    {
        throw new NotImplementedException();
    }

    public double GetWellBeing(PersonNeeds personNeeds, GlobalState globalState)
    {
        throw new NotImplementedException();
    }

    public void LearnFromDeath(ActionType neededActionToSurvive)
    {
        throw new NotImplementedException();
    }
}