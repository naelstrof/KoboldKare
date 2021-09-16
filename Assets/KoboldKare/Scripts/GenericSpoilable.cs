using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;


[System.Serializable]
public class SpoilIntensityEvent : UnityEvent<float> {}
public class GenericSpoilable : MonoBehaviourPun, ISpoilable {
    public VisualEffect effect;
    public SpoilableHandler handler;
    public SpoilIntensityEvent OnIntensityChange;
    public SpoilIntensityEvent OnFadeout;
    public bool canSpoilOverTime = true;
    public float spawnProtection = 4f;
    private int daysLeftOut = 0;
    private bool destroying = false;
    private float internalSpoilIntensity; 
    public float spoilIntensity {
        get {
            return internalSpoilIntensity;
        }
     set {
         if (!destroying) {
            internalSpoilIntensity = value;
            OnIntensityChange.Invoke(value);
            if (effect != null) {
                effect.SetFloat("Intensity",value);
            }
         }
        }
    }
    public void DayPassed() {
        if (!canSpoilOverTime) {
            return;
        }
        daysLeftOut++;
        if (daysLeftOut>=3 && photonView.IsMine) {
            SaveManager.Destroy(gameObject);
        }
    }
    public UnityEvent OnSpoilEvent;
    public UnityEvent onSpoilEvent => OnSpoilEvent;

    // Start is called before the first frame update
    void Start() {
        internalSpoilIntensity = 0f;
        destroying = false;
        handler.AddSpoilable(this);
        StartCoroutine(SpawnProtection());
    }
    public IEnumerator SpawnProtection() {
        while(spawnProtection > 0f) {
            internalSpoilIntensity = 0f;
            spawnProtection -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
    void OnDestroy() {
        handler.RemoveSpoilable(this);
    }
    public IEnumerator WaitThenDestroyObject(float delay) {
        yield return new WaitForSeconds(delay);
        if (photonView.IsMine) {
            SaveManager.Destroy(photonView.gameObject);
        }
    }
    public IEnumerator IntensityToZero(float delay) {
        destroying = true;
        float newDelay = Mathf.Max(delay/2f,0f);
        yield return new WaitForSeconds(delay/2f);
        float time = Time.timeSinceLevelLoad;
        while (Time.timeSinceLevelLoad < time + newDelay) {
            float intensity = Mathf.Lerp(internalSpoilIntensity, 0f, (Time.timeSinceLevelLoad - time)/newDelay);
            OnFadeout.Invoke(1f-intensity);
            OnIntensityChange.Invoke(intensity);
            if (effect != null) {
                effect.SetFloat("Intensity",intensity);
            }
            yield return new WaitForFixedUpdate();
        }
        destroying = false;
    }
    public void WaitThenDestroy(float delay) {
        StartCoroutine(IntensityToZero(delay));
        StartCoroutine(WaitThenDestroyObject(delay));
    }
}
