using System;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;

#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.Rendering;

[Serializable]
public class SteamWorkshopItem {
	public static string currentModRoot = "<not set>";
	[Flags]
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


	[SerializeField] private ExternalCatalogSetup targetCatalog;
	
	[Header("Mod meta data")]
	[SerializeField] private ulong publishedFileId = (ulong)PublishedFileId_t.Invalid;
	[SerializeField] private ERemoteStoragePublishedFileVisibility visibility;
	[SerializeField] private SteamWorkshopItemTag tags;
	[SerializeField, Tooltip("The loading priority for the mod, lower numbers have higher priority.")] private float loadPriority;
	
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
	private ExternalCatalogSetup loadedCatalog;
	
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
		// Gotta tell inspectors to reload in order to see the new ID.
		loadedCatalog = null;
		
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
	private string modBuildPath => $"{Application.persistentDataPath}/mods/{targetCatalog.CatalogName}/{EditorUserBuildSettings.activeBuildTarget}";

	private string modRoot {
		get {
			DirectoryInfo fullPath = Directory.GetParent(modBuildPath);
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
        rootNode["loadPriority"] = loadPriority;
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
		return targetCatalog != null && new DirectoryInfo(modRoot).Exists && File.Exists(jsonSavePath) && loadedCatalog != targetCatalog;
	}

	public string GetStatus(out MessageType messageType) {
		if (!IsValid()) {
			messageType = MessageType.Error;
			if (targetCatalog == null) {
				return "Please specify an associated External Catalog Setup asset to continue.";
			}
			if (previewSprite == null) {
				return "Please specify the preview texture, this is required, sorry!";
			}

			if (targetCatalog.AssetGroups.Count <= 0) {
				return "Target External catalog has no asset groups assigned. Please assign them.";
			}
			
			if (string.IsNullOrEmpty(targetCatalog.CatalogName)) {
				return "";
			}

			foreach (var assetGroup in targetCatalog.AssetGroups) {
				if (assetGroup == null) {
					return "One of the asset groups in the external catalog is null! Please fix it.";
				}
			}
			return "For somereason this mod is invalid! The GetStatus() function must have not been updated after IsValid was changed.";
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
		if (targetCatalog == null || previewSprite == null || targetCatalog.AssetGroups == null || targetCatalog.AssetGroups.Count == 0) {
			return false;
		}

		if (string.IsNullOrEmpty(targetCatalog.CatalogName)) {
			return false;
		}

		foreach (var targetGroup in targetCatalog.AssetGroups) {
			if (targetGroup == null) {
				return false;
			}
		}
		return true;
	}
	private void BuildForPlatform(BuildTarget target) {
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, target);
		//foreach (var targetGroup in targetCatalog.AssetGroups) {
			//targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "Mod.BuildPath", $"[UnityEngine.Application.persistentDataPath]/mods/{targetCatalog.CatalogName}/[BuildTarget]" );
			//targetGroup.Settings.profileSettings.SetValue( targetGroup.Settings.activeProfileId, "Mod.LoadPath", $"{Application.persistentDataPath}/mods/{targetCatalog.CatalogName}/[BuildTarget]" );
		//}
		ModManager.currentLoadingMod = modBuildPath;
		targetCatalog.BuildPath = $"[UnityEngine.Application.persistentDataPath]/mods/{targetCatalog.CatalogName}/[BuildTarget]";
		targetCatalog.RuntimeLoadPath = "{ModManager.currentLoadingMod}/[BuildTarget]";
		AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
		if (result == null) {
			throw new UnityException("Something went really wrong!");
		}

		if (!string.IsNullOrEmpty(result.Error)) {
			throw new UnityException(result.Error);
		}
	}

	public void Build() {
		var buildTargetMemory = EditorUserBuildSettings.activeBuildTarget;
		var packedMultiCatalogMode = AssetDatabase.LoadAssetAtPath<BuildScriptPackedMultiCatalogMode>(AssetDatabase.GUIDToAssetPath("541ccd6f1620abf489a055f1a3f1466c"));
		if (!packedMultiCatalogMode.ExternalCatalogs.Contains(targetCatalog)) {
			packedMultiCatalogMode.ExternalCatalogs.Add(targetCatalog);
		}
		var catalogMemory = packedMultiCatalogMode.ExternalCatalogs;
		packedMultiCatalogMode.ExternalCatalogs = new List<ExternalCatalogSetup>(new[] { targetCatalog });
		try {
			AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilderIndex = 4;
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
			RenderTexture oldTex = RenderTexture.active;
			RenderTexture.active = targetTexture;
			previewTextureWrite.ReadPixels(new Rect(0, 0, rect.width, rect.height), 0, 0);
			File.WriteAllBytes(previewTexturePath, previewTextureWrite.EncodeToPNG());
			RenderTexture.active = oldTex;

			Save();

			BuildForPlatform(BuildTarget.StandaloneWindows64);
			BuildForPlatform(BuildTarget.StandaloneLinux64);
			BuildForPlatform(BuildTarget.StandaloneOSX);
			lastMessage = "Successfully built! Upload when ready.";
			lastMessageType = MessageType.Info;
		} catch {
			lastMessage = "Failed to build! Check the console to see what went wrong! You may need to clear your build cache if considerable changes have been made.";
			lastMessageType = MessageType.Error;
			throw;
		} finally {
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, buildTargetMemory);
			packedMultiCatalogMode.ExternalCatalogs = catalogMemory;
			building = false;
		}
	}

