using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PlayableMapPostProcessor : ModPostProcessor {
    private struct ModStubPlayableMapPair {
        public ModManager.ModStub stub;
        public PlayableMap playableMap;
    }
    private List<ModStubPlayableMapPair> addedPlayableMaps;

    public override void Awake() {
        base.Awake();
        addedPlayableMaps = new ();
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
    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        if (locator.Locate(searchLabel.RuntimeKey, typeof(GameObject), out var locations)) {
            foreach (var resource in locations) {
                var handle = Addressables.LoadAssetAsync<PlayableMap>(resource);
                PlayableMap map = await handle.Task;
                if (!map) {
                    Addressables.Release(handle);
                    continue;
                }
                var copy = DeepCopyPlayableMap(map);
                Addressables.Release(handle);
                copy.stub = new ModManager.ModStub(data);
                PlayableMapDatabase.AddPlayableMap(copy);
                addedPlayableMaps.Add(new ModStubPlayableMapPair() {
                    playableMap = copy,
                    stub = new ModManager.ModStub(data)
                });
            }
        }
    }
    

    public override async Task HandleAssetBundleMod(ModManager.ModInfoData data, AssetBundle bundle) {
        var node = data.assets;
        if (node.HasKey("Scene")) {
            PlayableMap playableMap = ScriptableObject.CreateInstance<PlayableMap>();
            var request = bundle.LoadAssetAsync<Sprite>(node["SceneIcon"]);
            var icon = await request.AsSingleAssetTask<Sprite>();
            string sceneTitle = node.HasKey("SceneTitle") ? node["SceneTitle"] : "Unknown Map";
            string sceneDescription = node.HasKey("SceneDescription") ? node["SceneDescription"] : "No description provided.";
            playableMap.SetFromBundle(data.GetSceneBundleLocation(), node["Scene"], sceneTitle, icon, sceneDescription);
            var copy = DeepCopyPlayableMap(playableMap);
            PlayableMapDatabase.AddPlayableMap(copy);
            addedPlayableMaps.Add(new ModStubPlayableMapPair() {
                playableMap = copy,
                stub = new ModManager.ModStub(data)
            });
        }
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedPlayableMaps.Count;i++) {
            if(addedPlayableMaps[i].stub.GetRepresentedBy(data)) {
                PlayableMapDatabase.RemovePlayableMap(addedPlayableMaps[i].playableMap);
                addedPlayableMaps.RemoveAt(i);
                i--;
            }
        }
        return base.UnloadAssets(data);
    }
}
