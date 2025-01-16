using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Vilar.IK;
using UnityEngine.Events;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
#endif

namespace Vilar.AnimationStation {
	
#if UNITY_EDITOR
	[CustomEditor(typeof(AnimationStation))]
	public class AnimationStationInspector : Editor {

		protected List<AnimationStation> linkedStations = new List<AnimationStation>();
		private ReorderableList listGUI;
		private void DrawElement(Rect rect, int index, bool active, bool focused)
		{
			rect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none)-2;
			rect.x += 10;
			rect.width -= 20;
			rect.y += 1;

			EditorGUI.BeginChangeCheck();
			listGUI.list[index] = EditorGUI.ObjectField(rect, GUIContent.none, (AnimationStation)listGUI.list[index], typeof(AnimationStation), true);
			if (EditorGUI.EndChangeCheck()) {
				OnListChange(listGUI, true);
				EditorUtility.SetDirty(target);
			}
		}
		private void AddItem(ReorderableList list) {
			list.list.Add((AnimationStation)null);
			EditorUtility.SetDirty(target);
		}
		private void OnListChange(ReorderableList list, bool union) {
			var script = (AnimationStation) target;
			HashSet<AnimationStation> after = new HashSet<AnimationStation>((List<AnimationStation>)list.list);
			// Don't propagate null changes, user is probably about to drag in an item, if they don't it won't save anyway.
			if (after.Contains(null)) {
				return;
            }
			// if we're not removing one, then we happily absorb other connections.
			if (union) {
				foreach (AnimationStation s in list.list) {
					after.UnionWith(s.linkedStations.hashSet);
				}
			}
			after.RemoveWhere(o => o == null);
			HashSet<AnimationStation> before = new HashSet<AnimationStation>(script.linkedStations.hashSet);
			before.RemoveWhere(o => o == null);
			before.Add(script);
			after.Add(script);
			HashSet<AnimationStation> changed = AnimationStation.UpdateLinks(before, after);
			list.list = new List<AnimationStation>(script.linkedStations.hashSet);
			foreach(AnimationStation s in changed) {
                EditorUtility.SetDirty(s);
            }
			EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
		}


