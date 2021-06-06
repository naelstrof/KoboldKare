#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using UnityEditor.Localization.UI;
using System;
using System.Linq;

namespace UnityEditor.Localization.Plugins.InputTableCreator {
    public static class SerializedPropertyExtensions {
        public static TObject GetActualObjectForSerializedProperty<TObject>(this SerializedProperty property, System.Reflection.FieldInfo field) where TObject : class
        {
            try
            {
                if (property == null || field == null)
                    return null;
                var serializedObject = property.serializedObject;
                if (serializedObject == null)
                {
                    return null;
                }
                var targetObject = serializedObject.targetObject;
                var obj = field.GetValue(targetObject);
                if (obj == null)
                {
                    return null;
                }
                TObject actualObject = null;
                if (obj.GetType().IsArray)
                {
                    var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                    actualObject = ((TObject[])obj)[index];
                }
                else if (typeof(IList).IsAssignableFrom(obj.GetType()))
                {
                    var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                    actualObject = ((IList)obj)[index] as TObject;
                }
                else
                {
                    actualObject = obj as TObject;
                }
                return actualObject;
            }
            catch
            {
                return null;
            }
        }
    }
    [Serializable]
    [AssetTableCollectionExtension]
    public class InputTableCreator: CollectionExtension {
        public static string[] searchDirectories = {
            "Assets/Xelu_Free_Controller&Key_Prompts/Keyboard & Mouse/Light",
            "Assets/Xelu_Free_Controller&Key_Prompts/Xbox One",
            "Assets/Xelu_Free_Controller&Key_Prompts/PS4",
        };
        public static string[] controlList = {
            "<Keyboard>/a",
            "<Keyboard>/b",
            "<Keyboard>/c",
            "<Keyboard>/d",
            "<Keyboard>/e",
            "<Keyboard>/f",
            "<Keyboard>/g",
            "<Keyboard>/h",
            "<Keyboard>/i",
            "<Keyboard>/j",
            "<Keyboard>/k",
            "<Keyboard>/l",
            "<Keyboard>/m",
            "<Keyboard>/n",
            "<Keyboard>/o",
            "<Keyboard>/p",
            "<Keyboard>/q",
            "<Keyboard>/r",
            "<Keyboard>/s",
            "<Keyboard>/t",
            "<Keyboard>/u",
            "<Keyboard>/v",
            "<Keyboard>/w",
            "<Keyboard>/x",
            "<Keyboard>/y",
            "<Keyboard>/z",
            "<Keyboard>/1",
            "<Keyboard>/2",
            "<Keyboard>/3",
            "<Keyboard>/4",
            "<Keyboard>/5",
            "<Keyboard>/6",
            "<Keyboard>/7",
            "<Keyboard>/8",
            "<Keyboard>/9",
            "<Keyboard>/0",
            "<Keyboard>/backspace",
            "<Keyboard>/backquote",
            "<Keyboard>/tab",
            "<Keyboard>/capsLock",
            "<Keyboard>/leftShift",
            "<Keyboard>/leftCtrl",
            "<Keyboard>/rightShift",
            "<Keyboard>/rightCtrl",
            "<Keyboard>/ctrl",
            "<Keyboard>/shift",
            "<Keyboard>/leftMeta",
            "<Keyboard>/rightMeta",
            "<Keyboard>/meta",
            "<Keyboard>/leftAlt",
            "<Keyboard>/rightAlt",
            "<Keyboard>/alt",
            "<Keyboard>/space",
            "<Keyboard>/comma",
            "<Keyboard>/period",
            "<Keyboard>/slash",
            "<Keyboard>/quote",
            "<Keyboard>/leftBracket",
            "<Keyboard>/rightBracket",
            "<Keyboard>/backslash",
            "<Keyboard>/minus",
            "<Keyboard>/equals",
            "<Keyboard>/equals",
            "<Keyboard>/printScreen",
            "<Keyboard>/pause",
            "<Keyboard>/pause",
            "<Keyboard>/rightArrow",
            "<Keyboard>/leftArrow",
            "<Keyboard>/upArrow",
            "<Keyboard>/downArrow",
            "<Keyboard>/numpad1",
            "<Keyboard>/numpad2",
            "<Keyboard>/numpad3",
            "<Keyboard>/numpad4",
            "<Keyboard>/numpad5",
            "<Keyboard>/numpad6",
            "<Keyboard>/numpad7",
            "<Keyboard>/numpad8",
            "<Keyboard>/numpad9",
            "<Keyboard>/numpad0",
            "<Keyboard>/numpadEnter",
            "<Keyboard>/numpadPlus",
            "<Keyboard>/numpadMinus",
            "<Keyboard>/numpadMultiply",
            "<Keyboard>/numpadDivide",
            "<Keyboard>/f1",
            "<Keyboard>/f2",
            "<Keyboard>/f3",
            "<Keyboard>/f4",
            "<Keyboard>/f5",
            "<Keyboard>/f6",
            "<Keyboard>/f7",
            "<Keyboard>/f8",
            "<Keyboard>/f9",
            "<Keyboard>/f10",
            "<Keyboard>/f11",
            "<Keyboard>/f12",
            "<Keyboard>/escape",
            "<Keyboard>/enter",
            "<Keyboard>/insert",
            "<Keyboard>/home",
            "<Keyboard>/pageUp",
            "<Keyboard>/pageDown",
            "<Keyboard>/end",
            "<Keyboard>/scrollLock",

            "<Mouse>/rightButton",
            "<Mouse>/leftButton",
            "<Mouse>/middleButton",
            "<Mouse>/scroll",
            "<Mouse>/scroll/y",
            "<Mouse>/backButton",
            "<Mouse>/forwardButton",

            "<Gamepad>/rightShoulder",
            "<Gamepad>/leftShoulder",
            "<Gamepad>/leftStickPress",
            "<Gamepad>/rightStickPress",
            "<Gamepad>/leftStick/left",
            "<Gamepad>/leftStick/right",
            "<Gamepad>/leftStick/up",
            "<Gamepad>/leftStick/down",
            "<Gamepad>/rightStick/left",
            "<Gamepad>/rightStick/right",
            "<Gamepad>/rightStick/up",
            "<Gamepad>/rightStick/down",
            "<Gamepad>/leftStick",
            "<Gamepad>/rightStick",
            "<Gamepad>/leftStick/x",
            "<Gamepad>/leftStick/y",
            "<Gamepad>/rightStick/x",
            "<Gamepad>/rightStick/y",
            "<Gamepad>/buttonSouth",
            "<Gamepad>/buttonEast",
            "<Gamepad>/buttonWest",
            "<Gamepad>/buttonNorth",
            "<Gamepad>/dpad/up",
            "<Gamepad>/dpad/left",
            "<Gamepad>/dpad/right",
            "<Gamepad>/dpad/down",
            "<Gamepad>/start",
            "<Gamepad>/select",
            "<Gamepad>/leftTrigger",
            "<Gamepad>/rightTrigger",
            
            "<DualShock4GamepadHID>/systemButton",
            "<DualShock4GamepadHID>/touchButton",
            "<DualShock4GamepadHID>/leftTriggerButton",
            "<DualShock4GamepadHID>/rightTriggerButton",
        };
    }
    public class InputTableCreatorExtensionPropertyDrawerData {
        public SerializedProperty m_Collection;
        public SerializedProperty m_SheetsServiceProvider;
        public SerializedProperty m_SpreadSheetId;
        public SerializedProperty m_SheetId;
        public SerializedProperty m_Columns;
        public SerializedProperty m_RemoveMissingPulledKeys;

