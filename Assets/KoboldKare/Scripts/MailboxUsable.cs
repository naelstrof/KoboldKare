using System;
using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using UnityEngine;

public class MailboxUsable : GenericUsable {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private GameEventGeneric midnightEvent;
    [SerializeField] private AudioPack mailWaiting;
    [SerializeField] private Animator animator;
    private AudioSource mailWaitingSource;

    private bool hasMail = true;
    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }

    void Start() {
        midnightEvent.AddListener(OnMidnight);
        if (mailWaitingSource == null) {
            mailWaitingSource = gameObject.AddComponent<AudioSource>();
            mailWaitingSource.playOnAwake = false;
            mailWaitingSource.maxDistance = 12f;
            mailWaitingSource.minDistance = 1f;
            mailWaitingSource.rolloffMode = AudioRolloffMode.Linear;
            mailWaitingSource.spatialBlend = 1f;
            mailWaitingSource.loop = true;
        }
        mailWaiting.Play(mailWaitingSource);
    }

    private void OnDestroy() {
        midnightEvent.RemoveListener(OnMidnight);
    }

    void OnMidnight(object ignore) {
        if (ObjectiveManager.GetCurrentObjective() == null) {
            hasMail = true;
            mailWaitingSource.enabled = true;
            mailWaiting.Play(mailWaitingSource);
            animator.SetBool("HasMail", true);
        }
    }

    public override bool CanUse(Kobold k) {
        return hasMail && ObjectiveManager.GetCurrentObjective() == null;
    }

    public override void Use() {
        base.Use();
        hasMail = false;
        ObjectiveManager.GetMail();
        mailWaitingSource.Stop();
        mailWaitingSource.enabled = false;
        animator.SetBool("HasMail", false);
        animator.SetTrigger("GetMail");
    }
}