		private void CreateList()
		{
			bool dragable = false, header = true, add = true, remove = true;
			listGUI = new ReorderableList(linkedStations, typeof(AnimationStation), dragable, header, add, remove);
			//listGUI.drawHeaderCallback += rect => this._property.isExpanded = EditorGUI.ToggleLeft(rect, this._property.displayName, this._property.isExpanded, EditorStyles.boldLabel);
			listGUI.onCanRemoveCallback += (list) => { return linkedStations.Count > 0; };
			listGUI.drawElementCallback += this.DrawElement;
			listGUI.drawHeaderCallback += (rect) => { EditorGUI.LabelField(rect, "Linked Stations"); };
			listGUI.elementHeightCallback += (idx) => { return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none); };
			//listGUI.onChangedCallback += OnListChange;
			listGUI.onAddCallback += AddItem;
			listGUI.onRemoveCallback += RemoveItem;
		}

		private void RemoveItem(ReorderableList list) {
			list.list.RemoveAt(list.index);
            OnListChange(listGUI, false);
			EditorUtility.SetDirty(target);
		}

		void OnEnable() {
			SceneView.duringSceneGui -= this.OnSceneGUI;
			SceneView.duringSceneGui += this.OnSceneGUI;
			var script = (AnimationStation) target;
			script.linkedStations.hashSet.Add(script);
			linkedStations = new List<AnimationStation>(script.linkedStations.hashSet);
			CreateList();
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
		}

		void OnDisable() {
			SceneView.duringSceneGui -= this.OnSceneGUI;
			var script = (AnimationStation) target;
			if (script!=null) {
				script.DestroyPreview();
				foreach (AnimationStation linkedStation in script.linkedStations.hashSet) {
					if (linkedStation != null) {
						linkedStation?.DestroyPreview();
					}
				}
			}
		}

		void OnDestroy() {
			SceneView.duringSceneGui -= this.OnSceneGUI;
		}


		public override void OnInspectorGUI() {
			serializedObject.Update();
			var script = (AnimationStation) target;
			if (script.isPreviewAssigned) {
				var styleButton = new GUIStyle(GUI.skin.button);
				styleButton.fontStyle = FontStyle.Bold;
				styleButton.hover.textColor = Color.grey;
				styleButton.normal.textColor = Color.grey;
				styleButton.fixedWidth = 20;
				var styleButtonSelected = new GUIStyle(GUI.skin.button);
				styleButtonSelected.fontStyle = FontStyle.Bold;
				styleButtonSelected.fixedWidth = 20;
				styleButtonSelected.hover.textColor = Color.black;
				styleButtonSelected.normal.textColor = Color.black;

				GUILayout.BeginHorizontal();
				if (script.loops != null) {
					for (int i = 0; i < script.loops.Count; i++) {
						if (GUILayout.Button("" + i, i == script.selectedLoop?styleButtonSelected : styleButton)) {
							script.selectedLoop = i;
							script.progress = script.selectedLoop;
						}
					}
					if (script.loops.Count > 1) {
						if (GUILayout.Button("-", GUILayout.Width(20))) {
							script.RemoveLoop();
						}
					}
				}
				if (GUILayout.Button("+", GUILayout.Width(20))) {
					script.AddLoop();
				}
				GUILayout.EndHorizontal();
				Rect r = GUILayoutUtility.GetLastRect();
				r.y += 20;
				GUILayout.Label("");
				float newprogress = GUI.HorizontalSlider(r, script.progress, 0f, (float) (script.loops.Count - 1));
				if (script.progress != newprogress) {
					script.progress = newprogress;
					script.selectedLoop = Mathf.RoundToInt(newprogress);
				}

				SerializedProperty loops = serializedObject.FindProperty("loops");
				SerializedProperty speedProp = loops.GetArrayElementAtIndex(script.selectedLoop).FindPropertyRelative("speed");
				speedProp.floatValue = EditorGUILayout.FloatField("Speed", speedProp.floatValue);
				for (int i = 0; i < 10; i++) {
					GUILayout.BeginHorizontal();
					GUILayout.Label(AnimationLooper.GetTypeName(i + 1), GUILayout.Width(86));
					SerializedProperty motion = loops.GetArrayElementAtIndex(script.selectedLoop).FindPropertyRelative("motion");
					SerializedProperty attachments = loops.GetArrayElementAtIndex(script.selectedLoop).FindPropertyRelative("attachments");
					SerializedProperty attachmentTargets = loops.GetArrayElementAtIndex(script.selectedLoop).FindPropertyRelative("attachmentTargets");
					motion.GetArrayElementAtIndex(i).objectReferenceValue = (AnimationMotion) EditorGUILayout.ObjectField(GUIContent.none, motion.GetArrayElementAtIndex(i).objectReferenceValue, typeof(AnimationMotion), true);
					GUILayout.Label("Attach", GUILayout.Width(40));
					attachments.GetArrayElementAtIndex(i).intValue = (int)(AnimationLooper.TargetType) EditorGUILayout.EnumPopup((AnimationLooper.TargetType)attachments.GetArrayElementAtIndex(i).intValue, GUILayout.Width(100));
					attachmentTargets.GetArrayElementAtIndex(i).objectReferenceValue = (AnimationStation)EditorGUILayout.ObjectField(GUIContent.none, attachmentTargets.GetArrayElementAtIndex(i).objectReferenceValue, typeof(AnimationStation), true, GUILayout.Width(100));

					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Scale", GUILayout.Width(200));
					r = GUILayoutUtility.GetLastRect();
					r.x += 40f;
					r.width -= 40f;
					SerializedProperty motionScale = loops.GetArrayElementAtIndex(script.selectedLoop).FindPropertyRelative("motionScale");
					motionScale.GetArrayElementAtIndex(i).floatValue = (float) GUI.HorizontalSlider(r, motionScale.GetArrayElementAtIndex(i).floatValue, 0.1f, 2f);
					GUILayout.Label("Offset", GUILayout.Width(200));
					r = GUILayoutUtility.GetLastRect();
					r.x += 40f;
					r.width -= 40f;
					SerializedProperty motionOffset = loops.GetArrayElementAtIndex(script.selectedLoop).FindPropertyRelative("motionOffset");
					motionOffset.GetArrayElementAtIndex(i).floatValue = (float) GUI.HorizontalSlider(r, motionOffset.GetArrayElementAtIndex(i).floatValue, 0f, 1f);

					GUILayout.EndHorizontal();
				}
			}

			if (serializedObject.hasModifiedProperties) {
				EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
			}

			listGUI.DoLayoutList();

			DrawDefaultInspector();
		}

		void OnSceneGUI(SceneView sceneView) {
			var script = (AnimationStation) target;
			if (script!=null && script.isPreviewAssigned) {
				Vector3 updatePosition = Vector3.zero;
				Quaternion updateRotation = Quaternion.identity;
				int updateindex = 0;
				EditorGUI.BeginChangeCheck();
				for (int i = 0; i < 10; i++) {
					Vector3 globalPosition = script.transform.TransformPoint(script.loops[script.selectedLoop].targetPositions[i]);
					Quaternion globalRotation = script.transform.rotation * script.loops[script.selectedLoop].targetRotations[i];

					if (i >= 6) {
						Handles.color = Color.white;
					} else {
						if (i <= 1) {
							Handles.color = Color.green;
						} else {
							Handles.color = Color.red;
						}
                    }
					if (Handles.Button(globalPosition, Quaternion.LookRotation(-SceneView.currentDrawingSceneView.camera.transform.forward, SceneView.currentDrawingSceneView.camera.transform.up), 0.02f, 0.02f, Handles.RectangleHandleCap))
						script.editSelection = i;

					if (script.editSelection == i) {
						globalPosition = Handles.PositionHandle(globalPosition, script.transform.rotation);
						updatePosition = script.transform.InverseTransformPoint(globalPosition);
						globalRotation = Handles.RotationHandle(globalRotation, globalPosition);
						updateRotation = Quaternion.Inverse(script.transform.rotation) * globalRotation;
						updateindex = i;
						Handles.color = Color.blue;
						Handles.DrawDottedLine(globalPosition, globalPosition + globalRotation * Vector3.forward, 15f);
						Handles.color = Color.green;
						Handles.DrawDottedLine(globalPosition, globalPosition + globalRotation * Vector3.up, 15f);
						//Handles.ArrowHandleCap(0, globalPosition, globalRotation, 0.2f, EventType.Repaint);
					}
					Handles.color = Color.magenta;
					Handles.DrawLine(script.loops[script.selectedLoop].computedTargetPositions[i] + script.loops[script.selectedLoop].computedTargetRotations[i] * Vector3.forward * -0.02f,
						script.loops[script.selectedLoop].computedTargetPositions[i] + script.loops[script.selectedLoop].computedTargetRotations[i] * Vector3.forward * 0.02f);
					Handles.DrawLine(script.loops[script.selectedLoop].computedTargetPositions[i] + script.loops[script.selectedLoop].computedTargetRotations[i] * Vector3.forward * -0.02f,
						script.loops[script.selectedLoop].computedTargetPositions[i] + script.loops[script.selectedLoop].computedTargetRotations[i] * Vector3.up * 0.02f);
					Handles.DrawLine(script.loops[script.selectedLoop].computedTargetPositions[i] + script.loops[script.selectedLoop].computedTargetRotations[i] * Vector3.forward * 0.02f,
						script.loops[script.selectedLoop].computedTargetPositions[i] + script.loops[script.selectedLoop].computedTargetRotations[i] * Vector3.up * 0.02f);
				}

				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(target, "Moved Animation Station Widget");
					script.loops[script.selectedLoop].targetPositions[updateindex] = updatePosition;
					script.loops[script.selectedLoop].targetRotations[updateindex] = updateRotation;
				}
				if (!Application.isPlaying) UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				SceneView.RepaintAll();
			}
		}

	}
