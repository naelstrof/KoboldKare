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
	[NodeWidth(300)]
    public class TriggerUnityEvent : VisualLogicBaseNode {
		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public VisualLogicBaseNode input;
        public UnityEvent eventTrigger;
		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public VisualLogicBaseNode output;
        public override IEnumerator Trigger(GameObject self) {
            eventTrigger.Invoke();
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