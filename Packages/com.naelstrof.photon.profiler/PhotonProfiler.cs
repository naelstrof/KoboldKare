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
    private void OnSceneGUI() {
        Repaint();
    }
}
#endif

public class PhotonProfiler : MonoBehaviour {
    private int selectedFrame;
    private const int maxFrames = 200;
    private ByteFrame selectedByteFrame;
    private KeyValuePair<string,int>? selectedStackBytes;
    private List<KeyValuePair<string, int>> reuseSortedByteFrame;
    private static PhotonProfiler instance;
    private bool foldoutAnalysis = false;
    private class ByteFrame : Dictionary<string,int> {
        public int GetTotalByteCount() {
            int total = 0;
            foreach (var pair in this) {
                total += pair.Value;
            }
            return total;
        }
        public void Log(string stack, int byteCount) {
            if (ContainsKey(stack)) {
                this[stack] += byteCount;
            } else {
                Add(stack, byteCount);
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

    public static void LogReceive(int byteCount) {
#if UNITY_EDITOR
        instance.currentReceivedFrame.Log(Environment.StackTrace, byteCount);
#endif
    }

    public static void LogSend(int byteCount) {
#if UNITY_EDITOR
        instance.currentSentFrame.Log(Environment.StackTrace, byteCount);
#endif
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }
        
        instance = this;
#if UNITY_EDITOR
        receiveFrames = new Queue<ByteFrame>();
        sentFrames = new Queue<ByteFrame>();
        currentSentFrame = new ByteFrame();
        currentReceivedFrame = new ByteFrame();
#endif
    }

    private void LateUpdate() {
#if UNITY_EDITOR
        receiveFrames.Enqueue(currentReceivedFrame);
        sentFrames.Enqueue(currentSentFrame);
        currentReceivedFrame = new ByteFrame();
        if (receiveFrames.Count > maxFrames) {
            receiveFrames.Dequeue();
        }
        if (sentFrames.Count > maxFrames) {
            sentFrames.Dequeue();
        }
#endif
    }

#if UNITY_EDITOR
    public void OnInspectorGUI() {
        if (receiveFrames == null) {
            EditorGUILayout.LabelField("Profiler is only available while playing.");
            return;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        Rect screen = GUILayoutUtility.GetLastRect();
        
        GUILayout.Space(200f);
        EditorGUI.DrawRect(new Rect(screen.x, screen.y, screen.width, 200f), Color.black);
        int maxHeight = GetMaxByteTotal(receiveFrames);
        float heightAspect = 200f / (float)maxHeight;
        float x = screen.x+screen.width*(1f-(float)receiveFrames.Count/(float)maxFrames);
        int frameNum = 0;
        foreach (var frame in receiveFrames) {
            int width = Mathf.Max((int)((float)screen.width / (float)maxFrames), 1);
            float xPosition = x + frameNum * width;
            //float totalBarHeight = (float)frame.GetTotalByteCount() / (float)maxHeight;
            if (xPosition > screen.width) {
                break;
            }

            int height = Mathf.Max((int)(frame.GetTotalByteCount() * heightAspect), 1);
            Rect buttonRect = new Rect(xPosition, 200+screen.y-height, width, height);
            if (GUI.Button(buttonRect, Texture2D.whiteTexture)) {
                HandlePress(frame, null);
            }
            frameNum++;
        }

        if (selectedByteFrame != null) {
            foldoutAnalysis = EditorGUILayout.Foldout(foldoutAnalysis, "Frame analysis");
            if (foldoutAnalysis) {
                reuseSortedByteFrame ??= new List<KeyValuePair<string, int>>();
                reuseSortedByteFrame.Clear();
                reuseSortedByteFrame.AddRange(selectedByteFrame);
                reuseSortedByteFrame.Sort(SortByteData);
                foreach (var byteGroup in reuseSortedByteFrame) {
                    if (GUILayout.Button(byteGroup.Value.ToString())) {
                        selectedStackBytes = byteGroup;
                    }
                }
            }
        }

        if (selectedStackBytes != null) {
            EditorGUILayout.HelpBox($"{selectedStackBytes.Value.Value.ToString()} bytes were sent via the following stack trace:\n {selectedStackBytes.Value.Key}",
                MessageType.Info);
        }

        EditorUtility.SetDirty(gameObject);
    }

    private int SortByteData(KeyValuePair<string, int> a, KeyValuePair<string, int> b) {
        return b.Value.CompareTo(a.Value);
    }

    private void HandlePress(ByteFrame frame, KeyValuePair<string, int>? stackBytes) {
        if (EditorApplication.isPaused == false) {
            EditorApplication.isPaused = true;
        }
        selectedStackBytes = stackBytes;
        selectedByteFrame = frame;
    }
#endif
}
