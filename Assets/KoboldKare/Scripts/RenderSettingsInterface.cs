using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderSettingsInterface : MonoBehaviour {
    public float ambientIntensity {
        get {
            return RenderSettings.ambientIntensity;
        }
        set {
            StopAllCoroutines();
            StartCoroutine(LerpAmbientIntensityTo(value));
        }
    }
    public IEnumerator LerpAmbientIntensityTo(float target) {
        while (RenderSettings.ambientIntensity != target) {
            RenderSettings.ambientIntensity = Mathf.MoveTowards(RenderSettings.ambientIntensity, target, Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
    }
}
