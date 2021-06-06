// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Photon.Pun.Simple
{

	/// <summary>
	/// Extend this class and define the overrides to produce a mask logic. Call DrawGUI in Editor code to inline the GUI.
	/// </summary>
	[System.Serializable]
	public abstract class MaskLogic
	{
		public enum Operator { EQUALS, AND, OR }
		public Operator operation = Operator.AND;
		public int stateMask;
		public int notMask = 0;

		//cache
		protected int trueMask;

		protected abstract string[] EnumNames { get; }
		protected abstract int[] EnumValues { get; }
		protected abstract bool DefinesZero { get; }
		protected abstract int DefaultValue { get; }

		public MaskLogic()
		{
			stateMask = DefaultValue;
		}

		/// <summary>
		/// This likely is not needed to be run, but will clean up any possible errors from the editor. Bootstrap this on implementation Awakes.
		/// </summary>
		public void RecalculateMasks()
		{
			/// making sure notMask isn't retaining any value from edit mode. Likely not needed.
			if (operation == Operator.EQUALS)
				notMask = 0;

			/// Make sure only valid bits are true in the notMask (not enforced in editor code)
			notMask &= stateMask;
			/// Remove not bits from the stateMask for AND/OR operations, they will be handled by the notMask test
			//if (operation != Operator.EQUALS)
			trueMask = stateMask & ~notMask;
		}

#if UNITY_EDITOR
		protected abstract string EnumTypeName { get; }
#endif

		public bool Evaluate(int state)
		{

			/// This quick check allows all unchecked to work no matter the operator.
			if (stateMask == 0)
				return state == 0;

			switch (operation)
			{
				case Operator.EQUALS:
					{
						return (stateMask == state);
					}

				case Operator.OR:
					{
						return ((trueMask & state) != 0) || ((notMask & state) != notMask);
					}

				case Operator.AND:
					{
						return ((trueMask & state) == trueMask) && ((notMask & state) == 0);
					}

				/// Change a bad operation value to default.
				default:
					operation = Operator.EQUALS;
					return (stateMask == state);
			}
		}

#if UNITY_EDITOR

		protected const string HELP_URL = "https://docs.google.com/document/d/1ySmkOBsL0qJnIk7iN9lbXPlfmYTGkN7JFgKDBdqj9e8/edit#bookmark=kix.lw6o4eau4q8i";

		private static GUIStyle boxstyle;
		/// <summary>
		/// Call this code in OnGUI to draw this classes inspector.
		/// </summary>
		/// <param name="sp"></param>
		public void DrawGUI(SerializedProperty sp)
		{

			var stateMask = sp.FindPropertyRelative("stateMask");
			var notMask = sp.FindPropertyRelative("notMask");
			var operation = sp.FindPropertyRelative("operation");

			float lwidth = EditorGUIUtility.labelWidth - 40;

			EditorGUI.BeginChangeCheck();

			//EditorGUI.BeginDisabledGroup(stateMask.intValue == 0);
			///// All false (Despawned) then the only operation that makes sense is EQUALS
			//if (stateMask.intValue == 0 && operation.intValue != 0)
			//{
			//	operation.intValue = 0;
			//}

			Rect opRect = EditorGUILayout.GetControlRect(true, 16);
			//EditorGUI.LabelField(new Rect(opRect) { width = lwidth }, "Operator", (GUIStyle)"RightLabel");
			//var oldOp = operation.intValue;
			EditorUtils.PropertyFieldWithDocsLink(opRect, operation, new GUIContent("Mask Logic", operation.tooltip), HELP_URL);
			if (operation.intValue == 0)
			{
				/// Make sure the notMask gets cleared to avoid possible hard to find bugs later.
				notMask.intValue = 0;
				sp.serializedObject.ApplyModifiedProperties();
			}


			///// a zero statemask is not valid with any operation other than equals currently, so change the state to something when op is changed to something.
			//if (oldOp == (int)Operator.EQUALS && oldOp != operation.intValue)
			//{
			//	Debug.Log("HasCHanged");
			//	stateMask.intValue = 1;
			//	sp.serializedObject.ApplyModifiedProperties();
			//}

			//EditorGUI.PropertyField(new Rect(opRect) /*{ xMin = opRect.xMin + lwidth + 16, width = 82 }*/, operation, new GUIContent("Mask Logic", operation.tooltip));

			//EditorGUI.EndDisabledGroup();

			if (boxstyle == null) boxstyle = new GUIStyle("HelpBox") { padding = new RectOffset(4, 4, 4, 4) };
			EditorGUILayout.BeginVertical(boxstyle);

            if (operation.intValue == (int)Operator.EQUALS)
                EditorGUILayout.LabelField("If value of " + EnumTypeName + " EQUALS ");
            else
                EditorGUILayout.LabelField("If value of " + EnumTypeName + (stateMask.intValue == 0 ? " == 0" : ""));

            int firstSetBit = -1;
			var enumNames = EnumNames;

			for (int i = 0; i < enumNames.Length; ++i)
			{
				int value = EnumValues[i];
				string name = enumNames[i];

				if (value == 0)
					continue;

				bool set = (stateMask.intValue & value) != 0;
				bool not = (notMask.intValue & value) != 0;

				if (set)
					if (firstSetBit == -1)
						firstSetBit = i;

				EditorGUILayout.BeginHorizontal();

				Rect r = EditorGUILayout.GetControlRect();

				EditorGUI.BeginDisabledGroup(!set);
				EditorGUI.LabelField(new Rect(r) { xMin = r.xMin + lwidth + 32 }, name);
				EditorGUI.EndDisabledGroup();

				bool newset = EditorGUI.Toggle(new Rect(r) { xMin = r.xMin + lwidth + 16, width = 16 }, GUIContent.none, set);



                if (operation.intValue != (int)Operator.EQUALS)
				{
					/// Text for if nothing is selected.
					if (stateMask.intValue == 0 )
					{
						//if (i == 1)
						//	EditorGUI.LabelField(new Rect(r) { width = lwidth }, " ", (GUIStyle)"RightLabel");
					}
					else if (set)
					{
                        string conj = operation.intValue == (int)Operator.AND ? "and is " : "or is ";

                        EditorGUI.LabelField(new Rect(r) { width = lwidth }, (i == firstSetBit ? " is " : conj) + (not ? "not " : ""), (GUIStyle)"RightLabel");
						bool newnot = EditorGUI.Toggle(new Rect(r) { xMin = r.xMin + lwidth, width = 16 }, GUIContent.none, not);
						if (newnot != not)
						{
							if (newnot)
								notMask.intValue = notMask.intValue | value;
							else
								notMask.intValue = notMask.intValue & ~value;

							sp.serializedObject.ApplyModifiedProperties();
						}
					}
				}
				else
				{
					//if (i == 1)
					//	EditorGUI.LabelField(new Rect(r) { width = lwidth }, "If " + EnumTypeName + " ==", (GUIStyle)"RightLabel");
				}

				if (newset != set)
				{
					if (newset)
						stateMask.intValue = stateMask.intValue | value;
					else
						stateMask.intValue = stateMask.intValue & ~value;

					/// Remove any not bits for unused states, avoids the need for cleaning that up with an initialization at runtime.
					notMask.intValue = stateMask.intValue & notMask.intValue;

					sp.serializedObject.ApplyModifiedProperties();
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();

			if (EditorGUI.EndChangeCheck())
			{
				sp.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(sp.serializedObject.targetObject);
			}
		}
#endif
	}

}
