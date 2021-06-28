using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using KoboldKare;
using TypeReferences;
using System.Reflection;
using System;
using SolidUtilities.Extensions;
#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
#endif
namespace VisualLogic {

	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(VisualLogic.Function.FunctionMethodName))]
	public class FunctionMethodNameDrawer : PropertyDrawer {
		public List<string> stringsToDisplay = new List<string>();
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);


			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			//var enumRect = new Rect(position.x, position.y, 80, position.height);
			var typeRect = new Rect(position.x, position.y, position.width, position.height/2f-2f);
			var fieldRect = new Rect(position.x, position.y+position.height/2f+1f, position.width, position.height/2f-2f);

			EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("targetBehavior"), new GUIContent("Behavior"), false);
			string guid = property.FindPropertyRelative("targetBehavior").FindPropertyRelative("GUID").stringValue;
			string typeNameAndAssembly = property.FindPropertyRelative("targetBehavior").FindPropertyRelative("TypeNameAndAssembly").stringValue;
			SerializedProperty methodProperty = property.FindPropertyRelative("methodName");
			SerializedProperty parameterTypesProperty = property.FindPropertyRelative("parameterTypeNamesAndAssemblies");
			stringsToDisplay.Clear();
			stringsToDisplay.Add("(None)");
			Type t = Type.GetType(typeNameAndAssembly);
			List<Type> parameterTypes = new List<Type>();
			for(int i=0;i<parameterTypesProperty.arraySize;i++) {
				parameterTypes.Add(Type.GetType(parameterTypesProperty.GetArrayElementAtIndex(i).stringValue));
			}

			MethodInfo[] methods = t?.GetMethods();

			int selected = 0;
			if (t != null) {
				MethodInfo selectedMethod = t.GetMethod(methodProperty.stringValue, parameterTypes.ToArray());
				foreach(MethodInfo info in methods) {
					stringsToDisplay.Add(info.Name);
				}
				for(int i=0;i<methods.Length;i++) {
					if (methods[i] == selectedMethod) {
						selected = i+1;
						break;
					}
				}
			}
			selected = EditorGUI.Popup(fieldRect, "Method Name", selected, stringsToDisplay.ToArray());

			if (selected == 0) {
				methodProperty.stringValue = string.Empty;
			} else if (t!=null) {
				methodProperty.stringValue = stringsToDisplay[selected];
				var arguments = methods[selected-1].GetParameters();
				while(parameterTypesProperty.arraySize < arguments.Length) { parameterTypesProperty.InsertArrayElementAtIndex(0); }
				while(parameterTypesProperty.arraySize > arguments.Length) { parameterTypesProperty.DeleteArrayElementAtIndex(0); }
				for(int i=0;i<arguments.Length;i++) {
					parameterTypesProperty.GetArrayElementAtIndex(i).stringValue = Function.GetTypeNameAndAssembly(arguments[i].ParameterType);
				}
				parameterTypesProperty.serializedObject.ApplyModifiedProperties();
			}


			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return 38.0f;
		}
	}
	[CustomPropertyDrawer(typeof(VisualLogic.Function.FunctionParameter))]
	public class FunctionParameterDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Draw label
			Type t = Type.GetType(property.FindPropertyRelative("typeNameAndAssembly").stringValue);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(t.ToString()));

			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			var fieldRect = new Rect(position.x, position.y, position.width, position.height);

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
	//[NodeTint("#666622")]
	[NodeWidth(300)]
	public class Function : VisualLogicBaseNode {
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
		public class FunctionMethodName {
			[TypeOptions(Grouping = Grouping.ByAddComponentMenu)]
			public TypeReference targetBehavior;
			private MethodInfo internalMethod = null;
			public MethodInfo methodInfo {
				get {
					if (internalMethod == null || !Application.isPlaying) {
						List<Type> types = new List<Type>();
						for(int i=0;i<parameterTypeNamesAndAssemblies.Count;i++) {
							types.Add(Type.GetType(parameterTypeNamesAndAssemblies[i]));
						}
						internalMethod = targetBehavior.Type.GetMethod(methodName, types.ToArray());
					}
					return internalMethod;
				}
			}
			public string methodName;
			public List<string> parameterTypeNamesAndAssemblies = new List<string>();
		}
		[System.Serializable]
		public class FunctionParameter {
			private Type cachedType = null;
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
			public UnityEngine.Object objectValue;
		}
		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict, dynamicPortList = false)]
		public VisualLogicBaseNode input;

		[Input(backingValue = ShowBackingValue.Unconnected, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict, dynamicPortList = false)]
		public UnityEngine.Object targetObject;
		public FunctionMethodName method;

		[Input(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.None, dynamicPortList = true)]
		public List<FunctionParameter> parameters = new List<FunctionParameter>();

		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict, dynamicPortList = false)]
		public VisualLogicBaseNode output;


		[Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override, dynamicPortList = false)]
		public bool returnValue;
		public object actualReturnValue;
		public void OnValidate() {
			if (method.targetBehavior == null || method.targetBehavior.Type == null || string.IsNullOrEmpty(method.methodName)) {
				return;
			}
			ParameterInfo[] parameterInfos = method.methodInfo.GetParameters();
			while (parameters.Count < parameterInfos.Length) {
				parameters.Add(new FunctionParameter());
			}
			while (parameters.Count > parameterInfos.Length) {
				parameters.RemoveAt(parameters.Count-1);
			}
			for(int i=0;i<parameterInfos.Length;i++) {
				parameters[i].parameterType = parameterInfos[i].ParameterType;
			}
		}
		public override IEnumerator Trigger(GameObject self) {
			ParameterInfo[] pinfo = method.methodInfo.GetParameters();
			object[] para = new object[pinfo.Length];
			for (int i=0;i<parameters.Count;i++) {
				NodePort inputPort = GetInputPort("parameters "+i);
				if (inputPort != null) {
					object value = inputPort.GetInputValue();
					para[i] = value == null ? parameters[i].value : value;
				}
			}

            NodePort objPort = GetInputPort("targetObject");
			UnityEngine.Object inputPortGameObject;
			if (objPort.TryGetInputValue<UnityEngine.Object>(out inputPortGameObject)) {
				targetObject = inputPortGameObject;
			}
			if (targetObject == null) {
				Component c = self.GetComponent(method.targetBehavior);
				actualReturnValue = method.methodInfo.Invoke(c, para);
			} else if (targetObject.GetType() == method.targetBehavior.Type){
				actualReturnValue = method.methodInfo.Invoke(targetObject, para);
			} else if (targetObject.GetType() == typeof(GameObject)) {
				actualReturnValue = method.methodInfo.Invoke((targetObject as GameObject).GetComponent(method.targetBehavior), para);
			} else {
				throw new UnityException("Couldn't find " + method.targetBehavior + " on target gameobject");
			}

            NodePort port = GetOutputPort("output");
            if (port != null) {
                for (int i = 0; i < port.ConnectionCount; i++) {
                    NodePort connection = port.GetConnection(i);
                    yield return (connection.node as VisualLogicBaseNode).Trigger(self);
                }
            }
		}
		public override object GetValue(NodePort port) {
			if (port == GetOutputPort("returnValue")) {
				return actualReturnValue;
			}
			return null;
		}
	}
}