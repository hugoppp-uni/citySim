namespace CitySim.Backend;

public class Names
{
    private readonly string[] _names;

    public Names()
    {
        _names = File.ReadAllLines("Resources/Names.txt");
    }

    public string GetRandom()
    {
        return _names[Random.Shared.Next(_names.Length)];
    }
}