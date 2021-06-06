// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
	public class AutoZoom : MonoBehaviour
	{
		public static List<Transform> watched = new List<Transform>();
		public const float MAX_FOV = 75;
		public const float MIN_FOV = 15;

		[Range(.1f, .5f)]
		[HideInInspector] public float window = .25f;
		[ValueType("/sec")]
		[HideInInspector] public float panRate = 20f;
		[ValueType("/sec")]
		[HideInInspector] public float zoomRate = 200f;

		// Cache
		private Camera cam;

		private void Awake()
		{
			cam = GetComponent<Camera>();

			/// if not on a camera, this is a watched transform
			if (!cam)
				watched.Add(transform);
		}

		private void OnDestroy()
		{
			if (watched.Contains(transform))
				watched.Remove(transform);

		}

		private void LateUpdate()
		{
			if (!cam || !cam.isActiveAndEnabled)
				return;

			Bounds bounds = new Bounds();

			for (int i = 0; i < watched.Count; ++i)
			{
				Vector2 screenpoint = cam.WorldToViewportPoint(watched[i].position);
				bounds.Encapsulate(screenpoint + new Vector2(-0.5f, -0.5f));
			}

			if (watched.Count > 0)
			{
				cam.transform.Rotate(new Vector3(0, 1, 0), (bounds.center.x /*- .25f*/) * Time.deltaTime * panRate);
				float xAngle = bounds.extents.x - window;
				float yAngle = bounds.extents.y - window;
				float angle = xAngle > yAngle ? xAngle : yAngle;

				float fov = cam.fieldOfView + angle * Time.deltaTime * zoomRate;

				cam.fieldOfView = Mathf.Clamp(fov, MIN_FOV, MAX_FOV);
			}
		}


#if UNITY_EDITOR

		[CustomEditor(typeof(AutoZoom))]
		[CanEditMultipleObjects]
		public class AutoZoomEditor : AutomationHeaderEditor
		{
			protected override string Instructions
			{
				get
				{
					return "Attach this component to your Camera as well as any GameObjects you would like the keep in Camera view. Automatically adjusts the FOV to ensure objects are visible.";
				}
			}

			protected override void OnInspectorGUIInjectMiddle()
			{
				base.OnInspectorGUIInjectMiddle();

				var sp = serializedObject.GetIterator();
				sp.Next(true);

				/// We only want to put the controls in the inspector if this is attached to a camera.
				if ((target as AutoZoom).GetComponent<Camera>())
				{
					sp = serializedObject.FindProperty("window");
					EditorGUI.BeginChangeCheck();
					do
					{
						EditorGUILayout.PropertyField(sp);
					} while (sp.Next(false));

					if (EditorGUI.EndChangeCheck())
					{
						sp.serializedObject.ApplyModifiedProperties();
					}
				}
			}
		}

#endif
	}
}

