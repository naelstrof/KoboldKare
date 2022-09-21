using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KoboldKare;
using Photon.Pun;
using UnityEngine;

public class MailboxUsable : GenericUsable {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private AudioPack mailWaiting;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject spaceBeam;
    private AudioSource mailWaitingSource;

    private bool hasMail = true;
    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }

    void Start() {
        if (mailWaitingSource == null) {
            mailWaitingSource = gameObject.AddComponent<AudioSource>();
            mailWaitingSource.playOnAwake = false;
            mailWaitingSource.maxDistance = 12f;
            mailWaitingSource.minDistance = 1f;
            mailWaitingSource.rolloffMode = AudioRolloffMode.Linear;
            mailWaitingSource.spatialBlend = 1f;
            mailWaitingSource.loop = true;
        }

        spaceBeam.SetActive(true);
        mailWaiting.Play(mailWaitingSource);
        ObjectiveManager.AddObjectiveSwappedListener(OnObjectiveSwapped);
        OnObjectiveSwapped(ObjectiveManager.GetCurrentObjective());
    }

    private void OnDestroy() {
        ObjectiveManager.RemoveObjectiveSwappedListener(OnObjectiveSwapped);
    }

    void OnObjectiveSwapped(DragonMailObjective obj) {
        if (obj != null || !ObjectiveManager.HasMail()) {
            hasMail = false;
            mailWaitingSource.Stop();
            mailWaitingSource.enabled = false;
            animator.SetBool("HasMail", false);
            animator.SetTrigger("GetMail");
            spaceBeam.SetActive(false);
            return;
        }

        hasMail = true;
        mailWaitingSource.enabled = true;
        mailWaiting.Play(mailWaitingSource);
        animator.SetBool("HasMail", true);
        spaceBeam.SetActive(true);
    }

    /*void OnMidnight(object ignore) {
        if (ObjectiveManager.GetCurrentObjective() == null) {
            hasMail = true;
            mailWaitingSource.enabled = true;
            mailWaiting.Play(mailWaitingSource);
            animator.SetBool("HasMail", true);
        }
    }*/

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
