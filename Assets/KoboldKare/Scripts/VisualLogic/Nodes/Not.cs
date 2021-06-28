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
    public class Not : VisualLogicBaseNode {
		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public VisualLogicBaseNode input;
		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public VisualLogicBaseNode output;

		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public bool boolOutput;
		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public bool boolInput;
        public override IEnumerator Trigger(GameObject self) {
            //Trigger next nodes
            NodePort inputPort = GetInputPort("boolInput");
            if (!inputPort.TryGetInputValue<bool>(out boolOutput)) {
                boolOutput = true;
            }
            boolOutput = !boolOutput;

            NodePort port = GetOutputPort("output");
            if (port != null) {
                for (int i = 0; i < port.ConnectionCount; i++) {
                    NodePort connection = port.GetConnection(i);
                    yield return (connection.node as VisualLogicBaseNode).Trigger(self);
                }
            }
        }
        public override object GetValue(NodePort port) {
            if (port == GetOutputPort("boolOutput")) {
                return boolOutput;
            }
            return null;
        }
    }
}