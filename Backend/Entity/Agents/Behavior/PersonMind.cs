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
using static KerasApi;

public class PersonMind : IMind
{
    private const float LearningRate = 0.2f;// the learning rate is high because the expected value is not changed directly to 0 or 1
    private const double EgoScalar = 0.5;
    private const double IgnoreOwnBodyScalar = 0.1;
    private const double GoodWorldLensScalar = 0.8;
    private const double BadWorldLensScalar = 0.3;
    private const int CollectiveDecisionEvaluationDelay = 10;
    private static Model? _model;
    private static readonly int PersonalNeedsCount = new PersonNeeds().AsNormalizedArray().Length;
    private static readonly int GlobalStatesCount = new GlobalState(0,0,0).AsNormalizedArray().Length;
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
         //   throw new InvalidOperationException("PersonMind is not Initialized");
        }
        
        if (individualist is < 0 or > 1)
        {
            throw new InvalidArgumentError("The param individualist has to be in the range [0,1]");
        }

        _individualist = individualist;
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState)
    {
        if (_model == null)
        {
            _model = BuildModel("weightsFile");
        }
        Evaluate(personNeeds, globalState);
        var input = GetInputArray(personNeeds, globalState);
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
        
        void FinalEvaluate(PredictionData prediction,[Range(0,1)] double wellBeingDelta,[Range(0,1)] double evaluationFactor)
        {
            var expected = prediction.Output.numpy();
            var actionIndex = np.argmax( prediction.Output.numpy());
            expected[actionIndex] = wellBeingDelta > 0 ? 1 : 0;
            if (wellBeingDelta > 0)
            {
                expected[actionIndex] = 1 - Math.Pow(1 - expected[actionIndex], 1 + 3 * evaluationFactor);
            }
            else
            {
                expected[actionIndex] = Math.Pow(1 + expected[actionIndex], 1 + 3 * evaluationFactor) - 1;
            }
            lock (_model)
            {
                _model.fit(GetInputArray(prediction.Needs, prediction.GlobalState), expected);
            }

            _logger.Trace(wellBeingDelta > 0
                ? "An action was good for the individual"
                : "An action wasn't good for the individual");
        }
        var wellBeingDelta = 
            ApplyIndividualistFactorOnPersonalNeedsValues(currentPersonNeeds.AsNormalizedArray()).Sum() - 
            ApplyIndividualistFactorOnPersonalNeedsValues(_lastIndividualPrediction.Needs.AsNormalizedArray()).Sum();
        wellBeingDelta /= PersonalNeedsCount;
        FinalEvaluate(_lastIndividualPrediction, wellBeingDelta, _individualist);

        if (_lastPredictions.Count == CollectiveDecisionEvaluationDelay)
        {
            var data = _lastPredictions.Dequeue();
            wellBeingDelta = 
                ApplyIndividualistFactorOnGlobalStateValues(currentGlobalState.AsNormalizedArray()).Sum() -
                ApplyIndividualistFactorOnGlobalStateValues(data.GlobalState.AsNormalizedArray()).Sum();
            wellBeingDelta /= GlobalStatesCount;
            FinalEvaluate(data, wellBeingDelta, 1 - _individualist);
        }
    }

    /// <summary>
    /// Creates the input for the neural network by combining the array values of the personal Needs and
    /// the global state. During the creation, the<see cref="_individualist"/> gets considers and the needs gets
    /// slightly dramatized or the global state value are considered better as they actually are.
    /// </summary>
    /// <param name="needs"></param>
    /// <param name="globalState"></param>
    /// <returns>A 2D array where the first array contains the values of the
    /// needs and the second array the values of the global state</returns>
    private NDArray GetInputArray(PersonNeeds needs, GlobalState globalState)
    {
        var needsAry = needs.AsNormalizedArray();
        var globalStateAry = globalState.AsNormalizedArray();
        ApplyIndividualistFactorOnGlobalStateValues(globalStateAry);
        ApplyIndividualistFactorOnPersonalNeedsValues(needsAry);
        var ary = new[] { globalStateAry, needsAry }.Flatten().ToList()
            .ConvertAll(it => (float) it)
            .ToArray();
        return np.stack(new NDArray(ary,shape:ary.Length));
    }

    /// <summary>
    /// Improves or decrease the values of the global state based on <see cref="_individualist"/>
    /// </summary>
    /// <param name="globalStateValues"></param>
    /// <returns>The passed array</returns>
    private double[] ApplyIndividualistFactorOnGlobalStateValues(double[] globalStateValues)
    {
        if (_individualist > 0.5)
        {
            var goodWorldLensFactor = 1 - _individualist;
            for (var i = 0; i < globalStateValues.Length; i++)
            {
                globalStateValues[i] += (1 - globalStateValues[i]) * (0.5 - goodWorldLensFactor) * GoodWorldLensScalar;
            }
        }
        else
        {
            var badWorldLensFactor = 1 - _individualist;
            for (var i = 0; i < globalStateValues.Length; i++)
            {
                globalStateValues[i] -= (-1 - globalStateValues[i]) * (badWorldLensFactor - 0.5) * BadWorldLensScalar;
            }
        }

        return globalStateValues;
    }

    /// <summary>
    /// Improves or decrease the values of the personal needs based on <see cref="_individualist"/>
    /// </summary>
    /// <param name="personalNeedsValues"></param>
    /// <returns>The passed array</returns>
    private double[] ApplyIndividualistFactorOnPersonalNeedsValues(double[] personalNeedsValues)
    {
        var factor = 0.5 - _individualist;
        if (_individualist > 0.5)
        {
            for (var i = 0; i < personalNeedsValues.Length; i++)
            {
                personalNeedsValues[i] -= EgoScalar * personalNeedsValues[i] * personalNeedsValues[i] *
                    factor + EgoScalar * factor;
            }
        }
        else
        {
            for (var i = 0; i < personalNeedsValues.Length; i++)
            {
                personalNeedsValues[i] += IgnoreOwnBodyScalar * personalNeedsValues[i] * personalNeedsValues[i] *
                    factor - IgnoreOwnBodyScalar * factor;
            }
        }

        return personalNeedsValues;
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
        var inLayer = keras.Input((iLength+cLength),name:"feeling");
        var dense = layers.Dense(iLength+cLength, activation: "relu").Apply(inLayer);
        var output = layers.Dense(actions.Length, activation: "softmax").Apply(dense);
        var model = keras.Model(inLayer, output);
        model.summary();
        model.compile(
            optimizer: keras.optimizers.SGD(LearningRate),
            loss: keras.losses.CategoricalCrossentropy(from_logits: true)
        );
        
        if (File.Exists(weightsFile))
        {
            model.load_weights(weightsFile);
        }

        return model;
    }
}

internal record PredictionData(GlobalState GlobalState, PersonNeeds Needs, Tensor Output);