	private void ItemUpdate() {
		EditorUtility.DisplayProgressBar("Uploading...", "Setting item description...", 0f);
		ugcUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), (PublishedFileId_t)publishedFileId);
		if (!SteamUGC.SetItemDescription(ugcUpdateHandle, description)) {
			throw new UnityException("Failed to set item description.");
		}

		EditorUtility.DisplayProgressBar("Uploading...", "Setting item language...", 0f);
		if (!SteamUGC.SetItemUpdateLanguage(ugcUpdateHandle, language.ToString())) {
			throw new UnityException("Failed to set item update language.");
		}

		EditorUtility.DisplayProgressBar("Uploading...", "Setting item title...", 0f);
		if (!SteamUGC.SetItemTitle(ugcUpdateHandle, title)) {
			throw new UnityException("Failed to set item title.");
		}

		EditorUtility.DisplayProgressBar("Uploading...", "Setting item meta data...", 0f);
		if (!SteamUGC.SetItemMetadata(ugcUpdateHandle, Serialize())) {
			throw new UnityException("Failed to set item metaData.");
		}

		EditorUtility.DisplayProgressBar("Uploading...", "Setting item tags...", 0f);
		if (!SteamUGC.SetItemTags(ugcUpdateHandle, GetTags())) {
			throw new UnityException("Failed to set item tags.");
		}

		EditorUtility.DisplayProgressBar("Uploading...", "Setting item visibility...", 0f);
		if (!SteamUGC.SetItemVisibility(ugcUpdateHandle, visibility)) {
			throw new UnityException("Failed to set item visibility.");
		}

		EditorUtility.DisplayProgressBar("Uploading...", "Setting item preview...", 0f);
		if (!SteamUGC.SetItemPreview(ugcUpdateHandle, previewTexturePath)) {
			throw new UnityException("Failed to set item preview.");
		}

		EditorUtility.DisplayProgressBar("Uploading...", "Setting item content...", 0f);
		if (!SteamUGC.SetItemContent(ugcUpdateHandle, modRoot)) {
			throw new UnityException("Failed to set item content.");
		}

		uploading = true;
		var handle = SteamUGC.SubmitItemUpdate(ugcUpdateHandle, string.IsNullOrEmpty(changeNotes) ? null : changeNotes);
		onSubmitItemUpdateCallback.Set(handle);
		bool validStatus = true;
		while (uploading && validStatus) {
			var status = SteamUGC.GetItemUpdateProgress(ugcUpdateHandle, out ulong punBytesProcessed, out ulong punBytesTotal);
			switch (status) {
				case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
					EditorUtility.DisplayProgressBar("Uploading...", "Preparing configuration...",
						(float)punBytesProcessed / (float)punBytesTotal);
					break;
				case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
					EditorUtility.DisplayProgressBar("Uploading...", "Committing changes...",
						(float)punBytesProcessed / (float)punBytesTotal);
					break;
				case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
					EditorUtility.DisplayProgressBar("Uploading...", "Preparing content...",
						(float)punBytesProcessed / (float)punBytesTotal);
					break;
				case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
					EditorUtility.DisplayProgressBar("Uploading...", "Uploading preview file...",
						(float)punBytesProcessed / (float)punBytesTotal);
					break;
				case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
					EditorUtility.DisplayProgressBar("Uploading...", "Uploading content...",
						(float)punBytesProcessed / (float)punBytesTotal);
					break;
				default:
					validStatus = false;
					break;
			}
			Thread.Sleep(10);
		}
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

        if (rootNode.HasKey("loadPriority")) {
	        target.FindPropertyRelative("loadPriority").floatValue = rootNode["loadPriority"];
        }

        if (rootNode.HasKey("tags")) {
	        JSONArray array = rootNode["tags"].AsArray;
	        target.FindPropertyRelative("tags").intValue = (int)GetTagsFromJsonArray(array);
        }
        TryLoadPreview(target);
        loadedCatalog = targetCatalog;
        lastMessage = "Successfully loaded mod information from disk.";
        lastMessageType = MessageType.Info;
	}

	public void TryLoadPreview(SerializedProperty target) {
		if (targetCatalog == null || targetCatalog.AssetGroups == null || targetCatalog.AssetGroups.Count == 0) {
			return;
		}
		foreach (var targetGroup in targetCatalog.AssetGroups) {
			if (targetGroup == null) {
				return;
			}
		}
		if (!File.Exists(previewTexturePath)) {
			return;
		}
		var fileData = File.ReadAllBytes(previewTexturePath);
        var tex = new Texture2D(16, 16);
        tex.LoadImage(fileData);
        target.FindPropertyRelative("previewSprite").objectReferenceValue = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));
	}
}

#endif
