using CitySim.Backend.Entity.Agents;

namespace CitySim.Backend.Entity.Structures;

public class House : Structure
{
    public int FreeSpaces => MaxSpaces - _inhabitants.Count;
    public int MaxSpaces { get; private set; } = 3;
    private readonly List<Person> _inhabitants;
    public IReadOnlyList<Person> Inhabitants => _inhabitants;

    public House()
    {
        _inhabitants = new List<Person>(MaxSpaces);
    }

    public void AddInhabitant(Person p)
    {
        lock (_inhabitants)
        {
            if (FreeSpaces == 0)
                throw new InvalidOperationException("No free spaces available");
            _inhabitants.Add(p);
        }
    }

    public void RemoveInhabitant(Person p)
    {
        lock (_inhabitants)
            _inhabitants.Remove(p);
    }
}