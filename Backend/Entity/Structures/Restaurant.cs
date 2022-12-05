using CitySim.Backend.Entity.Agents;
using CitySim.Backend.World;
using System.Collections.Concurrent;

namespace CitySim.Backend.Entity.Structures;

//note this can be simplified to a way more performant ticket based version if performance is needed
//one counter for valid tickets, increases by capacity each tick (all tickets < counter are valid for this tick)
//one counter for last generated tickets, increased when enqueuing with min = valid ticket counter
//action should be handled if a given ticket < valid ticket counter
//restriction: tickets must always be turned in in the tick in which they are valid (action can't be skipped)
public class Restaurant : Structure
{
    public int MaxCapacityPerTick { get; private set; }= 1;
    private int _capacityLeft;

    private ConcurrentQueue<Person> _queue = new();
    public IReadOnlyCollection<Person> Queue => _queue;
    private HashSet<Person> _queuedForThisTick = new();

    private long _lastTick;

    public Restaurant()
    {
        _capacityLeft = MaxCapacityPerTick;
    }

    public bool TryEat(Person person)
    {
        lock (this)
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
        lock (this)
        {
            _queuedForThisTick.Clear();
            _capacityLeft = MaxCapacityPerTick;
            while(_queue.Count > 0 && _queuedForThisTick.Count < MaxCapacityPerTick)
            {
                if (_queue.TryDequeue(out var person) && person.IsAlive)
                {
                    _queuedForThisTick.Add(person);
                    _capacityLeft--;
                }
            }
        }
    }
}