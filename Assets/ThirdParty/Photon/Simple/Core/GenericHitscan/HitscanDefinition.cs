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
    public enum Replicate { None, CastOnly, Hits, HitsWithContactGroups }

    /// <summary>
    /// This is the inspector element for HitscanDefinition.
    /// </summary>
    [System.Serializable]
    public class HitscanDefinition
    {

        public Replicate ownerToAuthority = Replicate.Hits;
        public Replicate authorityToAll = Replicate.Hits;

        [Tooltip("This cast/overlap test will be done on the initiating client against colliders on these layers. " +
            "Exclude layers that won't include any objects that you don't want to test (such as walls).")]
        public LayerMask layerMask = -1;
        public bool useOffset;
        public Vector3 offset1 = new Vector3(0, 0, 0); // Used offset and first sphere of capsule
        public Vector3 offset2 = new Vector3(0, 1, 0); // Used for second sphere of capsule
        public Vector3 halfExtents = new Vector3(1, 1, 1);
        public Vector3 orientation = new Vector3(0, 0, 0);
        public HitscanType hitscanType = HitscanType.Raycast;
        public float distance = 100f;
        public float radius = 1;
        public bool nearestOnly = true;
        //public bool NearestOnly
        //{
        //	set { nearestOnly = value; }
        //	get
        //	{
        //		if (hitscanType.IsOverlap())
        //			return false;
        //		else
        //			return nearestOnly;
        //	}
        //}

    }


#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(HitscanDefinition))]
    public class HitscanDefinitionDrawer : PropertyDrawer
    {
        public static GUIStyle lefttextstyle = new GUIStyle
        {
            alignment = TextAnchor.UpperLeft,
            richText = true
        };

        private const float PAD = 4;
        private const float SPACING = 18;
        private const float FLDHGHT = 18;

        public override void OnGUI(Rect r, SerializedProperty _property, GUIContent _label)
        {
            r.yMax -= 6;
            // Draw leading black separator
            r.yMin += 2;
            EditorGUI.DrawRect(new Rect(r) { height = 2 }, new Color(.15f, .15f, .15f));
            r.yMin += 4;

            EditorGUI.BeginProperty(r, GUIContent.none, _property);

            int startingIndent = EditorGUI.indentLevel;

            SerializedProperty layerMask = _property.FindPropertyRelative("layerMask");

            SerializedProperty useOffset = _property.FindPropertyRelative("useOffset");
            SerializedProperty offset1 = _property.FindPropertyRelative("offset1");
            SerializedProperty offset2 = _property.FindPropertyRelative("offset2");
            SerializedProperty hitscanType = _property.FindPropertyRelative("hitscanType");
            SerializedProperty distance = _property.FindPropertyRelative("distance");
            SerializedProperty radius = _property.FindPropertyRelative("radius");

            SerializedProperty halfExtents = _property.FindPropertyRelative("halfExtents");
            SerializedProperty orientation = _property.FindPropertyRelative("orientation");

            SerializedProperty nearestOnly = _property.FindPropertyRelative("nearestOnly");

            HitscanType hitscantype = (HitscanType)hitscanType.intValue;

            EditorGUI.LabelField(new Rect(r) { height = FLDHGHT }, _label);
            r.yMin += SPACING;

            GUI.Box(r, GUIContent.none, (GUIStyle)"HelpBox");

            r.xMin += PAD;
            r.xMax -= PAD;
            r.height = FLDHGHT;

            r.y += PAD;

            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(r, hitscanType);

            r.y += SPACING;
            EditorGUI.PropertyField(r, layerMask);

            // Show dist for any of the casts
            if (hitscantype.IsCast())
            {
                r.y += SPACING;
                EditorGUI.PropertyField(r, distance);
            }

            r.y += SPACING;
            EditorGUI.PropertyField(r, nearestOnly);

            // Radius types
            if (hitscantype.UsesRadius())
            {
                r.y += SPACING;
                EditorGUI.PropertyField(r, radius);
            }

            // Offset (all but capsule)
            if (!hitscantype.IsCapsule())
            {
                r.y += SPACING;
                EditorGUI.LabelField(new Rect(r.xMin + 18, r.y, 100, FLDHGHT), new GUIContent("Offset"));
                EditorGUI.PropertyField(new Rect(r.xMin, r.y, 32, FLDHGHT), useOffset, GUIContent.none);

                if (useOffset.boolValue)
                    EditorGUI.PropertyField(new Rect(r.xMin + 100, r.y, r.width - 100, FLDHGHT), offset1, GUIContent.none);

            }

            // Show Point1 and Point2 for capsule.
            if (hitscantype.IsCapsule())
            {
                r.y += SPACING;
                EditorGUI.LabelField(new Rect(r.xMin, r.y, 100, FLDHGHT), new GUIContent("Offset1:"));
                EditorGUI.PropertyField(new Rect(r.xMin + 100, r.y, r.width - 100, FLDHGHT), offset1, GUIContent.none);

                r.y += SPACING;
                EditorGUI.LabelField(new Rect(r.xMin, r.y, 100, FLDHGHT), new GUIContent("Offset2:"));
                EditorGUI.PropertyField(new Rect(r.xMin + 100, r.y, r.width - 100, FLDHGHT), offset2, GUIContent.none);

            }
            else if (hitscantype.IsBox())
            {
                r.y += SPACING;
                EditorGUI.LabelField(new Rect(r.xMin, r.y, 100, FLDHGHT), new GUIContent("Half Extents:"));
                EditorGUI.PropertyField(new Rect(r.xMin + 100, r.y, r.width - 100, FLDHGHT), halfExtents, GUIContent.none);

                r.y += SPACING;
                EditorGUI.LabelField(new Rect(r.xMin, r.y, 100, FLDHGHT), new GUIContent("Orientation:"));
                EditorGUI.PropertyField(new Rect(r.xMin + 100, r.y, r.width - 100, FLDHGHT), orientation, GUIContent.none);

            }

            /// Not sure this is actually needed.
            bool haschanged = EditorGUI.EndChangeCheck();
            if (haschanged)
                EditorUtility.SetDirty(_property.serializedObject.targetObject);

            EditorGUI.EndProperty();

            // Draw closing black separator
            r.xMin -= PAD;
            r.xMax += PAD;
            EditorGUI.DrawRect(new Rect(r) { y = r.y + 26, height = 2 }, new Color(.15f, .15f, .15f));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty hitscanType = property.FindPropertyRelative("hitscanType");

            HitscanType hittype = (HitscanType)hitscanType.intValue;

            bool doesntNeedExtraLine = hittype == HitscanType.Raycast || hittype == HitscanType.OverlapSphere;
            bool needsExtraExtra = hittype == HitscanType.CapsuleCast || hittype == HitscanType.BoxCast;
            bool isOverlap = hittype.IsOverlap();
            const int SEPARATORS = 12;
            return SEPARATORS + (5 * SPACING + PAD * 2) + (isOverlap ? SPACING : 0) + (doesntNeedExtraLine ? 0 : (needsExtraExtra ? SPACING * 2 : SPACING)) + (hittype.IsCast() ? SPACING : 0);
        }
    }


#endif
}



