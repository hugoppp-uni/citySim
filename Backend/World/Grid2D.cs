using System.Collections;
using Mars.Common.Collections;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.World;

public class Grid2D<T> : IEnumerable<T>
{
    public Grid2D(int xSize, int ySize)
    {
        _array = new T[xSize, ySize];
    }

    private readonly T?[,] _array;
    public readonly K2DTree<T> Kd = new();
    public int YSize => _array.GetLength(1);

    public int XSize => _array.GetLength(0);

    public void Add<T2>(T2 structure) where T2 : T, IPositionable
    {
        Add(structure, structure.Position.PositionArray);
        Kd.Add(structure.Position, structure);
    }

    public void Add(T structure, double[] pos)
    {
        Kd.Add(pos, structure);
        _array[(int)pos[0], (int)pos[1]] = structure;
    }

    public T? this[int x, int y]
    {
        get
        {
            if (x >= XSize || y >= YSize || x < 0 || y < 0)
                return default;
            return _array[x, y];
        }
        set
        {
            if (value != null)
                Add(value, new double[] { x, y });
        }
    }

    public T? this[Position position] => this[(int)position.X, (int)position.Y];

    public IEnumerable<T> GetAdjecent(int x, int y)
    {
        return new T?[]
        {
            this[x - 1, y],
            this[x + 1, y],
            this[x, y - 1],
            this[x, y + 1]
        }.Where(o => o is not null)!;
    }


    public IEnumerator<T> GetEnumerator() =>
        _array.Cast<T?>().Where(structure => structure is not null).GetEnumerator()!;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}