using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueStarter : MonoBehaviour {
    [Serializable]
    private class DialogueInfo {
        public int minimumStarCount;
        public List<LocalizedString> dialogue;
    }

    [SerializeField] private Sprite useSprite;
    [SerializeField] private GameObject speechBackgroundBubble;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private TMPro.TMP_Text text;
    [SerializeField]
    private AudioPack sansUndertaleVocals;

    [SerializeField] private List<DialogueInfo> dialogueInfos;

    private AudioSource source;
    private WaitForSeconds textDelay;
    private WaitForSeconds lineDelay;
    private NetworkedEntity networkedEntity;
    
    private bool talking = false;

    private void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity != null) {
            networkedEntity.SetSprite(useSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private void Awake() {
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

    private bool OnUseRequested(NetworkedKobold k) {
        return !talking;
    }

    private void OnUse(NetworkedKobold k) {
        StartCoroutine(Talk());
    }

    private IEnumerator Talk() {
        while (talking) {
            yield return null;
        }

        speechBackgroundBubble.SetActive(true);

        source.enabled = true;
        animator.SetTrigger("Talk");
        talking = true;
        List<LocalizedString> dialogue = null;
        foreach (var dialogueCheck in dialogueInfos) {
            if (ObjectiveManager.GetStars() >= dialogueCheck.minimumStarCount) {
                dialogue = dialogueCheck.dialogue;
            }
        }
        dialogue ??= dialogueInfos[0].dialogue;
        foreach (var line in dialogue) {
            float startTime = Time.time;
            string targetString = line.GetLocalizedString();
            float duration = 0.025f*targetString.Length;
            text.text = targetString;
            text.maxVisibleCharacters = 0;
            while (Time.time < startTime + duration) {
                float t = (Time.time - startTime) / duration;
                text.maxVisibleCharacters = Mathf.RoundToInt(targetString.Length * t);
                sansUndertaleVocals.Play(source);
                yield return textDelay;
            }
            text.maxVisibleCharacters = targetString.Length;
            sansUndertaleVocals.Play(source);
            yield return lineDelay;
            text.text = "";
        }

        source.enabled = false;
        talking = false;
        speechBackgroundBubble.SetActive(false);
    }
}
