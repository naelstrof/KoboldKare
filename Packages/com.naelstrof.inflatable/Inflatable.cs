using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Naelstrof.Inflatable {
    public class Inflatable {
        private float currentSize;
        private float targetSize;
        private bool tweenRunning = false;
        private AnimationCurve bounceCurve;
        private float bounceDuration;
        private List<InflatableListener> listeners;
        
        public bool SetSize(float newSize, out IEnumerator tween) {
            targetSize = newSize;
            if (!tweenRunning) {
                tweenRunning = true;
                tween = TweenToNewSize();
                return true;
            }
            tween = null;
            return false;
        }

        public float GetSize() {
            return targetSize;
        }

        private IEnumerator TweenToNewSize() {
            float startSize = currentSize;
            float startTime = Time.time;
            float endTime = Time.time+bounceDuration;
            while (Time.time < endTime) {
                float t = (Time.time - startTime) / bounceDuration;
                currentSize = Mathf.LerpUnclamped(startSize, targetSize, bounceCurve.Evaluate(t));
                foreach (InflatableListener listener in listeners) {
                    listener.OnSizeChanged(currentSize);
                }
                yield return null;
            }

            currentSize = targetSize;
            foreach (InflatableListener listener in listeners) {
                listener.OnSizeChanged(currentSize);
            }
            tweenRunning = false;
        }

        public Inflatable(AnimationCurve bounceCurve, float bounceDuration) {
            listeners = new List<InflatableListener>();
            this.bounceDuration = bounceDuration;
            this.bounceCurve = bounceCurve;
        }
        
        public void AddListener(InflatableListener listener) {
            listener.OnEnable();
            listener.OnSizeChanged(currentSize);
            listeners.Add(listener);
        }
    }
}
