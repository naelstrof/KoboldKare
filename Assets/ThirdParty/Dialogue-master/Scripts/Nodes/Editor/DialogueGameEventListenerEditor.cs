using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Dialogue {
    [CustomNodeEditor(typeof(DialogueGameEventListener))]
    public class DialogueGameEventListenerEditor : NodeEditor {

        public override void OnBodyGUI() {
            serializedObject.Update();

            DialogueGameEventListener node = target as DialogueGameEventListener;

            if (node.events.Count == 0) {
                GUILayout.BeginHorizontal();
                NodeEditorGUILayout.PortField(GUIContent.none, target.GetInputPort("input"), GUILayout.MinWidth(0));
                NodeEditorGUILayout.PortField(GUIContent.none, target.GetOutputPort("output"), GUILayout.MinWidth(0));
                GUILayout.EndHorizontal();
            } else {
                NodeEditorGUILayout.PortField(GUIContent.none, target.GetInputPort("input"));
            }

            GUILayout.Space(-20);

            //NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("text"), GUIContent.none);
            NodeEditorGUILayout.DynamicPortList("events", typeof(DialogueBaseNode), serializedObject, NodePort.IO.Output, Node.ConnectionType.Override);

            serializedObject.ApplyModifiedProperties();
        }

        public override int GetWidth() {
            return 300;
        }
    }
}