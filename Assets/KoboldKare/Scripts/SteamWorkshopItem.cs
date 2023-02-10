using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PenetrationTech;
using SimpleJSON;
using UnityEngine;

#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class SteamWorkshopItem {
	[System.Flags]
	public enum SteamWorkshopItemTag {
		Characters = 1,
		Maps = 2,
		Items = 4,
		Plants = 8,
		Reagents = 16,
	}

	public enum SteamWorkshopLanguage {
		english,
		arabic,
		bulgarian,
		schinese,
		tchinese,
		czech,
		danish,
		dutch,
		finnish,
		french,
		german,
		greek,
		hungarian,
		italian,
		japanese,
		koreana,
		norwegian,
		polish,
		portuguese,
		brazilian,
		romanian,
		russian,
		spanish,
		latam,
		swedish,
		thai,
		turkish,
		ukrainian,
		vietnamese,
	}


	[SerializeField] private AddressableAssetGroup targetGroup;
	[SerializeField] private PublishedFileId_t publishedFileId = PublishedFileId_t.Invalid;
	[SerializeField] private SteamWorkshopLanguage language;
	[SerializeField] private ERemoteStoragePublishedFileVisibility visibility;
	[SerializeField] private string title;
	[SerializeField,TextArea] private string description;
	[SerializeField,TextArea] private string changeNotes;
	[SerializeField] private SteamWorkshopItemTag tags;
	[SerializeField] private AssetReferenceTexture previewTexture;
	[SerializeField, ReadOnly] private string previewTexturePath;

	private UGCUpdateHandle_t ugcUpdateHandle;
	private bool uploading = false;
	private bool building = false;
	
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
			if (Enum.TryParse(node.ToString(), out SteamWorkshopItemTag tag)) {
				allTags |= tag;
			}
		}
		return allTags;
	}
	
	private CallResult<CreateItemResult_t> onCreateItemCallback;
	private void OnCreateItem(CreateItemResult_t result, bool bIOFailure) {
		if (bIOFailure || result.m_eResult != EResult.k_EResultOK) {
			throw new UnityException($"Failed to upload workshop item, error code: {result}");
		}
		if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement) {
			throw new UnityException("Apparently you need to accept the workshop legal agreement, I have no idea what I need to do for that sorry!");
		}
		publishedFileId = result.m_nPublishedFileId;
		if (publishedFileId == PublishedFileId_t.Invalid) {
			throw new UnityException("Failed to upload, couldn't get a published file ID!");
		}
		ItemUpdate();
	}
	private CallResult<SubmitItemUpdateResult_t> onSubmitItemUpdateCallback;

	private void OnSubmitItemUpdateCallback(SubmitItemUpdateResult_t result, bool bIOFailure) {
		Debug.Log(result.m_eResult);
		uploading = false;
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
			string fullPathWithBuildTarget = targetGroup.GetSchema<BundledAssetGroupSchema>().BuildPath
				.GetValue(targetGroup.Settings);
			DirectoryInfo fullPath = Directory.GetParent(fullPathWithBuildTarget);
			return fullPath.FullName;
		}
	}

	private string jsonSavePath => $"{modRoot}/info.json";

	private string Serialize() {
		JSONNode rootNode = JSONNode.Parse("{}");
        rootNode["publishedFileId"] = publishedFileId.ToString();
        rootNode["description"] = description;
        rootNode["language"] = language.ToString();
        rootNode["title"] = title;
        rootNode["previewTexture"] = previewTexture.AssetGUID;
        var arrayNode = new JSONArray();
	
        foreach(var tag in GetTags()) {
			arrayNode.Add(tag.ToString());
		}
        rootNode["tags"] = arrayNode;
        return rootNode.ToString();
	}

	private void Save() {
        using FileStream file = new FileStream(jsonSavePath, FileMode.OpenOrCreate, FileAccess.Write);
        using StreamWriter writer = new StreamWriter(file);
        writer.Write(Serialize());
	}

	public void Upload() {
		//Build();
		CreateIfNeeded();
	}

	private void Build() {
		try {
			building = true;
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
				BuildTarget.StandaloneWindows64);
			AddressableAssetSettings.BuildPlayerContent();
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
			AddressableAssetSettings.BuildPlayerContent();
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
			AddressableAssetSettings.BuildPlayerContent();
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
			AddressableAssetSettings.BuildPlayerContent();
			Save();
		} finally {
			building = false;
		}
	}

	private void ItemUpdate() {
		ugcUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), publishedFileId);
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

		var handle = SteamUGC.SubmitItemUpdate(ugcUpdateHandle, changeNotes);
		uploading = true;
		onSubmitItemUpdateCallback.Set(handle);
	}

	private void CreateIfNeeded() {
	    onCreateItemCallback = new CallResult<CreateItemResult_t>(OnCreateItem);
	    onSubmitItemUpdateCallback = new CallResult<SubmitItemUpdateResult_t>(OnSubmitItemUpdateCallback);
		if (publishedFileId == PublishedFileId_t.Invalid) {
			var call = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
			onCreateItemCallback.Set(call);
		} else {
			ItemUpdate();
		}
	}

	private void Load() {
		using FileStream file = new FileStream(jsonSavePath, FileMode.Open, FileAccess.Read);
        using StreamReader reader = new StreamReader(file);
        var rootNode = JSONNode.Parse(reader.ReadToEnd());
        if (rootNode.HasKey("publishedFileID")) {
	        if (!ulong.TryParse(rootNode["publishedFileID"].ToString(), out ulong output)) {
		        throw new UnityException(
			        $"Failed to parse publishedFileID in file {jsonSavePath} as ulong... Invalid mod info format or corruption?");
	        }
	        publishedFileId = (PublishedFileId_t)output;
        }

        if (rootNode.HasKey("description")) {
	        description = rootNode["description"];
        }
        if (rootNode.HasKey("title")) {
	        title = rootNode["title"];
        }
        if (rootNode.HasKey("tags")) {
	        JSONArray array = rootNode["tags"].AsArray;
	        tags = GetTagsFromJsonArray(array);
        }

        if (rootNode.HasKey("previewTexture")) {
	        previewTexture = new AssetReferenceTexture(rootNode["previewTexture"]);
        }
	}
}

#endif
