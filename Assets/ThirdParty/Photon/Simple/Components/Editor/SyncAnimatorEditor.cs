// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

using Photon.Pun.Simple.Internal;
using Photon.Utilities;

using Photon.Compression;

using UnityEditor;

namespace Photon.Pun.Simple
{

	[CustomEditor(typeof(SyncAnimator))]
	[CanEditMultipleObjects]
	public class SyncAnimatorEditor : SyncObjectEditor
	{


        protected override string Instructions
        {
            get
            {
                return "Attach this component to any root or child GameObject with an Animator to sync its AnimatorController over the network." +
                    "\nThe GameObject must be networked (has a PhotonView).\n\n" +
                    passThruHelpText;
            }
        }


        protected override string HelpURL
        {
            get { return SimpleDocsURLS.SYNCCOMPS_PATH + "#syncanimator_component"; }
        }

        protected override string TextTexturePath
        {
            get { return "Header/SyncAnimatorText"; }
        }


        SyncAnimator t;
		Animator a;

		private static readonly GUIContent passthruLabel = new GUIContent(
			"Sync Pass Thru Methods",
			"'this.SetTrigger()', 'this.Play()', 'this.CrossFadeInFixedTime()', etc methods are provided by this class, and will pass through the same named commands to the Animator. " +
			"When enabled, these methods will be sent and triggered over the network. Disabling this allows these methods to act as if they were called directly on the Animator without any networking, which is convenient for testing.");

		private static readonly GUIContent statesLabel = new GUIContent(
			"Sync States",
			"When enabled, changes in the animator current state are transmitted.");

		private static readonly GUIContent layerWeightsLabel = new GUIContent(
			"Sync Layer Weights",
			"When enabled, changes to all layer weights will be synced.");

		private static readonly GUIContent paramsLabel = new GUIContent(
			"Sync Parameters",
			"When enabled, animator parameters will be networked and synced.");

#if SNS_SYNCIK
		private static readonly GUIContent ikFeetLabel = new GUIContent(
			"Sync IK Feet",
			"When enabled, Feet IK will be synced.");

		private static readonly GUIContent ikHandsLabel = new GUIContent(
			"Sync IK Hands",
			"When enabled, Hands IK will be synced.");
#endif

		private static readonly GUIContent label_syncLayers = new GUIContent(
			"Sync Layers",
			"State syncs for all layers, rather than just the root layer.");

		
		private static readonly GUIContent label_Interp = new GUIContent(
			"Interp",
			"Interpolation enables lerping(tweening) of values on clients between network updates.");

		private static readonly GUIContent label_Extrap = new GUIContent(
			"Extrap",
			"Extrapolation replicates previous values if new values from the network fail to arrive in time. When disabled, values default to default value for that parameter as defined in the Animator");

		private static readonly GUIContent label_Default = new GUIContent(
			"Defs",
			"Default value used for initial values and extrapolation.");

		private static readonly GUIContent index_Default = new GUIContent(
			"Index Animator Names",
			"Polls Animator Controller for all State, Trigger and Transition hashes. Indexed values require a tiny fraction of bandwidth to send compared to raw 32 hashes. This happens often automatically, but it never hurts to press this button after making changes to your Animator Controller.");
		
			
		public static string[] passThruTypeNames = System.Enum.GetNames(typeof(PassThruType));
		public static string passThruHelpText;
		static SyncAnimatorEditor()
		{
			passThruHelpText = "<b>BE SURE</b> to call " + typeof(SyncAnimator).Name + " commands using the Pass Thru methods:\n\n";
			foreach (var s in passThruTypeNames)
				passThruHelpText += "syncAnimator." + s + "()\n";

			passThruHelpText += "\n...to network them as events.\nThis is especially useful for triggers, as SetTrigger() often happens too quickly to sync as a parameter.";
		}

		public override void OnEnable()
		{
			base.OnEnable();

			t = target as SyncAnimator;

			if (!t)
				return;

			if (t)
				a = t.GetComponent<Animator>();

			if (a)
				t.RebuildIndexedNames();

			
			/// Initialize short term settings memory

			uid = t.GetInstanceID();

			if (!showSummary.ContainsKey(uid))
				showSummary[uid] = false;

			if (!showAdvancedParams.ContainsKey(uid))
				showAdvancedParams[uid] = false;
		}

		
		/// Short term memory for component settings. Resets on loading of project or compiling
		private int uid;
		private static Dictionary<int, bool> showSummary = new Dictionary<int, bool>();
		private static Dictionary<int, bool> showAdvancedParams = new Dictionary<int, bool>();

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();

