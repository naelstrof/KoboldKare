using System;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public class BufferPool : MonoBehaviour {
    private static BufferPool instance;
    [ThreadStatic]
    private BitBuffer bitBuffer;
    private Dictionary<int,byte[]> byteArrays;
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }

        instance = this;
        bitBuffer = new BitBuffer(1024);
        byteArrays = new Dictionary<int, byte[]>();
    }
    public static BitBuffer GetBitBuffer() {
        return instance.bitBuffer;
    }

    public static byte[] GetArrayBuffer(int length) {
        if (instance.byteArrays.ContainsKey(length)) {
            return instance.byteArrays[length];
        }
        instance.byteArrays.Add(length, new byte[length]);
        return instance.byteArrays[length];
    }
}
