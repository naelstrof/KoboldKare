using System.Collections.Generic;
using XNode;

namespace Dialogue {
	public class DialogueRandomBranch : DialogueBaseNode {
		[Output(dynamicPortList = true)] public List<DialogueBaseNode> possiblePaths;

		public override void Trigger(int priority) {
			int rng = UnityEngine.Random.Range(0, possiblePaths.Count - 1);
            NodePort port = GetOutputPort("possiblePaths " + rng);
            if (port == null) return;
            for (int i = 0; i < port.ConnectionCount; i++) {
                NodePort connection = port.GetConnection(i);
                (connection.node as DialogueBaseNode).Trigger(priority);
            }
		}

		public override object GetValue(NodePort port) {
			return null;
		}
		public override void Clear() {
		}
	}
}
