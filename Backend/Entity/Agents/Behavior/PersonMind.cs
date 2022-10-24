using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;

namespace CitySim.Backend.Entity.Agents.Behavior;

using System;
using System.Collections.Generic;
using World;

public class PersonMind : IMind
{
    private const float LEARNING_RATE = 0.01f;
    private static Model _model = null!;
    private double _i, _c;
    private PredictionData? _lastIndividualPrediction;
    private readonly Queue<PredictionData> _lastCollectivePredictions = new();

    public static void Init(string weightsFile)
    {
        _model = BuildModel(weightsFile);
    }

    public PersonMind(double i, double c)
    {
        if (_model == null)
        {
            throw new InvalidOperationException("PersonMind is not Initialized");
        }

        _i = i;
        _c = c;
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState)
    {
        if (_lastIndividualPrediction != null)
        {
            Evaluate(personNeeds, globalState);
        }

        var input = new[] { personNeeds.AsArray(), globalState.AsArray() };
        var output = _model.predict(new Tensor(input, new Shape(2)))[0];
        var data = new PredictionData(globalState, personNeeds, output);
        _lastCollectivePredictions.Enqueue(data);
        _lastIndividualPrediction = data;
        var actionIndex = np.argmax(output.numpy());
        return Enum.GetValues<ActionType>()[actionIndex];
    }

    private void Evaluate(
        PersonNeeds currentPersonNeeds,
        GlobalState currentGlobalState
    )
    {
        var expected = _lastIndividualPrediction!.Output;
        var wellBeingDelta = currentPersonNeeds.GetWellBeing() - _lastIndividualPrediction.Needs.GetWellBeing();
        var actionIndex = np.argmax(_lastIndividualPrediction.Output.numpy());
        var newExpected = expected.numpy();
        newExpected[actionIndex] = wellBeingDelta > 0 ? 1 : 0;
        _model.fit(GetInputArray(_lastIndividualPrediction.Needs, _lastIndividualPrediction.GlobalState), newExpected);
    }

    private double[][] GetInputArray(PersonNeeds needs, GlobalState globalState)
    {
        return new[] { needs.AsArray(), globalState.AsArray() };
    }

    public static void SaveWeights(string weightsFile)
    {
        _model.save_weights(weightsFile);
    }

    private static Model BuildModel(string weightsFile)
    {
        var layers = new LayersApi();
        int cLength = new GlobalState(0, 0, 0).AsArray().Length;
        int iLength = new PersonNeeds().AsArray().Length;
        var actions = new ActionType[Enum.GetValues(typeof(ActionType)).Length];
        var iIn = KerasApi.keras.Input(new Shape(1));
        var cIn = KerasApi.keras.Input(new Shape(1));
        var denseC = layers.Dense(cLength, activation: "relu").Apply(cIn);
        var denseI = layers.Dense(iLength, activation: "relu").Apply(iIn);
        var concatenated = layers.Concatenate().Apply(new Tensors(denseC, denseI));
        var output = layers.Dense(actions.Length, activation: "softMax").Apply(concatenated);
        var model = KerasApi.keras.Model(new Tensors(cIn, iIn), output);
        model.compile(
            optimizer: KerasApi.keras.optimizers.SGD(LEARNING_RATE),
            loss: KerasApi.keras.losses.CategoricalCrossentropy()
        );

        if (File.Exists(weightsFile))
        {
            model.load_weights(weightsFile);
        }

        return model;
    }
}

internal record PredictionData(GlobalState GlobalState, PersonNeeds Needs, Tensor Output);