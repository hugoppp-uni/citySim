namespace CitySim.Backend.Util;

public static class Extensions
{
    public static T RandomEnumValue<T>() where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values[Random.Shared.Next(values.Length)];
    }
}