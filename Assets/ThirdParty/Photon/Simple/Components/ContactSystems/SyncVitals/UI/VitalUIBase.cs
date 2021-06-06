// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using emotitron.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
	
	public abstract class VitalUIBase : VitalsUISrcBase
		, IOnVitalValueChange
		, IOnChangeOwnedVitals

	{
		public enum TargetField { Value, Max, MaxOverload }

		#region Inspector Items

		[Tooltip("Which vital type will be monitored.")]
		[HideInInspector] public VitalNameType targetVital = new VitalNameType(VitalType.Health);
		//public VitalNameType TargetVital { get { return targetVital; } }

		[Tooltip("Which value to track. Typically this is value, but can be other VitalDefinition values like Max/Full.")]
		[HideInInspector] public TargetField targetField = TargetField.Value;



		public Vitals Vitals
		{
			get { return vitals; }
			set
			{
				vitals = value;

				if (value == null)
					Vital = null;
				else
					Vital = vitals.GetVital(targetVital);
			}
		}

		[SerializeField][HideInInspector]
		protected int vitalIndex = -1;

		#endregion

		#region VitalsSource inspector field and Property


		public override IVitalsSystem ApplyVitalsSource(Object vs)
		{

			var ivc = base.ApplyVitalsSource(vs);

			/// Apply this to Vitals to trigger its properties for dealing with changing Vitals
			Vitals = (ivc != null) ? ivc.Vitals : null;
			vitalIndex = ivc == null ? -1 : ivc.Vitals.GetVitalIndex(targetVital);

#if UNITY_EDITOR

			/// Changes to index should update the default bars and such in the editor.
			if (vitalIndex != prevIndex)
				Recalculate();

			prevIndex = vitalIndex;

#endif
			return ivc;
		}

#if UNITY_EDITOR
		protected int prevIndex = -1;
#endif
		public abstract void Recalculate();

		#endregion

		[System.NonSerialized] protected Vital vital;

		public Vital Vital
		{
			get { return vital; }
			private set
			{
				if (vital != null)
					vital.RemoveIOnVitalChange(this);

				if (value != null)
					value.AddIOnVitalChange(this);

				vital = value;
				UpdateGraphics(vital);
			}
		}

		protected virtual void Awake()
		{
			/// Force a rebuild of the vitals... need to really make this more clear.
			ApplyVitalsSource(vitalsSource);

			// register for authority change callbacks that affect vitals
			if (monitor == MonitorSource.Auto || monitor == MonitorSource.Owned)
				OwnedIVitals.iOnChangeOwnedVitals.Add(this);
		}

		protected virtual void Start()
		{
			if (vital != null)
				UpdateGraphics(vital);
		}

		protected virtual void OnDestroy()
		{
			if (monitor == MonitorSource.Auto || monitor == MonitorSource.Owned)
				OwnedIVitals.iOnChangeOwnedVitals.Remove(this);

			if (vital != null)
				vital.RemoveIOnVitalChange(this);
		}


		public override void OnChangeOwnedVitals(IVitalsSystem added, IVitalsSystem removed)
		{
			if (added != null)
			{
				vitalsSource = added as Component;
				Vitals = added.Vitals;
			}
			else if (ReferenceEquals(removed.Vitals, vitals))
			{
				var lastitem = OwnedIVitals.LastItem;
				if (!ReferenceEquals(lastitem, null))
					Vitals = lastitem.Vitals;
			}
		}


		public void OnVitalParamChange(Vital vital)
		{
			/// TODO: different update graphics for max/full value changes
		}

		public virtual void OnVitalValueChange(Vital vital)
		{
			UpdateGraphics(vital);
		}

		public abstract void UpdateGraphics(Vital vital);

	}

#if UNITY_EDITOR

	[CustomEditor(typeof(VitalUIBase))]
	[CanEditMultipleObjects]
	public class VitalUIBaseEditor : VitalsUISrcBaseEditor
	{

		protected override string Instructions
		{
			get
			{
				return "This component should be placed on a UI Text or UI Image (with Image Type set to Filled ideally). " +
				"It can monitor IVitals on the root of this object, or of the local player gameobject.";
			}
		}

		SerializedProperty targetVital, targetField;
		bool vitalTargetExpanded = true;

		public override void OnEnable()
		{
			base.OnEnable();

			targetVital = serializedObject.FindProperty("targetVital");
			targetField = serializedObject.FindProperty("targetField");

			_target.ApplyVitalsSource(_target.vitalsSource);
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			vitalTargetExpanded = EditorGUILayout.Foldout(vitalTargetExpanded, "Target Vital");

			if (vitalTargetExpanded)
			{
				BeginVerticalBox();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(targetVital);
				if (EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();
					if (!Application.isPlaying)
						_target.ApplyVitalsSource(_target.vitalsSource);
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(targetField);
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

