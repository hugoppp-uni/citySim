namespace CitySim.Backend.Entity.Agents.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keras.Layers;
using Keras.Models;
using Numpy;
using World;

internal class PersonMind : IMind
{
    private static Model model;
    private double i, c;
    private (PersonNeeds personNeeds, double[] iOutput) lastPrediction;
    private double[] lastIndividualOutput;
    private LinkedList<(GlobalState globalState, double[] cOutput)> lastCollectivePredictions = new();

    public static void Init(string weightsFile) {
        model = BuildModel(weightsFile);
    }

    public PersonMind(double i, double c)
    {
        this.i = i;
        this.c = c;
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState)
    {
        var output = model.Predict(new NDarray[]{personNeeds.AsNdArray()*i, globalState.AsNdArray()*c});

        return ActionType.BuildHouse;
    }

    public void evaluate()
    {

    }

    public void SaveWeights(string weightsFile)
    {
        model.SaveWeight(weightsFile);
    }

    private static Model BuildModel(string weightsFile)
    {
        int cLength = new GlobalState(0, 0, 0).AsArray().Length;
        int iLength = new PersonNeeds().AsArray().Length;
        var actions = new ActionType[Enum.GetValues(typeof(ActionType)).Length];
        var iIn = new Keras.Layers.Input(shape: new Keras.Shape(iLength));
        var cIn = new Keras.Layers.Input(shape: new Keras.Shape(cLength));
        var denseC = new Keras.Layers.Dense(cLength, kernel_constraint: "nonNeg", activation: "relu");
        denseC.Set(cIn);
        var denseI = new Keras.Layers.Dense(iLength, kernel_constraint: "nonNeg", activation: "relu");
        denseI.Set(iIn);

        var merged = new Keras.Layers.Concatenate(denseI, denseC);
        var dense = new Dense(actions.Length, kernel_constraint: "nonNeg", activation: "softMax");
        dense.Set(merged);
        var model = new Keras.Models.Model(new BaseLayer[] { cIn, iIn }, new BaseLayer[] { dense });
        model.Compile(optimizer: "sgd", loss: "binary_crossentropy");

        if (File.Exists(weightsFile))
        {
            model.LoadWeight(weightsFile);
        }
        
        return model;
    }
}
