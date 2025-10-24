using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;

[CustomEditor(typeof(KoboldKareModdingProfile), true)]
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
                KoboldKareModdingProfile targetProfileObjects = (KoboldKareModdingProfile)curTarget;
                try {
                    targetProfileObjects.Build();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
                helpboxText += $"{targetProfileObjects.name}: {targetProfileObjects.GetStatus(out var messageType)}\n";
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
                KoboldKareModdingProfile modProfileObjects = (KoboldKareModdingProfile)target;
                EditorUtility.RevealInFinder(modProfileObjects.GetBuildFolder());
            };
            visualElement.Add(showBuildFolderButton);
        }

        return visualElement;
    }
}

public abstract class KoboldKareModdingProfile : ScriptableObject {
    [SerializeField] protected SteamWorkshopItem workshopItem;
    public abstract void Build();
    public string GetBuildFolder() {
        return workshopItem.GetModBuildPath(EditorUserBuildSettings.activeBuildTarget);
    }
    public string GetStatus(out MessageType messageType) {
        return workshopItem.GetStatus(out messageType);
    }
}

[CreateAssetMenu(menuName = "KoboldKare/Modding/Objects", fileName = "New KoboldKare Modding Profile Objects")]
public class KoboldKareModdingProfileObjects : KoboldKareModdingProfile {
    [SerializeField] private SteamWorkshopItem.ModObjects modObjects;
    public override void Build() {
        workshopItem.Build(modObjects);
    }
}

#endif