			Rect r = EditorGUILayout.GetControlRect();
			if (GUI.Button(r, index_Default))
			{
				t.RebuildIndexedNames();
				showSummary[uid] = true;
			}

			showSummary[uid] = IndentedFoldout(new GUIContent("Indexed Name Summary"), showSummary[uid], 1);
			
			if (showSummary[uid])
			{
				sb.Length = 0;
				sb.Append((uint)t.sharedTriggIndexes.Count).Append(" Triggers found.\n");
				for (int i = 0; i < t.sharedTriggIndexes.Count; ++i)
					sb.Append(t.sharedTriggNames[i]).Append(" : ").Append(t.sharedTriggIndexes[i]).Append("\n");
				sb.Append(((uint)t.sharedTriggIndexes.Count - 1).GetBitsForMaxValue() + 1).Append(" bits per indexed Trigger.\n33 bits per non-indexed Trigger.\n\n");

				sb.Append((uint)t.sharedStateIndexes.Count).Append(" States found.\n");
				for (int i = 0; i < t.sharedStateIndexes.Count; ++i)
					sb.Append(t.sharedStateNames[i]).Append(" : ").Append(t.sharedStateIndexes[i]).Append("\n");
				sb.Append(((uint)t.sharedStateIndexes.Count - 1).GetBitsForMaxValue() + 1).Append(" bits per indexed State.\n33 bits per non-indexed State.\n\n");

				//sb.Append((uint)t.sharedTransIndexes.Count).Append(" Transitions found.\n");
				//for (int i = 0; i < t.sharedTransIndexes.Count; ++i)
				//	sb.Append(t.sharedTransNames[i]).Append(" : ").Append(t.sharedTransIndexes[i].hash).Append("\n");
				//sb.Append(FloatCrusher.GetBitsForMaxValue((uint)t.sharedTransIndexes.Count - 1) + 1).Append(" bits per indexed Transition.\n33 bits per non-indexed Transitions.");

				EditorGUILayout.HelpBox(sb.ToString(), MessageType.None);
			}

			Divider();

			/// Passthrus
			t.syncPassThrus = EditorGUILayout.BeginToggleGroup(passthruLabel, t.syncPassThrus);
			if (t.syncPassThrus)
			{
				NormTimeCompressEnum(EditorGUILayout.GetControlRect(), new GUIContent("Compress NormalizedTime"), ref t.passthruNormTimeCompress);
				//EditorGUILayout.HelpBox(passThruHelpText, MessageType.None);
			}
			EditorGUILayout.EndToggleGroup();

			Divider();

			/// States
			t.syncStates = EditorGUILayout.BeginToggleGroup(statesLabel, t.syncStates);
			if (t.syncStates)
				StatesSection();
			EditorGUILayout.EndToggleGroup();

			Divider();

			/// Layer Weights
			t.syncLayerWeights = EditorGUILayout.BeginToggleGroup(layerWeightsLabel, t.syncLayerWeights);
			if (t.syncLayerWeights)
				LayerWeightsSection();
			EditorGUILayout.EndToggleGroup();

			Divider();
			
			/// Parameters
			t.syncParams = EditorGUILayout.BeginToggleGroup(paramsLabel, t.syncParams);
			if (t.syncParams)
				ParamSection();
			EditorGUILayout.EndToggleGroup();

			Divider();

#if SNS_SYNCIK

			/// IK
			t.syncIKFeet = EditorGUILayout.BeginToggleGroup(ikFeetLabel, t.syncIKFeet);
			if (t.syncIKFeet)
				IKFeetSection();
			EditorGUILayout.EndToggleGroup();

			Divider();

			t.syncIKHands = EditorGUILayout.BeginToggleGroup(ikHandsLabel, t.syncIKHands);
			if (t.syncIKHands)
				IKHandsSection();
			EditorGUILayout.EndToggleGroup();

#endif