        public string m_NewSheetName;
    }
    [CustomPropertyDrawer(typeof(InputTableCreator))]
    public class InputTableCreatorExtensionPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty m_property;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            m_property = property;
            EditorGUI.BeginProperty(position, label, property);
            var buttRect = new Rect(position.x, position.y, position.width, position.height);
            if (GUI.Button(buttRect, "Regenerate")) {
                Generate();
            }
            EditorGUI.EndProperty();
        }

        public void Generate() {
            InputTableCreator target = m_property.GetActualObjectForSerializedProperty<InputTableCreator>(fieldInfo);
            var collection = target.TargetCollection as AssetTableCollection;
            foreach( var table in collection.AssetTables) {
                foreach( string key in InputTableCreator.controlList) {
                    long id = 0;
                    if (collection.SharedData.Contains(key)) {
                        id = collection.SharedData.GetEntry(key).Id;
                    } else {
                        id = collection.SharedData.AddKey(key).Id;
                    }
                    Texture2D tex = GetTexture(key);
                    if (tex != null) {
                        collection.AddAssetToTable(table, id, GetTexture(key));
                    }
                }
                EditorUtility.SetDirty(table);
            }
            EditorUtility.SetDirty(collection.SharedData);
            //LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(this, collection);
            //foreach( AssetTable table in ltc.OnBeforeSerialize) {
                //foreach( string key in InputTableCreator.controlList) {
                    //if (!table.asset.SharedData.Contains(key)) {
                        //table.asset.SharedData.AddKey(key);
                    //}
                    //table.asset.SharedData.;
                //}
            //}
        }

        // This garbage just gets the names closer to what a free icon pack had. Pretty much completely arbitrary! :)
        public string ConvertKeyToValidSearch(string key) {
            string start = key.Substring(0, key.IndexOf('/'));
            string end = key.Substring(key.IndexOf('/')+1);
            if (start == "<Keyboard>" || start == "<Mouse>") {
                if (end == "slash") {
                    return "Question_Key";
                }
                // a-z, 0-9
                if (!end.Contains("Arrow") && start != "<Mouse>" && !end.Contains("Bracket")) {
                    end = end.Replace("left","");
                    end = end.Replace("right","");
                }
                end = end.Replace("/x","");
                end = end.Replace("/y","");
                string captialized = char.ToUpper(end[0]) + end.Substring(1);
                if (end.Length == 1) {
                    return end.ToUpper() + "_Key";
                }
                if (end.Contains("numpad")) {
                    // numpadEnter, numpadPlus
                    if (end.Contains("Enter") || end.Contains("Plus")) {
                        return captialized + "_Key";
                    }
                    // numpad0-numpad9, numpadDivide, numpadMultiply, numpadMinus,
                    return end.Substring(6) + "_Key";
                }
                if (captialized == "Equals") {
                    captialized = "Plus"; // there's no equal key graphic
                }
                return captialized + "_Key";
            }
            if (start == "<Gamepad>" || start == "<DualShock4GamepadHID>") {
                if (end.Contains("leftStick") || end.Contains("rightStick")) {
                    end = end.Replace("/left","");
                    end = end.Replace("/right","");
                    end = end.Replace("/up","");
                    end = end.Replace("/down","");
                }
                string captialized = char.ToUpper(end[0]) + end.Substring(1);
                if (start == "<DualShock4GamepadHID>") {
                    captialized = captialized.Replace("Button","");
                    if (end == "systemButton") {
                        return "PS4_Cross";
                    }
                    return "PS4_"+captialized;
                }
                if (end.Contains("dpad")) {
                    string dir = end.Substring(end.IndexOf('/')+1);
                    string cDir = char.ToUpper(dir[0]) + dir.Substring(1);
                    return "XboxOne_Dpad_" + cDir;
                }
                captialized = captialized.Replace("/x","");
                captialized = captialized.Replace("/y","");
                return "XboxOne_"+captialized;
            }
            return end;
        }

        public Texture2D GetTexture(string key) {
            foreach( string guid in AssetDatabase.FindAssets(ConvertKeyToValidSearch(key), InputTableCreator.searchDirectories)) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(ConvertKeyToValidSearch(key))) {
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                }
            }
            return null;
        }
    }
}
#endif
