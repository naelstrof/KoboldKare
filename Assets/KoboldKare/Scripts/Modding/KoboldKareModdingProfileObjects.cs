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
            text = "Build",
            tooltip = "Builds the mod content into the build folder."
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
            var uploadMetaData = new Button {
                name = "Upload To Steam",
                text = "Upload To Steam",
                tooltip = "Creates a Steam workshop item if one doesn't exist, then uploads the built mod content and metadata (name, description, etc)."
            };
            uploadMetaData.clicked += () => {
                KoboldKareModdingProfile modProfileObjects = (KoboldKareModdingProfile)target;
                modProfileObjects.Upload();
            };
            
            var uploadContentOnly = new Button {
                name = "Upload Content Only",
                text = "Upload Content Only",
                tooltip = "Uploads only the mod content, not the metadata (name, description, etc). Useful if you've edited the mod in Steam Workshop directly."
                
            };
            uploadContentOnly.clicked += () => {
                KoboldKareModdingProfile modProfileObjects = (KoboldKareModdingProfile)target;
                modProfileObjects.UploadContentOnly();
            };
            
            var showBuildFolderButton = new Button {
                name = "Show Build Folder",
                text = "Show Build Folder",
            };
            showBuildFolderButton.clicked += () => {
                KoboldKareModdingProfile modProfileObjects = (KoboldKareModdingProfile)target;
                EditorUtility.RevealInFinder(modProfileObjects.GetBuildFolder());
            };
            
            var showWorkshopItem = new Button {
                name = "Show Steam Workshop Item",
                text = "Show Steam Workshop Item",
                tooltip = "Opens your web browser to the Steam Workshop page for this mod."
            };
            showWorkshopItem.clicked += () => {
                KoboldKareModdingProfile modProfileObjects = (KoboldKareModdingProfile)target;
                modProfileObjects.ShowSteamWorkshopItem();
            };
            visualElement.Add(showBuildFolderButton);
            var separator = new VisualElement {
                style = {
                    height = 1,
                    marginTop = 6,
                    marginBottom = 6,
                    backgroundColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f))
                }
            };
            visualElement.Add(separator);
            visualElement.Add(uploadMetaData);
            visualElement.Add(uploadContentOnly);
            visualElement.Add(showWorkshopItem);
        }

        return visualElement;
    }
}

public abstract class KoboldKareModdingProfile : ScriptableObject {
    [SerializeField] protected SteamWorkshopItem workshopItem;
    public abstract void Build();

    public abstract void Upload();
    public abstract void UploadContentOnly();

    public void ShowSteamWorkshopItem() {
        workshopItem.ShowSteamWorkshopItem();
    }
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

    public override void Upload() {
        workshopItem.Upload(modObjects, true);
    }
    public override void UploadContentOnly() {
        workshopItem.Upload(modObjects, false);
    }
}

#endif