			EditorGUILayout.Space();

			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}

		private void StatesSection()
		{
			NormTimeCompressEnum(EditorGUILayout.GetControlRect(), new GUIContent("Compress NormalizedTime"), ref t.normalizedTimeCompress);

            InspectorWidgets.MiniToggle(t, EditorGUILayout.GetControlRect(), label_syncLayers, ref t.syncLayers);
		}

		private void LayerWeightsSection()
		{
			/// Compression enum for layer weights
			if (t.syncLayerWeights)
			{
				NormTimeCompressEnum(EditorGUILayout.GetControlRect(), new GUIContent("Compress LayerWeight"), ref t.layerWeightCompress);
			}
		}

		private void NormTimeCompressEnum(Rect r, GUIContent gc, ref NormalizedFloatCompression compression)
		{
			EditorGUI.LabelField(new Rect(r) { xMin = r.xMin + 16 }, gc, (GUIStyle)"MiniLabel");
			Rect enumrect = new Rect(r) { xMin = r.xMin + 170, y = r.y + 1 };
			var newweight = (NormalizedFloatCompression)EditorGUI.EnumPopup(enumrect, GUIContent.none, compression/*, (GUIStyle)"GV Gizmo DropDown"*/);
			if (newweight != compression)
			{
				Undo.RecordObject(t, "Change Normalized Float Compression");
				compression = newweight;
			}
		}

        #region Parameters

		static GUIContent triggerLabel = new GUIContent("Triggers", "WARNING: Triggers tend to fire and clear instantly, resulting in a failure to sync. Use the this.Trigger() pass through call instead.");

		static GUIStyle PaddedBoxGS;
		static GUIStyle PaddedGS;

		const float COL1 = 40;
		const float COL2 = 64;
		const float COL3 = 60;
		const float COL4 = 32;

