using XNode;

namespace Dialogue {
	public abstract class DialogueBaseNode : Node {
		[Input(backingValue = ShowBackingValue.Never)] public DialogueBaseNode input;
		[Output(backingValue = ShowBackingValue.Never)] public DialogueBaseNode output;

		abstract public void Trigger(int priority);

		public override object GetValue(NodePort port) {
			return null;
		}
		abstract public void Clear();

	}
}
