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
    public class GetBlackboardValue : VisualLogicBaseNode {
        public string blackboardName;
		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Multiple)] public bool outputValue;
        public override IEnumerator Trigger(GameObject self) {
            yield return null;
        }
        public override object GetValue(NodePort port) {
            if ((graph as VisualLogicGraph).blackboard.ContainsKey(blackboardName)) {
                return (graph as VisualLogicGraph).blackboard[blackboardName];
            }
            return null;
        }
    }
}