		private void ParamSection()
		{
			if (PaddedBoxGS == null)
				PaddedBoxGS = new GUIStyle((GUIStyle)"HelpBox") { normal = ((GUIStyle)"GroupBox").normal, padding = new RectOffset(4, 4, 4, 4) };

			if (PaddedGS == null)
				PaddedGS = new GUIStyle(PaddedBoxGS) { normal = ((GUIStyle)"MiniLabel").normal };

			t.useGlobalParamSettings = EditorGUILayout.ToggleLeft("Use Global Settings", t.useGlobalParamSettings);

			showAdvancedParams[uid] = IndentedFoldout(new GUIContent("Adv. Parameter Settings"), showAdvancedParams[uid], 1);

			if (showAdvancedParams[uid])
			{
				var indenthold = EditorGUI.indentLevel;

				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.GetControlRect(GUILayout.MinWidth(COL1));
				EditorGUILayout.LabelField(label_Interp, (GUIStyle)"MiniLabel", GUILayout.MaxWidth(COL2));
				EditorGUILayout.LabelField(label_Extrap, (GUIStyle)"MiniLabel", GUILayout.MaxWidth(COL3));
				EditorGUILayout.LabelField(label_Default, (GUIStyle)"MiniLabel", GUILayout.MaxWidth(COL4));
				EditorGUILayout.EndHorizontal();

                ParameterDefaults defs = t.sharedParamDefaults;

				if (t.useGlobalParamSettings)
				{
					EditorGUILayout.BeginHorizontal();
					InspectorWidgets.MiniToggle(t, EditorGUILayout.GetControlRect(GUILayout.MinWidth(COL1)), new GUIContent("Ints"), ref defs.includeInts);
					DrawInterp(AnimatorControllerParameterType.Int, ref defs.interpolateInts);
					DrawExtrap(AnimatorControllerParameterType.Int, ref defs.extrapolateInts);
					DrawDefaults(AnimatorControllerParameterType.Int, ref defs.defaultInt);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					InspectorWidgets.MiniToggle(t, EditorGUILayout.GetControlRect(GUILayout.MinWidth(COL1)), new GUIContent("Floats"), ref defs.includeFloats);
					DrawInterp(AnimatorControllerParameterType.Float, ref defs.interpolateFloats);
					DrawExtrap(AnimatorControllerParameterType.Float, ref defs.extrapolateFloats);
					DrawDefaults(AnimatorControllerParameterType.Float, ref defs.defaultFloat);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					InspectorWidgets.MiniToggle(t, EditorGUILayout.GetControlRect(GUILayout.MinWidth(COL1)), new GUIContent("Bools"), ref defs.includeBools);
					EditorGUILayout.GetControlRect(GUILayout.MaxWidth(COL2));
					DrawExtrap(AnimatorControllerParameterType.Bool, ref defs.extrapolateBools);
					DrawDefaults(AnimatorControllerParameterType.Bool, ref defs.defaultBool);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					var tr = EditorGUILayout.GetControlRect(GUILayout.MinWidth(COL1));
					InspectorWidgets.MiniToggle(t, tr, triggerLabel, ref defs.includeTriggers);
					if (t.sharedParamDefaults.includeTriggers)
					{
						GUI.DrawTexture(new Rect(tr) { x = 2, width = 16 }, EditorGUIUtility.FindTexture("console.warnicon"));
					}
					EditorGUILayout.GetControlRect(GUILayout.MaxWidth(COL2));
					DrawExtrap(AnimatorControllerParameterType.Trigger, ref defs.extrapolateTriggers);
					DrawDefaults(AnimatorControllerParameterType.Trigger, ref defs.defaultTrigger);
					EditorGUILayout.EndHorizontal();

				}
				else
				{

					var names = ParameterSettings.RebuildParamSettings(a, ref t.sharedParamSettings, ref t.paramCount, t.sharedParamDefaults);
					serializedObject.Update();

					var pms = t.sharedParamSettings;
					for (int i = 0; i < t.paramCount; ++i)
					{
						var pm = pms[i];

						EditorGUILayout.BeginHorizontal();

						// Type Letter Box (left vertical)
						EditorGUILayout.BeginVertical(PaddedBoxGS, GUILayout.MaxWidth(8));
						TypeLabel(EditorGUILayout.GetControlRect(GUILayout.MaxWidth(12)), pm.paramType);

						EditorGUILayout.EndVertical();

						// Main Vertical (Right)
						EditorGUILayout.BeginVertical((pm.include) ? PaddedBoxGS : PaddedGS);

						EditorGUILayout.BeginHorizontal();

						InspectorWidgets.MiniToggle(t, EditorGUILayout.GetControlRect(GUILayout.MinWidth(COL1)), new GUIContent(names[i]), ref pm.include);

						EditorGUI.BeginDisabledGroup(!pm.include);
						DrawInterp(pm.paramType, ref pm.interpolate);
						DrawExtrap(pm.paramType, ref pm.extrapolate);
						DrawDefaults(pm.paramType, ref pm.defaultValue);
						EditorGUI.EndDisabledGroup();

						EditorGUILayout.EndHorizontal();

						// Compression Row
						if (pm.include)
						{
							if (pm.paramType == AnimatorControllerParameterType.Float || pm.paramType == AnimatorControllerParameterType.Int)
							{
								var sharedPSs = serializedObject.FindProperty("sharedParamSettings");
								var ps = sharedPSs.GetArrayElementAtIndex(i);

								var fcrusher = (pm.paramType == AnimatorControllerParameterType.Float) ? ps.FindPropertyRelative("fcrusher") : ps.FindPropertyRelative("icrusher");

								var r = EditorGUILayout.GetControlRect(false, EditorGUI.GetPropertyHeight(fcrusher));

								Rect rectCrusher = new Rect(r) { height = 16 };
								EditorGUI.PropertyField(rectCrusher, fcrusher);
							}
						}

						// End Right Vertical
						EditorGUILayout.EndVertical();

						// End Parameter
						EditorGUILayout.EndHorizontal();

					}
				}

				EditorGUI.indentLevel = indenthold;
			}
		}

		private void TypeLabel(Rect r, AnimatorControllerParameterType ptype)
		{
			string typeinit =
						ptype == AnimatorControllerParameterType.Bool ? "B" :
						ptype == AnimatorControllerParameterType.Int ? "I" :
						ptype == AnimatorControllerParameterType.Float ? "F" :
						"T";

			EditorGUI.LabelField(r, typeinit, (GUIStyle)"MiniLabel");
		}

