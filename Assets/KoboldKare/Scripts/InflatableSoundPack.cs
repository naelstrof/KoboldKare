using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JigglePhysics;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Naelstrof.Easing;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class InflatableSoundPack : InflatableListener {
        [SerializeField] private AudioPack soundPack;
        [SerializeField] private AudioSource source;
        [SerializeField] private MonoBehaviour component;
        private WaitForSeconds waitTime;
        private float volume;
        
        private Coroutine routine;

        public InflatableSoundPack(AudioPack pack, AudioSource source, MonoBehaviour component) {
            soundPack = pack;
            this.source = source;
            this.component = component;
            source.loop = false;
        }

        public override void OnEnable() {
            source.enabled = false;
            waitTime = new WaitForSeconds(1f);
        }
        public override void OnSizeChanged(float newSize) {
            volume = Mathf.Clamp01(newSize);
            if (newSize <= 0f) {
                source.enabled = false;
                if (routine != null) {
                    component.StopCoroutine(routine);
                }
            } else {
                source.enabled = true;
                if (routine == null) {
                    routine = component.StartCoroutine(PlayGurgles());
                }
            }
        }

        private IEnumerator PlayGurgles() {
            while (component.isActiveAndEnabled && source.enabled) {
                if (!source.isPlaying) {
                    yield return waitTime;
                    soundPack.Play(source);
                    source.volume = volume;
                }
                yield return null;
            }

            routine = null;
        }
    }
}
