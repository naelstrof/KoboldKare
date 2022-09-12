using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueStarter : GenericUsable {
    [SerializeField] private Sprite useSprite;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private TMPro.TMP_Text text;
    [SerializeField]
    private AudioPack sansUndertaleVocals;
    [SerializeField]
    private List<LocalizedString> dialogue;

    private StringBuilder stringBuilder;
    private AudioSource source;
    private WaitForSeconds textDelay;
    private WaitForSeconds lineDelay;
    
    private bool talking = false;

    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }

    private void Awake() {
        stringBuilder = new StringBuilder();
        textDelay = new WaitForSeconds(0.16f);
        lineDelay = new WaitForSeconds(3f);
        if (source == null) {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.maxDistance = 12f;
            source.minDistance = 8f;
            source.volume = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.spatialBlend = 1f;
            source.loop = false;
        }
        source.enabled = false;
    }

    public override bool CanUse(Kobold k) {
        return !talking;
    }

    public override void Use() {
        base.Use();
        StartCoroutine(Talk());
    }

    private IEnumerator Talk() {
        while (talking) {
            yield return null;
        }

        source.enabled = true;
        animator.SetTrigger("Talk");
        talking = true;
        foreach (var line in dialogue) {
            float startTime = Time.time;
            float duration = 2f;
            string targetString = line.GetLocalizedString();
            while (Time.time < startTime + duration) {
                float t = (Time.time - startTime) / duration;
                int desiredLength = Mathf.RoundToInt(targetString.Length * t);
                if (stringBuilder.Length != desiredLength) {
                    stringBuilder.Clear();
                    stringBuilder.Append(targetString.Substring(0,desiredLength));
                    text.text = stringBuilder.ToString();
                    sansUndertaleVocals.Play(source);
                }
                yield return textDelay;
            }
            text.text = targetString;
            sansUndertaleVocals.Play(source);
            yield return lineDelay;
            text.text = "";
        }

        source.enabled = false;
        talking = false;
    }
}
