using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PhotonProfiler))]
public class PhotonProfilerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        ((PhotonProfiler)target).OnInspectorGUI();
    }
}
#endif

public class PhotonProfiler : MonoBehaviour {
    private int selectedFrame;
    private const int maxFrames = 200;
    private string selectedStack;
    private static PhotonProfiler instance;
    private class ByteFrame {
        private Dictionary<string, int> data;
        public Dictionary<string,int>.Enumerator GetEnumerator() {
            return data.GetEnumerator();
        }
        
        public int GetTotalByteCount() {
            int total = 0;
            foreach (var pair in data) {
                total += pair.Value;
            }
            return total;
        }

        public ByteFrame() {
            data = new Dictionary<string, int>();
        }
        public void Log(string stack, int byteCount) {
            if (data.ContainsKey(stack)) {
                data[stack] += byteCount;
            } else {
                data.Add(stack, byteCount);
            }
        }
    }

    private Queue<ByteFrame> receiveFrames;
    private Queue<ByteFrame> sentFrames;
    private ByteFrame currentSentFrame;
    private ByteFrame currentReceivedFrame;

    private int GetMaxByteTotal(Queue<ByteFrame> frames) {
        int maxTotal = 0;
        foreach (var frame in frames) {
            maxTotal = Mathf.Max(frame.GetTotalByteCount(), maxTotal);
        }
        return maxTotal;
    }

    public static void LogRecieve(int byteCount) {
        instance.currentReceivedFrame.Log(Environment.StackTrace, byteCount);
    }

    public static void LogSend(int byteCount) {
        instance.currentSentFrame.Log(Environment.StackTrace, byteCount);
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }
        
        instance = this;
        receiveFrames = new Queue<ByteFrame>();
        sentFrames = new Queue<ByteFrame>();
        currentSentFrame = new ByteFrame();
        currentReceivedFrame = new ByteFrame();
    }

    private void LateUpdate() {
        receiveFrames.Enqueue(currentReceivedFrame);
        sentFrames.Enqueue(currentSentFrame);
        currentReceivedFrame = new ByteFrame();
        if (receiveFrames.Count > maxFrames) {
            receiveFrames.Dequeue();
        }
        if (sentFrames.Count > maxFrames) {
            sentFrames.Dequeue();
        }
    }

#if UNITY_EDITOR
    public void OnInspectorGUI() {
        if (receiveFrames == null) {
            EditorGUILayout.LabelField("Profiler is only available while playing.");
            return;
        }

        GUILayout.Space(200f);
        EditorGUI.DrawRect(new Rect(0, 0, Screen.width, 200f), Color.black);
        int maxHeight = GetMaxByteTotal(receiveFrames);
        float heightAspect = 200f / (float)maxHeight;
        float x = Screen.width*(1f-(float)receiveFrames.Count/(float)maxFrames);
        int frameNum = 0;
        foreach (var frame in receiveFrames) {
            int total = frame.GetTotalByteCount();
            float width = ((float)maxFrames / (float)Screen.width);
            float xPosition = x + frameNum * width;
            //float totalBarHeight = (float)frame.GetTotalByteCount() / (float)maxHeight;
            float currentY = 0f;
            foreach (var data in frame) {
                GUI.Button(new Rect(xPosition, currentY, width, data.Value), Texture2D.whiteTexture);
                currentY += data.Value;
            }
            frameNum++;
        }
    }
#endif
}
