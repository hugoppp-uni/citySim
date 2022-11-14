using Tensorflow.NumPy;

namespace CitySim.Backend.Util.Learning;

/// <summary>
/// A task to be executed on the model by a <see cref="ModelWorker"/>.
/// If <see cref="Output"/> is an empty NDArray, the task is a prediction task.
/// The result will be set as the output.
/// If both the output and the input are not empty, the task is a training task. 
/// </summary>
public class ModelTask
{
    /// <summary>
    /// The input to be passed in the model
    /// </summary>
    public NDArray Input { get;}
    /// <summary>
    /// The expected output or the result of this task / model call
    /// </summary>
    public NDArray Output { get; set; }
    
    /// <summary>
    /// Creates an training task
    /// </summary>
    /// <param name="input">The input to feed the model</param>
    /// <param name="output">The expected output</param>
    public ModelTask(NDArray input, NDArray output)
    {
        Input = input;
        Output = output;
    }
    
    /// <summary>
    /// Creates a prediction task.
    /// </summary>
    /// <param name="input">The input to feed the model</param>
    public ModelTask(NDArray input):this(input, new NDArray(Array.Empty<double>()))
    { }
}