using Tensorflow.NumPy;

namespace CitySim.Backend.Util;

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
}