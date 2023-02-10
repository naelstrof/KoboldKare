using System;
using UnityEngine;

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
	private SteamWorkshopItem item;

	private SerializedObject serializedObject;
	private static AddressableAssetSettings settings;
	private static int initializeCount = 0;
	
	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
	[AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	private void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) {
		Debug.LogWarning(pchDebugText);
	}

	private void OnGUI() {
		if (!item.Busy()) {
			var itemProp = serializedObject.FindProperty("item");
			EditorGUILayout.PropertyField(itemProp);
			if (GUILayout.Button("Set Preview Texture")) {
				serializedObject.FindProperty("item").FindPropertyRelative("previewTexturePath").stringValue =
					EditorUtility.OpenFilePanel("Preview Texture", "", "png,jpg,gif");
			}

			serializedObject.ApplyModifiedProperties();
			if (GUILayout.Button("Upload")) {
				item.Upload();
			}
		}

		Rect progressBarRect = GUILayoutUtility.GetRect(new GUIContent("Progress bar"), GUIStyle.none);
		EditorGUI.ProgressBar(progressBarRect, item.GetProgress(), "Upload progress");
	}


	[MenuItem("Tools/KoboldKare/Upload Steam Workshop Mod")]
	private static void CreateWizard() {
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

    private void OnDisable() {
	    if (isValid) {
		    initializeCount--;
	    }

	    if (initializeCount==0) {
		    SteamAPI.Shutdown();
	    }
    }
}

#endif