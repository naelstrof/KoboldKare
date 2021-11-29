using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;

public class ReagentScanner : MonoBehaviour, IValuedGood {
    public bool firing = false;
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
            i.color = Color.Lerp(reagent.color, Color.white, 0.75f);
            t.color = Color.black;
            i.GetComponent<RectTransform>().sizeDelta = new Vector2( maxParentWidth*width01, i.GetComponent<RectTransform>().sizeDelta.y);
            g.transform.SetParent(scannerDisplay.transform, false);
            yield return new WaitForSeconds(scanDelay);
        }
    }
    public void OnFire(GameObject player) {
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
        if (hit.transform.root.transform.childCount > 10) {
            ReagentContents noReagents = new ReagentContents();
            StopAllCoroutines();
            StartCoroutine(RenderScreen(noReagents));
            return;
        }
        ReagentContents allReagents = new ReagentContents();
        foreach(GenericReagentContainer container in hit.transform.root.GetComponentsInChildren<GenericReagentContainer>()) {
            allReagents.AddMix(container.Peek());
        }
        StopAllCoroutines();
        StartCoroutine(RenderScreen(allReagents));
    }
    public void OnEndFire(GameObject player) {
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
}
