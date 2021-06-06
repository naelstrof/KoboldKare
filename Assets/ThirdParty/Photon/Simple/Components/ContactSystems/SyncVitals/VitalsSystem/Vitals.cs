// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
using Photon.Utilities;
#endif

namespace Photon.Pun.Simple
{
    /// <summary>
    /// Collection of Vitals with code to propagate damage and such from higher layers to lower layers.
    /// Primary use case would be Health as the lowest level [0] and higher levels being such things as
    /// armor and shields that mitigate damage to the lowest level.
    /// </summary>
    [System.Serializable]
    public class Vitals :
        //: IVitals
        IOnVitalValueChange,
        IOnVitalParamChange
    {

        public List<IOnVitalValueChange> OnVitalValueChangeCallbacks = new List<IOnVitalValueChange>();
        public List<IOnVitalParamChange> OnVitalParamChangeCallbacks = new List<IOnVitalParamChange>();

        [HideInInspector]
        public List<VitalDefinition> vitalDefs = new List<VitalDefinition>()
        {
            new VitalDefinition (100f, 125, 125f, 1f, 1f, 1f, 1f, 1f, "Health"),
            new VitalDefinition (100f, 125, 50, .667f, 1f, 1f, 1f, 0f, "Armor"),
            new VitalDefinition (200f, 250, 100, 1f, 1f, 20f, 1f, 10f, "Shield"),
        };

        public List<VitalDefinition> VitalDefs { get { return vitalDefs; } }

        // Vitals are not created until runtime. The definitions references are passed over to the new Vital[] instances.
        [System.NonSerialized]
        private Vital[] vitalArray;
        public Vital[] VitalArray
        {
            get
            {
                if (!initialized)
                    Initialize();

                return vitalArray;
            }
        }

        private Dictionary<int, Vital> vitalLookup = new Dictionary<int, Vital>();

        private int vitalCount;
        private bool initialized;

        public void Initialize()
        {
            if (initialized)
                return;


            /// Create Vital for each VitalDefinition at runtime
            vitalCount = vitalDefs.Count;
            vitalArray = new Vital[vitalCount];

            for (int i = 0; i < vitalCount; ++i)
            {
                VitalDefinition vdef = vitalDefs[i];
                Vital vital = new Vital(vdef);
                vitalArray[i] = vital;

                vital.Initialize(TickEngineSettings.netTickInterval);
                vital.AddIOnVitalChange(this);

                // add all of the vital types to the lookup. Customs use hash, enumerated are added both as hash and by their index
                int hash = vdef.VitalName.hash;
                if (vdef.VitalName.type != VitalType.None)
                {
                    if (!vitalLookup.ContainsKey(hash))
                        vitalLookup.Add(hash, vital);
                    else
                        Debug.LogWarning("VitalNameType hash collision! Vitals cannot have more than one of each Vital Type in its list.");
                }
            }

            initialized = true;
        }

        public void ResetValues()
        {
            for (int i = 0; i < vitalCount; ++i)
            {
                vitalArray[i].ResetValues();
            }
        }

        //public void SetTickInterval(float tickInterval)
        //{
        //	for (int i = 0; i < vitalCount; ++i)
        //		vitalDefs[i].SetTickInterval(tickInterval);
        //}

        /// <summary>
        /// Do not use this in a hot path please. Cache the Vital this returns.
        /// </summary>
        public Vital GetVital(VitalNameType vitalNameType)
        {
            if (!initialized)
                Initialize();

            Vital vital;
            vitalLookup.TryGetValue(vitalNameType.hash, out vital);
            return vital;
        }

        public int GetVitalIndex(VitalNameType vitalNameType)
        {
            if (vitalDefs == null)
                return -1;

            int hash = vitalNameType.hash;

            /// TODO: There should be a fast lookup array for targetVital
            for (int i = 0, cnt = vitalDefs.Count; i < cnt; ++i)
                if (vitalDefs[i].VitalName.hash == hash)
                    return i;

            return -1;
        }

        public SerializationFlags Serialize(VitalsData vdata, VitalsData lastVData, byte[] buffer, ref int bitposition, bool keyframe)
        {
            var currdatas = vdata.datas;
            var lastdatas = lastVData.datas;

            SerializationFlags flags = SerializationFlags.None;

            for (int i = 0, cnt = vitalCount; i < cnt; ++i)
                flags |= vitalDefs[i].Serialize(currdatas[i], lastdatas[i], buffer, ref bitposition, keyframe);

            return flags;
        }

        public SerializationFlags Deserialize(VitalsData vdata, byte[] buffer, ref int bitposition, bool keyframe)
        {
            var datas = vdata.datas;

            bool isComplete = true;
            bool hasChanged = false;

            for (int i = 0; i < vitalCount; ++i)
            {
                datas[i] = vitalDefs[i].Deserialize(buffer, ref bitposition, keyframe);

                /// We are using NegativeInfinity to indicate null, rather than a nullable struct
                if (datas[i].Value == double.NegativeInfinity)
                    isComplete = false;
                else
                    hasChanged |= true;
            }

            return
                isComplete ? SerializationFlags.IsComplete | SerializationFlags.HasContent :
                hasChanged ? SerializationFlags.HasContent :
                SerializationFlags.None;
        }

        public void Apply(VitalsData vdata)
        {
            var datas = vdata.datas;
            for (int i = 0; i < vitalCount; ++i)
                vitalArray[i].Apply(datas[i]);
        }


        //public double ApplyChange(double change, VitalNameType vitalNameType, bool overloading)
        //{
        //	var vital = GetVital(vitalNameType);
        //	return (vital == null) ? change : vital.ApplyChange(change, overloading);
        //}

        ///// <summary>
        ///// Returns how many charges were consumed.
        ///// </summary>
        //public double ApplyChange(double discharge, VitalNameType vitalNameType, IVitalsContactReactor overloading = null)
        //{
        //    Debug.Log("ApplyCharges " + vitalNameType + " " + discharge);

        //    var vital = GetVital(vitalNameType);
        //    if (vital == null)
        //        return 0;

        //    var consumed = vital.ApplyChange(discharge, overloading);
        //    return consumed;
        //}

        //public double TestApplyChange(double change, VitalNameType vitalNameType, bool overloading)
        //{
        //	var vital = GetVital(vitalNameType);
        //	return (vital == null) ? change : vital.TestApplyChange(change, overloading);
        //}

        private double ApplyCharges(int vitalIdx, double discharge, bool allowOverload, bool propagate)
        {

            double remaining = discharge;
            double consumed = 0;

            /// Apply damage in reverse layer order, the root vital being last, to follow the Health/Armor/Shield pattern.
            for (int i = vitalIdx; i >= 0; --i)
            {
                consumed += vitalArray[i].ApplyCharges(remaining, allowOverload, !propagate);

                if (propagate)
                {
                    remaining -= consumed;

                    if (remaining == 0)
                        break;
                }
                else
                    return consumed;
            }

            return consumed;
        }

        public double ApplyCharges(double discharge, bool allowOverload, bool propagate)
        {
            return ApplyCharges(vitalCount - 1, discharge, allowOverload, propagate);
        }

        public double ApplyCharges(VitalNameType vitalNameType, double discharge, bool allowOverload, bool propagate)
        {
            var vtype = vitalNameType.type;

            int vitalIdx;

            // Type None indicates no specific Vital, so this value should be applied to any complete stack.
            if (vtype == VitalType.None)
                vitalIdx = vitalCount - 1;
            else
            {
                vitalIdx = GetVitalIndex(vitalNameType);

                if (vitalIdx == -1)
                    return 0;
            }

            double consumed = ApplyCharges(vitalIdx, discharge, allowOverload, propagate);

            CheckForDisrupt(consumed);

            return consumed;
        }

        public void CheckForDisrupt(double consumed)
        {
            if (consumed == 0)
                return;

            if (consumed > 0)
                DisruptDecay();

            if (consumed < 0)
                DisruptRegen();
        }

        public void DisruptRegen()
        {
            for (int i = 0, cnt = vitalCount; i < cnt; ++i)
                vitalArray[i].DisruptRegen();
        }

        public void DisruptDecay()
        {
            for (int i = 0, cnt = vitalCount; i < cnt; ++i)
                vitalArray[i].DisruptDecay();
        }

        public void OnVitalValueChange(Vital vital)
        {
            for (int i = 0, cnt = OnVitalValueChangeCallbacks.Count; i < cnt; ++i)
                OnVitalValueChangeCallbacks[i].OnVitalValueChange(vital);
        }

        public void OnVitalParamChange(Vital vital)
        {
            for (int i = 0, cnt = OnVitalParamChangeCallbacks.Count; i < cnt; ++i)
                OnVitalParamChangeCallbacks[i].OnVitalParamChange(vital);
        }

        public void Simulate()
        {
            for (int i = 0, cnt = vitalCount; i < cnt; ++i)
                vitalArray[i].Simulate();
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(Vitals))]
    [CanEditMultipleObjects]
    public class VitalsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            SerializedProperty vitalDefs = property.FindPropertyRelative("vitalDefs");
            Vitals ivitals = (PropertyDrawerUtility.GetParent(vitalDefs) as Vitals);

