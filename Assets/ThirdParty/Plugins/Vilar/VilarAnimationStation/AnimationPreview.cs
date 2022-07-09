using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vilar.IK;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vilar.AnimationStation {
	
#if UNITY_EDITOR
	[CustomEditor(typeof(AnimationPreview))]
	public class AnimationPreviewInspector : Editor {

		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawHandles(AnimationPreview script, GizmoType gizmoType) {
			if (!Application.isPlaying) {
				script.Idle();
			}
			SceneView.RepaintAll();
		}

		void OnScene(SceneView sceneview) {
		}

	}
#endif

	public class AnimationPreview : MonoBehaviour {

		public static AnimationPreview instance;
		public static void EnsureInstance() {
			if (instance == null) instance = GameObject.FindObjectOfType<AnimationPreview>();
		}
		public static void SetTarget(int index, Vector3 position, Quaternion rotation) {
			EnsureInstance();
			instance._SetTarget(index, position, rotation);
		}
		public static void MovePreviews(Vector3 position, Quaternion rotation) {
			EnsureInstance();
			instance._MovePreviews(position, rotation);
		}
		public static void Solve() { instance._Solve(); }
		public static void Initialize() {
			EnsureInstance();
			instance._Initialize();
		}

		private void OnEnable() {
			instance = this;
			_Initialize();
		}

		public IKSolver IKSolver;
		private int previewCountdown=1;

		public void Idle() {
			previewCountdown--;
			if (previewCountdown==0) {
				Initialize();
			}
		}

		[ContextMenu("INITIALIZE")]
		public void _Initialize() {
			//transform.position = Vector3.zero;
			//transform.rotation = Quaternion.identity;
			IKSolver.Initialize();
			transform.position = -Vector3.up * 100f;
		}

		public void _MovePreviews(Vector3 position, Quaternion rotation) {
			previewCountdown = 3;
			transform.position = position;
			transform.rotation = rotation;
		}

		public void _SetTarget(int index, Vector3 position, Quaternion rotation) {
			IKSolver.SetTarget(index, position, rotation);
		}

		public void _Solve() {
			IKSolver.Solve();
		}

	}

}