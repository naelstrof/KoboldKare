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
        if (!src || !src.texture) return null;

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

        foreach (var resource in assetsHandle.Result) {
            var handle = Addressables.LoadAssetAsync<PlayableMap>(resource);
            PlayableMap map = await handle.Task;
            if (!map) {
                Addressables.Release(handle);
                continue;
            }
            var copy = DeepCopyPlayableMap(map);
            Addressables.Release(handle);
            
            if (!ModManager.TryUnloadModThatProvidesAssetNow(resource, out var stub)) {
                copy.stub = null;
                PlayableMapDatabase.AddPlayableMap(copy);
                addedPlayableMaps.Add(copy);
                continue;
            }
            copy.stub = stub;
            PlayableMapDatabase.AddPlayableMap(copy);
            addedPlayableMaps.Add(copy);
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
        addedPlayableMaps.Clear();
        return Task.CompletedTask;
    }
}
