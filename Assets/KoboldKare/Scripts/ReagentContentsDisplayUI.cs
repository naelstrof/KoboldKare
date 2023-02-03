using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReagentContentsDisplayUI : MonoBehaviour {
    private Kobold targetKobold;
    [SerializeField] private TargetReagentContents targetContents;
    [SerializeField]
    private RectTransform background;
    [SerializeField]
    private float volumeToPixels = 1f;

    private RectTransform rectTransform;

    [SerializeField] private AnimationCurve bounceCurve;

    [SerializeField] private Sprite imageSprite;
    private enum TargetReagentContents {
        Belly,
        Metabolized
    }

    private List<Reagent> reagents;
    private List<RectTransform> images;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        reagents = new List<Reagent>();
        images = new List<RectTransform>();
    }
    private void OnEnable() {
        targetKobold = GetComponentInParent<Kobold>();
        switch (targetContents) {
            case TargetReagentContents.Belly:
                targetKobold.bellyContainer.OnChange.AddListener(OnReagentContentsChangedOther);
                OnReagentContentsChanged(targetKobold.bellyContainer.GetContents());
                break;
            case TargetReagentContents.Metabolized:
                //targetKobold.bellyContainer.OnChange.AddListener(OnReagentContentsChanged);
                targetKobold.metabolizedContents.changed += OnReagentContentsChanged;
                OnReagentContentsChanged(targetKobold.metabolizedContents);
                break;
        }
    }

    private int SortReagent(Reagent a, Reagent b) {
        return a.id.CompareTo(b.id);
    }

    private void OnReagentContentsChanged(ReagentContents contents) {
        OnReagentContentsChangedOther(contents, GenericReagentContainer.InjectType.Inject);
    }

    private void OnReagentContentsChangedOther(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        reagents.Clear();
        foreach (var reagent in contents) {
            reagents.Add(reagent);
        }
        reagents.Sort(SortReagent);
        // Make sure our images are available and are the right color
        for (int i = images.Count; i < reagents.Count; i++) {
            Image targetImage = new GameObject("colorBlock", typeof(Image)).GetComponent<Image>();
            targetImage.sprite = imageSprite;
            targetImage.transform.SetParent(transform, false);
            targetImage.color = ReagentDatabase.GetReagent(reagents[i].id).GetColor();
            targetImage.type = Image.Type.Sliced;
            targetImage.fillCenter = true;
            targetImage.pixelsPerUnitMultiplier = 2f;
            RectTransform imageRect = targetImage.GetComponent<RectTransform>();
            imageRect.anchoredPosition = new Vector2(0f, 0.5f);
            imageRect.anchorMin = new Vector2(0f, 0f);
            imageRect.anchorMax = new Vector2(0f, 1f);
            imageRect.sizeDelta = new Vector2(0f, imageRect.sizeDelta.y);
            images.Add(targetImage.GetComponent<RectTransform>());
        }
        for (int i = reagents.Count; i < images.Count;) {
            Destroy(images[i].gameObject);
            images.RemoveAt(i);
        }

        if (isActiveAndEnabled) {
            StopAllCoroutines();
            for (int i = 0; i < reagents.Count; i++) {
                images[i].GetComponent<Image>().color = ReagentDatabase.GetReagent(reagents[i].id).GetColor();
                StartCoroutine(TweenWidth(images[i], Mathf.Min(reagents[i].volume * volumeToPixels,3000f)));
            }

            StartCoroutine(TweenWidth(rectTransform, Mathf.Min(contents.volume * volumeToPixels, 3000f)));
            StartCoroutine(TweenWidth(background, Mathf.Min(contents.GetMaxVolume() * volumeToPixels, 3000f)));
        } else {
            for (int i = 0; i < reagents.Count; i++) {
                images[i].GetComponent<Image>().color = ReagentDatabase.GetReagent(reagents[i].id).GetColor();
                images[i].sizeDelta = new Vector2(reagents[i].volume * volumeToPixels, images[i].sizeDelta.y);
            }

            rectTransform.sizeDelta = new Vector2(contents.volume * volumeToPixels, rectTransform.sizeDelta.y);
            background.sizeDelta = new Vector2(contents.GetMaxVolume() * volumeToPixels, background.sizeDelta.y);
        }
    }

    private IEnumerator TweenWidth(RectTransform targetImage, float targetWidth) {
        float startingWidth = targetImage.sizeDelta.x;
        float startTime = Time.time;
        float duration = 1f;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            float bounceSample = bounceCurve.Evaluate(t);
            targetImage.sizeDelta = new Vector2(
                Mathf.Clamp(Mathf.LerpUnclamped(startingWidth, targetWidth, bounceSample),0f,float.MaxValue),
                targetImage.sizeDelta.y);
            yield return null;
        }
        targetImage.sizeDelta = new Vector2(targetWidth, targetImage.sizeDelta.y);
    }
}
