using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class Inflatable {
        private float currentSize;
        private float targetSize;
        private bool tweenRunning = false;
        //[SerializeField] private AnimationCurve bounceCurve;
        //[SerializeField] private float bounceDuration;
        [SerializeField] private InflatableCurve bounce;

        public delegate void ChangedEvent(float newValue);

        public event ChangedEvent changed;
        
        [SerializeReference, SerializeReferenceButton]
        public List<InflatableListener> listeners = new List<InflatableListener>();

        public ReadOnlyCollection<InflatableListener> readOnlyListeners;
        public ReadOnlyCollection<InflatableListener> GetInflatableListeners() {
            return readOnlyListeners ??= listeners.AsReadOnly();
        }

        private bool initialized = false;

        private bool SetSize(float newSize, out IEnumerator tween) {
            targetSize = newSize;
            if (!tweenRunning) {
                tweenRunning = true;
                tween = TweenToNewSize();
                return true;
            }
            tween = null;
            return false;
        }

        public void SetSize(float newSize, MonoBehaviour tweener) {
            if (!initialized) {
                Debug.LogError("Inflatable wasn't initialized.", tweener);
                throw new UnityException("Inflatable wasn't initialized ");
            }

            if (tweener.isActiveAndEnabled) {
                if (SetSize(newSize, out IEnumerator tween)) {
                    tweener.StartCoroutine(tween);
                }
            } else {
                SetSizeInstant(newSize);
            }
        }

        public void SetSizeInstant(float newSize) {
            foreach (InflatableListener listener in listeners) {
                listener.OnSizeChanged(newSize);
            }
        }

        public void OnEnable() {
            foreach (var listener in listeners) {
                listener.OnEnable();
            }

            initialized = true;
        }

        public float GetSize() {
            return targetSize;
        }

        private IEnumerator TweenToNewSize() {
            float startSize = currentSize;
            float startTime = Time.time;
            float endTime = Time.time+bounce.GetBounceDuration();
            while (Time.time < endTime) {
                float t = (Time.time - startTime) / bounce.GetBounceDuration();
                currentSize = Mathf.LerpUnclamped(startSize, targetSize, bounce.EvaluateCurve(t));
                foreach (InflatableListener listener in listeners) {
                    listener.OnSizeChanged(currentSize);
                }
                changed?.Invoke(currentSize);
                yield return null;
            }

            currentSize = targetSize;
            foreach (InflatableListener listener in listeners) {
                listener.OnSizeChanged(currentSize);
            }
            changed?.Invoke(currentSize);
            tweenRunning = false;
        }
        public void AddListener(InflatableListener listener) {
            if (initialized) {
                listener.OnEnable();
                listener.OnSizeChanged(currentSize);
            }

            listeners.Add(listener);
        }
    }
}
