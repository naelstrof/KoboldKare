using System;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using SimpleJSON;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

#if !DISABLESTEAMWORKS
using System.Collections;
using System.IO;
using Steamworks;
using UnityEngine.AddressableAssets;
#endif

public class UploadWorkshopModWizard : ScriptableWizard {
	[SerializeField]
	private SteamWorkshopItem mod;

	private SerializedObject serializedObject;
	private static AddressableAssetSettings settings;
	private static int initializeCount = 0;
	
	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
	[AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	private void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) {
		Debug.LogWarning(pchDebugText);
	}

	private void OnGUI() {
		EditorGUILayout.LabelField("Status");
		string status = mod.GetStatus(out MessageType messageType);
		EditorGUILayout.HelpBox(status, messageType);
		//Rect progressBarRect = GUILayoutUtility.GetRect(new GUIContent("Progress bar"), GUIStyle.none);
		//progressBarRect.width -= 8;
		//progressBarRect.x += 4;
		//EditorGUI.ProgressBar(progressBarRect, mod.GetProgress(), "Upload progress");
		
		var modProp = serializedObject.FindProperty(nameof(mod));
		
		ulong publishedFiledID = (ulong)modProp.FindPropertyRelative("publishedFileId").longValue;
		if (publishedFiledID != (ulong)PublishedFileId_t.Invalid) {
			string url = $"https://steamcommunity.com/sharedfiles/filedetails/?id={publishedFiledID.ToString()}";
			if (EditorGUILayout.LinkButton(url)) {
				Application.OpenURL(url);
			}
		}

		if (mod.ShouldTryLoad()) {
			mod.Load(modProp);
			EditorUtility.SetDirty(this);
		}

		if (modProp.FindPropertyRelative("previewSprite").objectReferenceValue == null) {
			mod.TryLoadPreview(modProp);
			EditorUtility.SetDirty(this);
		} else {
			if (((Sprite)modProp.FindPropertyRelative("previewSprite").objectReferenceValue).texture.width > 1024 ||
			    ((Sprite)modProp.FindPropertyRelative("previewSprite").objectReferenceValue).texture.height > 1024) {
				EditorGUILayout.HelpBox( "The preview sprite is expected to be around 512x512, very large preview images may cause Steam to reject the upload.", MessageType.Warning);
			}
		}

		EditorGUILayout.PropertyField(modProp);
		serializedObject.ApplyModifiedProperties();
		GUILayout.BeginHorizontal();
		GUI.enabled = mod.IsValid();
		if (GUILayout.Button("Build")) {
			if (mod.IsBuilt()) {
				if (EditorUtility.DisplayDialog("Danger!",
					    "This will replace the old build (cache remains), are you sure?", "I'm sure","Cancel")) {
					mod.Build();
				}
			} else {
				mod.Build();
			}
		}
		GUI.enabled = mod.IsBuilt();
		if (GUILayout.Button("Upload")) {
			mod.Upload();
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
	}


	[MenuItem("Tools/KoboldKare/Upload Steam Workshop Mod")]
	private static void CreateWizard() {
		if (Application.isPlaying) {
			return;
		}

		DisplayWizard<UploadWorkshopModWizard>("Upload Mod to Steam Workshop", "Cancel");
	}
	private void OnEnable() {
	    if (initializeCount == 0) {
		    isValid = SteamAPI.Init();
		    if (!isValid) {
			    throw new UnityException("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
		    }
	    } else {
		    isValid = true;
	    }

	    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	    
		serializedObject = new SerializedObject(this);
	    initializeCount++;
	    if (m_SteamAPIWarningMessageHook == null) {
			// Set up our callback to receive warning messages from Steam.
			// You must launch with "-debug_steamapi" in the launch args to receive warnings.
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
		settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(AssetDatabase.GUIDToAssetPath("bae434cba05d81d4b9cb5c181a2fbf2c"));
    }

    private void OnInspectorUpdate() {
	    if (isValid) {
		    SteamAPI.RunCallbacks();
	    }
    }

    private void OnPlayModeStateChanged(PlayModeStateChange change) {
	    Close();
    }

    private void OnDisable() {
	    EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
	    if (isValid) {
		    initializeCount--;
	    }

	    if (initializeCount==0) {
		    SteamAPI.Shutdown();
	    }
    }
}

#endif