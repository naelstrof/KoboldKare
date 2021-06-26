using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Penetratable))]
public class PenetratableEditor : Editor {
    public enum PenetratableTool {
        None = 0,
        Path,
        Expand,
        Pull,
        Push
    }
    public PenetratableTool selectedTool = PenetratableTool.None;
    private string[] toolTexts = new string[]{"None", "Path tool", "Expand tool", "Pull tool", "Push tool"};
    public override void OnInspectorGUI() {
        selectedTool = (PenetratableTool)GUILayout.Toolbar((int)selectedTool, toolTexts);
        GUILayout.Space(16);
        DrawDefaultInspector();
        bool showWarning = false;
        if (serializedObject.FindProperty("path").arraySize < 4) {
            showWarning = true;
        }
        for (int i=0;i<4;i++) {
            if (serializedObject.FindProperty("path").GetArrayElementAtIndex(i).FindPropertyRelative("attachedTransform").objectReferenceValue == null) {
                showWarning = true;
            }
        }
        if (showWarning) {
            EditorGUILayout.HelpBox("Orifaces need at least 4 non-null path nodes in order to continue...", MessageType.Error);
        }
    }
    public void DrawBezier() {
        Penetratable p = (Penetratable)target;
        if(p.path != null && p.path.Count>=4) {
            Vector3 p0 = p.path[0].position;
            Vector3 p1 = p.path[1].position;
            Vector3 p2 = p.path[2].position;
            Vector3 p3 = p.path[3].position;
            for (float t = 0; t <= 1f-(1f/16f); t += 1f / 16f) {
                Vector3 startPoint = Bezier.BezierPoint(p0, p1, p2, p3, t);
                Vector3 endPoint = Bezier.BezierPoint(p0, p1, p2, p3, t+(1f/16f));
                //Vector3 normal = Bezier.BezierSlope(p0, p1, p2, p3, t);
                Handles.color = Color.yellow;
                Handles.DrawLine(startPoint, endPoint);
                //Handles.color = Color.blue;
                //Handles.DrawLine(startPoint, startPoint+normal*0.1f);
            }
            if (selectedTool == PenetratableTool.Expand) {
                foreach (Penetratable.PenetratableShape shape in p.shapes) {
                    Vector3 point = Bezier.BezierPoint(p0, p1, p2, p3, shape.alongPathAmount01);
                    Vector3 normal = Bezier.BezierSlope(p0, p1, p2, p3, shape.alongPathAmount01);
                    Handles.color = Color.blue;
                    Handles.DrawWireDisc(point, normal, shape.holeDiameter * 0.5f);
                }
            }
        }
    }
    public void OnSceneGUI() {
        Penetratable p = (Penetratable)target;
        if (p.path.Count < 4) {
            return;
        }
        for (int i=0;i<4;i++) {
            if (p.path[i].attachedTransform == null) {
                return;
            }
        }
        SerializedProperty shapes = serializedObject.FindProperty("shapes");
        SerializedProperty meshes = serializedObject.FindProperty("holeMeshes");
        for (int i = 0; i < shapes.arraySize; i++) {
            for (int o = 0; o < meshes.arraySize; o++) {
                SkinnedMeshRenderer renderer = (SkinnedMeshRenderer)(meshes.GetArrayElementAtIndex(o).objectReferenceValue);
                string expandShape = (shapes.GetArrayElementAtIndex(i).FindPropertyRelative("expandBlendshapeName").stringValue);
                int expandID = renderer.sharedMesh.GetBlendShapeIndex(expandShape);
                string pushShape = (shapes.GetArrayElementAtIndex(i).FindPropertyRelative("pushBlendshapeName").stringValue);
                int pushID = renderer.sharedMesh.GetBlendShapeIndex(pushShape);
                string pullShape = (shapes.GetArrayElementAtIndex(i).FindPropertyRelative("pullBlendshapeName").stringValue);
                int pullID = renderer.sharedMesh.GetBlendShapeIndex(pullShape);
                if (expandID != -1) { renderer.SetBlendShapeWeight(expandID, selectedTool == PenetratableTool.Expand ? 100f : 0f); }
                if (pullID != -1) { renderer.SetBlendShapeWeight(pullID, selectedTool == PenetratableTool.Pull ? 100f : 0f); }
                if (pushID != -1) { renderer.SetBlendShapeWeight(pushID, selectedTool == PenetratableTool.Push ? 100f : 0f); }
            }
        }
        SerializedProperty paths = serializedObject.FindProperty("path");
        if (selectedTool == PenetratableTool.Path) {
            for (int i = 0; i < paths.arraySize && i < 4; i++) {
                Transform attachedTransform = (Transform)(paths.GetArrayElementAtIndex(i).FindPropertyRelative("attachedTransform").objectReferenceValue);
                if (attachedTransform == null) {
                    continue;
                }
                SerializedProperty localOffset = paths.GetArrayElementAtIndex(i).FindPropertyRelative("localOffset");
                Vector3 globalPosition = Handles.PositionHandle(attachedTransform.TransformPoint(localOffset.vector3Value), attachedTransform.rotation);
                if (Vector3.Distance(attachedTransform.InverseTransformPoint(globalPosition), localOffset.vector3Value) > 0.001f) {
                    //Undo.RecordObject(target, "Dick origin move");
                    localOffset.vector3Value = attachedTransform.InverseTransformPoint(globalPosition);
                    serializedObject.ApplyModifiedProperties();
                    //EditorUtility.SetDirty(target);
                }
                if (i == 0) {
                    Handles.Label(attachedTransform.TransformPoint(localOffset.vector3Value), "ORIFACE ENTRANCE");
                } else if (i < 4) {
                    Handles.Label(attachedTransform.TransformPoint(localOffset.vector3Value), "BEZIER DEPTHS " + i);
                }
            }
        }
        Vector3 p0 = p.path[0].position;
        Vector3 p1 = p.path[1].position;
        Vector3 p2 = p.path[2].position;
        Vector3 p3 = p.path[3].position;
        if (selectedTool == PenetratableTool.Expand) {
            for (int i = 0; i < shapes.arraySize; i++) {
                string expandBlendshapeName = shapes.GetArrayElementAtIndex(i).FindPropertyRelative("expandBlendshapeName").stringValue;
                if (string.IsNullOrEmpty(expandBlendshapeName)) {
                    continue;
                }
                SerializedProperty alongPath = shapes.GetArrayElementAtIndex(i).FindPropertyRelative("alongPathAmount01");
                Vector3 pushTargetPoint = Bezier.BezierPoint(p0, p1, p2, p3, Mathf.Clamp01(alongPath.floatValue));
                Vector3 pushTargetTangent = Bezier.BezierSlope(p0, p1, p2, p3, Mathf.Clamp01(alongPath.floatValue));

                Vector3 pushGlobalPosition = Handles.PositionHandle(pushTargetPoint, Quaternion.LookRotation(pushTargetTangent));
                if (Vector3.Distance(pushTargetPoint, pushGlobalPosition) > 0.001f) {
                    float dir = Vector3.Dot(pushTargetPoint, pushGlobalPosition);
                    alongPath.floatValue = Mathf.Clamp01(alongPath.floatValue + dir);
                    serializedObject.ApplyModifiedProperties();
                }
                Handles.Label(pushGlobalPosition, expandBlendshapeName);
            }
        }
        if (selectedTool == PenetratableTool.Pull || selectedTool == PenetratableTool.Push) {
            for (int i = 0; i < shapes.arraySize; i++) {
                string pushBlendshapeName = shapes.GetArrayElementAtIndex(i).FindPropertyRelative(selectedTool == PenetratableTool.Pull ? "pullBlendshapeName" : "pushBlendshapeName" ).stringValue;
                if (string.IsNullOrEmpty(pushBlendshapeName)) {
                    continue;
                }
                SerializedProperty pushLocalOffset = shapes.GetArrayElementAtIndex(i).FindPropertyRelative(selectedTool == PenetratableTool.Pull ? "pullPositionOffset":"pushPositionOffset");
                float alongPath = shapes.GetArrayElementAtIndex(i).FindPropertyRelative("alongPathAmount01").floatValue;
                Vector3 pushTargetPoint = Bezier.BezierPoint(p0, p1, p2, p3, Mathf.Clamp01( alongPath + pushLocalOffset.floatValue));
                Vector3 pushTargetTangent = Bezier.BezierSlope(p0, p1, p2, p3, Mathf.Clamp01(alongPath + pushLocalOffset.floatValue));

                Vector3 pushGlobalPosition = Handles.PositionHandle(pushTargetPoint, Quaternion.LookRotation(pushTargetTangent));
                if (Vector3.Distance(pushTargetPoint, pushGlobalPosition) > 0.001f) {
                    float t = 0f;
                    Bezier.ClosestPointOnCurve(pushGlobalPosition, p0, p1, p2, p3, out t);
                    if (selectedTool == PenetratableTool.Pull) {
                        pushLocalOffset.floatValue = Mathf.Clamp(t - alongPath, -1f, 0f);
                    } else {
                        pushLocalOffset.floatValue = Mathf.Clamp(t - alongPath, 0f, 1f);
                    }
                    serializedObject.ApplyModifiedProperties();
                }
                Handles.Label(pushGlobalPosition, selectedTool == PenetratableTool.Pull ? pushBlendshapeName + " PULL POINT" : pushBlendshapeName + " PUSH POINT");
            }
        }
        DrawBezier();
    }
    public void OnDisable() {
        Penetratable p = (Penetratable)target;
        foreach (SkinnedMeshRenderer r in p.holeMeshes) {
            if (r == null) {
                continue;
            }
            foreach (var shape in p.shapes) {
                int expandIndex = r.sharedMesh.GetBlendShapeIndex(shape.expandBlendshapeName);
                int pullIndex = r.sharedMesh.GetBlendShapeIndex(shape.pullBlendshapeName);
                int pushIndex = r.sharedMesh.GetBlendShapeIndex(shape.pushBlendshapeName);
                if (expandIndex != -1) { r.SetBlendShapeWeight(expandIndex, 0f); }
                if (pullIndex != -1) { r.SetBlendShapeWeight(pullIndex, 0f); }
                if (pushIndex != -1) { r.SetBlendShapeWeight(pushIndex, 0f); }
            }
        }
    }
}
#endif
public class Penetratable : MonoBehaviour {
    public GenericReagentContainer connectedContainer;
    [Range(0f,1f)]
    public float allowedPenetrationDepth01 = 1f;
    public bool canAllTheWayThrough = true;
    public bool canSeeDickInside = false;
    public List<SkinnedMeshRenderer> holeMeshes;
    public Rigidbody koboldBody;
    public Kobold kobold;
    public Rigidbody GetBody(float alongPath01, bool reverse) {
        if (koboldBody == null || kobold.ragdolled) {
            return path[Mathf.FloorToInt(Mathf.Clamp01(alongPath01)*3.99f)].connectedBody;
        }
        return koboldBody;
    }
    [System.Serializable]
    public class PenetratablePath {
        public Transform attachedTransform;
        public Vector3 localOffset;
        public Rigidbody connectedBody;
        public Vector3 position {
            get { return attachedTransform.TransformPoint(localOffset); }
        }
        [HideInInspector]
        public Vector3 right;
        [HideInInspector]
        public Vector3 up;
        [HideInInspector]
        public Vector3 forward;
    }
    [HideInInspector]
    public float orifaceLength {
        get { return Bezier.BezierApproxLength(path[0].position, path[1].position, path[2].position, path[3].position); }
    }
    [System.Serializable]
    public class PenetratableShape {
        public float holeDiameter;
        public string expandBlendshapeName = "";
        public string pushBlendshapeName = "";
        public string pullBlendshapeName = "";
        public float pushPositionOffset;
        public float pullPositionOffset;
        public float alongPathAmount01;
        public bool canOverdriveShapes = true;
        public UnityEvent OnExpand;
        public UnityEvent OnEndExpand;
        [HideInInspector]
        public Dictionary<Mesh, int> expandBlendshape = new Dictionary<Mesh, int>();
        [HideInInspector]
        public Dictionary<Mesh, int> pushBlendshape = new Dictionary<Mesh, int>();
        [HideInInspector]
        public Dictionary<Mesh, int> pullBlendshape = new Dictionary<Mesh, int>();
        [HideInInspector]
        public bool triggeredEvent = false;
        [HideInInspector]
        public List<float> girths = new List<float>();
    }
    public List<PenetratableShape> shapes = new List<PenetratableShape>();
    public List<PenetratablePath> path = new List<PenetratablePath>();

