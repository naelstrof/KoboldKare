using System.Collections;
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
    public UnityEvent OnSuccess;
    public UnityEvent OnFailure;
    public float scanDelay = 0.09f;
    public IEnumerator RenderScreen(ReagentContents reagents) {
        for(int i=0;i<scannerDisplay.transform.childCount;i++) {
            Destroy(scannerDisplay.transform.GetChild(i).gameObject);
        }
        yield return new WaitForSeconds(scanDelay);
        scanBeam.SetActive(false);
        if (reagents.volume <= 0f) {
            OnFailure.Invoke();
            nothingFoundDisplay.SetActive(true);
        } else {
            nothingFoundDisplay.SetActive(false);
        }
        OnSuccess.Invoke();
        float maxVolume = reagents.volume;
        foreach(var reagent in ReagentDatabase.GetReagents()) {
            float rvolume = reagents.GetVolumeOf(reagent);
            if (rvolume <= 0.05f) {
                continue;
            }
            float width01 = rvolume/maxVolume;
            GameObject g = GameObject.Instantiate(scannerUIPrefab);
            float maxParentWidth = g.GetComponent<RectTransform>().sizeDelta.x;
            TMP_Text t = g.transform.Find("Label").GetComponent<TMP_Text>();
            Image i = g.transform.Find("Level").GetComponent<Image>();
            t.text =  reagent.localizedName.GetLocalizedString() + ": "+ rvolume.ToString("F2");
            i.color = Color.Lerp(reagent.color, Color.black, 0.75f);
            t.color = Color.white;
            i.GetComponent<RectTransform>().sizeDelta = new Vector2( maxParentWidth*width01, i.GetComponent<RectTransform>().sizeDelta.y);
            g.transform.SetParent(scannerDisplay.transform, false);
            yield return new WaitForSeconds(scanDelay);
        }
    }
    [PunRPC]
    protected override void OnFireRPC(int playerViewID) {
        base.OnFireRPC(playerViewID);
        if (firing) {
            return;
        }
        firing = true;
        scanBeam.SetActive(true);
        idleDisplay.SetActive(false);
        RaycastHit hit;
        if (!Physics.SphereCast(laserEmitterLocation.position, 0.25f, laserEmitterLocation.forward, out hit, 10f, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore)) {
            ReagentContents noReagents = new ReagentContents();
            StopAllCoroutines();
            StartCoroutine(RenderScreen(noReagents));
            return;
        }
        PhotonView rootView = hit.transform.GetComponentInParent<PhotonView>();
        if (rootView == null) {
            ReagentContents noReagents = new ReagentContents();
            StopAllCoroutines();
            StartCoroutine(RenderScreen(noReagents));
            return;
        }
        ReagentContents allReagents = new ReagentContents();
        foreach(GenericReagentContainer container in rootView.GetComponentsInChildren<GenericReagentContainer>()) {
            allReagents.AddMix(container.Peek());
        }
        StopAllCoroutines();
        StartCoroutine(RenderScreen(allReagents));
    }
    [PunRPC]
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

    [PunRPC]
    public void OnGrabRPC(int koboldID) {
        animator.SetBool("Open", true);
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity) {
        animator.SetBool("Open", false);
    }

    public Transform GrabTransform() {
        return center;
    }
}
