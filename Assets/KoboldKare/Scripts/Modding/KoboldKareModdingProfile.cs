using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;

[CustomEditor(typeof(KoboldKareModdingProfile))]
public class KoboldKareModdingProfileEditor : Editor {
    public override VisualElement CreateInspectorGUI() {
        var visualElement = new VisualElement();
        InspectorElement.FillDefaultInspector(visualElement, serializedObject, this);
        var buildButton = new Button {
            name = "Build",
            text = "Build"
        };
        var helpBox = new HelpBox();
        helpBox.text = "Press one of the buttons below to get started!";
        helpBox.messageType = HelpBoxMessageType.None;
        visualElement.Add(helpBox);
        buildButton.clicked += () => {
            var helpboxText = "";
            MessageType biggestMessageType = MessageType.None;
            foreach (var curTarget in targets) {
                KoboldKareModdingProfile targetProfile = (KoboldKareModdingProfile)curTarget;
                try {
                    targetProfile.Build();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
                helpboxText += $"{targetProfile.name}: {targetProfile.GetStatus(out var messageType)}\n";
                biggestMessageType = (MessageType)Mathf.Max((int)biggestMessageType, (int)messageType);
            }
            helpBox.text = helpboxText;
            helpBox.messageType = (HelpBoxMessageType)biggestMessageType;
            helpBox.MarkDirtyRepaint();
        };
        visualElement.Add(buildButton);
        if (!serializedObject.isEditingMultipleObjects) {
            var showBuildFolderButton = new Button {
                name = "Show Build Folder",
                text = "Show Build Folder",
            };
            showBuildFolderButton.clicked += () => {
                KoboldKareModdingProfile modProfile = (KoboldKareModdingProfile)target;
                EditorUtility.RevealInFinder(modProfile.GetBuildFolder());
            };
            visualElement.Add(showBuildFolderButton);
        }

        return visualElement;
    }
}

[CreateAssetMenu(menuName = "KoboldKare/Modding/KoboldKare Modding Profile", fileName = "New KoboldKare Modding Profile")]
public class KoboldKareModdingProfile : ScriptableObject {
    [SerializeField] private SteamWorkshopItem workshopItem;
    public void Build() {
        workshopItem.Build();
    }
    public string GetBuildFolder() {
        return workshopItem.GetModBuildPath(EditorUserBuildSettings.activeBuildTarget);
    }
    public string GetStatus(out MessageType messageType) {
        return workshopItem.GetStatus(out messageType);
    }
}

#endif
