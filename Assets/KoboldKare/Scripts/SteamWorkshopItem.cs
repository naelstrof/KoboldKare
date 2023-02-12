using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PenetrationTech;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[System.Serializable]
public class SteamWorkshopItem {
	public static string currentModRoot = "<not set>";
	[System.Flags]
	public enum SteamWorkshopItemTag {
		Characters = 1,
		Maps = 2,
		Items = 4,
		Plants = 8,
		Reagents = 16,
	}

	public enum SteamWorkshopLanguage {
		english, arabic, bulgarian, schinese, tchinese, czech, danish, dutch, finnish, french, german, greek, hungarian,
		italian, japanese, koreana, norwegian, polish, portuguese, brazilian, romanian, russian, spanish, latam, swedish,
		thai, turkish, ukrainian, vietnamese,
	}


	[SerializeField] private AddressableAssetGroup targetGroup;
	
	[Header("Mod meta data")]
	[SerializeField] private ulong publishedFileId = (ulong)PublishedFileId_t.Invalid;
	[SerializeField] private ERemoteStoragePublishedFileVisibility visibility;
	[SerializeField] private SteamWorkshopItemTag tags;
	
	[Header("Mod details")]
	[SerializeField] private Sprite previewSprite;
	[SerializeField] private SteamWorkshopLanguage language;
	[SerializeField] private string title;
	[SerializeField,TextArea] private string description;
	[SerializeField,TextArea] private string changeNotes;

	private UGCUpdateHandle_t ugcUpdateHandle;
	private bool uploading = false;
	private bool building = false;
	private string lastMessage = "";
	private MessageType lastMessageType;
	
	private List<string> GetTags() {
		List<string> newTags = new List<string>();
		foreach (SteamWorkshopItemTag tag in (SteamWorkshopItemTag[])Enum.GetValues(typeof(SteamWorkshopItemTag))) {
			if ((tag & tags) != 0) {
				newTags.Add(tag.ToString());
			}
		}
		return newTags;
	}
	private SteamWorkshopItemTag GetTagsFromJsonArray(JSONArray array) {
		SteamWorkshopItemTag allTags = 0;
		foreach (var node in array) {
			if (Enum.TryParse(node.Value, out SteamWorkshopItemTag tag)) {
				allTags |= tag;
			}
		}
		return allTags;
	}
	
	private CallResult<CreateItemResult_t> onCreateItemCallback;
	private void OnCreateItem(CreateItemResult_t result, bool bIOFailure) {
		if (bIOFailure || result.m_eResult != EResult.k_EResultOK) {
			lastMessageType = MessageType.Error;
			lastMessage = $"Failed to upload workshop item, error code: {result}, Check https://partner.steamgames.com/doc/api/ISteamUGC#CreateItemResult_t for more information.";
			throw new UnityException(lastMessage);
		}
		if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement) {
			lastMessageType = MessageType.Warning;
			lastMessage = "Apparently you need to accept the workshop legal agreement, you should be able to do that by visiting the URL provided below."; 
		}
		publishedFileId = (ulong)result.m_nPublishedFileId;
		if (publishedFileId == (ulong)PublishedFileId_t.Invalid) {
			lastMessageType = MessageType.Error;
			lastMessage = "Failed to upload, couldn't get a published file ID!";
			throw new UnityException(lastMessage);
		}
		Save();
		
