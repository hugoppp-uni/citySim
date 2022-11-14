using System.Text.Json;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;

namespace CitySim.Backend.Util;

public static class ModelVisualisation
{
    public static async Task SaveInsight(Model model, decimal stepSize, string name, CancellationToken cancellationToken)
    {
        var parameterCount = (int)model.Layers[0].output_shape.size;
        var data = GenerateDataPool(model, stepSize, cancellationToken).ToArray();
        if (cancellationToken.IsCancellationRequested) return;
        var input = np.stack(
            new NDArray(data, shape: (data.Length / parameterCount, parameterCount)));
        var tensors = model.predict(input, data.Length / parameterCount);
        if (cancellationToken.IsCancellationRequested) return;
        var resultData = tensors[0].numpy();
        var jsonData = new JsonData()
        {
            RowData = resultData.ToArray<float>(),
            RowCount = parameterCount,
            Resolution = (int)Math.Floor(2 / stepSize),
            OutCount = (int)model.Layers[^1].output_shape.size
        };
        await using var fileStream = File.Open($"{name}.json",FileMode.Create);
        await JsonSerializer.SerializeAsync(fileStream, jsonData,options: null, cancellationToken: cancellationToken);
    }

    private static List<float> GenerateDataPool(Model model, decimal stepSize, CancellationToken cancellationToken)
    {
        var parameterCount = (int)model.Layers[0].output_shape.size;
        var list = new List<float>((int)Math.Ceiling(Math.Pow((double)(2 / stepSize) + 1, parameterCount)) *
                                   parameterCount);
        int index;
        var values = new List<decimal>(parameterCount);
        for (var i = 0; i < parameterCount; i++)
        {
            values.Add(-1m);
        }

        var maxSteps = (int)(Math.Pow((double)Math.Floor(2 / stepSize) + 1, parameterCount));
        for (var step = 0; step < maxSteps - 1 && !cancellationToken.IsCancellationRequested; step++)
        {
            list.AddRange(values.Select(it => (float)it));
            values[0] += stepSize;
            index = 0;
            while (values[index] > 1)
            {
                values[index] = -1m;
                index++;
                values[index] += stepSize;
            }
        }

        list.AddRange(values.Select(it => (float)it));
        return list;
    }
}

internal class JsonData
{
    public Array RowData { get; set; }
    public int RowCount { get; set; }
    public int Resolution { get; set; }
    public int OutCount { get; set; }
}