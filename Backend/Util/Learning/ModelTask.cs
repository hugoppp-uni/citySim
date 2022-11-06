using Tensorflow.NumPy;

namespace CitySim.Backend.Util.Learning;

public class ModelTask
{
    public NDArray Input { get;}
    public NDArray Output { get; set; }
    
    public ModelTask(NDArray input, NDArray output)
    {
        Input = input;
        Output = output;
    }
    public ModelTask(NDArray input):this(input, new NDArray(Array.Empty<double>()))
    { }
}