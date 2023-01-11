using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using NetStack.Serialization;
using UnityEngine;

public class BufferPool : MonoBehaviour {
    private static BufferPool instance;
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }

        instance = this;
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
        
        BitBuffer buffer = new BitBuffer(length/4+1);
        buffer.FromArray(bytes, length);
        return buffer;
    }
}
