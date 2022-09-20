using PenetrationTech;
using Photon.Pun;
using UnityEngine;

public class Dildo : MonoBehaviour {
    private Penetrator listenPenetrator;
    public delegate void PenetrateAction(Penetrator penetrator, Penetrable penetrable);
    public static event PenetrateAction dildoPenetrateStart;
    public static event PenetrateAction dildoPenetrateEnd;

    private void Awake() {
        listenPenetrator = GetComponentInChildren<Penetrator>();
    }

    private void OnEnable() {
        listenPenetrator.penetrationStart += OnPenetrationStart;
        listenPenetrator.penetrationEnd += OnPenetrationEnd;
    }

    private void OnDisable() {
        listenPenetrator.penetrationStart -= OnPenetrationStart;
        listenPenetrator.penetrationEnd -= OnPenetrationEnd;
    }

    private void OnPenetrationStart(Penetrable penetrable) {
        dildoPenetrateStart?.Invoke(listenPenetrator, penetrable);
    }
    private void OnPenetrationEnd(Penetrable penetrable) {
        dildoPenetrateEnd?.Invoke(listenPenetrator, penetrable);
    }
}
