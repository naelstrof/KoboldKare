using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using KoboldKare;

public interface ITooltipDisplayable {
    public void OnTooltipDisplay(RectTransform panel);
}
public class TooltipDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public static GameObject tooltip;
    public GameObject tooltipPrefab;
    public UnityEngine.Object thingToDisplay;
    public Coroutine runningTask;
    public IEnumerator DisplayForSomeTime(float duration) {
        if (tooltip == null) {
            tooltip = GameObject.Instantiate(tooltipPrefab, null);
            tooltip.SetActive(false);
        }
        for(int i=0;i<tooltip.transform.childCount;i++) {
            Destroy(tooltip.transform.GetChild(i).gameObject);
        }
        Canvas c = transform.GetComponentInParent<Canvas>();
        tooltip.transform.SetParent(c.transform, false);
        tooltip.transform.position = transform.position;
        tooltip.transform.localScale = Vector3.one;
        tooltip.SetActive(true);
        ITooltipDisplayable displayable = thingToDisplay as ITooltipDisplayable;
        if (displayable != null) {
            displayable.OnTooltipDisplay(tooltip.GetComponent<RectTransform>());
        }
        yield return new WaitForSeconds(duration);
        tooltip.SetActive(false);
        tooltip.transform.SetParent(null);
    }
    public void OnDisable() {
        if (runningTask != null) {
            StopCoroutine(runningTask);
        }
        tooltip.SetActive(false);
        tooltip.transform.SetParent(null);
    }
    public void OnPointerEnter(PointerEventData eventData) {
        if (runningTask != null) {
            StopCoroutine(runningTask);
        }
        runningTask = StartCoroutine(DisplayForSomeTime(20f));
    }

    public void OnPointerExit(PointerEventData eventData) {
        OnDisable();
    }
}
