using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions {
    public static float Sum(this IEnumerable<float> source ) {
        if (source == null) throw new System.Exception("Null list");
        double sum = 0;
        foreach (float v in source) sum += v;
        return (float)sum;
    }
}
