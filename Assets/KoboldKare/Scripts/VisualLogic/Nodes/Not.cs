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
		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Multiple)] public bool boolOutput;
		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public bool boolInput;
        public override IEnumerator Trigger(GameObject self) {
            yield return null;
        }
        public override object GetValue(NodePort port) {
            if (port == GetOutputPort("boolOutput")) {
                NodePort input = GetInputPort("boolInput");
                bool test;
                if (input.TryGetInputValue<bool>(out test)) {
                    return !test;
                }
            }
            return null;
        }
    }
}