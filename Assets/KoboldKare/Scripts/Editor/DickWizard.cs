using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
using Naelstrof;

public class DickWizard : ScriptableWizard {
    public DickData dickDefaults;
    public enum DickCreationMode {
        AttachedToKobold,
        Dildo,
    }
    public enum DickLODGroup {
        HighQuality,
        LowQuality,
    }
    public Vector3 m_dickTipHole = Vector3.up*0.01f;
    public DickCreationMode mode;
    [Tooltip("The absolute root of the prefab, often containing the Armature and meshes.")]
    public Transform prefabRootTransform;
    [Tooltip("The base dick bone, this is where jigglebones and scaling will be applied.")]
    public Transform dickRootTransform;
    [Tooltip("The tip dick bone, this is where cum outputs, and collision spheres will be placed to detect penetrations.")]
    public Transform dickTipTransform;
    [Tooltip("Bones that get scaled (and dropped) for ball growth. They're optional.")]
    public List<Transform> balls;
    [Tooltip("Skinned mesh renderers, first is used for baking. Others are assumed to be LODs.")]
    public List<SkinnedMeshRenderer> dickMeshes;

    [HideInInspector]
    public SerializedObject obj;
    private string status = "Waiting...";
    [MenuItem("KoboldKare/Create Dick Wizard")]
    static void CreateWizard() {
        DickWizard d = ScriptableWizard.DisplayWizard<DickWizard>("Create New Dick Prefab");
        d.obj = new SerializedObject(d);
        //If you don't want to use the secondary button simply leave it out:
        //ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
    }
    void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    void OnGUI() {
        SerializedProperty first = obj.GetIterator();
        first.Next(true);
        EditorGUI.BeginChangeCheck();
        while (first.Next(false)) {
            if (first.name.StartsWith("m_")) {
                continue;
            }
            EditorGUILayout.PropertyField(first, new GUIContent(first.name));
        }
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(this, "Changed dick prop");
        }
        obj.ApplyModifiedProperties();

        Validate();

