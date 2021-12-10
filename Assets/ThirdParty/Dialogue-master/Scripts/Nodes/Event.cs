using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using KoboldKare;
namespace Dialogue {
	[NodeTint("#FFFFAA")]
	public class Event : DialogueBaseNode {

		public GameEventGeneric[] trigger; // Could use UnityEvent here, but UnityEvent has a bug that prevents it from serializing correctly on custom EditorWindows. So i implemented my own.

		public override void Clear() {
		}

		public override void Trigger(int priority) {
			for (int i = 0; i < trigger.Length; i++) {
				trigger[i].Raise(null);
			}
		}
	}
}