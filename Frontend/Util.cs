using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySim.Frontend
{
    internal class Util
    {
        public static float EaseOutElastic(float x)
        {
            float c4 = (2 * MathF.PI) / 3;

            return x == 0
              ? 0
              : x == 1
              ? 1
              : MathF.Pow(2, -10 * x) * MathF.Sin((x * 10 - 0.75f) * c4) + 1;
        }
    }
}
