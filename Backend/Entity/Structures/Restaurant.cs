using CitySim.Backend.Entity.Agents;
using CitySim.Backend.World;

namespace CitySim.Backend.Entity.Structures;

//note this can be simplified to a way more performant ticket based version if performance is needed
//one counter for valid tickets, increases by capacity each tick (all tickets < counter are valid for this tick)
//one counter for last generated tickets, increased when enqueuing with min = valid ticket counter
//action should be handled if a given ticket < valid ticket counter
//restriction: tickets must always be turned in in the tick in which they are valid (action can't be skipped)
public class Restaurant : Structure
{
    private const int MaxCapacityPerTick = 1;
    private int _capacityLeft = MaxCapacityPerTick;

    private Queue<Person> _queue = new();
    private HashSet<Person> _queuedForThisTick = new();

    private long _lastTick;

    public bool TryEat(Person person)
    {
        //lazy Tick
        if (_lastTick != WorldLayer.Instance.Context.CurrentTick)
        {
            _lastTick = WorldLayer.Instance.Context.CurrentTick;
            Tick();
        }

        if (_queuedForThisTick.Contains(person))
            return true;

        if (_queue.Contains(person)) //O(n)
            return false;

        lock (this)
        {
            if (_capacityLeft <= 0)
            {
                _queue.Enqueue(person);
                return false;
            }

            _capacityLeft--;
            return true;
        }
    }

    private void Tick()
    {
        _queuedForThisTick.Clear();
        _capacityLeft = Math.Max(0, MaxCapacityPerTick - _queue.Count);
        for (int i = 0; i < Math.Min(_queue.Count, MaxCapacityPerTick); i++)
        {
            if (_queue.Count > 0)
                _queuedForThisTick.Add(_queue.Dequeue());
        }
    }
}