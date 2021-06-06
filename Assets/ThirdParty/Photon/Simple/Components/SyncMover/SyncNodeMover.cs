// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

	/// <summary>
	/// Basic automatic transform mover for objects for network testing. Will only run if object has local authority.
	/// </summary>
	public class SyncNodeMover : SyncMoverBase<SyncNodeMover.TRSDefinition, SyncNodeMover.Frame>
		, IOnPreUpdate
		, IOnPreSimulate
		, IOnCaptureState
		, IOnNetSerialize
		, IOnSnapshot
		, IOnInterpolate
		, IReadyable
	{
		public enum Movement { Oscillate, Trigger }
		//public enum MovementRelation { Absolute, Relative }
		//public enum TType { Position, Rotation, Scale }
		//public enum Axis { None = 0, X = 1, Y = 2, XY = 3, Z = 4, XZ = 5, YZ = 6, XYZ = 7 }

		[System.Serializable]
		public class Node
		{
			public Vector3[] trs = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1) };
			public Vector3 Pos { get { return trs[0]; } set { trs[0] = value; } }
			public Vector3 Rot { get { return trs[1]; } set { trs[1] = value; } }
			public Vector3 Scl { get { return trs[2]; } set { trs[2] = value; } }
		}

		[System.Serializable]
		public class TRSDefinition : TRSDefinitionBase
		{
			public AxisMask includeAxes = AxisMask.XYZ;
			public MovementRelation relation = MovementRelation.Relative;
		}

		#region Inspector

		[Range(0, 2)]
		public float predictWithRTT = 1f;

		[HideInInspector] public List<Node> nodes = new List<Node>() { new Node(), new Node() };
		public Node StartNode { get { return nodes[0]; } }
		public Node EndNode { get { return nodes[nodes.Count - 1]; } }

		[HideInInspector] public Movement movement = Movement.Oscillate;
		[HideInInspector] public float oscillatePeriod = 1;
		[HideInInspector] public AnimationCurve oscillateCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(.5f, 1), new Keyframe(1, 0));
        [HideInInspector] public LiteFloatCrusher floatCrusher = new LiteFloatCrusher(LiteFloatCompressType.Bits10, LiteFloatCrusher.Normalization.Positive);

        // AutoSyncTransform Requirements

        #endregion


        // State
        protected float currentPhase;
		protected int queuedTargetNode;
		protected int targetNode;

		#region Frame

		public class Frame : FrameBase
		{
			public int targetNode;
			public float phase;
			public uint cphase;

			public Frame() : base() { }

			public Frame(int frameId) : base(frameId) { }

			public override void CopyFrom(FrameBase sourceFrame)
			{
				base.CopyFrom(sourceFrame);
				Frame src = sourceFrame as Frame;

				this.targetNode = src.targetNode;
				this.phase = src.phase;
				this.cphase = src.cphase;
			}

			public override void Clear()
			{
				base.Clear();

				this.targetNode = -1;
				this.phase = -1;
				this.cphase = 0;
			}

			public bool Compare(Frame otherFrame)
			{
				if (
					targetNode != otherFrame.targetNode ||
					phase != otherFrame.phase ||
					cphase != otherFrame.cphase
					)
					return false;

				return true;
			}
		}

		#endregion Frame

		#region Startup/Shutdown

		protected override void Reset()
		{
			base.Reset();
			oscillateCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(.5f, 1), new Keyframe(1, 0));
		}


		protected override void InitializeTRS(TRSDefinition def, TRS type)
		{
			/// Absolute only applies to oscillate.. make sure its false if we aren't oscillating
			if (movement != Movement.Oscillate)
				def.relation = MovementRelation.Relative;

			if (def.relation == MovementRelation.Relative && movement == Movement.Oscillate || movement == Movement.Trigger)
			{
				Vector3 currentVector;

				switch (type)
				{
					case TRS.Position:
						currentVector = def.local ? transform.localPosition : transform.position;
						break;

					case TRS.Rotation:
						currentVector = def.local ? transform.localEulerAngles : transform.eulerAngles;
						break;

					default:
						currentVector = def.local ? transform.localScale : transform.lossyScale;
						break;
				}

				nodes[0].trs[(int)type] += currentVector;
				nodes[1].trs[(int)type] += currentVector;
			}
		}

		#endregion

		#region Owner Loops

		public override void OnPreSimulate(int frameId, int subFrameId)
		{

			if (!isActiveAndEnabled || (photonView && !photonView.IsMine))
				return;

			/// Make sure previous lerp is fully applied to scene so our transform capture is based on the fixed time and not the last update time
			OwnerInterpolate();
		}

		public override void OnPreUpdate()
		{
			if (!isActiveAndEnabled || (photonView && !photonView.IsMine))
				return;

			OwnerInterpolate();
		}

		protected double timeoffset;

		private void OwnerInterpolate()
		{
			if (timeoffset == 0)
				timeoffset = DoubleTime.fixedTime;

			/// Oscilation doesn't lerp, it applies a Sin based on time.
			if (movement == Movement.Oscillate)
			{
				currentPhase = TimeToPhase(DoubleTime.time - timeoffset);

				float t = OscillatePhaseToLerpT(currentPhase);
				Oscillate(t);
			}
		}

		#endregion Owner Loops

		#region Trigger Handling

		public void Trigger(int targetNode)
		{
			queuedTargetNode = targetNode;
		}
		public void TriggerMin()
		{

		}

		public void TriggerMax()
		{

		}

		#endregion

		private void TriggerLerp() { }

		/// <summary>
		/// Movement based on oscilation
		/// </summary>
		private void Oscillate(float lerpT)
		{
			var start = nodes[0];
			var end = nodes[1];

			var posLerped = (posDef.includeAxes == 0) ? new Vector3(0, 0, 0) : Vector3.Lerp(start.trs[0], end.trs[0], lerpT);//  currentLerpT * position.oscillateRange + position.oscillateStart;
			var rotLerped = (rotDef.includeAxes == 0) ? new Vector3(0, 0, 0) : Vector3.Lerp(start.trs[1], end.trs[1], lerpT); //currentLerpT * rotation.oscillateRange + rotation.oscillateStart;
			var sclLerped = (sclDef.includeAxes == 0) ? new Vector3(1, 1, 1) : Vector3.Lerp(start.trs[2], end.trs[2], lerpT); //currentLerpT * scale.oscillateRange + scale.oscillateStart;

			ApplyOscillate(posLerped, rotLerped, sclLerped);
		}

		#region Networking Loops

		public void OnCaptureCurrentState(int frameId)
		{
			var frame = frames[frameId];

			frame.targetNode = targetNode;
			frame.phase = currentPhase;
			frame.cphase = (uint)floatCrusher.Encode(currentPhase);
		}

		public SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
		{

			var frame = frames[frameId];

			if (movement == Movement.Oscillate)
				floatCrusher.WriteValue(frame.phase, buffer, ref bitposition);

			return SerializationFlags.HasContent;
		}

		public SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival arrival)
		{
			var frame = frames[originFrameId];

			//frame.phase = floatCrusher.ReadValue(buffer, ref bitposition);
			frame.content = FrameContents.Complete;
			if (movement == Movement.Oscillate)
			{
				frame.cphase = (uint)floatCrusher.ReadCValue(buffer, ref bitposition);
				frame.phase = floatCrusher.Decode(frame.cphase);
			}

			return SerializationFlags.HasContent;
		}

		protected float snapPhase, targPhase;

		protected override void ApplySnapshot(Frame snapframe, Frame targframe, bool snapIsVaid, bool targIsValid)
		{
			if (snapIsVaid && snapframe.content == FrameContents.Complete)
			{
				if (movement == Movement.Oscillate)
				{
					if (predictWithRTT != 0)
					{
						//snapPhase = targPhase;

						float nudge = (NetMaster.RTT + TickEngineSettings.targetBufferInterval) * predictWithRTT;
						float nudgedSnapTime = snapframe.phase * oscillatePeriod + nudge;
						float nudgedTargtime = targframe.phase * oscillatePeriod + nudge;

						//Debug.Log(nudge + " time: " + targPhase * oscillatePeriod +
						//	" nudged: " + nudgedTargtime + " origphase: " + targPhase + " newphase: " + TimeToPhase(nudgedTargtime));

						snapPhase = TimeToPhase(nudgedSnapTime);
						targPhase = TimeToPhase(nudgedTargtime);
					}
					else
					{
						snapPhase = snapframe.phase;
						targPhase = targframe.phase;
					}

					float lerpT = (float)OscillatePhaseToLerpT(snapPhase);
					Oscillate(lerpT);
				}
			}
		}


		public override bool OnInterpolate(int snapFrameId, int targFrameId, float t)
		{

			if (!base.OnInterpolate(snapFrameId, targFrameId, t))
				return false;

			switch (movement)
			{
				case Movement.Oscillate:
					{

						if (targPhase < snapPhase)
							targPhase += 1;

						float phase = Mathf.Lerp(snapPhase, targPhase, t);

						if (phase >= 1)
							phase -= 1;

						var lerpT = (float)OscillatePhaseToLerpT(phase);

						Oscillate(lerpT);
						return true;
					}

				case Movement.Trigger:
					{
						break;
					}
				default:
					break;
			}

			return false;

		}

		protected float accumulatedTime;

		protected override void ConstructMissingFrame(Frame prevFrame, Frame snapframe, Frame targframe)
		{
			//base.ConstructMissingFrame(frameId);
			switch (movement)
			{
				// Oscilate is deterministic in nature. So for missing frames we can turn the previous t back into a moduls time value, 
				// and accurately predict t by adding a tick worth of time to that.
				case Movement.Oscillate:
					{
						if (snapframe.content == FrameContents.Complete)
						{
							float restoredTime = snapframe.phase * oscillatePeriod;
							float extrapTime = restoredTime + TickEngineSettings.netTickInterval;
							targframe.phase = TimeToPhase(extrapTime);
							targframe.content = FrameContents.Complete;
							return;
						}
						break;
					}

				default:
					break;
			}
		}

		/// <summary>
		/// Takes a normalized 0-1 value and returns a T value from the OscillateCurve.
		/// </summary>
		/// <param name="phase"></param>
		/// <returns></returns>
		protected float OscillatePhaseToLerpT(float phase)
		{
			return oscillateCurve.Evaluate(phase);
		}

		protected float TimeToPhase(double time)
		{
			return (float)((time % oscillatePeriod)) / oscillatePeriod;
		}

		//protected float PhaseToTime(float phase)
		//{
		//	return phase * oscillatePeriod;
		//}

		#endregion Networking Loops

		/// <summary>
		/// Apply a value to the indicated TRS axis, leaving the axes and TRS types not indicated as they are.
		/// </summary>
		/// <param name="pos"></param>
		private void ApplyOscillate(Vector3 pos, Vector3 rot, Vector3 scl)
		{
			var posIncludeAxes = posDef.includeAxes;
			var rotIncludeAxes = rotDef.includeAxes;
			var sclIncludeAxes = sclDef.includeAxes;


			// Scale
			if (sclIncludeAxes != AxisMask.None)
			{
				transform.localScale = new Vector3(
					((sclIncludeAxes & AxisMask.X) != 0) ? scl.x : transform.localScale.x,
					((sclIncludeAxes & AxisMask.Y) != 0) ? scl.y : transform.localScale.y,
					((sclIncludeAxes & AxisMask.Z) != 0) ? scl.z : transform.localScale.z);
			}

			// Rotation
			if (rotIncludeAxes != AxisMask.None)
			{
				if (rotDef.local)
				{
					transform.localEulerAngles = new Vector3(
						((rotIncludeAxes & AxisMask.X) != 0) ? rot.x : transform.localEulerAngles.x,
						((rotIncludeAxes & AxisMask.Y) != 0) ? rot.y : transform.localEulerAngles.y,
						((rotIncludeAxes & AxisMask.Z) != 0) ? rot.z : transform.localEulerAngles.z);
				}
				else
				{
					transform.eulerAngles = new Vector3(
						((rotIncludeAxes & AxisMask.X) != 0) ? rot.x : transform.eulerAngles.x,
						((rotIncludeAxes & AxisMask.Y) != 0) ? rot.y : transform.eulerAngles.y,
						((rotIncludeAxes & AxisMask.Z) != 0) ? rot.z : transform.eulerAngles.z);
				}
			}

			// Position
			if (posIncludeAxes != AxisMask.None)
			{
				if (posDef.local)
				{
					transform.localPosition = new Vector3(
						((posIncludeAxes & AxisMask.X) != 0) ? pos.x : transform.localPosition.x,
						((posIncludeAxes & AxisMask.Y) != 0) ? pos.y : transform.localPosition.y,
						((posIncludeAxes & AxisMask.Z) != 0) ? pos.z : transform.localPosition.z);
				}
				else
				{
					transform.position = new Vector3(
						((posIncludeAxes & AxisMask.X) != 0) ? pos.x : transform.position.x,
						((posIncludeAxes & AxisMask.Y) != 0) ? pos.y : transform.position.y,
						((posIncludeAxes & AxisMask.Z) != 0) ? pos.z : transform.position.z);
				}
			}

		}

	}

