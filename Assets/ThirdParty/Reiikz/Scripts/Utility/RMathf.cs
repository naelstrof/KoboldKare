using UnityEngine;

namespace Reiikz.UnityUtils {

    public class RMathf {
        public static float Round(float value, int digits)
        {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return Mathf.Round(value * mult) / mult;
        }
}

}