		private Rect ColLabel(string label, ref bool use)
		{
			Rect r = ColLabel(new GUIContent(label));
			Rect toggleR = r;
			toggleR.width = 16;
			InspectorWidgets.MiniToggle(t, r, GUIContent.none, ref use, false);
			
			return r;
		}
		private Rect ColLabel(GUIContent label, ref bool use)
		{
			Rect r = ColLabel(label);
			Rect toggleR = r;
			toggleR.width = 16;
			InspectorWidgets.MiniToggle(t, r, GUIContent.none, ref use, false);

			return r;
		}
		private Rect ColLabel(GUIContent label)
		{

			Rect r = EditorGUILayout.GetControlRect();
			r.xMin -= 2;
			Rect labelrect = new Rect(r.xMin + 16, r.yMin, r.width, r.height);
			EditorGUI.LabelField(labelrect, label, (GUIStyle)"MiniLabel");

			return r;
		}

		private static GUIContent intGC = new GUIContent("", "How interpolaton is handed for this parameter. Interpolation occurs in Update() between application of net updates.");
		private void DrawInterp(AnimatorControllerParameterType type, ref ParameterInterpolation i)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(COL2));

			if (type == AnimatorControllerParameterType.Bool || type == AnimatorControllerParameterType.Trigger)
				return;

			var newi = (ParameterInterpolation)EditorGUI.EnumPopup(r, intGC, i, (GUIStyle)"MiniPopup");

			if (newi != i)
			{
				Undo.RecordObject(target, "Modify Sync Animator");
				i = newi;
			}
		}

		private static GUIContent extGC = new GUIContent("", "How extrapolation is handed for this parameter. Extrapolation occurs when an update fails to arrive and a guess needs to be made for this value.");
		private void DrawExtrap(AnimatorControllerParameterType type, ref ParameterExtrapolation e)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(COL3));

			ParameterExtrapolation newe;

			

			if (type == AnimatorControllerParameterType.Bool || type == AnimatorControllerParameterType.Trigger)
				newe = (ParameterExtrapolation)EditorGUI.EnumPopup(r, extGC, (ParameterMissingHold)e, new GUIStyle("MiniPopup") { margin = new RectOffset(0,0,0,0) });
			else
				newe = (ParameterExtrapolation)EditorGUI.EnumPopup(r, extGC, e, new GUIStyle("MiniPopup") { margin = new RectOffset(0, 0, 0, 0) });

			if (newe != e)
			{
				Undo.RecordObject(target, "Modify Sync Animator");
				e = newe;
			}
			
		}
		
		private static GUIContent defGC = new GUIContent("", "The default value for this parameter when extropation occurs.");
		private void DrawDefaults(AnimatorControllerParameterType type, ref SmartVar v)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(COL4));

			//EditorGUI.LabelField(r, defGC, (GUIStyle)"MiniLabel");

			if (type == AnimatorControllerParameterType.Float)
			{
				var newv = EditorGUI.FloatField(r, defGC, v.Float, new GUIStyle("MiniTextField") { margin = new RectOffset(0,0,0,0) });
				if (newv != v)
				{
					Undo.RecordObject(target, "Modify Sync Animator");
					v = newv;
				}
			}
			else if (type == AnimatorControllerParameterType.Int)
			{
				var newv = EditorGUI.IntField(r, defGC, v.Int, new GUIStyle("MiniTextField") { margin = new RectOffset(0, 0, 0, 0) });
				if (newv != v)
				{
					Undo.RecordObject(target, "Modify Sync Animator");
					v = newv;
				}
			}
			else
			{
				var newv = EditorGUI.Toggle(r, defGC, v.Bool);
				if (newv != v)
				{
					Undo.RecordObject(target, "Modify Sync Animator");
					v = newv;
				}
			}

		}

#endregion

		private void IKFeetSection()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ikFeetPosCrusher"), new GUIContent("IK Hand Position"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ikFeetRotCrusher"), new GUIContent("IK Hand Rotation"));
		}

		private void IKHandsSection()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ikHandPosCrusher"), new GUIContent("IK Foot Position"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ikHandRotCrusher"), new GUIContent("IK Foot Rotation"));
		}
	}
}

