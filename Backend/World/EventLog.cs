using System.Collections;
using CircularBuffer;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Util;

namespace CitySim.Backend.World;

public readonly record struct EventLogEntry(string Log, Person Person, long Tick);

public class EventLog
{
    public const int Capacity = 10;
    private readonly CircularBuffer<EventLogEntry> _buffer = new(Capacity);

    public void Log(string log, Person person)
    {
        lock (_buffer)
            _buffer.PushFront(new EventLogEntry(log, person, WorldLayer.CurrentTick));
    }

    public int WriteToArray(EventLogEntry[] arr)
    {
        lock (_buffer)
        {
            return _buffer.WriteToArray(arr);
        }
    }
}