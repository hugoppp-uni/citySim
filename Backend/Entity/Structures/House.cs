using CitySim.Backend.Entity.Agents;

namespace CitySim.Backend.Entity.Structures;

public class House : Structure
{
    public int FreeSpaces { get; private set; } = 3;
    private readonly List<Person> _inhabitants;
    public IReadOnlyList<Person> Inhabitants => _inhabitants;

    public House()
    {
        _inhabitants = new List<Person>(FreeSpaces);
    }

    public void AddInhabitant(Person p)
    {
        _inhabitants.Add(p);
    }
}