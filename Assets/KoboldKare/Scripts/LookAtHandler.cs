using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LookAtHandler : MonoBehaviour
{
    private Vector3 _lookPosition = Vector3.zero;
    private float _weight, _bodyWeight, _headWeight, _eyesWeight, _clampWeight;
    private Animator _anim;
    private float bodyweight = 0f;

    private void Start() {
        _anim = GetComponent<Animator>();
        _weight = _bodyWeight = _headWeight = _eyesWeight = _clampWeight = 0;
    }
    public void SetLookAtPosition( Vector3 lookPosition ) {
        _lookPosition = lookPosition;
    }

    public void SetWeight(float weight) {
        _weight = weight;
    }

    public float GetWeight() {
        return _weight;
    }

    public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight) {
        _weight = weight;
        _bodyWeight = bodyWeight;
        _headWeight = headWeight;
        _eyesWeight = eyesWeight;
        _clampWeight = clampWeight;
    }
    private void OnAnimatorIK(int layerIndex) {
        if (!isActiveAndEnabled) {
            return;
        }
        bodyweight = Mathf.MoveTowards(bodyweight, _bodyWeight, Time.deltaTime);
        _anim.SetLookAtWeight(_weight, bodyweight, _headWeight, _eyesWeight, _clampWeight);
        _anim.SetLookAtPosition(_lookPosition);
    }
}
