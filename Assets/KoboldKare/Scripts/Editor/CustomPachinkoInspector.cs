using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Pachinko))]
public class CustomPachinkoInspector : Editor{
    Pachinko self;

    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        if(self == null){
            self = (Pachinko)target;
        }
        else{
            if(self.prizeList != null){
                GUILayout.Label("Prize Dictionary");
                if(self.prizeList != null){
                    for(int i = 0; i < self.prizeList.prizes.Count; i++){
                        EditorGUILayout.BeginHorizontal();
                        if(self.prizeList.prizes == null)
                            GUILayout.Label("Null Entry!");
                        else{
                            self.prizeList.prizes[i].prize = (GameObject)EditorGUILayout.ObjectField(self.prizeList.prizes[i].prize, typeof(GameObject),false);
                            GUILayout.Width(48);
                            self.prizeList.prizes[i].chance = EditorGUILayout.FloatField(self.prizeList.prizes[i].chance);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                /*
                for(int i = 0; i < self.prizeList.Count; i++){
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.FloatField(self.prizeList[i].chance);
                    EditorGUILayout.EndHorizontal();
                }*/

                if(GUILayout.Button("Add Prize Definition")){
                    if(self.prizeList != null)
                        self.prizeList.prizes.Add(new PachinkoPrizeList.PrizeEntry());
                    else
                        self.prizeList.prizes = new List<PachinkoPrizeList.PrizeEntry>();
                }
            }
        }
    }
}
