using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using SimpleJSON;
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

    public override void Save(JSONNode node) {
        base.Save(node);
        node["position.x"] = transform.position.x;
        node["position.y"] = transform.position.y;
        node["position.z"] = transform.position.z;
        node["rotation.x"] = transform.rotation.x;
        node["rotation.y"] = transform.rotation.y;
        node["rotation.z"] = transform.rotation.z;
        node["rotation.w"] = transform.rotation.w;
        node["scale.x"] = transform.localScale.x;
        node["scale.y"] = transform.localScale.y;
        node["scale.z"] = transform.localScale.z;
        node["starRequirement"] = starRequirement;
    }

    public override void Load(JSONNode node) {
        base.Load(node);
        float x = node["position.x"];
        float y = node["position.y"];
        float z = node["position.z"];
        transform.position = new Vector3(x, y, z);
        float rx = node["rotation.x"];
        float ry = node["rotation.y"];
        float rz = node["rotation.z"];
        float rw = node["rotation.w"];
        transform.rotation = new Quaternion(rx, ry, rz,rw);
        float sx = node["scale.x"];
        float sy = node["scale.y"];
        float sz = node["scale.z"];
        transform.localScale = new Vector3(sx, sy, sz);
        if (node.HasKey("starRequirement")) {
            starRequirement = node["starRequirement"];
            text.text = starRequirement.ToString();
        }
    }
}
