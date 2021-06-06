// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using Photon.Pun.Simple.Assists;
using UnityEditor;
#endif

namespace Photon.Pun.Simple
       
{
    public class SyncContactScan : SyncShootBase
        , IOnSnapshot
        , IOnAuthorityChanged
    {
#if UNITY_EDITOR

        //public Enum Preset { Weapon, Grabber }

        public void UsePreset(Preset preset)
        {
            
            switch (preset)
            {

                case Preset.WeaponScan:
                    triggerKey = KeyCode.R;
                    grab = false;
                    break;

                case Preset.ContactScan:
                    {
                        triggerKey = KeyCode.Alpha4;
                        hitscanDefinition = new HitscanDefinition() { hitscanType = HitscanType.OverlapSphere, radius = 2 };
                        break;
                    }

                case Preset.VitalsScan:
                    {
                        triggerKey = KeyCode.Alpha4;
                        hitscanDefinition = new HitscanDefinition() { hitscanType = HitscanType.OverlapSphere, radius = 2 };
                        break;
                    }

                case Preset.InventorysScan:
                    {
                        triggerKey = KeyCode.Alpha4;
                        hitscanDefinition = new HitscanDefinition() { hitscanType = HitscanType.OverlapSphere, radius = 2 };
                        break;
                    }

                default:
                    {
                        triggerKey = KeyCode.Alpha4;

                        // If either the poke or grab bits are set, use both for settings. Otherwise just leave defaults.
                        bool isPoke = (preset & Preset.Poke) != 0;
                        bool isGrab = (preset & Preset.Grab) != 0;

                        if (isPoke ||isGrab)
                        {
                            poke = isPoke;
                            grab = isGrab;
                        }

                        break;
                    }
            }
        }
#endif

        public override int ApplyOrder { get { return ApplyOrderConstants.HITSCAN; } }

        public bool poke = true;
        public bool grab = true;

        public HitscanDefinition hitscanDefinition;

        [Tooltip("Render widgets that represent the shape of the hitscan when triggered.")]
        public bool visualizeHitscan = true;


        protected override bool Trigger(Frame frame, int subFrameId, float timeshift = 0)
        {
            /// TEST
            if (GetComponent<SyncContact>() && !photonView.IsMine)
            {
                hitscanDefinition.VisualizeHitscan(origin);
                return true;
            }

            int nearest = -1;
            RaycastHit[] rayhits;
            Collider[] colhits;
            int count = hitscanDefinition.GenericHitscanNonAlloc(origin, out rayhits, out colhits, ref nearest, visualizeHitscan);

            if (count <=0)
                return true;

            //var contactTrigger = this.contactTrigger;// this.GetComponentInParent<ContactTrigger>();
            for (int h = 0; h < count; ++h)
            {
                var col = colhits[h];

                var otherCT = col.transform.GetNestedComponentInParents<IContactTrigger, NetObject>();

                if (ReferenceEquals(otherCT, null))
                    continue;

                if (otherCT.NetObj == contactTrigger.NetObj)
                    continue;

                if (poke)
                    contactTrigger.OnContact(otherCT, ContactType.Hitscan);

                if (grab)
                    otherCT.OnContact(contactTrigger, ContactType.Hitscan);
            }

            return true;
        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SyncContactScan))]
    [CanEditMultipleObjects]
    public class SyncContactScanEditor : SyncShootBaseEditor
    {
        protected override string Instructions
        {
            get
            {
                return "Attach this component to any root or child GameObject to define a networked hitscan. " +
                    "A NetObject is required on this object or a parent.\n\n" +
                    "Initiate a hitscan by calling:\n" +
                    "this" + typeof(SyncContactScan).Name + ".QueueTrigger()";
            }
        }

        protected override string HelpURL
        {
            get { return Internal.SimpleDocsURLS.SUBSYS_PATH + "#synccontactscan_component"; }
        }

        protected override string TextTexturePath
        {
            get
            {
                return "Header/SyncScanText";
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if ((target as SyncContactScan).visualizeHitscan && !EditorUserBuildSettings.development)
                EditorGUILayout.HelpBox("Hitscan visualizations will not appear in release builds. 'Development Build' in 'Build Settings' is currently unchecked.", MessageType.Error);
        }


    }
#endif
}
