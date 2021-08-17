using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using XNode;
using static XNode.Node;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualLogic {
#if UNITY_EDITOR
    [CustomEditor(typeof(Wait))]
    public class WaitEditor : Editor {
        public override void OnInspectorGUI() {
            EditorGUILayout.HelpBox("Graphs aren't safe to run asyncronously. Ensure that variable fetching doesn't cross over the wait.", MessageType.Warning);
            DrawDefaultInspector();
        }
    }
#endif
    //[NodeTint("#CCCCFF")]
	[NodeWidth(300)]
    public class Wait : VisualLogicBaseNode {
		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public VisualLogicBaseNode input;
        public float seconds;
		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public VisualLogicBaseNode output;
        public override IEnumerator Trigger(GameObject self) {
            yield return new WaitForSeconds(seconds);
            //Trigger next nodes
            NodePort port = GetOutputPort("output");
            if (port != null) {
                for (int i = 0; i < port.ConnectionCount; i++) {
                    NodePort connection = port.GetConnection(i);
                    yield return (connection.node as VisualLogicBaseNode).Trigger(self);
                }
            }
        }
    }

}