// Electro Gryphon Games - 2016
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

namespace UPersian.Editor
{
    /// <summary>
    /// RTL Context Menu adds functions to Right Click Context Menu in hierarchy.
    /// </summary>
    public class UPersianContext : UnityEditor.Editor
    {
        /// <summary>
        /// Holds Loaded resource for _rtlInputField
        /// </summary>
        private static InputField _rtlInputField;
        /// <summary>
        /// Holds Loaded resource for _rtlText
        /// </summary>
        private static RtlText _rtlText;
        /// <summary>
        /// Holds Loaded resource for _rtlButton
        /// </summary>
        private static Button _rtlButton;
        /// <summary>
        /// Holds Loaded resource for _rtlToggle
        /// </summary>
        private static Toggle _rtlToggle;

        /// <summary>
        /// Right click in hierarchy > UPersian > Rtl InputField
        /// Creates RTL inputField.
        /// </summary>
        [MenuItem("GameObject/UPersian/Rtl InputField", false, 10)]
        private static void CreateRtlInputField()
        {
            Transform parent = FindCorrectParent();
            if (parent == null) return;
            if (_rtlInputField == null) _rtlInputField = Resources.Load<InputField>("RtlInputField");
            InputField inputField = Instantiate(_rtlInputField);
            inputField.transform.SetParent(parent, false);
        }

        /// <summary>
        /// Right click in hierarchy > UPersian > Rtl Text
        /// Creates Rtl Text.
        /// </summary>
        [MenuItem("GameObject/UPersian/Rtl Text", false, 10)]
        private static void CreateRtlText()
        {
            Transform parent = FindCorrectParent();
            if (parent == null) return;
            if (_rtlText == null) _rtlText = Resources.Load<RtlText>("RtlText");
            RtlText rtlText = Instantiate(_rtlText);
            rtlText.transform.SetParent(parent, false);
        }
        
        /// <summary>
        /// Right click in hierarchy > UPersian > Rtl Button
        /// Creates Rtl Button.
        /// </summary>
        [MenuItem("GameObject/UPersian/Rtl Button", false, 10)]
        private static void CreateRtlButton()
        {
            Transform parent = FindCorrectParent();
            if (parent == null) return;
            if (_rtlButton == null) _rtlButton = Resources.Load<Button>("RtlButton");
            Button rtlText = Instantiate(_rtlButton);
            rtlText.transform.SetParent(parent.transform, false);
        }

        /// <summary>
        /// Right click in hierarchy > UPersian > Rtl Toggle
        /// Creates Rtl Toggle.
        /// </summary>
        [MenuItem("GameObject/UPersian/Rtl Toggle", false, 10)]
        private static void CreateRtlToggle()
        {
            Transform parent = FindCorrectParent();
            if (parent == null) return;
            if (_rtlToggle == null) _rtlToggle = Resources.Load<Toggle>("RtlToggle");
            Toggle rtlText = Instantiate(_rtlToggle);
            rtlText.transform.SetParent(parent.transform, false);
        }

        /// <summary>
        /// find selected parent or if nothing is selected, uses canvas as parent.
        /// </summary>
        /// <returns></returns>
        private static Transform FindCorrectParent()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                int selectionCount = Selection.gameObjects.Length;
                if (selectionCount == 0) return canvas.transform;
                GameObject selection = Selection.gameObjects[0];
                return selection.GetComponent<RectTransform>() != null ? selection.transform : canvas.transform;
            }
            Debug.Log("There is no Canvas");
            return null;
        }
    }
}