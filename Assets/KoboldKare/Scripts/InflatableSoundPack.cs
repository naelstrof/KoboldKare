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

        public InflatableSoundPack(AudioPack pack, AudioSource source) {
            soundPack = pack;
            this.source = source;
        }

        public override void OnEnable() {
            source.enabled = false;
        }
        public override void OnSizeChanged(float newSize) {
            source.enabled = newSize > 0f;
            if (!source.isPlaying && newSize > 0f) {
                soundPack.Play(source);
            }
            source.volume = Mathf.Clamp01(newSize);
        }
    }
}
