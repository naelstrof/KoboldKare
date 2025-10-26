using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEngine;

#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEditor;
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

	public delegate void StatusChangedAction(SteamWorkshopItem item, MessageType type, string message);
	
	public event StatusChangedAction statusChanged;
	
	[Header("Mod meta data")]
	[SerializeField] private string publishedFileId = PublishedFileId_t.Invalid.ToString();
	[SerializeField] private ERemoteStoragePublishedFileVisibility visibility;
	[SerializeField] private SteamWorkshopItemTag tags;
	[SerializeField, Tooltip("The loading priority for the mod, lower numbers have higher priority.")] private float loadPriority;
	
	[Header("Mod details")]
	[SerializeField] private Sprite previewSprite;
	[SerializeField] private SteamWorkshopLanguage language;
	[SerializeField] private string title;
	[SerializeField,TextArea] private string description;
	[SerializeField,TextArea] private string changeNotes;
	
	[NonSerialized] private static SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
	[AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) {
		Debug.LogWarning(pchDebugText);
	}
	
	private bool TryGetPublishedFileId(out PublishedFileId_t fileId) {
		if (ulong.TryParse(publishedFileId, out ulong output)) {
			fileId = (PublishedFileId_t)output;
			if (fileId == PublishedFileId_t.Invalid) {
				return false;
			}
			return true;
		}
		fileId = PublishedFileId_t.Invalid;
		return false;
	}


	public void ShowSteamWorkshopItem() {
		if (TryGetPublishedFileId(out var id)) {
			Application.OpenURL($"https://steamcommunity.com/sharedfiles/filedetails/?id={id}");
		} else {
			Debug.LogError("Cannot show workshop item, publishedFileId is invalid.");
		}
	}
	

	[Serializable]
	public abstract class ModContent {
		public abstract AssetBundleBuild[] GetBuilds();

		public void BuildForTarget(BuildTarget target, string modBuildPath) {
			if (!Directory.Exists(modBuildPath)) {
				Directory.CreateDirectory(modBuildPath);
			}
			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles( modBuildPath, GetBuilds(), BuildAssetBundleOptions.None, target);
			if (manifest) {
				foreach(var bundleName in manifest.GetAllAssetBundles()) {
					Debug.Log($"Successfully created AssetBundle {bundleName} at {modBuildPath}/{bundleName}");
				}
			} else {
				throw new UnityException("Build failed, see console for details.");
			}
		}
		public abstract void Serialize(JSONNode node);
	}

	[Serializable]
	public class ModObjects : ModContent {
		public GameObject[] playableCharacters;
		public GameObject[] cosmeticItems;
		public GameObject[] fruits;
		public GameObject[] equipmentStoreItems;
		public GameObject[] dicks;
		public GameObject[] seeds;
		public ScriptablePlant[] plants;
		public ScriptableReagent[] reagents;
		public ScriptableReagentReaction[] reagentReactions;
		public Equipment[] equipment;

		protected virtual string[] GetAssets() {
			HashSet<string> assetNames = new HashSet<string>();
			foreach(var asset in playableCharacters) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var asset in cosmeticItems) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var asset in fruits) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var asset in equipmentStoreItems) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var asset in dicks) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var asset in plants) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var asset in reagents) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var asset in reagentReactions) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var asset in equipment) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			return assetNames.ToArray();
		}

		private JSONArray GetAssetNames(ICollection<UnityEngine.Object> assets) {
			var arrayNode = new JSONArray();
			HashSet<string> assetNames = new HashSet<string>();
			foreach(var asset in assets) {
				assetNames.Add(AssetDatabase.GetAssetPath(asset));
			}
			foreach(var assetName in assetNames) {
				arrayNode.Add(assetName);
			}

			return arrayNode;
		}
		
		public override void Serialize(JSONNode rootNode) {
			rootNode["PlayableCharacter"] = GetAssetNames(playableCharacters);
			rootNode["Fruit"] = GetAssetNames(fruits);
			rootNode["Penis"] = GetAssetNames(dicks);
			rootNode["Reaction"] = GetAssetNames(reagentReactions);
			rootNode["Reagent"] = GetAssetNames(reagents);
			rootNode["Plant"] = GetAssetNames(plants);
			rootNode["Seed"] = GetAssetNames(seeds);
			rootNode["Cosmetic"] = GetAssetNames(cosmeticItems);
			rootNode["Equipment"] = GetAssetNames(equipment);
			rootNode["EquipmentStoreItem"] = GetAssetNames(equipmentStoreItems);
		}

		public override AssetBundleBuild[] GetBuilds() {
			AssetBundleBuild build = new AssetBundleBuild {
				assetBundleName = "bundle",
				assetNames = GetAssets(),
			};
			return new[] { build };
		}
	}
	
	[Serializable]
	public class ModScene : ModObjects {
		public SceneAsset scene;
		public Sprite sceneIcon;
		public string sceneTitle;
		public string sceneDescription;
		public override AssetBundleBuild[] GetBuilds() {
			AssetBundleBuild sceneBuild = new AssetBundleBuild {
				assetBundleName = "sceneBundle",
				assetNames = new[] { AssetDatabase.GetAssetPath(scene) },
			};
			var otherBuilds = base.GetBuilds();
			List<AssetBundleBuild> allBuilds = new List<AssetBundleBuild>(otherBuilds) { sceneBuild };
			return allBuilds.ToArray();
		}
		
		protected override string[] GetAssets() {
			HashSet<string> assets = new HashSet<string>(base.GetAssets()) { AssetDatabase.GetAssetPath(sceneIcon) };
			return assets.ToArray();
		}

		public override void Serialize(JSONNode node) {
			base.Serialize(node);
			node["Scene"] = AssetDatabase.GetAssetPath(scene);
			node["SceneTitle"] = sceneTitle;
			node["SceneDescription"] = sceneDescription;
			node["SceneIcon"] = AssetDatabase.GetAssetPath(sceneIcon);
		}
	}

	private string GetFileNameSafeTitle() {
		var newTitle = title;
		foreach (var c in Path.GetInvalidFileNameChars()) {
			newTitle = newTitle.Replace(c, '_');
		}
		return newTitle;
	}
	
	private UGCUpdateHandle_t ugcUpdateHandle;
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
			statusChanged?.Invoke(this, lastMessageType, lastMessage);
			Debug.LogError(lastMessage);
			throw new UnityException(lastMessage);
		}
		if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement) {
			lastMessageType = MessageType.Warning;
			lastMessage = "Apparently you need to accept the workshop legal agreement, you should be able to do that by visiting `https://steamcommunity.com/workshop/workshoplegalagreement/`."; 
			statusChanged?.Invoke(this, lastMessageType, lastMessage);
			Debug.LogError(lastMessage);
		}
		publishedFileId = result.m_nPublishedFileId.ToString();
		
		if (result.m_nPublishedFileId == PublishedFileId_t.Invalid) {
			lastMessageType = MessageType.Error;
			lastMessage = "Failed to upload, couldn't get a published file ID!";
			statusChanged?.Invoke(this, lastMessageType, lastMessage);
			Debug.LogError(lastMessage);
			throw new UnityException(lastMessage);
		}

		try {
			_ = ItemUpdate(uploadContent, includeMetadata);
		} catch (Exception e) {
			Debug.LogException(e);
			throw;
		}
	}
	private CallResult<SubmitItemUpdateResult_t> onSubmitItemUpdateCallback;

	private void OnSubmitItemUpdateCallback(SubmitItemUpdateResult_t result, bool bIOFailure) {
		if (result.m_eResult == EResult.k_EResultOK) {
			lastMessageType = MessageType.Info;
			lastMessage = "Upload success!";
			statusChanged?.Invoke(this, lastMessageType, lastMessage);
			Debug.Log(lastMessage);
		} else {
			lastMessageType = MessageType.Error;
			lastMessage = $"Upload failed with error {result.m_eResult}. Check https://partner.steamgames.com/doc/api/ISteamUGC#SubmitItemUpdateResult_t for more information.";
			statusChanged?.Invoke(this, lastMessageType, lastMessage);
			Debug.LogError(lastMessage);
		}

		EditorUtility.ClearProgressBar();
	}

	public bool IsBuilt() {
		return IsValid() && Directory.Exists(modRoot);
	}

	public string GetModBuildPath(BuildTarget target) {
		return $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}mods{Path.DirectorySeparatorChar}{GetFileNameSafeTitle()}{Path.DirectorySeparatorChar}{target}";
	}
	private string modRoot => $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}mods{Path.DirectorySeparatorChar}{GetFileNameSafeTitle()}";

	private string jsonSavePath => $"{modRoot}{Path.DirectorySeparatorChar}info.json";
	private string previewTexturePath => $"{modRoot}{Path.DirectorySeparatorChar}preview.png";

	private string Serialize(ModContent content) {
		JSONNode rootNode = JSONNode.Parse("{}");
        rootNode["publishedFileId"] = publishedFileId;
        rootNode["description"] = description;
        rootNode["language"] = language.ToString();
        rootNode["title"] = title;
        rootNode["visibility"] = visibility.ToString();
        rootNode["loadPriority"] = loadPriority;
        rootNode["version"] = "v0.0.1";
        var arrayNode = new JSONArray();
	
        foreach(var tag in GetTags()) {
			arrayNode.Add(tag);
		}
        rootNode["tags"] = arrayNode;

        var bundle = JSONNode.Parse("{}");
        content.Serialize(bundle);
        rootNode["bundle"] = bundle;

        return rootNode.ToString();
	}

	private void CreateModJSON(string filePath, ModContent content) {
        using FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        using StreamWriter writer = new StreamWriter(file);
        writer.Write(Serialize(content));
	}

	public bool ShouldTryLoad() {
		return new DirectoryInfo(modRoot).Exists && File.Exists(jsonSavePath);
	}

	public string GetStatus(out MessageType messageType) {
		if (!IsValid()) {
			messageType = MessageType.Error;
			if (!previewSprite) {
				return "Please specify the preview texture, this is required, sorry!";
			}
			if (previewSprite.rect.width > 1024 || previewSprite.rect.height > 1024) {
				return "The preview texture is expected to be around 512x512, very large preview images may cause Steam to reject the upload.";
			}

			if (string.IsNullOrEmpty(title)) {
				return "You must specify a title of the mod.";
			}

			return "For some reason this mod is invalid! The GetStatus() function must have not been updated after IsValid was changed.";
		}
		
		if (!SupportsBuildPlatform(BuildTarget.StandaloneLinux64) || !SupportsBuildPlatform(BuildTarget.StandaloneWindows64) || !SupportsBuildPlatform(BuildTarget.StandaloneOSX)) {
			messageType = MessageType.Error;
			return "Missing build support for one of the following platforms: Windows, Linux, OSX. Please use Unity Hub to install build support modules.";
		}
		
		if (!IsBuilt()) {
			messageType = MessageType.Warning;
			return $"Mod not found at build directory: {modRoot}.\nMod must be built before upload... This can take a very long time on the first run! (several hours)\nIt will be faster on subsequent runs (a few minutes).";
		}

		if (!string.IsNullOrEmpty(lastMessage)) {
			messageType = lastMessageType;
			return lastMessage;
		}

		if (!TryGetPublishedFileId(out var id)) {
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

	public void Upload(ModContent content, bool includeMetadata) {
		if (!IsBuilt()) {
			throw new UnityException("Mod must be built before upload.");
		}

		CreateIfNeeded(content, includeMetadata);
		
	}

	public bool IsValid() {
		if (!previewSprite) {
			return false;
		}

		if (string.IsNullOrEmpty(title)) {
			return false;
		}
		
		return true;
	}
	private static bool SupportsBuildPlatform(BuildTarget target) {
		var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
		if (moduleManager == null) {
			Debug.LogError("Failed to get ModuleManager type, assuming platform is supported.");
			return true;
		}
		var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		if (isPlatformSupportLoaded == null) {
			Debug.LogError("Failed to get IsPlatformSupportLoaded func, assuming platform is supported.");
			return true;
		}
		if (getTargetStringFromBuildTarget == null) {
			Debug.LogError("Failed to get GetTargetStringFromBuildTarget func, assuming platform is supported.");
			return true;
		}
		return (bool)isPlatformSupportLoaded.Invoke(null,new object[] {(string)getTargetStringFromBuildTarget.Invoke(null, new object[] {target})});
	}
	public void Build(ModContent modContent) {
		try {
			if (!IsValid()) {
				throw new Exception("Mod is not valid.");
			}
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

			CreateModJSON(jsonSavePath, modContent);

			modContent.BuildForTarget(BuildTarget.StandaloneWindows64, GetModBuildPath(BuildTarget.StandaloneWindows64));
			modContent.BuildForTarget(BuildTarget.StandaloneLinux64, GetModBuildPath(BuildTarget.StandaloneLinux64));
			modContent.BuildForTarget(BuildTarget.StandaloneOSX, GetModBuildPath(BuildTarget.StandaloneOSX));
			lastMessage = "Successfully built! Upload when ready.";
			lastMessageType = MessageType.Info;
			statusChanged?.Invoke(this, lastMessageType, lastMessage);
			Debug.Log(lastMessage);
		} catch (Exception e) {
			lastMessage = "Failed to build! Check the console to see what went wrong! You may need to clear your build cache if considerable changes have been made.";
			lastMessageType = MessageType.Error;
			statusChanged?.Invoke(this, lastMessageType, lastMessage);
			Debug.LogException(e);
			Debug.LogError(lastMessage);
			throw;
		}
	}

	private async Task ItemUpdate(ModContent content, bool uploadMetadata) {
		try {
			CreateModJSON(jsonSavePath, content);
			if (EditorUtility.DisplayCancelableProgressBar("Uploading...", "Creating upload handle...", 0f)) {
				EditorUtility.ClearProgressBar();
				StopSteamService();
				return;
			}

			if (!TryGetPublishedFileId(out var id)) {
				throw new UnityException("Failed to parse published file ID for update, is it correct?");
			}

			ugcUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), id);
			if (uploadMetadata) {
				if (!SteamUGC.SetItemDescription(ugcUpdateHandle, description)) {
					throw new UnityException("Failed to set item description.");
				}

				if (!SteamUGC.SetItemUpdateLanguage(ugcUpdateHandle, language.ToString())) {
					throw new UnityException("Failed to set item update language.");
				}

				if (!SteamUGC.SetItemTitle(ugcUpdateHandle, title)) {
					throw new UnityException("Failed to set item title.");
				}

				if (!SteamUGC.SetItemMetadata(ugcUpdateHandle, Serialize(content))) {
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
			}

			if (!SteamUGC.SetItemContent(ugcUpdateHandle, modRoot)) {
				throw new UnityException("Failed to set item content.");
			}

			var handle =
				SteamUGC.SubmitItemUpdate(ugcUpdateHandle, string.IsNullOrEmpty(changeNotes) ? null : changeNotes);
			onSubmitItemUpdateCallback.Set(handle);
			bool validStatus = true;
			while (validStatus && SteamReferenceCount > 0) {
				var status = SteamUGC.GetItemUpdateProgress(ugcUpdateHandle, out ulong punBytesProcessed,
					out ulong punBytesTotal);
				switch (status) {
					case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
						if (EditorUtility.DisplayCancelableProgressBar("Uploading...", "Preparing configuration...",
							    (float)punBytesProcessed / (float)punBytesTotal)) {
							EditorUtility.ClearProgressBar();
							StopSteamService();
							return;
						}

						break;
					case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
						if (EditorUtility.DisplayCancelableProgressBar("Uploading...", "Committing changes...",
							    (float)punBytesProcessed / (float)punBytesTotal)) {
							EditorUtility.ClearProgressBar();
							StopSteamService();
							return;
						}

						break;
					case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
						if (EditorUtility.DisplayCancelableProgressBar("Uploading...", "Preparing content...",
							    (float)punBytesProcessed / (float)punBytesTotal)) {
							EditorUtility.ClearProgressBar();
							StopSteamService();
							return;
						}

						break;
					case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
						if (EditorUtility.DisplayCancelableProgressBar("Uploading...", "Uploading preview file...",
							    (float)punBytesProcessed / (float)punBytesTotal)) {
							EditorUtility.ClearProgressBar();
							StopSteamService();
							return;
						}

						break;
					case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
						if (EditorUtility.DisplayCancelableProgressBar("Uploading...", "Uploading content...",
							    (float)punBytesProcessed / (float)punBytesTotal)) {
							EditorUtility.ClearProgressBar();
							StopSteamService();
							return;
						}

						break;
					default:
						validStatus = false;
						break;
				}

				await Task.Delay(100);
			}
		} finally {
			EditorUtility.ClearProgressBar();
			StopSteamService();
		}
	}

	private static int SteamReferenceCount;
	private static CancellationTokenSource steamRunningTokenSource = null;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init() {
		if (SteamReferenceCount > 0) {
			steamRunningTokenSource.Cancel();
			steamRunningTokenSource = null;
			Debug.Log("Steam API stopped.");
			m_SteamAPIWarningMessageHook = null;
			SteamClient.SetWarningMessageHook(null);
			SteamAPI.Shutdown();
		}
		SteamReferenceCount = 0;
		steamRunningTokenSource = null;
	}
	private static void StartSteamService() {
		if (SteamReferenceCount == 0 && steamRunningTokenSource == null) {
			if (!SteamAPI.Init()) {
				throw new UnityException("Unable to initialize Steam API. Is Steam running?");
			}
			if (m_SteamAPIWarningMessageHook == null) {
				m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
				SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
			}
			Debug.Log("Steam API initialized.");
			SteamReferenceCount++;
			steamRunningTokenSource = new CancellationTokenSource();
			_ = SteamRunUpdates(steamRunningTokenSource.Token);
		} else {
			SteamReferenceCount++;
		}
	}

	private static void StopSteamService() {
		if (SteamReferenceCount<=0) {
			return;
		}
		SteamReferenceCount--;
		if (SteamReferenceCount == 0) {
			steamRunningTokenSource.Cancel();
			steamRunningTokenSource = null;
			Debug.Log("Steam API stopped.");
			m_SteamAPIWarningMessageHook = null;
			SteamClient.SetWarningMessageHook(null);
			SteamAPI.Shutdown();
		}
	}

	private static async Task SteamRunUpdates(CancellationToken cancellationToken) {
		try {
			while (!cancellationToken.IsCancellationRequested) {
				SteamAPI.RunCallbacks();
				await Task.Delay(200, cancellationToken);
			}
		} catch (OperationCanceledException) { } catch (Exception e) {
			Debug.LogException(e);
			throw;
		}
	}
	
	

	private ModContent uploadContent;
	private bool includeMetadata;
	private void CreateIfNeeded(ModContent content, bool includeMetadata) {
		this.includeMetadata = includeMetadata;
		StartSteamService();
		uploadContent = content;
	    onCreateItemCallback = new CallResult<CreateItemResult_t>(OnCreateItem);
	    onSubmitItemUpdateCallback = new CallResult<SubmitItemUpdateResult_t>(OnSubmitItemUpdateCallback);
	    if (EditorUtility.DisplayCancelableProgressBar("Uploading...", "Creating new workshop item..", 0f)) {
		    EditorUtility.ClearProgressBar();
		    return;
	    }
		if (!TryGetPublishedFileId(out var id)) {
			var call = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
			onCreateItemCallback.Set(call);
			EditorUtility.ClearProgressBar();
		} else {
			EditorUtility.ClearProgressBar();
			CreateModJSON(jsonSavePath, uploadContent);
			_ = ItemUpdate(uploadContent, includeMetadata);
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
	        target.FindPropertyRelative("publishedFileId").stringValue = (string)rootNode["publishedFileId"];
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
        lastMessage = "Successfully loaded mod information from disk.";
        lastMessageType = MessageType.Info;
        statusChanged?.Invoke(this, lastMessageType, lastMessage);
	}

	public void TryLoadPreview(SerializedProperty target) {
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
