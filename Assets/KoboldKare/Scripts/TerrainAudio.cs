using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainAudio : MonoBehaviour {
    private Terrain targetTerrain;
    [System.Serializable]
    public class TerrainLayerPhysicsAudioGroupTuple {
        public TerrainLayerPhysicsAudioGroupTuple(TerrainLayer l) {
            layer = l;
        }
        public TerrainLayer layer;
        public PhysicMaterial material;
    }
    public List<TerrainLayerPhysicsAudioGroupTuple> pairs = new List<TerrainLayerPhysicsAudioGroupTuple>();
    void Start() {
        targetTerrain = GetComponent<Terrain>();
        if (!Application.isEditor) {
            return;
        }
        if (pairs.Count != targetTerrain.terrainData.terrainLayers.Length) {
            pairs.Clear();
            foreach (TerrainLayer l in targetTerrain.terrainData.terrainLayers) {
                pairs.Add(new TerrainLayerPhysicsAudioGroupTuple(l));
            }
        }
    }
    public PhysicMaterial GetMaterialAtPoint(Vector3 contactPoint) {
        int dominantIndex = getDominantTexture(contactPoint);
        if (dominantIndex < pairs.Count && pairs[dominantIndex] != null) {
            return pairs[dominantIndex].material;
        }
        return null;
    }
    private float[] getTextureMix(Vector3 worldPos) {
        int mapX = (int)(((worldPos.x - targetTerrain.transform.position.x) / targetTerrain.terrainData.size.x) * targetTerrain.terrainData.alphamapWidth);
        int mapZ = (int)(((worldPos.z - targetTerrain.transform.position.z) / targetTerrain.terrainData.size.z) * targetTerrain.terrainData.alphamapHeight);

        float[,,] splatmapData = targetTerrain.terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

        for (int i = 0; i < cellMix.Length; i++) {
            cellMix[i] = splatmapData[0, 0, i];
        }

        return cellMix;
    }

    private int getDominantTexture(Vector3 worldPos) {
        float[] mix = getTextureMix(worldPos);

        float maxMix = 0;
        int maxMixIndex = 0;

        for (int j = 0; j < mix.Length; j++) {
            if (mix[j] > maxMix) {
                maxMixIndex = j;
                maxMix = mix[j];
            }
        }

        return maxMixIndex;
    }
}
