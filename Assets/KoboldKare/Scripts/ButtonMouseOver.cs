using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler {
    private Vector3 defaultLocalScale;
    private Button internalAttachedButton;
    private Button attachedButton {
        get {
            if (internalAttachedButton == null) {
                internalAttachedButton = GetComponent<Button>();
            }
            return internalAttachedButton;
        }
    }
    private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

    public enum ButtonTypes{Default, MainMenu, Option, Save, NoScale}
    public enum EventType{ Hover, Click };
    public EventType lastEvent;
    public ButtonTypes buttonType;

    private void Start() {
        defaultLocalScale = transform.localScale;
    }
    public IEnumerator ScaleBack(float scaleDuration) {
        if (buttonType == ButtonTypes.NoScale) {
            yield break;
        }
        float startTime = Time.unscaledTime;
        while (isActiveAndEnabled && attachedButton.interactable && (startTime + scaleDuration) > Time.unscaledTime ) {
            transform.localScale = Vector3.Lerp(transform.localScale, defaultLocalScale, (Time.unscaledTime-startTime)/scaleDuration);
            yield return endOfFrame;
        }
        transform.localScale = defaultLocalScale;
    }
    public IEnumerator ScaleUp(float scaleDuration) {
        if (buttonType == ButtonTypes.NoScale) {
            yield break;
        }
        float startTime = Time.unscaledTime;
        while (isActiveAndEnabled && attachedButton.interactable && (startTime + scaleDuration) > Time.unscaledTime ) {
            transform.localScale = Vector3.Lerp(transform.localScale, defaultLocalScale*1.1f, (Time.unscaledTime-startTime)/scaleDuration);
            yield return endOfFrame;
        }
        transform.localScale = defaultLocalScale*1.1f;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!attachedButton.interactable) {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ScaleUp(0.3f));
        lastEvent = EventType.Hover;
        PlaySFX();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!attachedButton.interactable) {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ScaleBack(0.3f));
    }

    public void OnSelect(BaseEventData eventData) {
        if (!attachedButton.interactable) {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ScaleUp(0.3f));
        lastEvent = EventType.Hover;
        PlaySFX();
    }

    public void OnDeselect(BaseEventData eventData) {
        if (!attachedButton.interactable) {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ScaleBack(0.3f));
    }

    public void OnSubmit(BaseEventData eventData){
        lastEvent = EventType.Click;
        PlaySFX();
    }

    public void OnPointerClick(PointerEventData eventData){
        lastEvent = EventType.Click;
        PlaySFX();
    }

    private void PlaySFX(){
        GameManager.instance.PlayUISFX(this, lastEvent);
    }
}
