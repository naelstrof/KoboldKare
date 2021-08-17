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
    public class Branch : VisualLogicBaseNode {
		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public VisualLogicBaseNode input;
		[Input(backingValue = ShowBackingValue.Unconnected, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public bool shouldBranch;
		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public VisualLogicBaseNode pass, fail;
        public override IEnumerator Trigger(GameObject self) {
            //Trigger next nodes
            NodePort inputPort = GetInputPort("shouldBranch");
            shouldBranch = inputPort.GetInputValue<bool>();
            NodePort port = shouldBranch ? GetOutputPort("pass") : GetOutputPort("fail");
            if (port != null) {
                for (int i = 0; i < port.ConnectionCount; i++) {
                    NodePort connection = port.GetConnection(i);
                    yield return (connection.node as VisualLogicBaseNode).Trigger(self);
                }
            }
        }
    }

}