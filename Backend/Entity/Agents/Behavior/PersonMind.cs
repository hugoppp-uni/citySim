using System.ComponentModel.DataAnnotations;
using Mars.Numerics;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;
using NLog;

namespace CitySim.Backend.Entity.Agents.Behavior;

using System;
using System.Collections.Generic;
using World;

public class PersonMind : IMind
{
    private const float LearningRate = 0.01f;
    private const int CollectiveDecisionEvaluationDelay = 10;
    private static Model _model = null!;
    /// <summary>
    ///   How much the person is an individualist or a collectivist.
    ///   If the value is 0.5, the personal needs and the global state are handled exactly as the are.
    /// </summary>
    private readonly double _individualist;
    private PredictionData? _lastIndividualPrediction;
    private readonly Queue<PredictionData> _lastPredictions = new();
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static void Init(string weightsFile)
    {
        _model = BuildModel(weightsFile);
    }
    
    /// <param name="individualist">How much the person is an individualist or a collectivist.
    /// If the value is 0.5, the personal needs and the global state are handled exactly as the are.
    /// Has to be between 0 and 1.
    /// </param>
    /// <exception cref="InvalidOperationException">The static init has to be called first (once)</exception>
    /// <exception cref="InvalidArgumentError"></exception>
    public PersonMind([Range(0.0, 1.0)] double individualist)
    {
        if (_model == null)
        {
            throw new InvalidOperationException("PersonMind is not Initialized");
        }

        if (individualist is < 0 or > 1)
        {
            throw new InvalidArgumentError("The param individualist has to be in the range [0,1]");
        }

        _individualist = individualist;
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState)
    {
        Evaluate(personNeeds, globalState);
        var input = new Tensor(GetInputArray(personNeeds, globalState));
        Tensors output;
        
        lock (_model)// a model is not thread safe
        {
            output = _model.predict(input);
        }
        var data = new PredictionData(globalState, personNeeds, output);
        _lastPredictions.Enqueue(data);
        _lastIndividualPrediction = data;
        var actionIndex = np.argmax(output[0].numpy());
        _logger.Trace("Decided to do action: "+Enum.GetValues<ActionType>()[actionIndex]);
        return Enum.GetValues<ActionType>()[actionIndex];
    }

    private void Evaluate(
        PersonNeeds currentPersonNeeds,
        GlobalState currentGlobalState
    )
    {
        if (_lastIndividualPrediction == null) { return; }
        
        void FinalEvaluate(PredictionData prediction, double wellBeingDelta)
        {
            var expected = prediction.Output.numpy();
            var actionIndex = np.argmax( prediction.Output.numpy());
            expected[actionIndex] = wellBeingDelta > 0 ? 1 : 0;
            lock (_model)
            {
                _model.fit(GetInputArray(prediction.Needs, prediction.GlobalState), expected);
            }

            _logger.Trace(wellBeingDelta > 0
                ? "An action was good for the individual"
                : "An action wasn't good for the individual");
        }
        var wellBeingDelta = currentPersonNeeds.GetWellBeing() - _lastIndividualPrediction.Needs.GetWellBeing();
        FinalEvaluate(_lastIndividualPrediction, wellBeingDelta);

        if (_lastPredictions.Count == CollectiveDecisionEvaluationDelay)
        {
            var data = _lastPredictions.Dequeue();
            wellBeingDelta = currentGlobalState.GetGlobalWellBeing() - data.GlobalState.GetGlobalWellBeing();
            FinalEvaluate(data, wellBeingDelta);
        }
    }

    /// <summary>
    /// Creates the input for the neural network by combining the array values of the personal Needs and
    /// the global state. During the creation, the<see cref="_individualist"/> gets considers and the needs gets
    /// slightly dramatized or the global state value are considered better as they actually are.
    /// </summary>
    /// <param name="needs"></param>
    /// <param name="globalState"></param>
    /// <returns></returns>
    private double[] GetInputArray(PersonNeeds needs, GlobalState globalState)
    {
        var needsAry = needs.AsNormalizedArray();
        var globalStateAry = globalState.AsNormalizedArray();
        if (_individualist < 0.5)
        {
            var goodWorldLensFactor = 1 - _individualist;
            for (var i = 0; i < globalStateAry.Length; i++)
            {
                globalStateAry[i] += (1 - globalStateAry[i]) * (goodWorldLensFactor - 0.5);
            }
        }
        else
        {
            var egoFactor = 0.5 - _individualist;
            for (var i = 0; i < globalStateAry.Length; i++)
            {
                needsAry[i] -= needsAry[i] * needsAry[i] * egoFactor - egoFactor;
            }
        }

        return new[] { globalStateAry, needsAry }.Flatten();
    }

    public static void SaveWeights(string weightsFile)
    {
        lock (_model)
        {
            _model.save_weights(weightsFile);
        }
    }

    private static Model BuildModel(string weightsFile)
    {
        var layers = new LayersApi();
        var cLength = new GlobalState(0, 0, 0).AsNormalizedArray().Length;
        var iLength = new PersonNeeds().AsNormalizedArray().Length;
        var actions = new ActionType[Enum.GetValues(typeof(ActionType)).Length];
        var inLayer = KerasApi.keras.Input(new Shape(iLength+cLength));
        var dense = layers.Dense(iLength+cLength, activation: "relu").Apply(inLayer);
        var output = layers.Dense(actions.Length, activation: "softmax").Apply(dense);
        var model = KerasApi.keras.Model(new Tensors(inLayer), output);
        model.compile(
            optimizer: KerasApi.keras.optimizers.SGD(LearningRate),
            loss: KerasApi.keras.losses.CategoricalCrossentropy()
        );
        model.summary();
        if (File.Exists(weightsFile))
        {
            model.load_weights(weightsFile);
        }

        return model;
    }
}

internal record PredictionData(GlobalState GlobalState, PersonNeeds Needs, Tensor Output);