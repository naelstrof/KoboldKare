using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using XNode;
using static XNode.Node;

namespace VisualLogic {
    //[NodeTint("#CCCCFF")]
    public class Destroy : VisualLogicBaseNode {
		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public VisualLogicBaseNode input;
		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public VisualLogicBaseNode output;

		[Input(backingValue = ShowBackingValue.Unconnected, connectionType = ConnectionType.Override)] public UnityEngine.Object objectInput;
        public override IEnumerator Trigger(GameObject self) {
            //Trigger next nodes
            NodePort inputPort = GetInputPort("objectInput");
            UnityEngine.Object obj;
            if (inputPort.TryGetInputValue<UnityEngine.Object>(out obj)) {
                GameObject.Destroy(obj);
            } else if (objectInput != null) {
                GameObject.Destroy(objectInput);
            }

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