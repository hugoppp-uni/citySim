using System.Collections;
using CircularBuffer;

namespace CitySim.Backend.World;

public readonly record struct EventLogEntry(string Log, long tick);

public class EventLog
{
    public const int Capacity = 10;
    private readonly CircularBuffer<EventLogEntry> _buffer = new(Capacity);

    public void Log(string log)
    {
        lock (_buffer)
            _buffer.PushFront(new EventLogEntry(log, WorldLayer.CurrentTick));
    }

    public void WriteToArray(EventLogEntry[] arr)
    {
        int i = 0;
        lock (_buffer)
        {
            foreach (var eventLogEntry in _buffer)
            {
                arr[i++] = eventLogEntry;
            }
        }
    }
}