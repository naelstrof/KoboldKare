using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This WOULD've just simply inherited HashSet<T>, but due to Unity concurrenting the crap out of serialization, it was impossible to enumerate over it.
// (See post #5 here: https://forum.unity.com/threads/iserializationcallbackreceiver-throwing-exceptions.329917/)

// So instead we just have this dumbo wrapper class because fuck me lmao
[System.Serializable]
public class SerializableHashSet<T> : ISerializationCallbackReceiver {
    public HashSet<T> hashSet = new HashSet<T>();
    [SerializeField] public List<T> values = new List<T>();

    public void OnBeforeSerialize() {
        values = new List<T>(hashSet);
    }

    public void OnAfterDeserialize() {
        hashSet = new HashSet<T>(values);
    }
}
