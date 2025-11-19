using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(BakeTexture))]
public class BakeTextureEditor : Editor {
    SerializedProperty ResultTexture;
    SerializedProperty texture;

    void OnEnable() {
        ResultTexture = serializedObject.FindProperty("ResultTexture");
        texture = serializedObject.FindProperty("texture");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(ResultTexture);
        EditorGUILayout.PropertyField(texture);
        if (GUILayout.Button("Bake")) {
            ((BakeTexture)serializedObject.targetObject).Bake();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
public class BakeTexture : MonoBehaviour {
    public RenderTexture ResultTexture;
    public Texture2D texture;
    // Use this for initialization
    public void Bake() {
		RenderTexture.active = ResultTexture; // activate rendertexture for drawtexture;
		GL.PushMatrix();                       // save matrixes
		GL.LoadPixelMatrix(0, ResultTexture.width, ResultTexture.height, 0);      // setup matrix for correct size
        Graphics.Blit(texture, ResultTexture);
		//Graphics.DrawTexture(new Rect(0, 0, rt.width, rt.height), rt);
		GL.PopMatrix();
		RenderTexture.active = null;// turn off rendertexture               

        /*Texture2D frame = new Texture2D(ResultTexture.width, ResultTexture.height);
        frame.ReadPixels(new Rect(0, 0, ResultTexture.width, ResultTexture.height), 0, 0, false);
        frame.Apply();
        byte[] bytes = frame.EncodeToPNG();
        FileStream file = File.Open(@"C:\Works.png", FileMode.Create);
        BinaryWriter binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();*/
    }
}
