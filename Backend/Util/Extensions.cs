namespace CitySim.Backend.Util;

public static class Extensions
{
    public static T RandomEnumValue<T>() where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values[Random.Shared.Next(values.Length)];
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
}