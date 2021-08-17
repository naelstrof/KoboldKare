using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;
using KoboldKare;
using System;
using SolidUtilities.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualLogic {
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(VisualLogic.VisualLogicGraph.BlackboardValue))]
	public class BlackboardValueDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Draw label
			//position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			var nameRect = new Rect(position.x, position.y, position.width/2-1, position.height);
			var typeRect = new Rect(position.x+position.width/2+1, position.y, position.width/4-2, position.height);
			var fieldRect = new Rect(position.x+position.width*(3f/4f)+1, position.y, position.width/4-1, position.height);
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);


			Type t = Type.GetType(property.FindPropertyRelative("typeNameAndAssembly").stringValue);
            Type[] possibleTypes = {typeof(string), typeof(int), typeof(float), typeof(double), typeof(bool), typeof(Vector3)};
            List<string> possibleStrings = new List<string>();
            for (int i=0;i<possibleTypes.Length;i++){
                possibleStrings.Add(possibleTypes[i].ToString());
            }
            possibleStrings.Add("Reference");

            int selected = possibleStrings.Count-1;
            if (t!=null) {
                for (int i=0;i<possibleTypes.Length;i++){
                    if (t.ToString() == possibleStrings[i]) {
                        selected = i;
                        break;
                    }
                }
            }
            selected = EditorGUI.Popup(typeRect, selected, possibleStrings.ToArray());
            if (selected<possibleTypes.Length) {
                property.FindPropertyRelative("typeNameAndAssembly").stringValue = VisualLogicGraph.GetTypeNameAndAssembly(possibleTypes[selected]);
            } else {
                property.FindPropertyRelative("typeNameAndAssembly").stringValue = VisualLogicGraph.GetTypeNameAndAssembly(typeof(GameObject));
            }
            t = Type.GetType(property.FindPropertyRelative("typeNameAndAssembly").stringValue);
			// Draw fields - passs GUIContent.none to each so they are drawn without labels
			//EditorGUI.LabelField(enumRect, t.ToString(), GUIStyle.none);
			if (t == typeof(string)) {
				EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("stringValue"), GUIContent.none);
			} else if (t==typeof(int)) {
				EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("intValue"), GUIContent.none);
			} else if (t==typeof(float)) {
				EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("floatValue"), GUIContent.none);
			} else if (t==typeof(double)) {
				EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("doubleValue"), GUIContent.none);
			} else if (t==typeof(bool)) {
				EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("boolValue"), GUIContent.none);
			} else if (t==typeof(Vector3)) {
				EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("vector3Value"), GUIContent.none);
			} else if (t.InheritsFrom(typeof(UnityEngine.Object))) {
				EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("objectValue"), GUIContent.none);
			} else {
				EditorGUI.LabelField(fieldRect, "<no serialize>", GUIStyle.none);
			}

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
#endif
    [CreateAssetMenu(menuName = "VisualLogic/Graph", order = 0)]
    public class VisualLogicGraph : NodeGraph {
		public static void MakeSureTypeHasName(Type type) {
			if (type != null && type.FullName == null)
				throw new ArgumentException($"'{type}' does not have full name.", nameof(type));
		}
		public static string GetTypeNameAndAssembly(Type type) {
			MakeSureTypeHasName(type);
			return type != null
				? $"{type.FullName}, {type.GetShortAssemblyName()}"
				: string.Empty;
		}
		[System.Serializable]
		public class BlackboardValue {
			private Type cachedType = null;
            public string name;
			public Type parameterType {
				get {
					if (cachedType == null || !Application.isPlaying) {
						cachedType = Type.GetType(typeNameAndAssembly);
					}
					return cachedType;
				}
				set {
					typeNameAndAssembly = GetTypeNameAndAssembly(value);
					cachedType = null;
				}
			}
			public object value {
				get {
					if (parameterType == typeof(string)) {
						return stringValue;
					} else if (parameterType==typeof(int)) {
						return intValue;
					} else if (parameterType==typeof(float)) {
						return floatValue;
					} else if (parameterType==typeof(double)) {
						return doubleValue;
					} else if (parameterType==typeof(Vector3)) {
						return vector3Value;
					} else if (parameterType==typeof(bool)) {
						return boolValue;
					} else if (parameterType.InheritsFrom(typeof(UnityEngine.Object))) {
						return objectValue;
					}
					return null;
				}
			}
			public string typeNameAndAssembly;
			public string stringValue;
			public int intValue;
			public float floatValue;
			public float doubleValue;
			public Vector3 vector3Value;
			public bool boolValue;
			public UnityEngine.Object objectValue;
		}
        [NonSerialized]
        public Dictionary<string, object> blackboard = new Dictionary<string, object>();

        // Just blast through the enuemrator all in one go if we can, because the graph suffers from async issues.
        public IEnumerator TriggerWrapper(IEnumerator task) {
            while(task.MoveNext()) {
                var c = task.Current;
                if (c is WaitForSeconds || c is WaitForSecondsRealtime || c is WaitForFixedUpdate || c is WaitUntil) {
                    yield return c;
                }
                if (c is IEnumerator) {
                    var cn = TriggerWrapper(c as IEnumerator);
                    if (cn != null) {
                        yield return cn;
                    }
                }
            }
        }
        public Task TriggerEvent(GameObject self, Event.EventType type) {
            foreach(var node in nodes) {
                if (node is Event && (node as Event).eventType == type) {
                    return new Task(TriggerWrapper((node as Event).Trigger(self)));
                }
            }
            return null;
        }
    }
}