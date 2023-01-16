using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.World;

namespace Plugins;

public class SnakeMind : IMind
{
    public SnakeMind()
    {
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState, Distances distances, double wellBeing)
    {
        ticker++;

        if (personNeeds.Sleepiness < 0.3)
        {
            return ActionType.Sleep;
        }

        if (personNeeds.Hunger < 0.6)
        {
            return ActionType.Eat;
        }

        if (restaurantLikelyHood > 0.8 || distances.GetDistanceToNearestRestaurant() > 7 || globalState.RestaurantScoreAverage < 0.5)
        {
            if ((new Random().Next(0, 7)) == 0 || globalState.RestaurantScoreAverage < 0.5 || distances.GetDistanceToNearestRestaurant() > 7)
            {
                return ActionType.BuildRestaurant;
            }
            restaurantLikelyHood -= 1.0;
        }

        if ((houseLikelyHood > 0.8|| globalState.Housing < 1.3) && (new Random().Next(0, 9)) == 0)
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
            houseLikelyHood += 0.6;
        }
        else if (neededActionToSurvive == ActionType.Eat)
        {
            restaurantLikelyHood += 0.2;
        }
    }

    private int ticker = 0;
    private double restaurantLikelyHood = 1.0;
    private double houseLikelyHood = 1.0;
}
