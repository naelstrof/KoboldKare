using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler {
    private Vector3 defaultLocalScale;
    private Button internalAttachedButton;
    private const float scaleFactor = 1.05f;
    private const float scaleDuration = 0.2f;
    
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> OnButtonPress = new List<GameEventResponse>();
    
    private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

    public enum EventType{ Hover, Click };
    
    private EventType lastEvent = EventType.Hover;
    private Button attachedButton;

    void Awake() {
        attachedButton = GetComponent<Button>();
        attachedButton.onClick.AddListener(() => {
            if (OnButtonPress != null) {
                foreach(var response in OnButtonPress) {
                    response?.Invoke(this);
                }
            }
        });
    }

    private void OnDisable() {
        transform.localScale = defaultLocalScale;
    }

    private void Start() {
        defaultLocalScale = transform.localScale;
    }
    public IEnumerator ScaleBack(float scaleDuration) {
        float startTime = Time.unscaledTime;
        while (isActiveAndEnabled && attachedButton.interactable && (startTime + scaleDuration) > Time.unscaledTime ) {
            transform.localScale = Vector3.Lerp(transform.localScale, defaultLocalScale, (Time.unscaledTime-startTime)/scaleDuration);
            yield return endOfFrame;
        }
        transform.localScale = defaultLocalScale;
    }
    public IEnumerator ScaleUp(float scaleDuration) {
        float startTime = Time.unscaledTime;
        while (isActiveAndEnabled && attachedButton.interactable && (startTime + scaleDuration) > Time.unscaledTime ) {
            transform.localScale = Vector3.Lerp(transform.localScale, defaultLocalScale*scaleFactor, (Time.unscaledTime-startTime)/scaleDuration);
            yield return endOfFrame;
        }
        transform.localScale = defaultLocalScale*scaleFactor;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!attachedButton.interactable) {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ScaleUp(scaleDuration));
        lastEvent = EventType.Hover;
        PlaySFX();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!attachedButton.interactable) {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ScaleBack(scaleDuration));
    }

    public void OnSelect(BaseEventData eventData) {
        if (!attachedButton.interactable) {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ScaleUp(scaleDuration));
        lastEvent = EventType.Hover;
        PlaySFX();
    }

    public void OnDeselect(BaseEventData eventData) {
        if (!attachedButton.interactable) {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ScaleBack(scaleDuration));
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
