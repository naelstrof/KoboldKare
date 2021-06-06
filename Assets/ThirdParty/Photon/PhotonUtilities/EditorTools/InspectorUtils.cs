

using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Photon.Utilities
{
#if UNITY_EDITOR

	/// <summary>
	/// Hacky method of getting a real object in the property drawer
	/// </summary>
	public static class PropertyDrawerUtility
	{
		public static T GetActualObjectForSerializedProperty<T>(this FieldInfo fieldInfo, SerializedProperty property) where T : class
		{
			var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
			
			if (obj == null) { return null; }

			T actualObject = null;
			if (obj.GetType().IsArray)
			{
				var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
				actualObject = ((T[])obj)[index];
			}
			else
			{
				actualObject = obj as T;
			}
			return actualObject;
		}

		public static object GetParent(SerializedProperty prop)
		{
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements.Take(elements.Length - 1))
			{
				if (element.Contains("["))
				{
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue(obj, elementName, index);
				}
				else
				{
					obj = GetValue(obj, element);
				}
			}
			return obj;
		}

		/// <summary>
		/// Will attempt to find the index of a property drawer item. Returns -1 if it appears to not be an array type.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static int GetIndexOfDrawerObject(SerializedProperty property, bool reportError = true)
		{
			string path = property.propertyPath;

			int start = path.IndexOf("[") + 1;
			int len = path.IndexOf("]") - start;

			if (len < 1)
				return -1;

			int index = -1;
			if ((len > 0 && Int32.TryParse(path.Substring(path.IndexOf("[") + 1, len), out index)) == false && reportError)
			{
				Debug.Log("Attempted to find the index of a non-array serialized property.");
			}
			return index;
		}

		private static object GetValue(object source, string name)
		{
			if (source == null)
				return null;
			var type = source.GetType();
			var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (f == null)
			{
				var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p == null)
					return null;
				return p.GetValue(source, null);
			}
			return f.GetValue(source);
		}

		private static object GetValue(object source, string name, int index)
		{
			var enumerable = GetValue(source, name) as IEnumerable;
			var enm = enumerable.GetEnumerator();
			while (index-- >= 0)
				enm.MoveNext();
			return enm.Current;
		}
	}
#endif


	public class MinMaxRangeAttribute : PropertyAttribute
	{
		public readonly float max;
		public readonly float min;

		public MinMaxRangeAttribute(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
	class MinMaxRangeDrawer : PropertyDrawer
	{

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

			if (property.propertyType == SerializedPropertyType.Vector2)
			{
				Vector2 range = property.vector2Value;
				float min = range.x;
				float max = range.y;
				MinMaxRangeAttribute attr = attribute as MinMaxRangeAttribute;
				EditorGUI.BeginChangeCheck();
				EditorGUI.MinMaxSlider(position, label, ref min, ref max, attr.min, attr.max);
				if (EditorGUI.EndChangeCheck())
				{
					range.x = min;
					range.y = max;
					property.vector2Value = range;
				}
			}
			else
			{
				EditorGUI.LabelField(position, label, "Use only with Vector2");
			}
		}
	}
#endif

	/// <summary>
	/// Custom drawer attribute that will put bit range slider in the editor.
	/// </summary>
	public class BitsPerRangeAttribute : PropertyAttribute
	{
		public readonly int max;
		public readonly int min;
		public readonly string label;
		public readonly bool showLabel;
		public readonly string tooltip;
		public bool show;

		public BitsPerRangeAttribute(int min, int max, bool show, bool zeroBase = false, string label = "Max:", bool showLabel = true, string tooltip = "")
		{
			this.show = show;
			this.min = min;
			this.max = max;
			this.label = label;
			this.showLabel = showLabel;
			this.tooltip = tooltip;
		}
	}

#if UNITY_EDITOR


	[CustomPropertyDrawer(typeof(BitsPerRangeAttribute))]
	public class BitsPerRangeDrawer : PropertyDrawer
	{
		const float labelHeight = 20;
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			
			BitsPerRangeAttribute attr = attribute as BitsPerRangeAttribute;
			if (attr.show)
				return attr.showLabel ? 66 : (66 - labelHeight); //EditorGUI.GetPropertyHeight(property, label, true);
			else
				return 0;
		}

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{

			BitsPerRangeAttribute attr = attribute as BitsPerRangeAttribute;
			if (!attr.show)
				return;

			Rect ir = EditorGUI.IndentedRect(r);

			GUIContent emptyContent = new GUIContent("", attr.tooltip);

			float _labelheight = attr.showLabel ? labelHeight : 0;

			if (attr.showLabel)
				GUI.Label(new Rect(ir.xMin, ir.yMin, ir.width - 1, 17), property.displayName + ":", "OL Title");

			GUI.Box(new Rect(ir.xMin, ir.yMin + _labelheight, ir.width, ir.height - 22), GUIContent.none, "GroupBox");

			float padding = 6f;
			Rect row = new Rect(ir.xMin + padding, ir.yMin + _labelheight + padding, ir.width - padding * 2, 17);

			int value = property.intValue;

			//GUI.Label(row, property.displayName + ":", "BoldLabel");

			row.width -= 28;

			int holdindent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			property.intValue = EditorGUI.IntSlider(row, emptyContent, property.intValue, attr.min, attr.max);
			EditorGUI.indentLevel = holdindent;
			row.width += 28;

			GUI.Label(row, "bits", "RightLabel");

			row.y += 17f;
			row.height = 17f;
			GUI.Label(row, attr.label + " " + ((uint)System.Math.Pow(2, property.intValue)).ToString());

		}
	}
#endif

	/// <summary>
	/// Read Only attribute
	/// </summary>
	public class ReadOnlyAttribute : PropertyAttribute
	{

	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			//EditorGUI.PropertyField(position, property, label, true);
			EditorGUI.LabelField(r, label);
			EditorGUI.PropertyField(new Rect(r.width - 50 + 14, r.yMin, 50, r.height), property, GUIContent.none, true);
			GUI.enabled = true;
		}
	}
#endif


	/// <summary>
	/// Single Layer at a time variant of LayerMask
	/// </summary>
	[System.Serializable]
	public class SingleUnityLayer
	{
		[SerializeField]
		[HideInInspector]
		private int m_LayerIndex = 0;

		public int LayerIndex
		{
			get { return m_LayerIndex; }
		}

		public void Set(int _layerIndex)
		{
			if (_layerIndex > 0 && _layerIndex < 32)
			{
				m_LayerIndex = _layerIndex;
			}
		}

		public int Mask
		{
			get { return 1 << m_LayerIndex; }
		}

		public static implicit operator int(SingleUnityLayer m)
		{
			return m.LayerIndex;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(SingleUnityLayer))]
	public class SingleUnityLayerPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
		{
			EditorGUI.BeginProperty(_position, GUIContent.none, _property);
			SerializedProperty layerIndex = _property.FindPropertyRelative("m_LayerIndex");
			_position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
			if (layerIndex != null)
			{
				layerIndex.intValue = EditorGUI.LayerField(_position, layerIndex.intValue);
			}
			EditorGUI.EndProperty();
		}
	}
#endif


}

