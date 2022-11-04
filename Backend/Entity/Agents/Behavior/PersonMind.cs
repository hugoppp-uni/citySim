using System.ComponentModel.DataAnnotations;
using Mars.Numerics;
using Tensorflow;
using Tensorflow.NumPy;
using NLog;

namespace CitySim.Backend.Entity.Agents.Behavior;

using System;
using System.Collections.Generic;
using World;
public class PersonMind : IMind
{
    private const double EgoScalar = 0.5;
    private const double IgnoreOwnBodyScalar = 0.1;
    private const double GoodWorldLensScalar = 0.8;
    private const double BadWorldLensScalar = 0.3;
    private const int CollectiveDecisionEvaluationDelay = 10;

    private static readonly int PersonalNeedsCount = new PersonNeeds().AsNormalizedArray().Length;
    private static readonly int GlobalStatesCount = new GlobalState(0, 0, 0).AsNormalizedArray().Length;
    private readonly ModelWorker _modelWorker;

    /// <summary>
    ///   How much the person is an individualist or a collectivist.
    ///   If the value is 0.5, the personal needs and the global state are handled exactly as the are.
    /// </summary>
    private readonly double _individualist;

    private PredictionData? _lastIndividualPrediction;
    private readonly Queue<PredictionData> _lastPredictions = new();
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <param name="individualist">How much the person is an individualist or a collectivist.
    /// If the value is 0.5, the personal needs and the global state are handled exactly as the are.
    /// Has to be between 0 and 1.
    /// </param>
    /// /// <param name="modelWorker">The Worker to run the neural network model operations on
    /// </param>
    /// <exception cref="InvalidArgumentError"></exception>
    public PersonMind([Range(0.0, 1.0)] double individualist, ModelWorker modelWorker)
    {
        _modelWorker = modelWorker;
        if (individualist is < 0 or > 1)
        {
            throw new InvalidArgumentError("The param individualist has to be in the range [0,1]");
        }

        _individualist = individualist;
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState)
    {
        Evaluate(personNeeds, globalState);
        var needsAry = personNeeds.AsNormalizedArray();
        var globalStateAry = globalState.AsNormalizedArray();
        ApplyIndividualistFactorOnPersonalNeedsValues(needsAry);
        ApplyIndividualistFactorOnGlobalStateValues(globalStateAry);
        var task = new ModelTask(GetInputArray(needsAry, globalStateAry));
        Monitor.Enter(task);
        _modelWorker.Queue(task);
        Monitor.Wait(task);
        Monitor.Exit(task);
        var data = new PredictionData((double[])globalStateAry.Clone(),(double[])needsAry.Clone(), task.Output);
        _lastPredictions.Enqueue(data);
        _lastIndividualPrediction = data;
        var actionIndex = np.argmax(task.Output);
        _logger.Trace("Decided to do action: " + Enum.GetValues<ActionType>()[actionIndex]);
        return Enum.GetValues<ActionType>()[actionIndex];
    }

    private void Evaluate(
        PersonNeeds currentPersonNeeds,
        GlobalState currentGlobalState
    )
    {
        if (_lastIndividualPrediction == null)
        {
            return;
        }

        void FinalEvaluate(PredictionData prediction, [Range(0, 1)] double wellBeingDelta,
            [Range(0, 1)] double evaluationFactor)
        {
            var actionIndex = (int)np.argmax(prediction.Output);
            var expected = prediction.Output.ToArray<float>();
            if (wellBeingDelta >= 0)
            {
                expected[actionIndex] =
                    (float)(1 - 0.9 * Math.Pow(1 - (double)expected[actionIndex], 1 + 3 * evaluationFactor));
            }
            else
            {
                expected[actionIndex] =
                    (float)(0.9 * Math.Pow((double)expected[actionIndex], 1 + 3 * evaluationFactor));
            }

            var newExpected = new NDArray(expected);
            newExpected = np.stack(newExpected);
            var task = new ModelTask(GetInputArray(prediction.NormalizedNeeds, prediction.NormalizedGlobalState), newExpected);
            Monitor.Enter(task);
            _modelWorker.Queue(task);
            Monitor.Wait(task);
            Monitor.Exit(task);
            _logger.Trace(wellBeingDelta > 0
                ? "An action was good for the individual"
                : "An action wasn't good for the individual");
        }

        var wellBeingDelta =
            ApplyIndividualistFactorOnPersonalNeedsValues(currentPersonNeeds.AsNormalizedArray()).Sum() -
            _lastIndividualPrediction.NormalizedNeeds.Sum();
        wellBeingDelta /= PersonalNeedsCount;
        FinalEvaluate(_lastIndividualPrediction, wellBeingDelta, _individualist);

        if (_lastPredictions.Count == CollectiveDecisionEvaluationDelay)
        {
            var data = _lastPredictions.Dequeue();
            wellBeingDelta =
                ApplyIndividualistFactorOnGlobalStateValues(currentGlobalState.AsNormalizedArray()).Sum() -
                data.NormalizedGlobalState.Sum();
            wellBeingDelta /= GlobalStatesCount;
            return;
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
    private static NDArray GetInputArray(double[] needs, double[] globalState)
    {
        var ary = new[] { globalState, needs }.Flatten().ToList()
            .ConvertAll(it => (float)it)
            .ToArray();
        return np.stack(new NDArray(ary, shape: ary.Length));
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
}

internal record PredictionData(double[] NormalizedGlobalState, double[] NormalizedNeeds, NDArray Output);