    private List<Dick> penetrators = new List<Dick>();
    private List<Dick> sortedPenetrators = new List<Dick>();
    public void AddPenetrator(Dick d) {
        penetrators.Add(d);
        foreach(var shape in shapes) {
            shape.girths.Add(0f);
        }
        sortedPenetrators.Add(d);
    }
    public void RemovePenetrator(Dick d) {
        penetrators.Remove(d);
        foreach(var shape in shapes) {
            shape.girths.RemoveAt(0);
        }
        sortedPenetrators.Remove(d);
    }
    public List<Dick> GetPenetrators() {
        return penetrators;
    }
    public bool ContainsPenetrator(Dick d) {
        return penetrators.Contains(d);
    }

    void Start() {
        if (holeMeshes.Count <= 0) {
            return;
        }
        // Set up the paths to have orthonormalized forwards, rights, and ups. The forward axis follows the curve, the other two are arbitrary.
        // They're used in circle packing offsets for multiple penetrations.
        for(int i=0;i<path.Count-1;i++) {
            PenetratablePath current = path[i];
            PenetratablePath next = path[i+1];
            current.forward = current.attachedTransform.InverseTransformDirection(next.position-current.position);
            Vector3 mostPerpendicular = current.attachedTransform.right;
            float minDot = Vector3.Dot(current.forward, current.attachedTransform.right);
            if (Vector3.Dot(current.forward, current.attachedTransform.up) < minDot) {
                minDot = Vector3.Dot(current.forward, current.attachedTransform.up);
                mostPerpendicular = current.attachedTransform.up;
            }
            if (Vector3.Dot(current.forward, current.attachedTransform.forward) < minDot) {
                minDot = Vector3.Dot(current.forward, current.attachedTransform.forward);
                mostPerpendicular = current.attachedTransform.forward;
            }
            current.right = mostPerpendicular;
            Vector3.OrthoNormalize(ref current.forward, ref current.right, ref current.up);
        }

        PenetratablePath currentn = path[path.Count-1];
        currentn.forward = (currentn.position - path[path.Count-2].position).normalized;
        Vector3 mostPerpendicularn = currentn.attachedTransform.right;
        float minDotn = Vector3.Dot(currentn.forward, currentn.attachedTransform.right);
        if (Vector3.Dot(currentn.forward, currentn.attachedTransform.up) < minDotn) {
            minDotn = Vector3.Dot(currentn.forward, currentn.attachedTransform.up);
            mostPerpendicularn = currentn.attachedTransform.up;
        }
        if (Vector3.Dot(currentn.forward, currentn.attachedTransform.forward) < minDotn) {
            minDotn = Vector3.Dot(currentn.forward, currentn.attachedTransform.forward);
            mostPerpendicularn = currentn.attachedTransform.forward;
        }
        currentn.right = mostPerpendicularn;
        Vector3.OrthoNormalize(ref currentn.forward, ref currentn.right, ref currentn.up);

        // Cache the blendshape IDs, so we don't have to do a lookup constantly.
        foreach(PenetratableShape shape in shapes) {
            foreach (SkinnedMeshRenderer renderer in holeMeshes) {
                shape.expandBlendshape[renderer.sharedMesh] = renderer.sharedMesh.GetBlendShapeIndex(shape.expandBlendshapeName);
                shape.pushBlendshape[renderer.sharedMesh] = renderer.sharedMesh.GetBlendShapeIndex(shape.pushBlendshapeName);
                shape.pullBlendshape[renderer.sharedMesh] = renderer.sharedMesh.GetBlendShapeIndex(shape.pullBlendshapeName);
            }
        }
    }
    public void OnValidate() {
        if (path != null && path.Count >= 4) {
            for(int i=0;i<4;i++) {
                if (path[i].attachedTransform == null) {
                    return;
                }
            }
        }
        foreach(PenetratableShape shape in shapes) {
            shape.alongPathAmount01 = Mathf.Clamp01(shape.alongPathAmount01);
            shape.holeDiameter = Mathf.Max(shape.holeDiameter, 0f);
        }
    }
    public Vector3 GetTangent(float pointAlongPath01, bool reverse) {
        // Simply gets the tangent along the path. Values outside 01 are clamped since the tangent should extend straight outward infinitely at the ends of the path.
        pointAlongPath01 = Mathf.Clamp01(pointAlongPath01);
        if (reverse) {
            return Bezier.BezierSlope(path[3].position, path[2].position, path[1].position, path[0].position, pointAlongPath01);
        } else {
            return Bezier.BezierSlope(path[0].position, path[1].position, path[2].position, path[3].position, pointAlongPath01);
        }
    }
    // Gets the 3d position of the path in world space. From 01 oriface space. Values outside 01 are either clamped or extend infinitely straight out at the ends of the path.
    public Vector3 GetPoint(float pointAlongPath01, bool reverse) {
        Vector3 position = Vector3.zero;
        if ((pointAlongPath01 >= 0f && pointAlongPath01 < 1f)) {
            position = reverse?Bezier.BezierPoint(path[3].position, path[2].position, path[1].position, path[0].position, pointAlongPath01) : Bezier.BezierPoint(path[0].position, path[1].position, path[2].position, path[3].position, pointAlongPath01);
        } else if ( pointAlongPath01 < 0f ) {
            position = (reverse?path[3].position:path[0].position) - GetTangent(0f, reverse).normalized*(-pointAlongPath01*orifaceLength);
        } else if (pointAlongPath01 > 1f) {
            if (canAllTheWayThrough) {
                position = (reverse?path[0].position:path[3].position) + GetTangent(1f, reverse).normalized*((pointAlongPath01-1f)*orifaceLength);
            } else {
                position = (reverse?path[0].position:path[3].position);
            }
        }
        return position;
    }
    void Update() {
        if (path.Count < 4) {
            return;
        }
        Vector3 p0 = path[0].position;
        Vector3 p1 = path[1].position;
        Vector3 p2 = path[2].position;
        Vector3 p3 = path[3].position;
        // Reset all girths to zero, and make sure they're the right size;
        foreach (var shape in shapes) {
            for(int i=0;i<shape.girths.Count;i++) {
                shape.girths[i] = 0f;
            }
        }
        // Get girths at entrance and exit (for circlePacking)
        foreach(var penetrator in penetrators) {
            float rootPenetrationDepth = (penetrator.penetrationDepth01-1f) * penetrator.GetLength();
            float targetEntrancePoint = penetrator.backwards ? 1f * orifaceLength : 0f * orifaceLength;
            float sampleEntrancePoint = (targetEntrancePoint - rootPenetrationDepth) / penetrator.GetLength();
            float targetExitPoint = penetrator.backwards ? 0f * orifaceLength : 1f * orifaceLength;
            float sampleExitPoint = (targetExitPoint - rootPenetrationDepth) / penetrator.GetLength();
            penetrator.girthAtEntrance = penetrator.GetWorldGirth(1f-sampleEntrancePoint);
            penetrator.girthAtExit = penetrator.GetWorldGirth(1f-sampleExitPoint);
        }

        // We assume the whole girth of the object will be just the biggest two penetrators combined.
        // This is because circle packing reduces the radius needed, and we don't care if we're slightly too small.
        float girthEntranceTotal = 0f;
        sortedPenetrators.Sort((a,b)=>(b.girthAtEntrance.CompareTo(a.girthAtEntrance)));
        for (int i=0;i<2&&i<sortedPenetrators.Count;i++) {
            girthEntranceTotal += sortedPenetrators[i].girthAtEntrance;
        }

        float girthExitTotal = 0f;
        sortedPenetrators.Sort((a,b)=>(b.girthAtExit.CompareTo(a.girthAtExit)));
        for (int i=0;i<2&&i<sortedPenetrators.Count;i++) {
            girthExitTotal += sortedPenetrators[i].girthAtExit;
        }

        // Set their paths so they try not to collide.
        // Our circle packing algorithm is really simple, we use an angle to place the circle in a unique quadrant,
        // and just use the radius to make sure it stays "inside" the total girth.
        // We don't care too much for clipping, as long as it seems like we've "filled" the hole.
        float angleMultiplier = (2f*Mathf.PI)/((float)penetrators.Count);
        int counter = 0;
        foreach(var penetrator in penetrators) {
            float x = Mathf.Sin(angleMultiplier*(float)counter);
            float y = Mathf.Cos(angleMultiplier*(float)counter);
            if (penetrator.backwards) {
                float entranceMovementAdjustment = (girthEntranceTotal - penetrator.girthAtEntrance)*0.5f;
                float exitMovementAdjustment = (girthExitTotal - penetrator.girthAtExit)*0.5f;
                Vector3 rightOffset = path[3].attachedTransform.TransformDirection(path[3].right)*x*entranceMovementAdjustment;
                Vector3 upOffset = path[3].attachedTransform.TransformDirection(path[3].up)*y*entranceMovementAdjustment;
                Vector3 outRightOffset = path[0].attachedTransform.TransformDirection(path[0].right)*x*exitMovementAdjustment;
                Vector3 outUpOffset = path[0].attachedTransform.TransformDirection(path[0].up)*y*exitMovementAdjustment;
                penetrator.SetHolePositions(p3-rightOffset-upOffset, p2, p1, p0+outRightOffset+outUpOffset);
            } else {
                float entranceMovementAdjustment = (girthEntranceTotal - penetrator.girthAtEntrance)*0.5f;
                float exitMovementAdjustment = (girthExitTotal - penetrator.girthAtExit)*0.5f;
                Vector3 rightOffset = path[0].attachedTransform.TransformDirection(path[0].right)*x*entranceMovementAdjustment;
                Vector3 upOffset = path[0].attachedTransform.TransformDirection(path[0].up)*y*entranceMovementAdjustment;
                Vector3 outRightOffset = path[3].attachedTransform.TransformDirection(path[3].right)*x*exitMovementAdjustment;
                Vector3 outUpOffset = path[3].attachedTransform.TransformDirection(path[3].up)*y*exitMovementAdjustment;
                penetrator.SetHolePositions(p0+rightOffset+upOffset, p1, p2, p3-outRightOffset-outUpOffset);
            }
            counter++;
        }
        // Add up all the girths inside
        int penetratorNum = 0;
        foreach(var penetrator in penetrators) {
            if (penetrator == null) {
                continue;
            }
            float rootPenetrationDepth = (penetrator.penetrationDepth01-1f) * penetrator.GetLength();
            foreach (var shape in shapes) {
                if (string.IsNullOrEmpty(shape.expandBlendshapeName)) {
                    continue;
                }
                //float tipPenetrationDepth = penetrator.targetDick.penetrationDepth01 * penetrator.targetDick.GetLength();
                float shapeTargetPoint = penetrator.backwards ? (1f-shape.alongPathAmount01) * orifaceLength : shape.alongPathAmount01 * orifaceLength;
                float shapeSamplePoint = (shapeTargetPoint - rootPenetrationDepth) / penetrator.GetLength();
                float shapeGirth = penetrator.GetWorldGirth(1f-shapeSamplePoint);
                shape.girths[penetratorNum] = shapeGirth;
            }
            penetratorNum++;
        }
        // Finally set the blendshape, and execute triggers.
        foreach (var shape in shapes) {
            float totalGirth = 0f;
            shape.girths.Sort((a,b)=>(b.CompareTo(a)));
            for(int i=0;i<shape.girths.Count&&i<2;i++) {
                totalGirth += shape.girths[i];
            }

            if (totalGirth > 0f && !shape.triggeredEvent) {
                shape.triggeredEvent = true;
                shape.OnExpand.Invoke();
            } else if (totalGirth <= 0f && shape.triggeredEvent) {
                shape.triggeredEvent = false;
                shape.OnEndExpand.Invoke();
            }
            if (!shape.canOverdriveShapes) {
                totalGirth = Mathf.Min(totalGirth,shape.holeDiameter);
            }
            float triggerAmount = (totalGirth / shape.holeDiameter);
            foreach(var mesh in holeMeshes) {
                mesh.SetBlendShapeWeight(shape.expandBlendshape[mesh.sharedMesh], triggerAmount*100f);
            }
        }
    }
    public void LateUpdate() {
        if (path.Count < 4) {
            return;
        }
        foreach(var penetrator in penetrators) {
            if (penetrator == null) {
                continue;
            }
            foreach (var shape in shapes) {
            }
        }
    }
    public void FixedUpdate() {
        float springStrength = 100f;
        float deflectionForgivenessDegrees = 10f;
        float deflectionSpringStrength = 10f;
        float overallDamping = 0.7f;
        foreach(var penetrator in penetrators) {
            if (penetrator == null || penetrator.body == null) {
                continue;
            }
            float dickLength = penetrator.GetLength();
            float penetrationDepth = penetrator.penetrationDepth01;

            float tipTargetPoint = penetrationDepth*dickLength/orifaceLength;
            float rootTargetPoint = (penetrationDepth-1f)*dickLength/orifaceLength;

            float weight = 1f-Mathf.Clamp01(-penetrator.penetrationDepth01);
            if (penetrator.isDildo) {
                penetrator.body.angularDrag = Mathf.Lerp(0.05f,6f,weight);
                penetrator.body.drag = Mathf.Lerp(0f,6f,weight);
                penetrator.body.useGravity = !(penetrationDepth > 1f);
                // If we're not quite "in", push ourselves to just have the tip at the entrance.
                // But only if we're a dildo, otherwise kobolds get vaccumed in
                Vector3 tipTargetPosition = GetPoint(tipTargetPoint, penetrator.backwards);
                if (tipTargetPoint<0f && penetrator.isDildo) {
                    tipTargetPosition = GetPoint(0f,penetrator.backwards);
                }
                Vector3 rootTargetPosition = GetPoint(rootTargetPoint, penetrator.backwards);
                Vector3 diff = rootTargetPosition - penetrator.GetWorldRootPosition();
                penetrator.body.position += diff;
                penetrator.body.rotation = Quaternion.FromToRotation(penetrator.dickRoot.TransformDirection(penetrator.dickForward), (tipTargetPosition-rootTargetPosition).normalized)*penetrator.body.rotation;
            } else {
                // Kill cyclical adjustments
                if (penetrator.kobold != null) {
                    foreach(var dickset in kobold.activeDicks) {
                        foreach (var holeset in penetrator.kobold.penetratables) {
                            if (dickset.dick.holeTarget == holeset.penetratable && kobold.transform.lossyScale.y > penetrator.kobold.transform.lossyScale.y) {
                                return;
                            }
                        }
                    }
                }
                Rigidbody targetBody = GetBody(tipTargetPoint, penetrator.backwards);
                //Vector3 tipTargetPosition = GetPoint(tipTargetPoint, penetrator.backwards);
                //Vector3 diff = penetrator.dickTip.position - tipTargetPosition;
                //targetBody.AddForceAtPosition(diff*springStrength*weight, tipTargetPosition, ForceMode.Acceleration);
                //targetBody.position = targetBody.position+diff;

                //rtargetBody.AddForceAtPosition(rdiff*springStrength*weight, rootTargetPosition, ForceMode.Acceleration);

                /*Vector3 tipTangent = GetTangent(tipTargetPoint, penetrator.backwards);
                Vector3 rootTangent = GetTangent(rootTargetPoint, penetrator.backwards);
                Vector3 tangent = Vector3.Lerp(tipTangent, rootTangent, 0.5f); */
                //rtargetBody.velocity += (penetrator.body.velocity-targetBody.velocity)*overallDamping;

                Rigidbody rtargetBody = GetBody(0, penetrator.backwards);
                rtargetBody.velocity += (penetrator.body.velocity-rtargetBody.velocity)*overallDamping;

                Vector3 dickForward = penetrator.dickRoot.TransformDirection(penetrator.dickForward);

                Vector3 tangent = GetTangent(0,penetrator.backwards);
                Vector3 cross = Vector3.Cross(-tangent, dickForward);
                float angleDiff = Mathf.Max(Vector3.Angle(-tangent, penetrator.dickRoot.TransformDirection(penetrator.dickForward)) - deflectionForgivenessDegrees, 0f);
                targetBody.angularVelocity -= targetBody.angularVelocity*overallDamping;
                targetBody.AddTorque(-cross * angleDiff * deflectionSpringStrength * weight, ForceMode.Acceleration);

                Vector3 rootTargetPosition = GetPoint(0, penetrator.backwards);
                Vector3 wantedPosition = penetrator.GetWorldRootPosition()+dickForward*(1f-penetrator.penetrationDepth01)*dickLength;
                Vector3 rdiff = wantedPosition - rootTargetPosition;
                rtargetBody.AddForceAtPosition(rdiff*springStrength*weight, rootTargetPosition, ForceMode.Acceleration);
            }

            //penetrator.body.AddForceAtPosition((rdir*rdist*weight*springStrength), penetrator.dickRoot.position, ForceMode.Acceleration);
            //GetBody(rootTargetPoint, penetrator.backwards).AddForceAtPosition((-rdir*rdist*weight*springStrength), rootTargetPosition, ForceMode.Acceleration);

            // This was meant to rotate a penis attached to a ragdolled kobold into the right direction-- though it's unecessary provided that
            // we move the tip into position.
            /*if (!penetrator.isDildo && (penetrator.kobold != false && penetrator.body != penetrator.kobold.body)) {
                Vector3 tipTangent = GetTangent(tipTargetPoint, penetrator.backwards);
                Vector3 rootTangent = GetTangent(rootTargetPoint, penetrator.backwards);
                Vector3 tangent = Vector3.Lerp(tipTangent, rootTangent, 0.5f);
                Vector3 cross = Vector3.Cross(-tangent, penetrator.dickRoot.TransformDirection(penetrator.dickForward));
                float angleDiff = Mathf.Max(Vector3.Angle(-tangent, penetrator.dickRoot.TransformDirection(penetrator.dickForward)) - deflectionForgivenessDegrees, 0f);
                penetrator.body.angularVelocity *= 0.9f;
                penetrator.body.AddTorque(cross * angleDiff * deflectionSpringStrength * weight, ForceMode.VelocityChange); }*/
        }
    }
}
