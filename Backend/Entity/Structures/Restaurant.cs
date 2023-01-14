using CitySim.Backend.Entity.Agents;
using CitySim.Backend.World;
using System.Collections.Concurrent;
using Mars.Interfaces.Agents;

namespace CitySim.Backend.Entity.Structures;

//note this can be simplified to a way more performant ticket based version if performance is needed
//one counter for valid tickets, increases by capacity each tick (all tickets < counter are valid for this tick)
//one counter for last generated tickets, increased when enqueuing with min = valid ticket counter
//action should be handled if a given ticket < valid ticket counter
//restriction: tickets must always be turned in in the tick in which they are valid (action can't be skipped)
public class Restaurant : Structure , ITickClient
{
    public int MaxCapacityPerTick { get; private set; }= 1;
    private int _capacityLeft;

    private ConcurrentQueue<Person> _queue = new();
    public IReadOnlyCollection<Person> Queue => _queue;
    private double _usageScore = - 4;
    public double UsageScore => (-Math.Abs(2.0 * _usageScore)) / (10.0 + Math.Abs(_usageScore))+1;
    private HashSet<Person> _queuedForThisTick = new();

    private long _lastTick;

    public Restaurant()
    {
        _capacityLeft = MaxCapacityPerTick;
        WorldLayer.Instance.RegisterAgent.Invoke(WorldLayer.Instance, this);
    }

    public bool TryEat(Person person)
    {
        lock (this)
        {
            //lazy Tick
            /*if (_lastTick != WorldLayer.Instance.Context.CurrentTick)
            {
                Tick();
                _lastTick = WorldLayer.Instance.Context.CurrentTick;
            }*/

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
            _usageScore++;
            return true;
        }
    }

    public void Tick()
    {
        lock (this)
        {
            _usageScore = (0.95 * _usageScore);
            _usageScore += _queue.Count;
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
            _usageScore -= _capacityLeft * 0.2;
        }
    }
}