// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Text;
#endif

namespace Photon.Pun.Simple
{
	/// <summary>
	/// Base class for Vital UI components that can find source.
	/// </summary>
	public abstract class VitalsUISrcBase : MonoBehaviour
		, IOnChangeOwnedVitals
	{

		public enum MonitorSource { Auto, Owned, Self, GameObject }

		[Tooltip("Where this VitalUI will look for Vitals data.")]
		[HideInInspector] public MonitorSource monitor = MonitorSource.Auto;

		[Tooltip("Object that this VitalUI will search for an IVitalsSystem vitals data source.")]

		[HideInInspector]
		[SerializeField]
		public Object vitalsSource;

		[System.NonSerialized] public Vitals vitals;

		public abstract void OnChangeOwnedVitals(IVitalsSystem added, IVitalsSystem removed);

		protected virtual void Reset()
		{
			ApplyVitalsSource(null);
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{

		}
#endif

		#region VitalsSource inspector field and Property

		public virtual IVitalsSystem ApplyVitalsSource(Object srcObj)
		{
			GameObject vitalsSrcGO;
			Component vitalsSrcComp;

			if (monitor == MonitorSource.Auto)
			{
				if (srcObj == null)
				{
					srcObj = (Object)GetComponentInParent<IVitalsSystem>();
					monitor = MonitorSource.Self;
				}

				if (srcObj == null)
				{
					srcObj = (Object)OwnedIVitals.LastItem;
					monitor = MonitorSource.Owned;
				}
			}

			/// Override the value if it doesn't conform to the type being monitored.
			if (monitor == MonitorSource.Owned)
			{
				var ownedVitals = OwnedIVitals.LastItem;
				vitalsSrcComp = ownedVitals as Component;
				vitalsSrcGO = null;
			}
			else if (monitor == MonitorSource.Self)
			{
				vitalsSrcGO = gameObject;
				vitalsSrcComp = null;
			}
			// Auto - try self first - if no vitals found fall back to owned
			else
			{
				vitalsSource = srcObj;
				vitalsSrcGO = srcObj as GameObject;
				vitalsSrcComp = srcObj as Component;
			}

			IVitalsSystem vs;

			/// Get the actual value we want
			if (vitalsSrcGO)
			{
				vs = FindIVitalComponentOnGameObj(vitalsSrcGO);

				if (vs != null)
					vitalsSource = (vs as Component).gameObject;

			}
			else if (vitalsSrcComp)
			{
				vs = vitalsSrcComp as IVitalsSystem;

				if (monitor == MonitorSource.GameObject)
					vitalsSource = vitalsSrcComp.gameObject;
			}
			else
			{
				vs = null;
				vitalsSource = null;
			}

			return vs;

		}

		#endregion

		private static IVitalsSystem FindIVitalComponentOnGameObj(GameObject go)
		{
			/// May be null because vitalsSource is a gameoject, need to turn that into a vitalcomp
			if (go)
			{
				IVitalsSystem ivitalcomp = go.GetComponentInParent<IVitalsSystem>();
				if (ivitalcomp == null)
					ivitalcomp = go.GetComponentInChildren<IVitalsSystem>();
				return ivitalcomp;
			}
			return null;
		}

	}

#if UNITY_EDITOR

	[CustomEditor(typeof(VitalsUISrcBase))]
	[CanEditMultipleObjects]
	public class VitalsUISrcBaseEditor : ReactorHeaderEditor
	{
		public static StringBuilder strb = new StringBuilder(64);

		protected VitalsUISrcBase _target;

		SerializedProperty monitor, vitalsSource;
		bool vitalSourceExpanded = true;

		public override void OnEnable()
		{
			base.OnEnable();
			_target = (VitalsUISrcBase)target;
			_target.ApplyVitalsSource(_target.vitalsSource);

			monitor = serializedObject.FindProperty("monitor");
			vitalsSource = serializedObject.FindProperty("vitalsSource");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			vitalSourceExpanded = EditorGUILayout.Foldout(vitalSourceExpanded, "Vitals Data Source");

			if (vitalSourceExpanded)
			{

				BeginVerticalBox();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(monitor);
				if (EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();
					if (!Application.isPlaying)
						_target.ApplyVitalsSource(_target.vitalsSource);
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.ObjectField(vitalsSource, typeof(Object));
				if (EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();
					if (!Application.isPlaying)
						_target.ApplyVitalsSource(_target.vitalsSource);
				}


				EndVerticalBox();
			}

		}

	}

#endif
}

