using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RealtimeReflectionProbe : MonoBehaviour {
    public ReflectionProbe probeA;
    public ReflectionProbe probeB;
    public ReflectionProbe outputProbe;
    private RenderTexture outputTexture;
    public float updateInterval = 3f;
    private Vector3 APos, BPos;
    private bool flip;
    private Camera cachedCamera;
    private bool grassEnabled = true;
    private Camera cam {
        get {
            if (cachedCamera == null || !cachedCamera.isActiveAndEnabled) {
                cachedCamera = Camera.current;
            }
            if (cachedCamera == null || !cachedCamera.isActiveAndEnabled) {
                cachedCamera = Camera.main;
            }
            return cachedCamera;
        }
    }
    public void Start() {
        outputTexture = new RenderTexture(probeB.resolution, probeB.resolution, 0, RenderTextureFormat.ARGBHalf);
        outputTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        outputTexture.useMipMap = true;
        probeA.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
        probeB.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
        probeB.RenderProbe();
        outputProbe.customBakedTexture = outputTexture;
        StartCoroutine(UpdateReflectionProbeOnInterval(updateInterval));
    }
    public void OnDestroy() {
        outputTexture.Release();
    }
    private void OnEnable() {
        RenderPipelineManager.beginCameraRendering += OnBeginRender;
    }
    private void OnDisable() {
        RenderPipelineManager.beginCameraRendering -= OnBeginRender;
        foreach (var t in Terrain.activeTerrains) {
            t.drawTreesAndFoliage = true;
        }
    }
    void OnBeginRender(ScriptableRenderContext context, Camera camera) {
        if (camera == cam) {
            if (!grassEnabled) {
                foreach (var t in Terrain.activeTerrains) {
                    t.drawTreesAndFoliage = true;
                }
            }
            grassEnabled = true;
        } else {
            if (grassEnabled) {
                foreach (var t in Terrain.activeTerrains) {
                    t.drawTreesAndFoliage = false;
                }
                grassEnabled = false;
            }
        }
    }
    public IEnumerator UpdateReflectionProbeOnInterval(float interval) {
        while(probeA != null && probeB != null && outputProbe != null) {
            int renderID = -1;
            if (flip) {
                if (cam!=null) {
                    probeB.transform.position = cam.transform.position;
                }
                renderID = probeB.RenderProbe();
                BPos = probeB.transform.position;
                yield return new WaitUntil(()=>probeB.IsFinishedRendering(renderID));
            } else {
                if (cam!=null) {
                    probeA.transform.position = cam.transform.position;
                }
                renderID = probeA.RenderProbe();
                APos = probeA.transform.position;
                yield return new WaitUntil(()=>probeA.IsFinishedRendering(renderID));
            }
            //probeA.enabled = false;
            float blend = 0f;
            while(blend < 1f) {
                blend = Mathf.Clamp01(blend + (1f/updateInterval) * Time.fixedDeltaTime);
                ReflectionProbe.BlendCubemap(probeB.realtimeTexture, probeA.realtimeTexture, flip ? 1f-blend : blend, outputTexture);
                outputProbe.transform.position = Vector3.Lerp(BPos, APos, flip ? 1f-blend : blend);
                yield return new WaitForFixedUpdate();
            }
            flip = !flip;
        }
    }
}
