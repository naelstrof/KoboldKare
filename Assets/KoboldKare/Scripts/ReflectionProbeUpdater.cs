using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using UnityEngine;

public class ReflectionProbeUpdater : MonoBehaviour {
    //public List<GameEvent> gameEventsToTriggerOn = new List<GameEvent>();
    public List<ReflectionProbe> probes = new List<ReflectionProbe>();
    //private bool rendering;
    //public void Awake() {
    //foreach( GameEvent e in gameEventsToTriggerOn) {
    //e.RegisterListener(this);
    //}
    //}
    //public void OnDestroy() {
    //foreach( GameEvent e in gameEventsToTriggerOn) {
    //e.UnregisterListener(this);
    //}
    //}
    public void Start() {
        StartCoroutine(UpdateProbes());
    }
    IEnumerator UpdateProbes() {
        //rendering = true;
        foreach (ReflectionProbe p in probes) {
            //p.resolution = 32;
            p.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
            //p.GetComponent<HDAdditionalReflectionData>().resolution = PlanarReflectionAtlasResolution.PlanarReflectionResolution64;
            //HDAdditionalReflectionDataExtensions.RequestRenderNextUpdate(p);
            p.RenderProbe(); // Force it to render, to prevent stutter later
            yield return new WaitForEndOfFrame();
        }
        //rendering = false;
    }
    //public void OnEventRaised(GameEvent e) {
        //if (!rendering) {
            //rendering = true;
            //StartCoroutine(UpdateProbes());
        //}
    //}
}