        if (GUILayout.Button("Create")) {
            status = "ERROR: Check the logs...";
            Vector3 dickForward, dickUp, dickRight;
            GetDickOrthoWorldSpace(out dickForward, out dickUp, out dickRight);


            GenericReagentContainer dickContainer = dickRootTransform.gameObject.AddComponent<GenericReagentContainer>();
            // Dildos need to be able to "drink" eggplant juice
            dickContainer.containerType = mode == DickCreationMode.AttachedToKobold ? ReagentContents.ReagentContainerType.Sealed : ReagentContents.ReagentContainerType.Mouth;

            JiggleBone jiggleBone = dickRootTransform.gameObject.AddComponent<JiggleBone>();
            // Dildos usually look odd with gravity, since their hitboxes don't align.
            jiggleBone.gravity = mode == DickCreationMode.AttachedToKobold ? dickDefaults.jiggleGravity : Vector3.zero;
            jiggleBone.elasticity = mode == DickCreationMode.AttachedToKobold ? dickDefaults.jiggleElasticityCurve : dickDefaults.dildoJiggleElasticCurve;
            jiggleBone.friction = mode == DickCreationMode.AttachedToKobold ? dickDefaults.jiggleFrictionCurve : dickDefaults.dildoJiggleFrictionCurve;
            jiggleBone.root = dickRootTransform.GetChild(0);

            // If we're a dildo, we probably command the entirety of the object-- so our rigidbody should be higher up in the hierarchy.
            Rigidbody dickBody = (mode == DickCreationMode.AttachedToKobold ? dickRootTransform : prefabRootTransform).gameObject.AddComponent<Rigidbody>();
            Dick dick = (mode == DickCreationMode.AttachedToKobold ? dickRootTransform : prefabRootTransform).gameObject.AddComponent<Dick>();
            dick.dickForwardAxis = dickRootTransform.InverseTransformDirection(dickForward);
            dick.dickUpAxis = dickRootTransform.InverseTransformDirection(dickUp);
            dick.dickRightAxis = dickRootTransform.InverseTransformDirection(dickRight);
            dick.dickTransform = dickRootTransform;

            dick.bakeMeshes = new List<SkinnedMeshRenderer>();
            dick.bakeMeshes.Add(dickMeshes[0]);
            dick.deformationTargets = new List<SkinnedMeshRenderer>(dickMeshes);
            dick.plappingSounds = new List<AudioClip>(dickDefaults.plappingSounds);
            dick.pumpingSounds = new List<AudioClip>(dickDefaults.pumpingSounds);
            dick.slidingSound = dickDefaults.slidingSound;
            dick.blendshapeIDs.Clear();
            // blendshape ids are interleaved, each bakemesh has 4 blendshape targets. 0 is "none" so they have to all be offset by 1
            // This is only used for baking... so ugh I don't want to unofuscate it.
            foreach (var renderer in dick.bakeMeshes) {
                dick.blendshapeIDs.Add(0);
                if (renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.squishBlendshapeName) == -1) {
                    dick.blendshapeIDs.Add(0);
                } else {
                    dick.blendshapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.squishBlendshapeName)+1);
                }
                if (renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.pullBlendshapeName) == -1) {
                    dick.blendshapeIDs.Add(0);
                } else {
                    dick.blendshapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.pullBlendshapeName)+1);
                }
                if (renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.cumBlendshapeName) == -1) {
                    dick.blendshapeIDs.Add(0);
                } else {
                    dick.blendshapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.cumBlendshapeName)+1);
                }
            }
            Naelstrof.DickEditor.GenerateCurves(dick, 16);

            // Only create balls if we're not a dildo
            if (mode == DickCreationMode.AttachedToKobold) {
                // Create balls
                GenericReagentContainer ballsContainer = prefabRootTransform.gameObject.AddComponent<GenericReagentContainer>();
                ballsContainer.containerType = ReagentContents.ReagentContainerType.Sealed;
                GenericInflatable ballsInflatable = prefabRootTransform.gameObject.AddComponent<GenericInflatable>();
                // If we have a BiggerBalls blendshape
                if (dickMeshes[0].sharedMesh.GetBlendShapeIndex(dickDefaults.biggerBallsBlendshapeName) != -1) {
                    ballsInflatable.targetRenderers = new List<SkinnedMeshRenderer>(dickMeshes);
                    ballsInflatable.shapeCurves.Add(new GenericInflatable.InflatableBlendshape(dickDefaults.zeroToOneLogCurve, dickDefaults.biggerBallsBlendshapeName));
                }
                int odd = 0;
                foreach (var ball in balls) {
                    ballsInflatable.transformCurves.Add(new GenericInflatable.InflatableTransform(ball, dickDefaults.ballsScaleCurve,
                                                                                                          dickDefaults.ballsRotateCurve,
                                                                                                          dickDefaults.ballsTranslateCurve,
                                                                                                          // Left ball rotates to the left, right ball rotates right
                                                                                                          (odd % 2 == 0 ? 1 : -1) * ball.InverseTransformDirection(dickForward),
                                                                                                          ball.InverseTransformDirection(dickUp) * dickDefaults.ballsTranslationOffset));
                    odd++;
                }
                ballsInflatable.bounceCurve = dickDefaults.bouncyCurve;
                ballsInflatable.container = ballsContainer;
                ballsInflatable.reagentVolumeDivisor = dickDefaults.ballsVolumeDivisor;
                //


                // Dick flaccidity inflatable, also only needed for kobold attachables, dildos should just be fully erect all the time.
                GenericInflatable dickInflatable = prefabRootTransform.gameObject.AddComponent<GenericInflatable>();
                dickInflatable.targetRenderers = new List<SkinnedMeshRenderer>(dickMeshes);
                if (dickMeshes[0].sharedMesh.GetBlendShapeIndex(dickDefaults.partialFlaccidName) != -1) {
                    dickInflatable.shapeCurves.Add(new GenericInflatable.InflatableBlendshape(dickDefaults.partialFlaccidCurve, dickDefaults.partialFlaccidName));
                }
                if (dickMeshes[0].sharedMesh.GetBlendShapeIndex(dickDefaults.fullFlaccidName) != -1) {
                    dickInflatable.shapeCurves.Add(new GenericInflatable.InflatableBlendshape(dickDefaults.fullFlaccidCurve, dickDefaults.fullFlaccidName));
                }

                // We only activate the jigglebone when we're half-erect. Allows for some really cool telescoping action!
                GenericInflatable.InflationChangeEvent jiggleBoneEvent = new GenericInflatable.InflationChangeEvent();
                Arg argument = new Arg();
                argument.argType = Arg.ArgType.Float;
                jiggleBoneEvent.SetMethod(jiggleBone, "set_active", true, new Arg[] { argument });
                dickInflatable.eventListeners.Add(new GenericInflatable.InflatableChangeEventCurve(dickDefaults.partialFlaccidCurve, jiggleBoneEvent));

                // Dick needs to know how erect it is, we pass it a full analogue value from the inflatable.
                GenericInflatable.InflationChangeEvent dickArousalEvent = new GenericInflatable.InflationChangeEvent();
                dickArousalEvent.SetMethod(dick, "set_arousal", true, new Arg[] { argument });
                dickInflatable.eventListeners.Add(new GenericInflatable.InflatableChangeEventCurve(dickDefaults.arousalCurve, dickArousalEvent));

                dickInflatable.container = dickContainer;
                // Only blood gives boners
                dickInflatable.reagentMasks = new List<ReagentData.ID>();
                dickInflatable.reagentMasks.Add(ReagentData.ID.Blood);
                dickInflatable.tweenDuration = 1.5f;
                dickInflatable.bounceCurve = dickDefaults.arousalBounceCurve;

                dick.balls = ballsContainer;

                CharacterJoint joint = dickRootTransform.gameObject.AddComponent<CharacterJoint>();
                joint.anchor = Vector3.zero;
                joint.axis = dickRight;
                joint.swingAxis = -dickForward;
                var lowlimit = joint.lowTwistLimit;
                lowlimit.limit = -75f;
                lowlimit.bounciness = 0.1f;
                joint.lowTwistLimit = lowlimit;
                var highlimit = joint.highTwistLimit;
                highlimit.limit = 75f;
                highlimit.bounciness = 0.1f;
                joint.highTwistLimit = highlimit;
                var swingSpring = joint.swingLimitSpring;
                swingSpring.damper = 0.5f;
                joint.swingLimitSpring = swingSpring;
                var swing1Limit = joint.swing1Limit;
                swing1Limit.limit = 45f;
                swing1Limit.bounciness = 0.1f;
                joint.swing1Limit = swing1Limit;
                var swing2Limit = joint.swing2Limit;
                swing2Limit.limit = 10f;
                swing2Limit.bounciness = 0.1f;
                joint.swing2Limit = swing2Limit;
                joint.enableProjection = true;
                joint.projectionDistance = 0.1f;
                joint.projectionAngle = 5f;
                joint.massScale = 10f;

                DickInfo info = prefabRootTransform.gameObject.AddComponent<DickInfo>();
                DickInfo.DickSet set = new DickInfo.DickSet();
                set.dickRoot = prefabRootTransform;
                set.dick = dick;
                set.container = dickContainer;
                set.joint = joint;
                set.balls = ballsInflatable;
                set.attachPoint = Equipment.AttachPoint.Crotch;
                set.attachPosition = prefabRootTransform.InverseTransformPoint(dickRootTransform.position);
                set.parent = HumanBodyBones.Hips;
                info.dicks.Add(set);
            }

            GameObject hitboxObject = new GameObject("ScalableHitbox");
            hitboxObject.layer = LayerMask.NameToLayer(dickDefaults.penetratorLayerName);
            hitboxObject.transform.parent = dickRootTransform;
            hitboxObject.transform.localPosition = Vector3.zero;
            dickRootTransform.gameObject.layer = LayerMask.NameToLayer(dickDefaults.penetratorLayerName);
            CapsuleCollider capsule = hitboxObject.AddComponent<CapsuleCollider>();
            float extraLengthBehindRoot = dick.GetWorldLength() - Vector3.Distance(dickTipTransform.TransformPoint(m_dickTipHole), dickRootTransform.position);
            capsule.height = hitboxObject.transform.InverseTransformVector(Vector3.one*(dick.GetWorldLength()-extraLengthBehindRoot)).x;
            capsule.center = dick.dickForwardAxis * capsule.height / 2f;
            capsule.radius = dick.GetGirthWorld(dick.GetWorldLength() * 0.5f, Dick.BlendshapeType.None)*0.5f;
            if (Mathf.Abs(dick.dickForwardAxis.x) > 0.7) {
                capsule.direction = 0;
            } else if (Mathf.Abs(dick.dickForwardAxis.y) > 0.7) {
                capsule.direction = 1;
            } else if (Mathf.Abs(dick.dickForwardAxis.z) > 0.7) {
                capsule.direction = 2;
            }

            capsule.transform.localRotation = Quaternion.identity;

            SphereCollider sphere = dickTipTransform.gameObject.AddComponent<SphereCollider>();
            sphere.radius = dick.GetWorldLength() * 0.15f;
            sphere.center = m_dickTipHole;
            sphere.isTrigger = true;
            dickTipTransform.gameObject.layer = LayerMask.NameToLayer(dickDefaults.penetratorLayerName);

            dick.hitBoxCollider = capsule.transform;
            dick.selfColliders.Clear();
            dick.selfColliders.Add(capsule);
            dick.selfColliders.Add(sphere);

            GameObject streamObject = PrefabUtility.InstantiatePrefab(dickDefaults.outputStreamPrefab, dickTipTransform) as GameObject;
            streamObject.transform.localPosition = m_dickTipHole;
            streamObject.transform.localRotation = Quaternion.identity;
            dick.stream = streamObject.GetComponentInChildren<StreamRenderer>();
            streamObject.SetActive(false);
            dick.strandMaterial = dickDefaults.strandMaterial;
            dick.body = dickBody;

            foreach(var mesh in dickMeshes) {
                // Needed because scaled dicks will get culled incorrectly.
                mesh.updateWhenOffscreen = true;
                // FIXME: The DickDeformation shader's shadow pass flickers if it's not set to twosided. Who knows why! This might not be true anymore though.
                mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
            }


            GenericInflatable dickScaleInflatable = dickRootTransform.gameObject.AddComponent<GenericInflatable>();
            dickScaleInflatable.transformCurves.Add(new GenericInflatable.InflatableTransform((mode == DickCreationMode.AttachedToKobold ? dickRootTransform : prefabRootTransform), dickDefaults.dickScaleCurve, dickDefaults.dickRotateCurve, dickDefaults.dickTranslateCurve,
                                                    dickRootTransform.InverseTransformDirection(dickRight), Vector3.zero));
            // Need to make sure the dildo can GRO
            if (mode == DickCreationMode.Dildo) {
                dickScaleInflatable.reagentMasks = new List<ReagentData.ID>();
                dickScaleInflatable.reagentMasks.Add(ReagentData.ID.GrowthSerum);
                dickScaleInflatable.reagentMasks.Add(ReagentData.ID.EggplantJuice);
            }
            dickScaleInflatable.reagentVolumeDivisor = dickDefaults.dickScaleVolumeDivisor;
            dickScaleInflatable.bounceCurve = dickDefaults.bouncyCurve;
            dickScaleInflatable.container = dickContainer;

            if (dickMeshes.Count > 1) {
                LODGroup group = prefabRootTransform.gameObject.AddComponent<LODGroup>();
                List<LOD> lods = new List<LOD>();
                int current = 0;
                foreach (var dickRenderer in dickMeshes) {
                    lods.Add(new LOD(dickDefaults.lodScreenTransitionHeightCurve.Evaluate((float)(current++)/(float)(dickMeshes.Count)), new Renderer[] { dickRenderer }));
                }

                group.SetLODs(lods.ToArray());
            }

            if (mode == DickCreationMode.Dildo) {
                GenericGrabbable grabbable = prefabRootTransform.gameObject.AddComponent<GenericGrabbable>();
                grabbable.renderers = dickMeshes.ToArray();
                grabbable.bodies = new Rigidbody[] { dickBody };
                grabbable.grabbableType = GrabbableType.Dildo;
                grabbable.center = dickRootTransform;
                dick.OnPenetrate.AddListener(() => { jiggleBone.active = 0f; });
                dick.OnDepenetrate.AddListener(() => { jiggleBone.active = 1f; });

                prefabRootTransform.gameObject.AddComponent<PhotonView>();
                prefabRootTransform.gameObject.AddComponent<PhotonRigidbodyView>();
                GenericLODConsumer consumer = prefabRootTransform.gameObject.AddComponent<GenericLODConsumer>();
                consumer.resource = GenericLODConsumer.ConsumerType.PhysicsItem;
                consumer.trackedRigidbodies = new List<Rigidbody>();
                consumer.trackedRigidbodies.Add(dickBody);
                //prefabRootTransform.gameObject.AddComponent<GenericUsable>();
                //prefabRootTransform.gameObject.AddComponent<GenericEquipment>();
            }
            status = "Success! <3";
        }
        EditorGUILayout.LabelField("Status: ", status);
    }

    private void Validate() {
        if (dickMeshes != null && dickMeshes.Count >0) {
            foreach (var renderer in dickMeshes) {
                if (renderer == null) {
                    continue;
                }
                if (renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.biggerBallsBlendshapeName) == -1) {
                    EditorGUILayout.HelpBox("Dick mesh " + renderer + " is missing shape \""+dickDefaults.biggerBallsBlendshapeName+"\", its optional, though this shape is triggered to increase ball size.", MessageType.Info);
                }
                if (renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.partialFlaccidName) == -1) {
                    EditorGUILayout.HelpBox("Dick mesh " + renderer + " is missing shape \"" + dickDefaults.partialFlaccidName + "\", its optional, though this shape is triggered to show half-flaccidity", MessageType.Info);
                }
                if (renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.fullFlaccidName) == -1) {
                    EditorGUILayout.HelpBox("Dick mesh " + renderer + " is missing shape \"" + dickDefaults.fullFlaccidName + "\", its optional, though this shape is triggered to show full-flaccidity", MessageType.Info);
                }
                if (renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.squishBlendshapeName) == -1) {
                    EditorGUILayout.HelpBox("Dick mesh " + renderer + " is missing shape \"" + dickDefaults.squishBlendshapeName + "\", its optional, though this shape is triggered during compression.", MessageType.Info);
                }
                if (renderer.sharedMesh.GetBlendShapeIndex(dickDefaults.pullBlendshapeName) == -1) {
                    EditorGUILayout.HelpBox("Dick mesh " + renderer + " is missing shape \"" + dickDefaults.pullBlendshapeName + "\", its optional, though this shape is triggered during compression.", MessageType.Info);
                }
            }
        }
    }
    void OnSceneGUI(SceneView sceneView) {
        EditorGUI.BeginChangeCheck();
        if (dickTipTransform != null) {
            Vector3 newPos = dickTipTransform.InverseTransformPoint(Handles.PositionHandle(dickTipTransform.TransformPoint(m_dickTipHole), dickTipTransform.rotation));
            if (Vector3.Distance(newPos, m_dickTipHole) > 0.0001f) {
                obj.FindProperty("m_dickTipHole").vector3Value = newPos;
                //m_dickTipHole = newPos;
                obj.ApplyModifiedProperties();
            }
            Handles.Label(dickTipTransform.TransformPoint(m_dickTipHole), "DickTipHole");
        }
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(this, "Changed dick tip pos");
        }
    }
    void GetDickOrthoWorldSpace(out Vector3 dickForward, out Vector3 dickUp, out Vector3 dickRight) {
        dickForward = (dickTipTransform.position - dickRootTransform.position).normalized;
        if (Vector3.Dot(dickForward, dickRootTransform.up) > 0.9f) {
            // if the dick root is y forward, then we should use z forward instead.
            dickUp = dickRootTransform.forward;
        } else {
            // Otherwise up should work fine.
            dickUp = dickRootTransform.up;
        }
        dickRight = Vector3.Cross(dickForward, dickUp);
        Vector3.OrthoNormalize(ref dickForward, ref dickUp, ref dickRight);
    }
}
#endif
