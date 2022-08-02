using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.VFX;

public class StarDoor : GenericUsable {
    [SerializeField] private Sprite useSprite;
    [SerializeField]
    private AudioPack starDoorBreak;
    [SerializeField]
    private int starRequirement = 1;

    [SerializeField] private TMP_Text text;
    [SerializeField] private Renderer starRenderer;
    [SerializeField] private VisualEffect starDissolveVFX;
    
    private Material starMaterial;
    private AudioSource starDoorBreakSource;
    private static readonly int Progress1 = Shader.PropertyToID("_DissolveProgress");

    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }

    public override bool CanUse(Kobold k) {
        return ObjectiveManager.GetStars() >= starRequirement;
    }

    public override void Use() {
        StartCoroutine(DissolveRoutine());
    }

    private void Start() {
        text.text = starRequirement.ToString();
        starMaterial = starRenderer.material;
        if (starDoorBreakSource == null) {
            starDoorBreakSource = gameObject.AddComponent<AudioSource>();
            starDoorBreakSource.playOnAwake = false;
            starDoorBreakSource.maxDistance = 10f;
            starDoorBreakSource.minDistance = 0.2f;
            starDoorBreakSource.rolloffMode = AudioRolloffMode.Linear;
            starDoorBreakSource.spatialBlend = 1f;
            starDoorBreakSource.loop = true;
        }
    }

    private IEnumerator DissolveRoutine() {
        starDissolveVFX.gameObject.SetActive(true);
        starDoorBreakSource.enabled = true;
        starDoorBreak.Play(starDoorBreakSource);
        
        float startTime = Time.time;
        float duration = 3f;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            starMaterial.SetFloat(Progress1, t);
            yield return null;
        }

        starDoorBreakSource.enabled = false;
        if (photonView.IsMine) {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
}