#endif


	[System.Serializable]
	public class AnimationStationInfo {
		public bool needsPenetrator;
		public Kobold user;
    }

	[System.Serializable]
	public class AnimationStationHashSet : SerializableHashSet<AnimationStation> { }

	[SelectionBase]
	[ExecuteAlways]
	public class AnimationStation : MonoBehaviourPun {

		public static string GetTypeName(int index) {
			return System.Enum.GetNames(typeof(AnimationLooper.TargetType)) [index];
		}

		//public enum AnimationFlags {
			//None = 0,
        //}

		[HideInInspector] public float progress;
		[HideInInspector] public int selectedLoop = 0;
		[HideInInspector] public List<AnimationLooper> loops;
		[HideInInspector] public int editSelection = 0;
		[HideInInspector] public float animProgress = 0f;
		private Vector3 lastScale;
		private float modifiedProgress;
		private Vector3 lookAtPosition;
		private float lookAtWeight;

		public GameObject previewCharacter;
		private GameObject previewCharacterInstance;
		private IKSolver previewCharacterIKSolver;
		//private AnimationStationHashSet internalLinkedStations;
        [HideInInspector] public AnimationStationHashSet linkedStations = new AnimationStationHashSet();
		public bool isPreviewAssigned => previewCharacter != null;
		private Dictionary<HumanBodyBones, Quaternion> restPoseCache;
		//private bool initialized=false;
		public AnimationStationInfo info;
		private Vector2 hipOffset;
		public void SetHipOffset(Vector2 hipOffset) {
			this.hipOffset = hipOffset;
		}

		private void Update() {
#if UNITY_EDITOR
			if (Application.isPlaying) {
				if (info.user != null) {
					Advance(Time.deltaTime);
				}
			} else {
				if (Selection.activeGameObject == gameObject) {
					if (lastScale != transform.localScale) {
						lastScale = transform.localScale;
                        DestroyPreview();
                    }
					//UpdatePreview(Time.deltaTime);
					foreach (AnimationStation linkedStation in linkedStations.hashSet) {
						if (linkedStation != null) {
							linkedStation.progress = progress;
							linkedStation.animProgress = animProgress;
							linkedStation.UpdatePreview(Time.deltaTime);
						}
					}
					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				}
			}
#else
			if (info.user != null) {
                Advance(Time.deltaTime);
			}
#endif
		}

		public void SetProgress(float newProgress) {
			foreach (var linkedStation in linkedStations.hashSet) {
				linkedStation.progress = newProgress;
			}
		}

		public void UpdatePreview(float dT) {
			TryInstantiatePreview();
			Advance(dT);
			SetPreview();
		}

		void Awake() {
			info = new AnimationStationInfo();
		}

		public void OnStartAnimation(Kobold user) {
			info.user = user;
			foreach (var linkedStation in linkedStations.hashSet) {
				linkedStation.animProgress = 0f;
			}
		}

		private void TryInstantiatePreview() {
			if (previewCharacterInstance == null && previewCharacter!=null && transform!=null) {
				previewCharacterInstance = Instantiate(previewCharacter, transform);
				previewCharacterInstance.hideFlags = HideFlags.HideAndDontSave;
				previewCharacterInstance.GetComponentInChildren<Animator>().enabled=false;
				restPoseCache = new Dictionary<HumanBodyBones, Quaternion>();
				foreach(HumanBodyBones humanBodyBone in Enum.GetValues(typeof(HumanBodyBones)))	{
					if (humanBodyBone!=HumanBodyBones.LastBone) {
						if (previewCharacterInstance.GetComponentInChildren<Animator>().GetBoneTransform(humanBodyBone)!=null) {
							restPoseCache.Add(humanBodyBone, previewCharacterInstance.GetComponentInChildren<Animator>().GetBoneTransform(humanBodyBone).localRotation);
						}
					}
				}
				previewCharacterIKSolver = previewCharacterInstance.GetComponentInChildren<IKSolver>();
				previewCharacterIKSolver.Initialize();
			}
		}

		private void OnDisable() {
			DestroyPreview();
		}

		public void Advance(float time) {
			if (loops!=null && loops.Count>0) {
				if (animProgress<0f) animProgress=0f;
				//modifiedProgress = Mathf.Clamp(progress + Mathf.Cos(Time.timeSinceLevelLoad * 0.38f) * 0.4f + Mathf.Cos(Time.timeSinceLevelLoad * 1.13f) * 0.2f, 0f, loops.Count - 1);
				modifiedProgress = Mathf.Clamp(progress, 0, loops.Count - 1);
				float blendedSpeed = loops[Mathf.FloorToInt(modifiedProgress)].speed;
				if (modifiedProgress < loops.Count - 1) blendedSpeed = Mathf.Lerp(blendedSpeed, loops[Mathf.CeilToInt(modifiedProgress)].speed, Mathf.Repeat(modifiedProgress , 1f));
				animProgress = Mathf.Repeat((animProgress + time * blendedSpeed) , 100f);
				for (int i = 0; i < 10; i++) {
					SetTargetPosition(i);
				}
			}
		}

		private void SetTargetPosition(int index) {
			for (int i = 0; i < ((modifiedProgress < loops.Count - 1) ? 2 : 1); i++) {
				AnimationLooper currentLoop = loops[Mathf.FloorToInt(modifiedProgress) + i];
				Vector3 targetPosition = transform.TransformPoint(currentLoop.targetPositions[index]);
				Quaternion targetRotation = transform.rotation * currentLoop.targetRotations[index];
				if (currentLoop.motion[index] != null) {
					targetPosition += targetRotation * Vector3.right * (currentLoop.motion[index].X.Evaluate(Mathf.Repeat(((animProgress + currentLoop.motion[index].offset.x * 0.01f + currentLoop.motionOffset[index]) *
						currentLoop.motion[index].timescale.x), 1f)) * (currentLoop.motion[index].scale.x * 0.01f * currentLoop.motionScale[index]));
					targetPosition += targetRotation * Vector3.up * (currentLoop.motion[index].Y.Evaluate(Mathf.Repeat(((animProgress + currentLoop.motion[index].offset.y * 0.01f + currentLoop.motionOffset[index]) *
						currentLoop.motion[index].timescale.y) , 1f)) * (currentLoop.motion[index].scale.y * 0.01f * currentLoop.motionScale[index]));
					targetPosition += targetRotation * Vector3.forward * (currentLoop.motion[index].Z.Evaluate(Mathf.Repeat(((animProgress + currentLoop.motion[index].offset.z * 0.01f + currentLoop.motionOffset[index]) *
						currentLoop.motion[index].timescale.z), 1f)) * (currentLoop.motion[index].scale.z * 0.01f * currentLoop.motionScale[index]));
					targetRotation = targetRotation * Quaternion.Euler(
						currentLoop.motion[index].RX.Evaluate(Mathf.Repeat(((animProgress + currentLoop.motion[index].offsetR.x * 0.01f + currentLoop.motionOffset[index]) *
							currentLoop.motion[index].timescaleR.x), 1f)) * (currentLoop.motion[index].scaleR.x * currentLoop.motionScale[index]),
						0f,
						0f);
					targetRotation = targetRotation * Quaternion.Euler(
						0f,
						currentLoop.motion[index].RY.Evaluate(((animProgress + currentLoop.motion[index].offsetR.y * 0.01f + currentLoop.motionOffset[index]) *
							currentLoop.motion[index].timescaleR.y) % 1f) * (currentLoop.motion[index].scaleR.y * currentLoop.motionScale[index]),
						0f);
					targetRotation = targetRotation * Quaternion.Euler(
						0f,
						0f,
						currentLoop.motion[index].RZ.Evaluate(((animProgress + currentLoop.motion[index].offsetR.z * 0.01f + currentLoop.motionOffset[index]) *
							currentLoop.motion[index].timescaleR.z) % 1f) * (currentLoop.motion[index].scaleR.z * currentLoop.motionScale[index]));
				}
				if (currentLoop.attachments[index] != AnimationLooper.TargetType.NONE ) {
                    AnimationLooper attachLoop = currentLoop;
					Quaternion attachRotation = transform.rotation;

					if (currentLoop.attachmentTargets[index] != null) {
						attachLoop = currentLoop.attachmentTargets[index].loops[Mathf.FloorToInt(modifiedProgress) + i];
						attachRotation = currentLoop.attachmentTargets[index].transform.rotation;
                    }
					int attachmentIndex = (int)currentLoop.attachments[index] - 1;
					if (attachLoop.motion[attachmentIndex] != null) {
						if (attachLoop != null) {
							targetPosition += attachRotation * attachLoop.targetRotations[attachmentIndex] * Vector3.right * (attachLoop.motion[attachmentIndex].X.Evaluate(Mathf.Repeat(((animProgress +
									attachLoop.motion[attachmentIndex].offset.x * 0.01f +
									attachLoop.motionOffset[attachmentIndex]) *
								attachLoop.motion[attachmentIndex].timescale.x) , 1f)) * attachLoop.motion[attachmentIndex].scale.x * 0.01f * attachLoop.motionScale[attachmentIndex]);
							targetPosition += attachRotation * attachLoop.targetRotations[attachmentIndex] * Vector3.up * (attachLoop.motion[attachmentIndex].Y.Evaluate(Mathf.Repeat(((animProgress +
									attachLoop.motion[attachmentIndex].offset.y * 0.01f +
									attachLoop.motionOffset[attachmentIndex]) *
								attachLoop.motion[attachmentIndex].timescale.y), 1f)) * attachLoop.motion[attachmentIndex].scale.y * 0.01f * attachLoop.motionScale[attachmentIndex]);
							targetPosition += attachRotation * attachLoop.targetRotations[attachmentIndex] * Vector3.forward * (attachLoop.motion[attachmentIndex].Z.Evaluate(Mathf.Repeat(((animProgress +
									attachLoop.motion[attachmentIndex].offset.z * 0.01f +
									attachLoop.motionOffset[attachmentIndex]) *
								attachLoop.motion[attachmentIndex].timescale.z), 1f)) * attachLoop.motion[attachmentIndex].scale.z * 0.01f * attachLoop.motionScale[attachmentIndex]);
						}
					}
				}

				if (index == (int)IKTargetSet.parts.HIPS) {
					targetPosition += targetRotation*(new Vector3(hipOffset.x, 0, hipOffset.y)*0.25f);
				}

				currentLoop.computedTargetPositions[index] = targetPosition;
                currentLoop.computedTargetRotations[index] = targetRotation;
                //currentLoop.computedTargetVelocities[index] = targetVelocity;
			}
		}

		[ContextMenu("Zero All Data")]
		public void ZeroData() {
			previewCharacterIKSolver.Initialize();
			previewCharacterInstance.transform.position = Vector3.zero;
			previewCharacterInstance.transform.rotation = Quaternion.identity;
			previewCharacterInstance.transform.position = transform.position;
			previewCharacterInstance.transform.rotation = transform.rotation;
			for (int i = 0; i < 10; i++) {
				loops[selectedLoop].targetPositions[i] = previewCharacterIKSolver.targets.GetLocalPosition(i);
				loops[selectedLoop].targetRotations[i] = previewCharacterIKSolver.targets.GetLocalRotation(i);
			}
			for (int i = 0; i < 10; i++) {
				loops[0].targetRotations[i] = Quaternion.identity;
			}
		}

		public void AddLoop() {
			if (loops == null) loops = new List<AnimationLooper>();
			AnimationLooper loop;
			loop = loops.Count!=0 ? new AnimationLooper(loops[selectedLoop]) : new AnimationLooper();
			loops.Add(loop);
			selectedLoop = loops.Count - 1;
			progress = selectedLoop;
			if (loops.Count==1) {
				ZeroData();
			}
		}

		public void RemoveLoop() {
			loops.RemoveAt(loops.Count - 1);
			if (selectedLoop > loops.Count - 1) selectedLoop = loops.Count - 1;
			progress = selectedLoop;
		}

		[ContextMenu("Add Joint Data")]
		public void AddJointData() {
			for (int loopIndex = 0; loopIndex < loops.Count; loopIndex++) {
				for (int i = 6; i < 10; i++) {
					loops[loopIndex].targetPositions[i] = previewCharacterIKSolver.targets.GetLocalPosition(i);
					loops[loopIndex].targetRotations[i] = Quaternion.Inverse(transform.rotation) * previewCharacterIKSolver.targets.GetLocalRotation(i);
				}
				for (int i = 6; i < 10; i++) {
					loops[loopIndex].targetRotations[i] = Quaternion.identity;
				}
			}
		}

		public Vector3 ComputeTargetPosition(int index) {
			Vector3 blendedPosition = loops[Mathf.FloorToInt(modifiedProgress)].computedTargetPositions[index];
			if (modifiedProgress < loops.Count - 1) {
                blendedPosition = Vector3.Lerp(
                    blendedPosition,
                    loops[Mathf.CeilToInt(modifiedProgress)].computedTargetPositions[index],
                    Mathf.Repeat(modifiedProgress , 1f)
                );
			}
			return blendedPosition;
		}

		public void SetLookAtPosition(Vector3 worldPoint) {
			lookAtPosition = worldPoint;
		}

		public void SetLookAtWeight(float weight) {
			lookAtWeight = weight;
		}

		private Quaternion LookAtRotation(Quaternion currentHeadRotation) {
			Vector3 blendedHeadPos = Vector3.Lerp(loops[Mathf.FloorToInt(modifiedProgress)].computedTargetPositions[(int)IKTargetSet.parts.HEAD], loops[Mathf.CeilToInt(modifiedProgress)].computedTargetPositions[(int)IKTargetSet.parts.HEAD], Mathf.Repeat(modifiedProgress , 1f));
			Vector3 lookdir = lookAtPosition - blendedHeadPos;
			return Quaternion.Lerp(currentHeadRotation,Quaternion.FromToRotation(currentHeadRotation*Vector3.forward, lookdir.normalized) * currentHeadRotation, lookAtWeight);
		}

		public Quaternion ComputeTargetRotation(int index) {
			Quaternion blendedRotation = loops[Mathf.FloorToInt(modifiedProgress)].computedTargetRotations[index];

			if (modifiedProgress >= loops.Count - 1) {
				if (index == (int)IKTargetSet.parts.HEAD && Application.isPlaying) {
					return LookAtRotation(blendedRotation);
				}
				return blendedRotation;
			}
            blendedRotation = Quaternion.Lerp(
                loops[Mathf.FloorToInt(modifiedProgress)].computedTargetRotations[index],
                loops[Mathf.CeilToInt(modifiedProgress)].computedTargetRotations[index],
                Mathf.Repeat(modifiedProgress, 1f)
            );
			if (index == (int)IKTargetSet.parts.HEAD && Application.isPlaying) {
				return LookAtRotation(blendedRotation);
			}
			return blendedRotation;
		}

		public void SetPreview() {
			if (previewCharacterInstance!=null && previewCharacterIKSolver != null) {
				previewCharacterInstance.transform.position = transform.position;
				previewCharacterInstance.transform.rotation = transform.rotation;
				previewCharacterIKSolver.ForceBlend(1f);
				foreach (KeyValuePair<HumanBodyBones, Quaternion> p in restPoseCache) {
					previewCharacterInstance.GetComponentInChildren<Animator>().GetBoneTransform(p.Key).localRotation = p.Value;
				}
				for (int i = 0; i < 10; i++) {
					previewCharacterIKSolver.SetTarget(i, ComputeTargetPosition(i), ComputeTargetRotation(i));
				}
				previewCharacterIKSolver.Solve();
			}
		}

		public void DestroyPreview() {
			DestroyImmediate(previewCharacterInstance);
			previewCharacterInstance = null;
		}

		public void SetCharacter(IKSolver IK) {
			for (int i = 0; i < 10; i++) {
				IK.SetTarget(i, ComputeTargetPosition(i), ComputeTargetRotation(i));
			}
		}

		/// <summary>
		/// Alters a group of animation stations, cleaning up orphans, and updating references on them.
		/// </summary>
		/// <param name="before">The group of animation stations before the change.</param>
		/// <param name="after">The group of animations after the change.</param>
		/// <returns>Every "changed" animation station, including orphans.</returns>
		public static HashSet<AnimationStation> UpdateLinks(HashSet<AnimationStation> before, HashSet<AnimationStation> after) {
			before.ExceptWith(after);
			// Orphaned nodes should just get cleared completely...
			foreach(AnimationStation orphanStation in before) {
				orphanStation.linkedStations.hashSet.Clear();
				orphanStation.DestroyPreview();
			}
			// Everything else has a strong link with every other.
			foreach (AnimationStation station in after) {
				station.linkedStations.hashSet.Clear();
				station.linkedStations.hashSet.UnionWith(after);
				// But without referencing itself.
				//station.linkedStations.hashSet.Remove(station);
			}
			after.UnionWith(before);
			return after;
		}

		public void LinkStation(AnimationStation stationToLink) {
			HashSet<AnimationStation> before = new HashSet<AnimationStation>(linkedStations.hashSet);
			linkedStations.hashSet.Add(stationToLink);
			HashSet<AnimationStation> after = new HashSet<AnimationStation>(linkedStations.hashSet);
			after.UnionWith(stationToLink.linkedStations.hashSet);
			UpdateLinks(before, after);
		}

		private void OnValidate() {
#if UNITY_EDITOR
			linkedStations.hashSet.Add(this);
			if (loops==null || loops.Count==0) {
				AddLoop();
				ZeroData();
			}
#endif
		}
        private void OnDrawGizmos() {
			Gizmos.DrawIcon(transform.position, "ico_fug.png", true);
        }

        private void OnDestroy() {
            foreach (AnimationStation linkedStation in linkedStations.hashSet) {
                if (linkedStation != this && linkedStation != null) {
                    linkedStation.linkedStations.hashSet.Remove(this);
                }
            }
			DestroyPreview();
		}

	}

}