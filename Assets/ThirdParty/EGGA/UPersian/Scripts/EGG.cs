#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UPersian.Scripts
{
    [ExecuteInEditMode]
    public class EGG : MonoBehaviour
    {
        public static Texture2D EGGLogo;

        void OnEnable()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowCallback;
        }

        void OnDisable()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowCallback;
        }

        public static void HierarchyWindowCallback(int instanceID, Rect selectionRect)
        {
            var go = (GameObject)EditorUtility.InstanceIDToObject(instanceID);
            if (go == null || go.GetComponent<EGG>() == null) return;
            float offX = 0;
            if (EGGLogo == null)
            {
                EGGLogo = Resources.Load<Texture2D>("Icon/EggIcon");
            }
            Graphics.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().width - selectionRect.height - 5 - offX, selectionRect.y, selectionRect.height, selectionRect.height), EGGLogo);
        }
    }
}
#endif