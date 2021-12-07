using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(PachinkoPrizeList))]
public class CustomPachinkoPrizeInspector : Editor{
    PachinkoPrizeList self;

    public GameObject savestuff,loadstuff;

    void Start(){
        var stuff = savestuff.GetComponentsInChildren<GameObject>();
        foreach (var item in stuff){
            item.SetActive(true);
        }
        stuff = loadstuff.GetComponentsInChildren<GameObject>();
        foreach (var item in stuff)
        {
            item.SetActive(false);
        }
    }

    
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();

        EditorGUILayout.HelpBox("Pachinko Prize Probabilities", MessageType.Info);
        if(self == null)
            self = (PachinkoPrizeList)target;
        else{
            var sum = self.GetPrizes().Sum(x => x.chance);
            foreach (var item in self.GetPrizes()){
                EditorGUILayout.Space(5);
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Width(200);
                EditorGUILayout.LabelField(item.prize.name);
                EditorGUILayout.LabelField((item.chance/sum).ToString("P"));
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
