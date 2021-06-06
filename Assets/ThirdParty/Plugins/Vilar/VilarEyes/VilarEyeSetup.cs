#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class VilarEyeSetup : ScriptableWizard {
	public GameObject leftEye;
	public GameObject rightEye;
	int error = 0;
	bool finished = false;
	[MenuItem("VRChat SDK/Vilar's EyeTrack Setup")]
	static void CreateWizard() {
		ScriptableWizard.DisplayWizard<VilarEyeSetup>("Set Eye Spacing", "Go", "Cancel");
	}
	void OnWizardUpdate() {
		helpString = "Please specify left and right eye";
	}
	void OnWizardCreate() {
		/*if (prefab == null) {
			throw new UnityException ("Aaaaah please specify a prefab you dummy.");
		}

		Transform obj = container.transform.Find(searchString);
		while ( obj != null ) {
			GameObject pobj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			pobj.transform.position = obj.position;
			pobj.transform.localScale = obj.localScale;
			pobj.transform.rotation = obj.rotation;
			DestroyImmediate(obj.gameObject);
			obj = container.transform.Find(searchString);
		}*/

	}

	void setMaterialOffsets(Material[] mats, float offset) {
		for (int i = 0; i < mats.Length; i++) {
			mats[i].SetFloat("_EyeOffset", offset);
		}
	}

	void setMaterialMaya(Material[] mats, bool maya) {
		for (int i = 0; i < mats.Length; i++) {
			mats[i].SetFloat("_MayaModel", maya?1f:0f);
		}
	}

	void setEyeOffset() {
		Material[] leftEyeMat = leftEye.GetComponent<MeshRenderer>().sharedMaterials;
		Material[] rightEyeMat = rightEye.GetComponent<MeshRenderer>().sharedMaterials;
		float offset = Vector3.Distance(leftEye.transform.position, rightEye.transform.position);
		setMaterialOffsets(leftEyeMat, -offset/2f);
		setMaterialOffsets(rightEyeMat, offset/2f);
		bool kindofUp = Vector3.Dot(leftEye.transform.up, Vector3.up)>0.5f;
		setMaterialMaya(leftEyeMat, kindofUp);
		setMaterialMaya(rightEyeMat, kindofUp);
	}

	void OnGUI() {
		leftEye = (GameObject)EditorGUILayout.ObjectField("Left Eye", leftEye, typeof(GameObject), true);
		rightEye = (GameObject)EditorGUILayout.ObjectField("Right Eye", rightEye, typeof(GameObject), true);
		EditorGUILayout.HelpBox("Drag your eyes into the slots provided, and click the Go Button to set up your eye offsets.", MessageType.Info);
		if (leftEye && rightEye && GUILayout.Button("Go!")) {
			SkinnedMeshRenderer leftEyeError = leftEye.GetComponent<SkinnedMeshRenderer>();
			if (leftEyeError) {
			EditorGUILayout.HelpBox("Done! :)", MessageType.Info);
				error = 1;
			} else {
				setEyeOffset();
				if (error==0) finished = true;
			}
		}
		if (error > 0) {
			switch (error) {
				case 1:
					EditorGUILayout.HelpBox("Skinned Mesh Renderer detected! Remove the armature modifier from your eye objects!", MessageType.Error);
					break;
			}
		} else if (finished) {
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Fixed!", MessageType.Info);
		}
	}

}
#endif