using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace VisualLogic {
	public abstract class VisualLogicBaseNode : Node {
		//[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public VisualLogicBaseNode input;
		//[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] public VisualLogicBaseNode output;
		public abstract IEnumerator Trigger(GameObject self);
		public override object GetValue(NodePort port) {
			return null;
		}
	}
}