            int vitalsCount = vitalDefs.arraySize;

            int addIndex = -1;
            //int delIndex = -1;
            for (int i = 0; i < vitalsCount; ++i)
            {

                var element = vitalDefs.GetArrayElementAtIndex(i);
                r.height = EditorGUI.GetPropertyHeight(element);

                EditorGUI.PropertyField(new Rect(r), element, false);

                if (i > 0)
                {
                    const int XWIDTH = 18;
                    Rect xr = r;
                    xr.xMin = xr.xMax - XWIDTH - VitalDefinitionDrawer.PAD;
                    xr.yMin += VitalDefinitionDrawer.PAD;
                    xr.height = ADD_BUTTON_HGHT;
                    xr.width = XWIDTH;
                    if (GUI.Button(xr, "X"))
                    {
                        Undo.RecordObject(vitalDefs.serializedObject.targetObject, "Delete Vital");
                        ivitals.VitalDefs.Remove(ivitals.VitalDefs[i]);
                        EditorUtility.SetDirty(vitalDefs.serializedObject.targetObject);
                        AssetDatabase.Refresh();
                        break;
                    }
                }

                r.yMin += r.height + VitalDefinitionDrawer.PAD + 2;

                r.height = ADD_BUTTON_HGHT;

                const int ADDMARGIN = 80;
                Rect addrect = new Rect(r) { xMin = r.xMin + 4 + ADDMARGIN, xMax = r.xMax - ADDMARGIN };
                if (GUI.Button(addrect, "Add Vital", (GUIStyle)"MiniToolbarButton"))
                    addIndex = i + 1;

                r.yMin += r.height + VitalDefinitionDrawer.PAD;
            }

            if (addIndex != -1)
            {
                Undo.RecordObject(vitalDefs.serializedObject.targetObject, "Add Vital");
                vitalDefs.InsertArrayElementAtIndex(addIndex);
                EditorUtility.SetDirty(vitalDefs.serializedObject.targetObject);
                property.serializedObject.ApplyModifiedProperties();
                AssetDatabase.Refresh();
            }

        }

        public const float ADD_BUTTON_HGHT = 16;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            SerializedProperty vitalDefs = property.FindPropertyRelative("vitalDefs");
            Vitals ivitals = (PropertyDrawerUtility.GetParent(vitalDefs) as Vitals);
            int vitalsCount = ivitals.vitalDefs.Count;

            float h = 0;

            for (int i = 0; i < vitalsCount; ++i)
            {
                h += EditorGUI.GetPropertyHeight(vitalDefs.GetArrayElementAtIndex(i));
                h += ADD_BUTTON_HGHT + VitalDefinitionDrawer.PAD * 2 + 2;
            }

            return h;
        }
    }
#endif
}


