using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PlayableMapPostProcessor : ModPostProcessor {
    private List<PlayableMap> addedPlayableMaps;

    public override void Awake() {
        base.Awake();
        addedPlayableMaps = new List<PlayableMap>();
    }

    private Sprite DeepCopySprite(Sprite src) {
        if (src == null || src.texture == null) return null;

        Texture2D srcTex = src.texture;
        Rect texRect = src.textureRect;
        int w = (int)texRect.width;
        int h = (int)texRect.height;

        RenderTexture rt = RenderTexture.GetTemporary(srcTex.width, srcTex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        RenderTexture prev = RenderTexture.active;
        try {
            Graphics.Blit(srcTex, rt);
            RenderTexture.active = rt;

            Texture2D copyTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            copyTex.ReadPixels(new Rect(texRect.x, texRect.y, w, h), 0, 0);
            copyTex.Apply();

            Vector2 pivotNorm = new Vector2(src.pivot.x / src.rect.width, src.pivot.y / src.rect.height);
            Sprite newSprite = Sprite.Create(copyTex, new Rect(0, 0, w, h), pivotNorm, src.pixelsPerUnit, 0, SpriteMeshType.FullRect, src.border);
            newSprite.name = src.name + "_copy";
            return newSprite;
        } finally {
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
        }
    }
    
    private struct PlayableMapLoadInfo {
        public IResourceLocation location;
        public ModManager.ModStub? stub;
    }
    
    private PlayableMap DeepCopyPlayableMap(PlayableMap src) {
        var playableMapCopy = Object.Instantiate(src);
        Sprite newSprite = DeepCopySprite(src.GetPreview());
        playableMapCopy.SetPreview(newSprite);
        return playableMapCopy;
    }
    public override async Task LoadAllAssets() {
        addedPlayableMaps.Clear();
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        List<PlayableMapLoadInfo> playableMaps = new List<PlayableMapLoadInfo>();
        
        // Only allow one map to be loaded at a time...
        foreach (var resource in assetsHandle.Result) {
            if (ModManager.TryUnloadModThatProvidesAssetNow(resource, out var stub)) {
                playableMaps.Add(new PlayableMapLoadInfo() {
                    location = resource,
                    stub = stub
                });
            } else {
                playableMaps.Add(new PlayableMapLoadInfo() {
                    location = resource,
                    stub = null
                });
            }
        }
        Addressables.Release(assetsHandle);

        foreach (var loadInfo in playableMaps) {
            if (loadInfo.stub == null) {
                var builtInMapHandle = Addressables.LoadAssetAsync<PlayableMap>(loadInfo.location);
                PlayableMap builtInMap = await builtInMapHandle.Task;
                var copy = DeepCopyPlayableMap(builtInMap);
                copy.stub = null;
                PlayableMapDatabase.AddPlayableMap(copy);
                addedPlayableMaps.Add(copy);
                Addressables.Release(builtInMapHandle);
                continue;
            }

            var opHandle = Addressables.LoadAssetAsync<PlayableMap>(loadInfo.location);
            PlayableMap map = await opHandle.Task;
            if (!map) {
                Addressables.Release(opHandle);
                continue;
            }

            var playableMapCopy = DeepCopyPlayableMap(map);
            playableMapCopy.stub = loadInfo.stub;
            PlayableMapDatabase.AddPlayableMap(playableMapCopy);
            addedPlayableMaps.Add(playableMapCopy);
            Addressables.Release(opHandle);
        }
    }

    public override async Task HandleAssetBundleMod(ModManager.ModAssetBundle mod) {
        var node = mod.info.assets;
        if (node.HasKey("Scene")) {
            PlayableMap playableMap = ScriptableObject.CreateInstance<PlayableMap>();
            var icon = await mod.bundle.LoadAssetAsync<Sprite>(node["SceneIcon"]).AsSingleAssetTask<Sprite>();
            string sceneTitle = node.HasKey("SceneTitle") ? node["SceneTitle"] : "Unknown Map";
            string sceneDescription = node.HasKey("SceneDescription") ? node["SceneDescription"] : "No description provided.";
            playableMap.SetFromBundle(mod.GetSceneBundleLocation(), node["Scene"], sceneTitle, icon, sceneDescription);
            var copy = DeepCopyPlayableMap(playableMap);
            PlayableMapDatabase.AddPlayableMap(copy);
            addedPlayableMaps.Add(copy);
        }
    }

    public override Task UnloadAllAssets() {
        foreach (var playableMap in addedPlayableMaps) {
            PlayableMapDatabase.RemovePlayableMap(playableMap);
        }
        return Task.CompletedTask;
    }
}
