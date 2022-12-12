using CitySim.Backend.World;

namespace CitySim.Backend.Entity.Agents.Behavior;

public class UserMind : IMind
{
    public UserMind()
    {
        throw new NotImplementedException();
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState, Distances distances)
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