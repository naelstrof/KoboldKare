using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using KoboldKare;
namespace VisualLogic {
	//[NodeTint("#666622")]
	public class Event : VisualLogicBaseNode {
		public enum EventType {
			OnUse,
			Start,
			Update,
		};
		public EventType eventType;
		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict )]
		public VisualLogicBaseNode eventListener;

		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.None, dynamicPortList = true)]
		public List<bool> outputParameters = new List<bool>();
		public object[] parameters;
		public override IEnumerator Trigger(GameObject self) {
            NodePort port = GetOutputPort("eventListener");
            if (port != null) {
                for (int i = 0; i < port.ConnectionCount; i++) {
                    NodePort connection = port.GetConnection(i);
                    yield return (connection.node as VisualLogicBaseNode).Trigger(self);
                }
            }
		}
		public override object GetValue(NodePort port) {
			for(int i=0;i<outputParameters.Count;i++) {
				if (port.fieldName == "outputParameters "+i) {
					return parameters[i];
				}
			}
			return null;
		}
	}
}