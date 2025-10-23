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
}

#endif
