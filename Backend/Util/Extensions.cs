namespace CitySim.Backend.Util;

public static class Extensions
{
    public static T RandomEnumValue<T>()
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Shared.Next(values.Length));
    }
}