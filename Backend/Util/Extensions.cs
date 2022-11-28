using Tensorflow.NumPy;

namespace CitySim.Backend.Util;
using CircularBuffer;
public static class Extensions
{
    public static T RandomEnumValue<T>() where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values[Random.Shared.Next(values.Length)];
    }
    
    public static string JoinDataToString(this NDArray ary)
    {
        return string.Join(",", ary.ToArray().Select(it => it.ToString()));
    }
    
   public static void Deconstruct<T>(this T?[] items, out T? t0)
    {
        t0 = items.Length > 0 ? items[0] : default(T);
    }

    public static void Deconstruct<T>(this T?[] items, out T? t0, out T? t1)
    {
        t0 = items.Length > 0 ? items[0] : default(T);
        t1 = items.Length > 1 ? items[1] : default(T);
    }
    
    public static int WriteToArray<T>(this CircularBuffer<T> buffer, T[] arr)
    {
        var i = 0;
        lock (buffer)
        {
            foreach (var eventLogEntry in buffer)
            {
                arr[i++] = eventLogEntry;
            }
        }

        return i;
    }
}