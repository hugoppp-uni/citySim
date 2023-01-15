using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.World;

namespace Plugins;

// This is an example for implementing IMind.
// Duplicate this file and change the name of the class to your group name.
public class FahrerfluchtMind : IMind
{
    public FahrerfluchtMind()
    {
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState, Distances distances, double wellBeing)
    {
        ticker++;

        if (personNeeds.Sleepiness < 0.4)
        {
            return ActionType.Sleep;
        }

        if (personNeeds.Hunger < 0.5)
        {
            return ActionType.Eat;
        }

        if (restaurantLikelyHood > 1.0 || distances.GetDistanceToNearestRestaurant() > 6 || globalState.RestaurantScoreAverage < 0.3)
        {
            if ((new Random().Next(0, 7)) == 0 || globalState.RestaurantScoreAverage < 0.1 || distances.GetDistanceToNearestRestaurant() > 6)
            {
                return ActionType.BuildRestaurant;
            }
            restaurantLikelyHood -= 1.0;
        }

        if ((houseLikelyHood > 1.0 || globalState.Housing < 1.5) && (new Random().Next(0, 9)) == 0)
        {
            houseLikelyHood = 0.0;
            return ActionType.BuildHouse;
        }

        return ActionType.Work;
    }

    public void LearnFromDeath(ActionType neededActionToSurvive)
    {
        if (neededActionToSurvive == ActionType.Sleep)
        {
            houseLikelyHood += 0.5;
        }
        else if (neededActionToSurvive == ActionType.Eat)
        {
            restaurantLikelyHood += 0.1;
        }
    }

    private int ticker = 0;
    private double restaurantLikelyHood = 1.0;
    private double houseLikelyHood = 1.0;
}