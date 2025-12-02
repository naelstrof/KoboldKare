using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class ReagentScanner : GenericWeapon, IValuedGood, IGrabbable {
    public bool firing = false;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Transform center;
    public GameObject canvas;
    public GameObject scannerDisplay;
    public GameObject idleDisplay;
    public GameObject scanBeam;
    public GameObject nothingFoundDisplay;
    public GameObject scannerUIPrefab;
    public Transform laserEmitterLocation;
    
    [SerializeField]
    private UnityEvent OnSuccess;
    [SerializeField]
    private UnityEvent OnFailure;
    
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onSuccessResponses = new List<GameEventResponse>();
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onFailureResponses = new List<GameEventResponse>();
    
    public float scanDelay = 0.09f;
    private static RaycastHit[] hits = new RaycastHit[32];
    private static RaycastHitComparer comparer = new RaycastHitComparer();

    private void Awake() {
        GameEventSanitizer.SanitizeRuntime(OnSuccess, onSuccessResponses, this);
        GameEventSanitizer.SanitizeRuntime(OnFailure, onFailureResponses, this);
    }
    private void OnValidate() {
        GameEventSanitizer.SanitizeEditor(nameof(OnSuccess), nameof(onSuccessResponses), this);
        GameEventSanitizer.SanitizeEditor(nameof(OnFailure), nameof(onFailureResponses), this);
    }

    private class RaycastHitComparer : IComparer<RaycastHit> {
        public int Compare(RaycastHit x, RaycastHit y) {
            return x.distance.CompareTo(y.distance);
        }
    }

    public IEnumerator RenderScreen(ReagentContents reagents) {
        for(int i=0;i<scannerDisplay.transform.childCount;i++) {
            Destroy(scannerDisplay.transform.GetChild(i).gameObject);
        }
        yield return new WaitForSeconds(scanDelay);
        scanBeam.SetActive(false);
        if (reagents.volume <= 0f) {
            foreach (var resp in onFailureResponses) {
                resp?.Invoke(this);
            }
            nothingFoundDisplay.SetActive(true);
        } else {
            nothingFoundDisplay.SetActive(false);
        }

        foreach (var resp in onSuccessResponses) {
            resp?.Invoke(this);
        }
        
        float maxVolume = reagents.volume;
        foreach(var reagent in ReagentDatabase.GetAssets()) {
            float rvolume = reagents.GetVolumeOf(reagent);
            if (rvolume <= 0.05f) {
                continue;
            }
            float width01 = rvolume/maxVolume;
            GameObject g = GameObject.Instantiate(scannerUIPrefab);
            float maxParentWidth = g.GetComponent<RectTransform>().sizeDelta.x;
            TMP_Text t = g.transform.Find("Label").GetComponent<TMP_Text>();
            Image i = g.transform.Find("Level").GetComponent<Image>();
            t.text =  reagent.GetLocalizedName().GetLocalizedString() + ": "+ rvolume.ToString("F2");
            i.color = Color.Lerp(reagent.GetColor(), Color.black, 0.75f);
            t.color = Color.white;
            i.GetComponent<RectTransform>().sizeDelta = new Vector2( maxParentWidth*width01, i.GetComponent<RectTransform>().sizeDelta.y);
            g.transform.SetParent(scannerDisplay.transform, false);
            yield return new WaitForSeconds(scanDelay);
        }
    }
    
    // FIXME FISHNET
    //[PunRPC]
    protected override void OnFireRPC(int playerViewID) {
        base.OnFireRPC(playerViewID);
        if (firing) {
            return;
        }
        firing = true;
        scanBeam.SetActive(true);
        idleDisplay.SetActive(false);
        int hitCount = Physics.SphereCastNonAlloc(laserEmitterLocation.position-laserEmitterLocation.forward*0.25f, 0.75f, laserEmitterLocation.forward,
            hits, 10f, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore);
        if (hitCount <= 0) {
            ReagentContents noReagents = new ReagentContents();
            StopAllCoroutines();
            StartCoroutine(RenderScreen(noReagents));
            return;
        }
        Array.Sort(hits, 0, hitCount, comparer);

        GenericReagentContainer[] containers = null;
        for (int i = 0; i < hitCount; i++) {
            RaycastHit hit = hits[i];
            if (Vector3.Dot(hit.normal, laserEmitterLocation.forward) > 0f) {
                continue;
            }

            PhotonView rootView = hit.collider.GetComponentInParent<PhotonView>();
            if (rootView != null) {
                GenericReagentContainer[] containersCheck = rootView.GetComponentsInChildren<GenericReagentContainer>();
                if (containersCheck.Length > 0) {
                    float vol = 0f;
                    foreach (var cont in containersCheck) {
                        vol += cont.volume;
                    }
                    if (vol > 0.01f) {
                        containers = containersCheck;
                        break;
                    }
                }
            }
        }

        if (containers == null || containers.Length == 0) {
            ReagentContents noReagents = new ReagentContents();
            StopAllCoroutines();
            StartCoroutine(RenderScreen(noReagents));
            return;
        }

        ReagentContents allReagents = new ReagentContents();
        foreach(GenericReagentContainer container in containers) {
            allReagents.AddMix(container.Peek());
        }
        StopAllCoroutines();
        StartCoroutine(RenderScreen(allReagents));
    }
    
    // FIXME FISHNET
    //[PunRPC]
    protected override void OnEndFireRPC(int viewID) {
        firing = false;
    }
    public Vector3 GetWeaponPositionOffset(Transform grabber) {
        return (grabber.up * 0.1f + grabber.right * 0.5f - grabber.forward * 0.25f);
    }

    public bool ShouldSave() {
        return true;
    }
    public float GetWorth() {
        return 15f;
    }

    public bool CanGrab(Kobold kobold) {
        return true;
    }

    // FIXME FISHNET
    //[PunRPC]
    public void OnGrabRPC(int koboldID) {
        animator.SetBool("Open", true);
    }

    // FIXME FISHNET
    // [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity) {
        animator.SetBool("Open", false);
    }

    public Transform GrabTransform() {
        return center;
    }
}
