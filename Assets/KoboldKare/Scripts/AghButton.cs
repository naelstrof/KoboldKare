using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

public static class AghButton {
    [MenuItem("KoboldKare/HomogenizeButtons")]
    public static void HomogenizeButtons() {
        ColorBlock block = new ColorBlock();
        block.normalColor = Color.white;
        block.colorMultiplier = 1f;
        block.highlightedColor = new Color(0.49f,1f,0.9435571f, 1f);
        block.pressedColor = new Color(0.1090246f, 0.4716981f, 0.7129337f, 1f);
        block.selectedColor = new Color(0.2559185f, 0.764151f, 0.7129337f, 1f);
        block.disabledColor = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.5019608f);
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Changed button colors");
        var undoIndex = Undo.GetCurrentGroup();
        foreach(GameObject g in Selection.gameObjects) {
            foreach(Button b in g.GetComponentsInChildren<Button>(true)) {
                Undo.RecordObject(b, "Changed button color");
                b.colors = block;
                EditorUtility.SetDirty(b);
            }
        }
        Undo.CollapseUndoOperations(undoIndex);
    }
}

#endif