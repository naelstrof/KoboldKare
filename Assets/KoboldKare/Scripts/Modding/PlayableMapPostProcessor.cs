using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    public override async Task LoadAllAssets() {
        addedPlayableMaps.Clear();
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        foreach (var resource in assetsHandle.Result) {
            var opHandle = Addressables.LoadAssetAsync<PlayableMap>(resource);
            PlayableMap map = await opHandle.Task;
            if (!map) {
                Addressables.Release(opHandle);
                continue;
            }
            var playableMapCopy = Object.Instantiate(map);

            Sprite newSprite = DeepCopySprite(map.GetPreview());
            playableMapCopy.SetPreview(newSprite);
            
            if (ModManager.TryUnloadModThatProvidesAssetNow(resource, out var stub)) {
                Debug.Log(playableMapCopy.name + " is provided by mod " + stub.title + ", assigning stub.");
                playableMapCopy.stub = stub;
            }
            PlayableMapDatabase.AddPlayableMap(playableMapCopy);
            addedPlayableMaps.Add(playableMapCopy);
            Addressables.Release(opHandle);
        }
        Addressables.Release(assetsHandle);
    }

    public override async Task HandleAssetBundleMod(ModManager.ModAssetBundle mod) {
        var node = mod.info.assets;
        if (node.HasKey("Scene")) {
            PlayableMap playableMap = ScriptableObject.CreateInstance<PlayableMap>();
            var icon = await mod.bundle.LoadAssetAsync<Object>(node["SceneIcon"]).AsSingleAssetTask();
            string sceneTitle = node.HasKey("SceneTitle") ? node["SceneTitle"] : "Unknown Map";
            string sceneDescription = node.HasKey("SceneDescription") ? node["SceneDescription"] : "No description provided.";
            playableMap.SetFromBundle(mod.GetSceneBundleLocation(), node["Scene"], sceneTitle, icon as Sprite, sceneDescription);
            PlayableMapDatabase.AddPlayableMap(playableMap);
            addedPlayableMaps.Add(playableMap);
        }
    }

    public override Task UnloadAllAssets() {
        foreach (var playableMap in addedPlayableMaps) {
            PlayableMapDatabase.RemovePlayableMap(playableMap);
        }
        return Task.CompletedTask;
    }
}
