namespace CitySim.Backend.Util;

public readonly ref struct Safe2DArrayView<T>  where T : struct
{
    private readonly T[,] _array;

    public Safe2DArrayView(T[,] array) => _array = array;

    public T? this[int x, int y]
    {
        get
        {
            if (x >= _array.GetLength(0) || y >= _array.GetLength(1) || x < 0 || y < 0)
                return null;
            return _array[x, y];
        }
    }
}