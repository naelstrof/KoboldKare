using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class ExplosionReaction : ReagentReaction {
    private class ExplosionBehaviour : MonoBehaviourPun {
        public AudioPack sizzle;
        public GameObject explosion;
        public GenericReagentContainer container;
        private void Start() {
            GameManager.instance.SpawnAudioClipInWorld(sizzle, transform.position);
            StartCoroutine(ExplosionRoutine());
        }
        private IEnumerator ExplosionRoutine() {
            yield return new WaitForSeconds(3f);
            if (photonView.IsMine) {
                PhotonNetwork.Instantiate(explosion.name, transform.position, Quaternion.identity);
                container.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.All, container.volume);
            }
        }
    }

    [SerializeField]
    private AudioPack sizzle;
    [SerializeField]
    private GameObject explosion;
    public override void React(GenericReagentContainer container) {
        base.React(container);
        if (!container.TryGetComponent(out ExplosionBehaviour behaviour)) {
            behaviour = container.gameObject.AddComponent<ExplosionBehaviour>();
            behaviour.sizzle = sizzle;
            behaviour.explosion = explosion;
            behaviour.container = container;
        }
    }
}
