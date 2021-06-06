using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vilar.AnimationStation {
	
	[CreateAssetMenu(fileName = "Data", menuName = "Data/AnimationMotion", order = 1)]
	public class AnimationMotion : ScriptableObject {

		public AnimationCurve X = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
		public AnimationCurve Y = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
		public AnimationCurve Z = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
		public Vector3 scale = Vector3.zero;
		public Vector3 timescale = Vector3.one;
		public Vector3 offset = Vector3.zero;
		public AnimationCurve RX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
		public AnimationCurve RY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
		public AnimationCurve RZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
		public Vector3 scaleR = Vector3.zero;
		public Vector3 timescaleR = Vector3.one;
		public Vector3 offsetR = Vector3.zero;

	}

}