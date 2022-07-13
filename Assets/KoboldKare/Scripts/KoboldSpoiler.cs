using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class KoboldSpoiler : MonoBehaviourPun, ISpoilable {
    [SerializeField]
    private CanvasGroup fadeOut;
    [SerializeField]
    private VisualEffect effect;
    [SerializeField]
    private AudioSource mudBubbles;
    [SerializeField]
    private AudioSource handGrabbies;
    [SerializeField]
    private Rigidbody body;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private UnityEvent OnSpoilEvent;
    void Start() {
        SpoilableHandler.AddSpoilable(this);
        OnSpoilEvent.AddListener(WaitThenDestroy);
    }
    void OnDestroy() {
        SpoilableHandler.RemoveSpoilable(this);
        OnSpoilEvent.RemoveListener(WaitThenDestroy);
    }
    private float internalSpoilIntensity = 0f;
    public float spoilIntensity {
        get => internalSpoilIntensity;
        set {
            internalSpoilIntensity = value;
            mudBubbles.enabled = value>0f;
            handGrabbies.enabled = value>0f;
            effect.enabled = value>0f;
            effect.SetFloat("Intensity",value);
            mudBubbles.volume = value;
            handGrabbies.volume = value;
        }
    }
    public UnityEvent onSpoilEvent => OnSpoilEvent;
    public IEnumerator IntensityToZero(float delay) {
        float newDelay = Mathf.Max(delay/2f,0f);
        yield return new WaitForSeconds(delay/2f);
        float time = Time.timeSinceLevelLoad;
        while (Time.timeSinceLevelLoad < time + newDelay) {
            float intensity = Mathf.Lerp(internalSpoilIntensity, 0f, (Time.timeSinceLevelLoad - time)/newDelay);
            fadeOut.alpha = 1f-intensity;
            spoilIntensity = intensity;
            yield return new WaitForFixedUpdate();
        }
    }
    private IEnumerator WaitThenDestroyObject(float delay) {
        yield return new WaitForSeconds(delay);
        if (photonView.IsMine) {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
    private void WaitThenDestroy() {
        animator.SetTrigger("EtherealGrab");
        body.isKinematic = true;
        StartCoroutine(IntensityToZero(5f));
        StartCoroutine(WaitThenDestroyObject(5f));
    }
}