		ItemUpdate();
	}
	private CallResult<SubmitItemUpdateResult_t> onSubmitItemUpdateCallback;

	private void OnSubmitItemUpdateCallback(SubmitItemUpdateResult_t result, bool bIOFailure) {
		if (result.m_eResult == EResult.k_EResultOK) {
			lastMessageType = MessageType.Info;
			lastMessage = "Upload success! Check the URL to see your post";
		} else {
			lastMessageType = MessageType.Error;
			lastMessage = $"Upload failed with error {result.m_eResult}. Check https://partner.steamgames.com/doc/api/ISteamUGC#SubmitItemUpdateResult_t for more information.";
		}

		uploading = false;
		EditorUtility.ClearProgressBar();
	}

	public bool IsBuilt() {
		return IsValid() && Directory.Exists(modRoot);
	}

	public bool Busy() {
		return building || uploading;
	}
	public float GetProgress() {
		if (uploading) {
			SteamUGC.GetItemUpdateProgress(ugcUpdateHandle, out ulong punBytesProcessed, out ulong punBytesTotal);
			return (float)punBytesProcessed / (float)punBytesTotal;
		} else {
			return 0f;
		}
	}

	private string modRoot {
		get {
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "Mod.BuildPath", $"{Application.persistentDataPath}/mods/{targetGroup.name}/{EditorUserBuildSettings.activeBuildTarget}" );
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "ContentCatalog.BuildPath", $"{Application.persistentDataPath}/mods/{targetGroup.name}/{EditorUserBuildSettings.activeBuildTarget}" );
			string fullPathWithBuildTarget = targetGroup.GetSchema<BundledAssetGroupSchema>().BuildPath.GetValue(targetGroup.Settings);
			DirectoryInfo fullPath = Directory.GetParent(fullPathWithBuildTarget);
			return fullPath.FullName;
		}
	}

	private string jsonSavePath => $"{modRoot}{Path.DirectorySeparatorChar}info.json";
	private string previewTexturePath => $"{modRoot}{Path.DirectorySeparatorChar}preview.png";

	private string Serialize() {
		JSONNode rootNode = JSONNode.Parse("{}");
        rootNode["publishedFileId"] = publishedFileId.ToString();
        rootNode["description"] = description;
        rootNode["language"] = language.ToString();
        rootNode["title"] = title;
        rootNode["visibility"] = visibility.ToString();
        var arrayNode = new JSONArray();
	
        foreach(var tag in GetTags()) {
			arrayNode.Add(tag.ToString());
		}
        rootNode["tags"] = arrayNode;
        return rootNode.ToString();
	}

	private static bool SupportsBuildPlatform(BuildTarget target) {
		var moduleManager = System.Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
		var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		return (bool)isPlatformSupportLoaded.Invoke(null,new object[] {(string)getTargetStringFromBuildTarget.Invoke(null, new object[] {target})});
	}

	private void Save() {
        using FileStream file = new FileStream(jsonSavePath, FileMode.OpenOrCreate, FileAccess.Write);
        using StreamWriter writer = new StreamWriter(file);
        writer.Write(Serialize());
	}

	public bool ShouldTryLoad() {
		return targetGroup != null && publishedFileId == (ulong)PublishedFileId_t.Invalid && new DirectoryInfo(modRoot).Exists && File.Exists(jsonSavePath);
	}

	public string GetStatus(out MessageType messageType) {
		if (!IsValid()) {
			messageType = MessageType.Error;
			if (targetGroup == null) {
				return "Please specify an associated Addressable Asset Group to continue.";
			}
			if (previewSprite == null) {
				return "Please specify the preview texture, this is required, sorry!";
			}
		}
		if (!SupportsBuildPlatform(BuildTarget.StandaloneLinux64) || !SupportsBuildPlatform(BuildTarget.StandaloneWindows64) || !SupportsBuildPlatform(BuildTarget.StandaloneWindows) || !SupportsBuildPlatform(BuildTarget.StandaloneOSX)) {
			messageType = MessageType.Error;
			return "Missing build support for one of the following platforms: Windows, Linux, OSX. Please use Unity Hub to install build support modules.";
		}
		
		if (!IsBuilt()) {
			messageType = MessageType.Warning;
			return $"Mod not found at build directory: {modRoot}.\nMod must be built before upload... This can take a very long time on the first run! (several hours)\nIt will be faster on subsequent runs (a few minutes).";
		}

		if (building) {
			messageType = MessageType.Info;
			return "Building for all platforms...";
		}

		if (uploading) {
			messageType = MessageType.Info;
			return "Uploading...";
		}

		if (!string.IsNullOrEmpty(lastMessage)) {
			messageType = lastMessageType;
			return lastMessage;
		}

		if (publishedFileId == (ulong)PublishedFileId_t.Invalid) {
			messageType = MessageType.Warning;
			return $"Publish id {publishedFileId} is invalid, this will create a new file on the workshop on upload!";
		}

		if (!File.Exists(previewTexturePath)) {
			messageType = MessageType.Error;
			return "Failed to find the preview texture, this is required! Rebuild the mod to regenerate it...";
		}

		messageType = MessageType.Info;
		return $"Looking good, ready to upload!\nBuild is located at {modRoot}.\nMake sure to hit Build if you made changes.";
	}

	public void Upload() {
		if (!IsBuilt()) {
			Build();
		}

		CreateIfNeeded();
	}

	public bool IsValid() {
		return targetGroup != null && previewSprite != null;
	}

	private async Task UpdateProgress() {
		await Task.Run(() => {
			while (uploading) {
				SteamUGC.GetItemUpdateProgress(ugcUpdateHandle, out ulong punBytesProcessed, out ulong punBytesTotal);
				EditorUtility.DisplayProgressBar(GetStatus(out MessageType ignoreType), "Uploading...",
					(float)punBytesProcessed / (float)punBytesTotal);
				Thread.Sleep(10);
			}
		});
	}

	public void Build() {
		try {
			building = true;
			currentModRoot = modRoot;
			var directoryInfo = new DirectoryInfo(modRoot);
			if (directoryInfo.Exists) {
				directoryInfo.Delete(true);
			}

			Directory.CreateDirectory(modRoot);
			
			// Write out the sprite to a render texture, so that we can ignore its "readOnly" state.
			var rect = previewSprite.textureRect;
			RenderTexture targetTexture = new RenderTexture((int)rect.width, (int)rect.height, 0);
			CommandBuffer buffer = new CommandBuffer();
			buffer.Blit(previewSprite.texture, targetTexture, Vector2.one, previewSprite.textureRectOffset);
			Graphics.ExecuteCommandBuffer(buffer);
			
			// Then read-back the render texture to a regular texture2D.
			Texture2D previewTextureWrite = new Texture2D((int)rect.width, (int)rect.height);
			RenderTexture.active = targetTexture;
			previewTextureWrite.ReadPixels(new Rect(0, 0, rect.width, rect.height), 0, 0);
			File.WriteAllBytes(previewTexturePath, previewTextureWrite.EncodeToPNG());
			
			Save();

			var buildTargetMemory = EditorUserBuildSettings.activeBuildTarget;
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "Mod.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetGroup.name}/[BuildTarget]" );
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "ContentCatalog.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetGroup.name}/[BuildTarget]" );
			AddressableAssetSettings.BuildPlayerContent();
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "Mod.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetGroup.name}/[BuildTarget]" );
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "ContentCatalog.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetGroup.name}/[BuildTarget]" );
			AddressableAssetSettings.BuildPlayerContent();
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "Mod.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetGroup.name}/[BuildTarget]" );
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "ContentCatalog.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetGroup.name}/[BuildTarget]" );
			AddressableAssetSettings.BuildPlayerContent();
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "Mod.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetGroup.name}/[BuildTarget]" );
			targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "ContentCatalog.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetGroup.name}/[BuildTarget]" );
			AddressableAssetSettings.BuildPlayerContent();
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, buildTargetMemory);
		} finally {
			building = false;
		}
	}

	private void ItemUpdate() {
		EditorUtility.DisplayProgressBar("Uploading...", "Setting item information....", 0f);
		ugcUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), (PublishedFileId_t)publishedFileId);
		if (!SteamUGC.SetItemDescription(ugcUpdateHandle, description)) {
			throw new UnityException("Failed to set item description.");
		}

		if (!SteamUGC.SetItemUpdateLanguage(ugcUpdateHandle, language.ToString())) {
			throw new UnityException("Failed to set item update language.");
		}

		if (!SteamUGC.SetItemTitle(ugcUpdateHandle, title)) {
			throw new UnityException("Failed to set item title.");
		}

		if (!SteamUGC.SetItemMetadata(ugcUpdateHandle, Serialize())) {
			throw new UnityException("Failed to set item metaData.");
		}

		if (!SteamUGC.SetItemTags(ugcUpdateHandle, GetTags())) {
			throw new UnityException("Failed to set item tags.");
		}

		if (!SteamUGC.SetItemVisibility(ugcUpdateHandle, visibility)) {
			throw new UnityException("Failed to set item visibility.");
		}

		if (!SteamUGC.SetItemPreview(ugcUpdateHandle, previewTexturePath)) {
			throw new UnityException("Failed to set item preview.");
		}

		if (!SteamUGC.SetItemContent(ugcUpdateHandle, modRoot)) {
			throw new UnityException("Failed to set item content.");
		}

		var handle = SteamUGC.SubmitItemUpdate(ugcUpdateHandle, string.IsNullOrEmpty(changeNotes) ? null : changeNotes);
		uploading = true;
		Task.Run(UpdateProgress);
		onSubmitItemUpdateCallback.Set(handle);
	}

	private void CreateIfNeeded() {
	    onCreateItemCallback = new CallResult<CreateItemResult_t>(OnCreateItem);
	    onSubmitItemUpdateCallback = new CallResult<SubmitItemUpdateResult_t>(OnSubmitItemUpdateCallback);
		if (publishedFileId == (ulong)PublishedFileId_t.Invalid) {
			EditorUtility.DisplayProgressBar("Uploading...", "Creating new workshop item..", 0f);
			var call = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
			onCreateItemCallback.Set(call);
		} else {
			ItemUpdate();
		}
	}

	public void Load(SerializedProperty target) {
		using FileStream file = new FileStream(jsonSavePath, FileMode.Open, FileAccess.Read);
        using StreamReader reader = new StreamReader(file);
        var rootNode = JSONNode.Parse(reader.ReadToEnd());
        if (rootNode.HasKey("publishedFileId")) {
	        if (!ulong.TryParse(rootNode["publishedFileId"], out ulong output)) {
		        throw new UnityException( $"Failed to parse publishedFileID in file {jsonSavePath} as ulong... Invalid mod info format or corruption?");
	        }
	        target.FindPropertyRelative("publishedFileId").longValue = (long)output;
        }

        if (rootNode.HasKey("description")) {
	        target.FindPropertyRelative("description").stringValue = rootNode["description"];
        }

        if (rootNode.HasKey("language")) {
	        if (Enum.TryParse(rootNode["language"], out SteamWorkshopLanguage languageOutput)) {
		        target.FindPropertyRelative("language").intValue = (int)languageOutput;
	        }
        }

        if (rootNode.HasKey("visibility")) {
	        if (Enum.TryParse(rootNode["visibility"], out ERemoteStoragePublishedFileVisibility visibilityOutput)) {
		        target.FindPropertyRelative("visibility").intValue = (int)visibilityOutput;
	        }
        }

        if (rootNode.HasKey("title")) {
	        target.FindPropertyRelative("title").stringValue = rootNode["title"];
        }
        if (rootNode.HasKey("tags")) {
	        JSONArray array = rootNode["tags"].AsArray;
	        target.FindPropertyRelative("tags").intValue = (int)GetTagsFromJsonArray(array);
        }
	}
}

#endif
