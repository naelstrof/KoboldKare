// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
	[System.Serializable]
	public class TRSDefinitionBase
	{
		public bool local = true;
	}

	[System.Serializable]
	public abstract class SyncMoverBase<TTRSDef, TFrame> : SyncObject<TFrame> 
		, ITransformController
		, IOnPreSimulate
		, IOnPreUpdate
		//, ITransformController
		where TTRSDef : TRSDefinitionBase, new()
		where TFrame : FrameBase, new()
	{
		public enum MovementRelation { Absolute, Relative }
		//public enum AxisMask { None = 0, X = 1, Y = 2, XY = 3, Z = 4, XZ = 5, YZ = 6, XYZ = 7 }

		#region Inspector

		[HideInInspector] public TTRSDef posDef = new TTRSDef();
		[HideInInspector] public TTRSDef rotDef = new TTRSDef();
		[HideInInspector] public TTRSDef sclDef = new TTRSDef();

		#endregion

		#region Interface Requirements

		

		/// TODO: This may need a different reply for Trigger - unless I can make that deterministic.
		public virtual bool HandlesInterpolation { get { return true; } }
		public virtual bool HandlesExtrapolation { get { return true; } }

#if UNITY_EDITOR
		/// Suppress the automatic adding of a NetObject
		public override bool AutoAddNetObj { get { return false; } }

		public virtual bool AutoSync
		{
			get { return false; }
		}
#endif
		#endregion

#if UNITY_EDITOR
		protected static List<ITransformController> foundTransformControllers = new List<ITransformController>();
#endif


		// Cached items
		protected Rigidbody rb;
		protected Rigidbody2D rb2d;
		[System.NonSerialized]
		public SyncTransform syncTransform;

		#region Startup/Shutdown

		public override void OnAwakeInitialize(bool isNetObject)
		{
			/// If not a NetObject, we need to register our timing callbacks directly with the NetMaster
			/// since there is no NetObject to generate these.
			if (!isNetObject)
			{
                NetMasterCallbacks.onPreSimulates.Add(this);
                NetMasterCallbacks.onPreUpdates.Add(this);
			}

			rb = GetComponent<Rigidbody>();
			rb2d = GetComponent<Rigidbody2D>();

			/// Force RBs to be kinematic
			if ((rb && !rb.isKinematic) || (rb2d && !rb2d.isKinematic))
			{
				Debug.LogWarning(GetType().Name + " doesn't work with non-kinematic rigidbodies. Setting to kinematic.");
				if (rb)
					rb.isKinematic = true;
				else
					rb2d.isKinematic = true;
			}

			syncTransform = GetComponent<SyncTransform>();

			Recalculate();
		}

		public override void OnStartInitialize(bool isNetObject)
		{
			InitializeTRS(posDef, TRS.Position);
			InitializeTRS(rotDef, TRS.Rotation);
			InitializeTRS(sclDef, TRS.Scale);
		}


		/// Handling for if this is not a netobject... ties directly into timing callbacks of NetMaster
		private void OnDestroy()
		{
			if (!netObj)
			{
                NetMasterCallbacks.onPreSimulates.Remove(this);
                NetMasterCallbacks.onPreUpdates.Remove(this);
			}
		}

		public virtual void Recalculate()
		{

		}

		protected abstract void InitializeTRS(TTRSDef def, TRS type);

		public abstract void OnPreSimulate(int frameId, int subFrameId);
		public abstract void OnPreUpdate();

		#endregion

	}

#if UNITY_EDITOR

	//[CustomEditor(typeof(SyncMoverBase<>))]
	[CanEditMultipleObjects]
	public abstract class SyncMoverBaseEditor : SyncObjectEditor // HeaderEditorBase
	{

		protected SerializedProperty posDef, rotDef, sclDef;

		protected const float AXIS_LAB_WID = 14f;
		protected const float RANGE_LABEL_WIDTH = 48;

		public override void OnEnable()
		{
			base.OnEnable();

			posDef = serializedObject.FindProperty("posDef");
			rotDef = serializedObject.FindProperty("rotDef");
			sclDef = serializedObject.FindProperty("sclDef");
		}

		protected override string TextTexturePath { get { return "Header/SyncMoverText"; } }
		
		//public override void OnInspectorGUI()
		//{
		//	base.OnInspectorGUI();

		//	EditorGUILayout.LabelField("TEST");
		//	EditorGUILayout.BeginVertical("HelpBox");
		//	var sp = serializedObject.FindProperty("posDef").FindPropertyRelative("local");
		//	while (!sp.hasVisibleChildren)
		//	{
		//		EditorGUILayout.PropertyField(sp);
		//		if (!sp.NextVisible(false))
		//			break;
		//	}
		//	EditorGUILayout.EndVertical();
		//}

		//protected abstract void DrawTRSs();

		protected void DrawTRSElementHeader()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(RANGE_LABEL_WIDTH + 8));
			EditorGUILayout.LabelField(" Pos", GUILayout.MinWidth(16));
			EditorGUILayout.LabelField(" Rot", GUILayout.MinWidth(16));
			EditorGUILayout.LabelField(" Scl", GUILayout.MinWidth(16));
			EditorGUILayout.EndHorizontal();
		}
		protected virtual void DrawTRSElementRow(GUIContent label, string property)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, GUILayout.MaxWidth(RANGE_LABEL_WIDTH + 8));
			EditorGUILayout.PropertyField(posDef.FindPropertyRelative(property), GUIContent.none, GUILayout.MinWidth(16));
			EditorGUILayout.PropertyField(rotDef.FindPropertyRelative(property), GUIContent.none, GUILayout.MinWidth(16));
			EditorGUILayout.PropertyField(sclDef.FindPropertyRelative(property), GUIContent.none, GUILayout.MinWidth(16));
			EditorGUILayout.EndHorizontal();
		}

	}

#endif
}

