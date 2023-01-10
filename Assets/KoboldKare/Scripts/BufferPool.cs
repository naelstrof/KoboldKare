using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using NetStack.Serialization;
using UnityEngine;

public class BufferPool : MonoBehaviour {
    private static BufferPool instance;
    [ThreadStatic]
    private BitBuffer bitBuffer;
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }

        instance = this;
        bitBuffer = new BitBuffer(1024);
    }
    public static BitBuffer GetBitBuffer() {
        instance.bitBuffer.Clear();
        return instance.bitBuffer;
    }
    public static short SerializeBitBuffer(StreamBuffer outStream, object customObject) {
        BitBuffer buffer = (BitBuffer)customObject;
        short size = (short)buffer.Length;
        byte[] bytes = new byte[size];
        buffer.ToArray(bytes);
        outStream.Write(bytes, 0, size);
        return size;
    }
    public static object DeserializeBitBuffer(StreamBuffer inStream, short length) {
        byte[] bytes = new byte[length];
        inStream.Read(bytes, 0, length);
        BitBuffer buffer = GetBitBuffer();
        buffer.FromArray(bytes, length);
        return buffer;
    }
}