#if UNITY_EDITOR

	[CustomEditor(typeof(SyncNodeMover))]
	[CanEditMultipleObjects]
	public class SimpleNodeMoverEditor : SyncMoverBaseEditor // HeaderEditorBase
	{

		SerializedProperty
			floatCrusher,
			nodes,
			movement,
			oscillateCurve,
			oscillatePeriod;

		protected class TRS_SP
		{
			public SerializedProperty
			relation,
			includeAxes,
			local;
		}

		TRS_SP posSPs = new TRS_SP();
		TRS_SP rotSPs = new TRS_SP();
		TRS_SP sclSPs = new TRS_SP();

		public override void OnEnable()
		{
			base.OnEnable();

			nodes = serializedObject.FindProperty("nodes");
			movement = serializedObject.FindProperty("movement");
			floatCrusher = serializedObject.FindProperty("floatCrusher");


			oscillatePeriod = serializedObject.FindProperty("oscillatePeriod");
			oscillateCurve = serializedObject.FindProperty("oscillateCurve");

			InitSP(posDef, posSPs);
			InitSP(rotDef, rotSPs);
			InitSP(sclDef, sclSPs);
		}

		protected void InitSP(SerializedProperty trs, TRS_SP trsSP)
		{
			trsSP.relation = trs.FindPropertyRelative("relation");
			trsSP.includeAxes = trs.FindPropertyRelative("includeAxes");
			trsSP.local = trs.FindPropertyRelative("local");
		}

		public override void OnInspectorGUI()
		{

			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();

			/// Movement dropdown
			EditorGUILayout.PropertyField(movement);

			if (movement.intValue == (int)SyncNodeMover.Movement.Oscillate)
			{
				EditorGUILayout.BeginVertical("HelpBox");
				/// Rate
				EditorGUILayout.BeginHorizontal(/*GUILayout.Width(100), GUILayout.MinWidth(75)*/);
				EditorGUILayout.PropertyField(oscillatePeriod);
				EditorGUILayout.LabelField(" sec(s)", GUILayout.MaxWidth(48));
				EditorGUILayout.EndHorizontal();

				/// Curve
				EditorGUILayout.PropertyField(oscillateCurve);

				/// normalized float crusher
				EditorGUILayout.BeginVertical("HelpBox");
				EditorGUILayout.LabelField(new GUIContent(floatCrusher.displayName, "The compressor used to serialize the normalized lerp T value"));
				EditorGUILayout.PropertyField(floatCrusher);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndVertical();
			}



			DrawWarningBoxes();
			DrawVerticalTRSs();

			//EditorGUILayout.BeginVertical("HelpBox");
			//DrawTRS(posSPs, TRS.Position, "Pos");
			//DrawTRS(rotSPs, TRS.Rotation, "Rot");
			//DrawTRS(sclSPs, TRS.Scale, "Scl");
			//EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("Nodes:", (GUIStyle)"BoldLabel");
			//DrawNode(0);
			//DrawNode(1);

			DrawHorizontalNodes();


			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();

		}

		protected void DrawVerticalTRSs()
		{
			EditorGUILayout.BeginVertical("HelpBox");

			DrawTRSElementHeader();

			EditorGUILayout.BeginHorizontal();
			DrawTRSElementRow(new GUIContent("Axes", "Axes that will be affected. Unselected axes will not have values applied to them."), "includeAxes");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			DrawTRSElementRow(new GUIContent("Relation", ""), "relation");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			DrawTRSElementRow(new GUIContent("Local", ""), "local");
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.EndVertical();
		}

		protected void DrawHorizontalTRS(TRS_SP trsSP, TRS type, string label)
		{

			EditorGUILayout.BeginVertical("HelpBox");

			{
				/// Restrict
				EditorGUILayout.PropertyField(trsSP.includeAxes, new GUIContent(label + " Axes"));

				if (trsSP.includeAxes.intValue != 0)
				{
					/// Relation
					EditorGUILayout.PropertyField(trsSP.relation);

					/// Local
					EditorGUI.BeginDisabledGroup(type == TRS.Scale);
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Local", GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
					EditorGUILayout.GetControlRect(GUILayout.MaxWidth(AXIS_LAB_WID));
					EditorGUILayout.PropertyField(trsSP.local, GUIContent.none);
					EditorGUILayout.EndHorizontal();
					EditorGUI.EndDisabledGroup();
				}
			}
			EditorGUILayout.EndVertical();
		}

		protected void DrawHorizontalNodes()
		{
			for (int i = 0; i < nodes.arraySize; ++i)
			{
				EditorGUILayout.BeginVertical("HelpBox");

				EditorGUILayout.LabelField("Node [" + i + "]");

				var node = nodes.GetArrayElementAtIndex(i);

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField("Pos", GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
				DrawAxes(node.FindPropertyRelative("trs").GetArrayElementAtIndex(0), (AxisMask)posDef.FindPropertyRelative("includeAxes").intValue);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Rot", GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
				DrawAxes(node.FindPropertyRelative("trs").GetArrayElementAtIndex(1), (AxisMask)rotDef.FindPropertyRelative("includeAxes").intValue);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Scl", GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
				DrawAxes(node.FindPropertyRelative("trs").GetArrayElementAtIndex(2), (AxisMask)sclDef.FindPropertyRelative("includeAxes").intValue);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();
			}
		}

		//protected void DrawNode(int node, TRS type, AxisMask mask)
		//{
		//	/// Start
		//	EditorGUILayout.BeginHorizontal();
		//	EditorGUILayout.LabelField("Node " + node, GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
		//	DrawAxes(nodes.GetArrayElementAtIndex(0).FindPropertyRelative("trs").GetArrayElementAtIndex((int)type), mask);
		//	EditorGUILayout.EndHorizontal();
		//}

		const float FLOAT_WIDTH = 10f;

		protected GUIStyle vertV3Style;
		protected GUIStyle vertV3StyleBlank;

		protected void DrawVerticalNode(int nodeId)
		{
			if (vertV3Style == null)
				vertV3Style = new GUIStyle("HelpBox") { padding = new RectOffset(8, 8, 8, 8) };

			if (vertV3StyleBlank == null)
				vertV3StyleBlank = new GUIStyle("HelpBox") { padding = new RectOffset(8, 8, 8, 8), normal = ((GUIStyle)"Label").normal };

			var node = nodes.GetArrayElementAtIndex(nodeId);

			float px, rx, sx;
			float py, ry, sy;
			float pz, rz, sz;

			var posmask = (AxisMask)posDef.FindPropertyRelative("includeAxes").intValue;
			var rotmask = (AxisMask)rotDef.FindPropertyRelative("includeAxes").intValue;
			var sclmask = (AxisMask)sclDef.FindPropertyRelative("includeAxes").intValue;

			var trs = node.FindPropertyRelative("trs");
			var pos = trs.GetArrayElementAtIndex(0);
			var rot = trs.GetArrayElementAtIndex(1);
			var scl = trs.GetArrayElementAtIndex(2);


			EditorGUILayout.BeginHorizontal("HelpBox");


			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
			EditorGUILayout.LabelField("[" + nodeId + "]", GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
			EditorGUILayout.BeginVertical(vertV3StyleBlank, GUILayout.MinWidth(16));
			EditorGUILayout.LabelField("X:", labelright, GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
			EditorGUILayout.LabelField("Y:", labelright, GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
			EditorGUILayout.LabelField("Z:", labelright, GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();

			// Position
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(" Pos", GUILayout.MinWidth(16));
			EditorGUILayout.BeginVertical(posmask != 0 ? vertV3Style : vertV3StyleBlank);
			if (posmask != 0)
			{
				px = DrawVerticalAxis(pos, 0, posmask);
				py = DrawVerticalAxis(pos, 1, posmask);
				pz = DrawVerticalAxis(pos, 2, posmask);

				var newpos = new Vector3(px, py, pz);
				if (newpos != pos.vector3Value)
					pos.vector3Value = newpos;
			}
			else
				DrawEmptyTRS();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();

			// Rotation
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(" Rot", GUILayout.MinWidth(16));
			EditorGUILayout.BeginVertical(rotmask != 0 ? vertV3Style : vertV3StyleBlank);
			if (rotmask != 0)
			{
				rx = DrawVerticalAxis(rot, 0, rotmask);
				ry = DrawVerticalAxis(rot, 1, rotmask);
				rz = DrawVerticalAxis(rot, 2, rotmask);


				var newrot = new Vector3(rx, ry, rz);
				if (newrot != rot.vector3Value)
					rot.vector3Value = newrot;
			}
			else
				DrawEmptyTRS();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();

			// Scale
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(" Scl", GUILayout.MinWidth(16));
			EditorGUILayout.BeginVertical(sclmask != 0 ? vertV3Style : vertV3StyleBlank);
			if (sclmask != 0)
			{
				sx = DrawVerticalAxis(scl, 0, sclmask);
				sy = DrawVerticalAxis(scl, 1, sclmask);
				sz = DrawVerticalAxis(scl, 2, sclmask);

				var newscl = new Vector3(sx, sy, sz);
				if (newscl != scl.vector3Value)
					scl.vector3Value = newscl;
			}
			else
				DrawEmptyTRS();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();






		}

		protected float DrawVerticalAxis(SerializedProperty element, int axis, AxisMask mask)
		{
			//EditorGUILayout.BeginHorizontal();

			bool use = ((int)mask & (1 << axis)) != 0;
			float oldval = element.vector3Value[axis];
			EditorGUI.BeginDisabledGroup(!use);
			//EditorGUILayout.LabelField(" ?", GUILayout.MaxWidth(AXIS_LAB_WID));
			float newval = EditorGUILayout.DelayedFloatField(oldval, GUILayout.MinWidth(FLOAT_WIDTH));
			newval = (use) ? newval : 0;
			EditorGUI.EndDisabledGroup();

			//EditorGUILayout.EndHorizontal();

			return newval;
		}

		protected void DrawEmptyTRS()
		{
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(AXIS_LAB_WID));
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(AXIS_LAB_WID));
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(AXIS_LAB_WID));
		}


		protected void DrawAxes(SerializedProperty v3, AxisMask axis)
		{
			const float FLOAT_WIDTH = 10f;
			var oldval = v3.vector3Value;

			float x, y, z;

			bool usex = (axis & AxisMask.X) != 0;
			bool usey = (axis & AxisMask.Y) != 0;
			bool usez = (axis & AxisMask.Z) != 0;

			/// X
			EditorGUI.BeginDisabledGroup(!usex);
			EditorGUILayout.LabelField(" x", GUILayout.MaxWidth(AXIS_LAB_WID));
			float newx = EditorGUILayout.DelayedFloatField(oldval.x, GUILayout.MinWidth(FLOAT_WIDTH));
			x = (usex) ? newx : 0;
			EditorGUI.EndDisabledGroup();

			/// Y
			EditorGUI.BeginDisabledGroup(!usey);
			EditorGUILayout.LabelField(" y", GUILayout.MaxWidth(AXIS_LAB_WID));
			float newy = EditorGUILayout.DelayedFloatField(oldval.y, GUILayout.MinWidth(FLOAT_WIDTH));
			y = (usey) ? newy : 0;
			EditorGUI.EndDisabledGroup();


			/// Z
			EditorGUI.BeginDisabledGroup(!usez);
			EditorGUILayout.LabelField(" z", GUILayout.MaxWidth(AXIS_LAB_WID));
			float newz = EditorGUILayout.DelayedFloatField(oldval.z, GUILayout.MinWidth(FLOAT_WIDTH));
			z = (usez) ? newz : 0;
			EditorGUI.EndDisabledGroup();

			var newval = new Vector3(x, y, z);
			if (v3.vector3Value != newval)
				v3.vector3Value = newval;
		}

		protected void DrawWarningBoxes()
		{
			var _target = target as SyncNodeMover;

			#region Warning Boxes

			_target.GetOrAddNetObj();

			if (!_target.NetObj)
			{
				EditorGUILayout.HelpBox(
					"This GameObject does not have a " + typeof(NetObject).Name + ". Motion will be applied locally without networking.", MessageType.Info);
			}

			#endregion
		}

	}



#endif
}

