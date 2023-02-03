using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InflatableCurve", menuName = "Data/InflatableCurve", order = 1)]
public class InflatableCurve : ScriptableObject {
    [SerializeField]
    private AnimationCurve bounceCurve;
    [SerializeField]
    private float bounceDuration;

    public virtual float GetBounceDuration() => bounceDuration;
    public virtual float EvaluateCurve(float t) => bounceCurve.Evaluate(t);
}
