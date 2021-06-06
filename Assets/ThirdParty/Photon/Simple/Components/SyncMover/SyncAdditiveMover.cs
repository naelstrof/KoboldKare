// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using emotitron.Compression;
using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

    /// <summary>
    /// Basic automatic transform mover for objects for network testing. Will only run if object has local authority.
    /// </summary>
    public class SyncAdditiveMover : NetComponent // SyncMoverBase<SyncAdditiveMover.TRSDefinition, SyncAdditiveMover.Frame> // SyncObject<SyncMover.Frame> // MonoBehaviour
        , ITransformController
        , IOnPreUpdate
        , IOnPreSimulate
    {

        [System.Serializable]
        public class TRSDefinition : TRSDefinitionBase
        {
            public Vector3 addVector = new Vector3(0, 0, 0);
        }

        #region Inspector

        [HideInInspector] public TRSDefinition posDef = new TRSDefinition();
        [HideInInspector] public TRSDefinition rotDef = new TRSDefinition();
        [HideInInspector] public TRSDefinition sclDef = new TRSDefinition();

        // AutoSyncTransform Requirements

#if UNITY_EDITOR

        [Tooltip("Automatically Adds/Removes SyncTransform as needed, and makes a best guess at settings. Ideally disable this once things are working and tweak the SyncTransform settings yourself.")]
        [HideInInspector] public bool autoSync = false;
        public bool AutoSync
        {
            get { return autoSync; }
        }

#endif
        #endregion

        public bool HandlesInterpolation { get { return true; } }
        public bool HandlesExtrapolation { get { return true; } }


        #region Startup/Shutdown

        #endregion

        #region Owner Loops


        public void OnPreSimulate(int frameId, int subFrameId)
        {
            if (!isActiveAndEnabled || (photonView && !photonView.IsMine))
                return;

            /// Make sure previous lerp is fully applied to scene so our transform capture is based on the fixed time and not the last update time
            AddVector();
        }

        public void OnPreUpdate()
        {

            if (!isActiveAndEnabled /*|| (photonView && !photonView.IsMine)*/)
                return;

            AddVector();
        }

        #endregion Owner Loops

        /// <summary>
        /// Movement based on continuous addition.
        /// </summary>
        private void AddVector()
        {
            /// Time delta since last Lerp call.
            float delta = DoubleTime.mixedDeltaTime;

            /// Scale
            transform.localScale += (sclDef.addVector * delta);

            /// Rot
            if (rotDef.local)
                transform.localEulerAngles += rotDef.addVector * delta;
            else
                transform.eulerAngles += rotDef.addVector * delta;
            //transform.Rotate(rotDef.addVector * mixedDelta, rotDef.local ? Space.Self : Space.World);

            /// Pos
            transform.Translate(posDef.addVector * delta, posDef.local ? Space.Self : Space.World);
        }


        #region Auto Set Transform

#if UNITY_EDITOR

        public void AutoSetSyncTransform()
        {
            if (!autoSync)
                return;

            var syncTransform = GetComponent<SyncTransform>();

            if (!syncTransform)
                syncTransform = gameObject.AddComponent<SyncTransform>();

            AutoSetSyncTransformEnablesAdditive();

        }

        public void AutoSetSyncTransformEnablesAdditive()
        {
            var syncTransform = GetComponent<SyncTransform>();

            var st = syncTransform;

            // Position
            {
                var def = posDef;
                var c = st.transformCrusher.PosCrusher;
                bool local = def.local;

                c.local = local;

                if (local)
                {
                    bool iszero = def.addVector.magnitude == 0;
                    c.XCrusher.Enabled = !iszero;
                    c.YCrusher.Enabled = !iszero;
                    c.ZCrusher.Enabled = !iszero;
                }
                else
                {
                    var addVector = def.addVector;
                    c.XCrusher.Enabled = addVector.x != 0;
                    c.YCrusher.Enabled = addVector.y != 0;
                    c.ZCrusher.Enabled = addVector.z != 0;
                }
            }

            /// Rotation
            {
                var def = rotDef;
                var c = st.transformCrusher.RotCrusher;
                bool local = def.local;

                c.local = local;
                bool iszero = def.addVector.magnitude == 0;

                if (iszero || !rotDef.local)
                {
                    c.TRSType = TRSType.Quaternion;
                    c.QCrusher.Enabled = !iszero;
                }
                else
                {
                    c.TRSType = TRSType.Euler;
                    var addVector = def.addVector;

                    c.XCrusher.Enabled = local ? addVector.x != 0 : !iszero;
                    c.YCrusher.Enabled = local ? addVector.y != 0 : !iszero;
                    c.ZCrusher.Enabled = local ? addVector.z != 0 : !iszero;
                }
            }


            /// Scale
            {
                var def = sclDef;
                var c = st.transformCrusher.SclCrusher;

                bool usescl = (sclDef.addVector.sqrMagnitude != 0);
                if (usescl)
                {
                    bool iszero = def.addVector.magnitude == 0;

                    bool local = def.local;
                    var addVector = sclDef.addVector;
                    c.uniformAxes = ElementCrusher.UniformAxes.NonUniform;
                    c.XCrusher.Enabled = local ? addVector.x != 0 : !iszero;
                    c.YCrusher.Enabled = local ? addVector.y != 0 : !iszero;
                    c.ZCrusher.Enabled = local ? addVector.z != 0 : !iszero;
                    c.local = def.local;
                }
                else
                {
                    c.uniformAxes = ElementCrusher.UniformAxes.XYZ;
                    c.UCrusher.Enabled = false;
                }
            }
        }


#endif
        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SyncAdditiveMover))]
    [CanEditMultipleObjects]
    public class SyncAdditiveMoverEditor : SyncMoverBaseEditor // HeaderEditorBase
    {

        SerializedProperty
            autoSync;

        protected class TRS_SP
        {
            public SerializedProperty
            addVector,
            relation,
            includeAxes,
            local;
        }

        TRS_SP posSPs = new TRS_SP();
        TRS_SP rotSPs = new TRS_SP();
        TRS_SP sclSPs = new TRS_SP();

        readonly GUIContent addVectorContent = new GUIContent("Add", "Applies this vector to the selected TRS type.");


        public override void OnEnable()
        {
            base.OnEnable();

            autoSync = serializedObject.FindProperty("autoSync");

            InitSP(posDef, posSPs);
            InitSP(rotDef, rotSPs);
            InitSP(sclDef, sclSPs);
        }

        protected void InitSP(SerializedProperty trs, TRS_SP trsSP)
        {
            trsSP.addVector = trs.FindPropertyRelative("addVector");
            trsSP.relation = trs.FindPropertyRelative("relation");
            trsSP.includeAxes = trs.FindPropertyRelative("includeAxes");
            trsSP.local = trs.FindPropertyRelative("local");
        }

        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();

            (target as SyncAdditiveMover).AutoSetSyncTransform();
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            /// AutoSync
            EditorGUILayout.PropertyField(autoSync);

            DrawWarningBoxes();

            DrawTRS(posSPs, TRS.Position, "Position:");
            DrawTRS(rotSPs, TRS.Rotation, "Rotation");
            DrawTRS(sclSPs, TRS.Scale, "Scale");

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

        }

        protected void DrawTRS(TRS_SP trsSP, TRS type, string label)
        {
            const float RANGE_LABEL_WIDTH = 42;

            EditorGUILayout.LabelField(System.Enum.GetName(typeof(TRS), type) + ":", (GUIStyle)"BoldLabel");

            EditorGUILayout.BeginVertical("HelpBox");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(addVectorContent, GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
            DrawAxes(trsSP.addVector, AxisMask.XYZ);
            EditorGUILayout.LabelField("/sec", GUILayout.MaxWidth(32));
            EditorGUILayout.EndHorizontal();

            /// Local
            EditorGUI.BeginDisabledGroup(type == TRS.Scale);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Local", GUILayout.MaxWidth(RANGE_LABEL_WIDTH));
            EditorGUILayout.GetControlRect(GUILayout.MaxWidth(AXIS_LAB_WID));
            EditorGUILayout.PropertyField(trsSP.local, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        protected void DrawAxes(SerializedProperty v3, AxisMask axis)
        {
            var oldval = v3.vector3Value;

            float x, y, z;

            bool usex = (axis & AxisMask.X) != 0;
            bool usey = (axis & AxisMask.Y) != 0;
            bool usez = (axis & AxisMask.Z) != 0;

            x = DrawAxis(" x", !usex, oldval.x);
            y = DrawAxis(" y", !usey, oldval.y);
            z = DrawAxis(" z", !usez, oldval.z);

            var newval = new Vector3(x, y, z);
            if (v3.vector3Value != newval)
                v3.vector3Value = newval;
        }

        protected float DrawAxis(string label, bool disabled, float oldval)
        {
            const float FLOAT_WIDTH = 10f;

            EditorGUI.BeginDisabledGroup(disabled);
            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(AXIS_LAB_WID));
            float newval = EditorGUILayout.DelayedFloatField(oldval, GUILayout.MinWidth(FLOAT_WIDTH));
            newval = (disabled) ? 0 : newval;
            EditorGUI.EndDisabledGroup();

            return newval;
        }

        protected void DrawWarningBoxes()
        {
            var _target = target as SyncAdditiveMover;

            #region Warning Boxes

            var isynctrans = _target.GetComponent<ISyncTransform>();
            _target.GetOrAddNetObj();


            if (ReferenceEquals(isynctrans, null) && _target.NetObj)
            {
                EditorGUILayout.HelpBox(
                    target.GetType().Name + " requires a " + typeof(ISyncTransform).Name +
                    " when networked.", MessageType.Warning);
            }

            if (!_target.NetObj)
            {
                EditorGUILayout.HelpBox(
                    "This GameObject does not have a " + typeof(NetObject).Name + ". Motion will be applied locally without networking. Enabling AutoSync below will set up a SyncTransform for you.", MessageType.Info);
            }

            #endregion
        }
    }

#endif
}

