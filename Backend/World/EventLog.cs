using System.Collections;
using CircularBuffer;
using CitySim.Backend.Entity.Agents;

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
        int i = 0;
        lock (_buffer)
        {
            foreach (var eventLogEntry in _buffer)
            {
                arr[i++] = eventLogEntry;
            }
        }

        return i;
    }
}