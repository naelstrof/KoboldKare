using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;

[CreateAssetMenu(menuName = "KoboldKare/Modding/Scene", fileName = "New KoboldKare Modding Profile Scene")]
public class KoboldKareModdingProfileScene : KoboldKareModdingProfile {
    [SerializeField] private SteamWorkshopItem.ModScene modScene;
    public override void Build() {
        workshopItem.Build(modScene);
    }

    public override void Upload() {
        workshopItem.Upload(modScene, true);
    }
    public override void UploadContentOnly() {
        workshopItem.Upload(modScene, false);
    }

    protected override void OnValidate() {
        base.OnValidate();
        modScene.OnValidate();
    }
}

#endif
