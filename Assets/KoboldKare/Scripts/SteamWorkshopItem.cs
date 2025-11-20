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
using UnityEngine.VFX;

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
	
	[SerializeField, HideInInspector] private string guid;
	
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

	private string GetGUID() {
		if (string.IsNullOrEmpty(guid)) {
			guid = System.Guid.NewGuid().ToString();
		}

		return guid;
	}
	

	[Serializable]
	public abstract class ModContent {
		public abstract AssetBundleBuild[] GetBuilds(string uniqueString);

		public AssetBundleManifest BuildForTarget(BuildTarget target, string modBuildPath, string uniqueString) {
			if (!Directory.Exists(modBuildPath)) {
				Directory.CreateDirectory(modBuildPath);
			}
			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles( modBuildPath, GetBuilds(uniqueString), BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension , target);
			if (manifest) {
				foreach(var bundleName in manifest.GetAllAssetBundles()) {
					Debug.Log($"Successfully created AssetBundle {bundleName} at {modBuildPath}/{bundleName}");
				}
			} else {
				throw new UnityException("Build failed, see console for details.");
			}

			return manifest;
		}
		public abstract void Serialize(JSONNode node, string uniqueString);

		public static void GetShaderAssets(string[] assetNames, string uniqueString, out string[] shaderDepArray) {
			var shaderVariantCollection = new ShaderVariantCollection();
			var shaderDeps = new HashSet<string>();
			foreach (var asset in assetNames) {
				var deps = AssetDatabase.GetDependencies(asset, true);
				foreach (var dep in deps) {
					var t = AssetDatabase.GetMainAssetTypeAtPath(dep);
					if (t == typeof(Shader) || t == typeof(VisualEffectAsset)) {
						shaderDeps.Add(dep);
					}
					if (t == typeof(Material)) {
						var mat = AssetDatabase.LoadAssetAtPath<Material>(dep);
						if (!mat.shader) {
							continue;
						}

						var keywords = mat.shaderKeywords ?? Array.Empty<string>();
						try {
							shaderVariantCollection.Add(new ShaderVariantCollection.ShaderVariant(mat.shader, PassType.ScriptableRenderPipeline, keywords));
						} catch (ArgumentException e) {
						}
						try {
							shaderVariantCollection.Add(new ShaderVariantCollection.ShaderVariant(mat.shader, PassType.ScriptableRenderPipelineDefaultUnlit, keywords));
						} catch (ArgumentException e) {
						}
						try {
							shaderVariantCollection.Add(new ShaderVariantCollection.ShaderVariant(mat.shader, PassType.Meta, keywords));
						} catch (ArgumentException e) {
						}
						try {
							shaderVariantCollection.Add(new ShaderVariantCollection.ShaderVariant(mat.shader, PassType.Normal, keywords));
						} catch (ArgumentException e) {
						}
						try {
							shaderVariantCollection.Add(new ShaderVariantCollection.ShaderVariant(mat.shader, PassType.MotionVectors, keywords));
						} catch (ArgumentException e) {
						}
						try {
							shaderVariantCollection.Add(new ShaderVariantCollection.ShaderVariant(mat.shader, PassType.ShadowCaster, keywords));
						} catch (ArgumentException e) {
						}
					}
				}
			}
			AssetDatabase.CreateAsset(shaderVariantCollection, $"Assets/ModShaderVariantCollection_{uniqueString}.asset");
			shaderDeps.Add($"Assets/ModShaderVariantCollection_{uniqueString}.asset");
			shaderDepArray = shaderDeps.ToArray();
		}
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
		
		protected virtual string[] GetAssetAddressableNames(string uniqueString) {
			HashSet<string> addressableNames = new HashSet<string>();
			foreach(var asset in playableCharacters) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var asset in cosmeticItems) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var asset in fruits) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var asset in equipmentStoreItems) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var asset in dicks) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var asset in plants) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var asset in reagents) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var asset in reagentReactions) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var asset in equipment) {
				addressableNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			return addressableNames.ToArray();
		}

		private JSONArray GetAssetNames(ICollection<UnityEngine.Object> assets, string uniqueString) {
			var arrayNode = new JSONArray();
			HashSet<string> assetNames = new HashSet<string>();
			foreach(var asset in assets) {
				assetNames.Add($"{AssetDatabase.GetAssetPath(asset)}_{uniqueString}");
			}
			foreach(var assetName in assetNames) {
				arrayNode.Add(assetName);
			}

			return arrayNode;
		}
		
		public override void Serialize(JSONNode rootNode, string uniqueString) {
			rootNode["BundleName"] = $"bundle_{uniqueString}";
			rootNode["ShaderBundleName"] = $"shaderbundle_{uniqueString}";
			rootNode["PlayableCharacter"] = GetAssetNames(playableCharacters, uniqueString);
			rootNode["Fruit"] = GetAssetNames(fruits, uniqueString);
			rootNode["Penis"] = GetAssetNames(dicks, uniqueString);
			rootNode["Reaction"] = GetAssetNames(reagentReactions, uniqueString);
			rootNode["Reagent"] = GetAssetNames(reagents, uniqueString);
			rootNode["Plant"] = GetAssetNames(plants, uniqueString);
			rootNode["Seed"] = GetAssetNames(seeds, uniqueString);
			rootNode["Cosmetic"] = GetAssetNames(cosmeticItems, uniqueString);
			rootNode["Equipment"] = GetAssetNames(equipment, uniqueString);
			rootNode["EquipmentStoreItem"] = GetAssetNames(equipmentStoreItems, uniqueString);
		}

		public override AssetBundleBuild[] GetBuilds( string uniqueString ) {
			var assets = GetAssets();
			GetShaderAssets(assets, uniqueString, out var shaderDeps);
			AssetBundleBuild shaderBuild = new AssetBundleBuild {
				assetBundleName = $"shaderBundle_{uniqueString}",
				assetNames = shaderDeps.ToArray(),
			};
			AssetBundleBuild build = new AssetBundleBuild {
				assetBundleName = $"bundle_{uniqueString}",
				assetNames = assets,
				addressableNames = GetAssetAddressableNames(uniqueString),
			};
			return new[] { build, shaderBuild };
		}
	}
	
	[Serializable]
	public class ModScene : ModObjects {
		public SceneAsset scene;
		public Sprite sceneIcon;
		public string sceneTitle;
		public string sceneDescription;
		public override AssetBundleBuild[] GetBuilds( string uniqueString ) {
			var otherBuilds = base.GetBuilds(uniqueString);
			var scenePath = AssetDatabase.GetAssetPath(scene);
			GetShaderAssets(new [] {scenePath}, uniqueString, out var shaderDeps);
			for (int i = 0; i < otherBuilds.Length; i++) {
				if (otherBuilds[i].assetBundleName == $"shaderBundle_{uniqueString}") {
					var currentAssets = new HashSet<string>(otherBuilds[i].assetNames);
					foreach(var shaderDep in shaderDeps) {
						currentAssets.Add(shaderDep);
					}
					otherBuilds[i].assetNames = currentAssets.ToArray();
				}
			}
			AssetBundleBuild sceneBuild = new AssetBundleBuild {
				assetBundleName = $"sceneBundle_{uniqueString}",
				assetNames = new[] { scenePath },
			};
			List<AssetBundleBuild> allBuilds = new List<AssetBundleBuild>(otherBuilds) { sceneBuild };
			return allBuilds.ToArray();
		}
		
		protected override string[] GetAssets() {
			HashSet<string> assets = new HashSet<string>(base.GetAssets()) { AssetDatabase.GetAssetPath(sceneIcon) };
			return assets.ToArray();
		}

		public override void Serialize(JSONNode node, string uniqueString) {
			base.Serialize(node, uniqueString);
			node["SceneBundleName"] = $"scenebundle_{uniqueString}";
			node["ShaderBundleName"] = $"shaderbundle_{uniqueString}";
			node["Scene"] = $"{AssetDatabase.GetAssetPath(scene)}_{uniqueString}";
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

	public string GetModBuildPath(BuildTarget? target) {
		if (target != null) {
			return $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}mods{Path.DirectorySeparatorChar}{GetFileNameSafeTitle()}{Path.DirectorySeparatorChar}{target.Value}";
		} else {
			return $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}mods{Path.DirectorySeparatorChar}{GetFileNameSafeTitle()}{Path.DirectorySeparatorChar}Universal";
		}
	}
	private string modRoot => $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}mods{Path.DirectorySeparatorChar}{GetFileNameSafeTitle()}";

	private string jsonSavePath => $"{modRoot}{Path.DirectorySeparatorChar}info.json";
	private string previewTexturePath => $"{modRoot}{Path.DirectorySeparatorChar}preview.png";

	private string Serialize(ModContent content, string uniqueString) {
		JSONNode rootNode = JSONNode.Parse("{}");
        rootNode["publishedFileId"] = publishedFileId;
        rootNode["description"] = description;
        rootNode["language"] = language.ToString();
        rootNode["title"] = title;
        rootNode["visibility"] = visibility.ToString();
        rootNode["loadPriority"] = loadPriority;
        rootNode["version"] = "v0.0.2";
        var arrayNode = new JSONArray();
	
        foreach(var tag in GetTags()) {
			arrayNode.Add(tag);
		}
        rootNode["tags"] = arrayNode;

        var bundle = JSONNode.Parse("{}");
        content.Serialize(bundle, uniqueString);
        rootNode["bundle"] = bundle;

        return rootNode.ToString();
	}

	private void CreateModJSON(string filePath, ModContent content, string uniqueString) {
        using FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        using StreamWriter writer = new StreamWriter(file);
        writer.Write(Serialize(content,uniqueString));
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

			CreateModJSON(jsonSavePath, modContent, GetGUID());
			
			var universalBuildPath = GetModBuildPath(null);
			Directory.CreateDirectory(universalBuildPath);

			var windowsBuildPath = GetModBuildPath(BuildTarget.StandaloneWindows64);
			var windowsManifest = modContent.BuildForTarget(BuildTarget.StandaloneWindows64, windowsBuildPath, GetGUID());
			foreach(var bundleName in windowsManifest.GetAllAssetBundles()) {
				if (!bundleName.Contains("shaderbundle")) {
					var sourcePath = $"{windowsBuildPath}/{bundleName}";
					var destPath = $"{universalBuildPath}/{bundleName}";
					File.Move(sourcePath, destPath);
					File.Move($"{sourcePath}.manifest", $"{destPath}.manifest");
				}
			}
			var windowsManifestPath = $"{windowsBuildPath}/StandaloneWindows64";
			File.Delete(windowsManifestPath);
			File.Delete($"{windowsManifestPath}.manifest");
			
			var linuxBuildPath = GetModBuildPath(BuildTarget.StandaloneLinux64);
			var linuxManifest = modContent.BuildForTarget(BuildTarget.StandaloneLinux64, linuxBuildPath, GetGUID());
			foreach(var bundleName in linuxManifest.GetAllAssetBundles()) {
				if (!bundleName.Contains("shaderbundle")) {
					var sourcePath = $"{linuxBuildPath}/{bundleName}";
					var manifestPath = $"{linuxBuildPath}/{bundleName}.manifest";
					File.Delete(sourcePath);
					File.Delete(manifestPath);
				}
			}
			
			var linuxManifestPath = $"{linuxBuildPath}/StandaloneLinux64";
			File.Delete(linuxManifestPath);
			File.Delete($"{linuxManifestPath}.manifest");
			
			var macBuildPath = GetModBuildPath(BuildTarget.StandaloneOSX);
			var macManifest = modContent.BuildForTarget(BuildTarget.StandaloneOSX, macBuildPath, GetGUID());
			foreach(var bundleName in macManifest.GetAllAssetBundles()) {
				if (!bundleName.Contains("shaderbundle")) {
					var sourcePath = $"{macBuildPath}/{bundleName}";
					var manifestPath = $"{macBuildPath}/{bundleName}.manifest";
					File.Delete(sourcePath);
					File.Delete(manifestPath);
				}
			}
			
			var macManifestPath = $"{macBuildPath}/StandaloneOSX";
			File.Delete(macManifestPath);
			File.Delete($"{macManifestPath}.manifest");
			
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
			CreateModJSON(jsonSavePath, content, GetGUID());
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

				if (!SteamUGC.SetItemMetadata(ugcUpdateHandle, Serialize(content, GetGUID()))) {
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
			CreateModJSON(jsonSavePath, uploadContent, GetGUID());
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

	public void OnValidate() {
		if (string.IsNullOrEmpty(guid)) {
			guid = Guid.NewGuid().ToString();
		}
	}
}

#endif
