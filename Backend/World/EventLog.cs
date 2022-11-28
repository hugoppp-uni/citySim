using System.Collections;
using CircularBuffer;

namespace CitySim.Backend.World;

public readonly record struct EventLogEntry(string Log, long tick);
public class EventLog : IEnumerable<EventLogEntry>
{
    private readonly CircularBuffer<EventLogEntry> _buffer = new(10);

    public void Log(string log)
    {
        lock (_buffer)
            _buffer.PushFront(new EventLogEntry(log, WorldLayer.CurrentTick));
    }

    public IEnumerator<EventLogEntry> GetEnumerator()
    {
        lock (_buffer)
        {
            return _buffer.GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}