using System.Collections;
using CitySim.Backend.Entity;
using Mars.Common.Collections;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.World;

public class StructureCollection : IEnumerable<Structure>
{
    public StructureCollection(int xSize, int ySize)
    {
        _array = new Structure[xSize, ySize];
    }

    private readonly Structure?[,] _array;
    public readonly K2DTree<Structure> Kd = new();
    public int YSize => _array.GetLength(1);

    public int XSize => _array.GetLength(0);

    public void Add(Structure structure)
    {
        Kd.Add(structure.Position, structure);
        _array[(int)structure.Position.X, (int)structure.Position.Y] = structure;
    }

    public Structure? this[int x, int y]
    {
        get
        {
            if (x >= XSize || y >= YSize || x < 0 || y < 0)
                return null;
            return _array[x, y];
        }
    }

    public Structure? this[Position position]
    {
        get => this[(int)position.X, (int)position.Y];
    }


    public IEnumerator<Structure> GetEnumerator() =>
        _array.Cast<Structure?>().Where(structure => structure is not null).GetEnumerator()!;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}