
#if OCULUS

#if USING_XR_MANAGEMENT && USING_XR_SDK_OCULUS
#define USING_XR_SDK
#endif

using System.Text;
using UnityEngine;

#if UNITY_2017_2_OR_NEWER
using InputTracking = UnityEngine.XR.InputTracking;
using Node = UnityEngine.XR.XRNode;
#else
using InputTracking = UnityEngine.VR.InputTracking;
using Node = UnityEngine.VR.VRNode;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
	public struct OVRInputFrame
	{
		public Vector3 bodyPos, bodyRot, headPos, handPosL, handPosR;
		public Vector2 stickL, stickR;
		public Quaternion headRot, handRotL, handRotR;
		public bool a, b, x, y, thumbL, thumbR, gripL, gripR, triggerL, triggerR;
		public float triggerLValue, triggerRValue, gripLValue, gripRValue;

		private static StringBuilder sb;
		public override string ToString()
		{
			if (sb == null)
				sb = new StringBuilder();

			sb.Length = 0;
			sb.Append("Body: ").Append(bodyPos).Append(" ").Append(bodyRot).Append("\n");
			sb.Append("Head: ").Append(headPos).Append(" ").Append(headRot).Append("\n");
			sb.Append("Left: ").Append(handPosL).Append(" ").Append(handRotL).Append("\n");
			sb.Append("Rght: ").Append(handPosR).Append(" ").Append(handRotR).Append("\n");
			sb.Append("Stick ").Append(stickL).Append(" <-> ").Append(stickR).Append("\n");

			sb.Append("Bttns ").Append(a).Append(" ").Append(b).Append(" ").Append(x).Append(" ").Append(y).Append("\n");
			sb.Append("Thumb ").Append(thumbL).Append(" ").Append(thumbR).Append("\n");
			sb.Append("Grip ").Append(gripL).Append(" ").Append(gripR).Append("\n");
			sb.Append("Trigger ").Append(triggerL).Append(" ").Append(triggerR).Append("\n");

			return sb.ToString();
		}
	}

	public class SimpleOVRInputSync : NetComponent
		, IOnPreSimulate
	{


		public OVRInputFrame PollInputs()
		{
			return new OVRInputFrame()
			{
				bodyPos = PollVInput(Node.HardwareTracker, OVRPlugin.Node.TrackerOne, NodeStatePropertyType.Position),
				bodyRot = PollVInput(Node.TrackingReference, OVRPlugin.Node.TrackerOne, NodeStatePropertyType.Orientation),

				headPos = PollVInput(Node.HardwareTracker, OVRPlugin.Node.Head, NodeStatePropertyType.Position),
				headRot = PollQInput(Node.Head, OVRPlugin.Node.Head, NodeStatePropertyType.Orientation),

				handPosL = PollVInput(Node.LeftHand, OVRPlugin.Node.HandLeft, NodeStatePropertyType.Position),
				handRotL = PollQInput(Node.LeftHand, OVRPlugin.Node.HandLeft, NodeStatePropertyType.Orientation),

				handPosR = PollVInput(Node.RightHand, OVRPlugin.Node.HandRight, NodeStatePropertyType.Position),
				handRotR = PollQInput(Node.RightHand, OVRPlugin.Node.HandRight, NodeStatePropertyType.Orientation),

				stickR = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick),
				stickL = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick),

				a = OVRInput.Get(OVRInput.Button.One),
				b = OVRInput.Get(OVRInput.Button.Two),
				x = OVRInput.Get(OVRInput.Button.Three),
				y = OVRInput.Get(OVRInput.Button.Four),

				thumbL = OVRInput.Get(OVRInput.Button.SecondaryThumbstick),
				thumbR = OVRInput.Get(OVRInput.Button.PrimaryThumbstick),

				gripL = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger),
				gripR = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger),

				triggerL = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger),
				triggerR = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger),

				triggerLValue = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger),
				triggerRValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger),

				gripLValue = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger),
				gripRValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger),
			};
		}

		private Vector3 PollVInput(Node node, OVRPlugin.Node ovrNode, NodeStatePropertyType type)
		{
			Vector3 value;
			OVRNodeStateProperties.GetNodeStatePropertyVector3(node, type, ovrNode, OVRPlugin.Step.Render, out value);
			return value;
		}

		private Quaternion PollQInput(Node node, OVRPlugin.Node ovrNode, NodeStatePropertyType type)
		{
			Quaternion value;
			OVRNodeStateProperties.GetNodeStatePropertyQuaternion(node, type, ovrNode, OVRPlugin.Step.Render, out value);
			return value;
				
		}

		public void OnPreSimulate(int frameId, int subFrameId)
		{
			var val = PollInputs();
			SimpleOVRDebug.Clear();
			SimpleOVRDebug.Log(val.ToString());
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(SimpleOVRInputSync))]
	[CanEditMultipleObjects]
	public class SimpleOVRInputSyncEditor : HeaderEditorBase
	{

	}
#endif

}

#endif