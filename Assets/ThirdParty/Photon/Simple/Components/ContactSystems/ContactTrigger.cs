// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using Photon.Pun.Simple.ContactGroups;
using System.Reflection;
using System;

#if UNITY_EDITOR
using UnityEditor;
using Photon.Pun.Simple.Assists;
using Photon.Pun.Simple.Internal;
#endif

namespace Photon.Pun.Simple
{

    public class ContactTrigger : MonoBehaviour
        , IContactTrigger
        , IContactable
        , IOnPreSimulate
        , IOnStateChange

    {
        // Static Constructor
        static ContactTrigger()
        {
            FindDerivedTypesFromAssembly();
        }

        #region Presets
#if UNITY_EDITOR

        public ContactTrigger UsePreset(Preset preset)
        {
            switch (preset)
            {
                case Preset.ContactScan:
                    {
                        break;
                    }

                case Preset.VitalsScan:
                    {
                        SetAllowedType(typeof(IVitalsSystem));
                        break;
                    }
                case Preset.InventorysScan:
                    {
                        SetAllowedType(typeof(IInventorySystem));
                        break;
                    }

                case Preset.HealthPickup:
                    {
                        SetAllowedType(typeof(IVitalsSystem));
                        break;
                    }

                case Preset.RechargeZone:
                    {
                        SetAllowedType(typeof(IVitalsSystem));
                        break;
                    }
                case Preset.DamageZone:
                    {
                        SetAllowedType(typeof(IVitalsSystem));
                        break;
                    }

                case Preset.WeaponMelee:
                    {
                        SetAllowedType(typeof(IVitalsSystem));
                        break;
                    }

                case Preset.WeaponCannon:
                    {
                        SetAllowedType(typeof(IVitalsSystem));
                        break;
                    }

                case Preset.WeaponScan:
                    {
                        SetAllowedType(typeof(IVitalsSystem));
                        break;
                    }
            }
            return this;
        }
#endif
        #endregion

        #region Inspector

        [Tooltip("If ITriggeringComponent has multiple colliders, they will all be capable of triggering Enter/Stay/Exit events. Enabling this prevents that, and will suppress multiple calls on the same object.")]
        [SerializeField] public bool preventRepeats = true;
        public bool PreventRepeats { get { return preventRepeats; } set { preventRepeats = value; } }

        [SerializeField]
        [HideInInspector]
        public int[] _ignoredSystems;
        protected List<Type> ignoredSystems = new List<Type>();

        #endregion

        public List<IOnContactEvent> OnContactEventCallbacks = new List<IOnContactEvent>(1);

        private List<IContactSystem> _contactSystems = new List<IContactSystem>(0);
        public List<IContactSystem> ContactSystems { get { return _contactSystems; } }


        [Tooltip("This ContactTrigger can act as a proxy of another. " +
            "For example projectiles set the proxy as the shooters ContactTrigger, so projectile hits can be treated as hits by the players weapon. " +
            "Default setting is 'this', indicating this isn't a proxy.")]
        public IContactTrigger _proxy;
        public IContactTrigger Proxy
        {
            get { return this._proxy; }
            set { this._proxy = value; }
        }

        public byte Index { get; set; }
        
        // cache
        protected NetObject netObj;
        public NetObject NetObj { get { return netObj; } }

        protected ISyncContact syncContact;
        public ISyncContact SyncContact { get { return syncContact; } }

        protected IContactGroupsAssign contactGroupsAssign;
        public IContactGroupsAssign ContactGroupsAssign { get { return contactGroupsAssign; } }

        internal ContactType usedContactTypes;

        /// Hashsets used to make sure things only trigger once per tick, so multiple colliders
        /// can't cause multiple retriggers
        protected HashSet<IContactSystem> triggeringHitscans = new HashSet<IContactSystem>();
        protected HashSet<IContactSystem> triggeringEnters = new HashSet<IContactSystem>();
        protected HashSet<IContactSystem> triggeringStays = new HashSet<IContactSystem>();

#if UNITY_EDITOR

        protected virtual void Reset()
        {
            this._proxy = this;
            PollInterfaces();

            FindDerivedTypesFromAssembly();
            GetAllowedTypesFromHashes();
        }

#endif
        public static List<IContactSystem> tempFindSystems = new List<IContactSystem>(2);

        public virtual void PollInterfaces()
        {
            _contactSystems.Clear();
            transform.GetNestedComponentsInParents<IContactSystem, NetObject>(tempFindSystems);

            // Selectively add ContactSystems selected as valid interactions
            for(int i = 0, cnt = tempFindSystems.Count; i < cnt;  ++i)
            {
                var found = tempFindSystems[i];
                var foundType = found.GetType();

                bool ignoreSystem = false;
                foreach(var ignored in ignoredSystems)
                {
                    if (foundType.CheckIsAssignableFrom(ignored))
                    {
                        ignoreSystem = true;
                        break;
                    }
                }

                if (!ignoreSystem)
                    _contactSystems.Add(tempFindSystems[i]);

            }
            // Find callback interfaces, but don't recurse past the end of this NetObject, or past any children ContactTriggers
            transform.GetNestedComponentsInChildren(this.OnContactEventCallbacks, true, typeof(NetObject), typeof(IContactTrigger));

        }

        public virtual void Awake()
        {
            if (_proxy == null)
                _proxy = this;

            GetAllowedTypesFromHashes();

            PollInterfaces();

            netObj = transform.GetParentComponent<NetObject>();
            syncContact = GetComponent<ISyncContact>();

            /// Associate a group assign with this component.
            contactGroupsAssign = GetComponent<ContactGroupAssign>();
            // If no group assign on this node, find parent and use that if set to apply to children
            if (ReferenceEquals(contactGroupsAssign, null))
            {
                var found = transform.GetNestedComponentInParent<IContactGroupsAssign, NetObject>();
                if (found != null && found.ApplyToChildren)
                    contactGroupsAssign = found;
            }

            // cache the mask of ContactTypes that will be responded to
            foreach (IOnContactEvent cb in OnContactEventCallbacks)
                usedContactTypes |= cb.TriggerOn;
        }

        protected virtual void OnEnable()
        {
            if (preventRepeats)
            {
                NetMasterCallbacks.RegisterCallbackInterfaces(this);

                triggeringHitscans.Clear();
                triggeringEnters.Clear();
                triggeringStays.Clear();
            }
        }

        protected virtual void OnDisable()
        {
            if (preventRepeats)
                NetMasterCallbacks.RegisterCallbackInterfaces(this, false, true);
        }

        public void OnStateChange(ObjState newState, ObjState previousState, Transform attachmentTransform, Mount attachTo = null, bool isReady = true)
        {
            // TODO: this is test code. Might need to be more selective about clearing this on state changes.
            if (preventRepeats)
                this.triggeringEnters.Clear();
        }

        #region Triggers

        #region Enter

        private void OnTriggerEnter2D(Collider2D other)
        {
            Contact(other, ContactType.Enter);
        }
        private void OnTriggerEnter(Collider other)
        {
            Contact(other, ContactType.Enter);
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            Contact(collision.collider, ContactType.Enter);
        }
        private void OnCollisionEnter(Collision collision)
        {
            Contact(collision.collider, ContactType.Enter);
        }

        #endregion Enter

        #region Stay

        private void OnTriggerStay2D(Collider2D other)
        {
            Contact(other, ContactType.Stay);
        }
        private void OnTriggerStay(Collider other)
        {
            Contact(other, ContactType.Stay);
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            Contact(collision.collider, ContactType.Stay);
        }
        private void OnCollisionStay(Collision collision)
        {
            Contact(collision.collider, ContactType.Stay);
        }

        #endregion Stay

        #region Exit

        private void OnTriggerExit2D(Collider2D other)
        {
            Contact(other, ContactType.Exit);
        }
        private void OnTriggerExit(Collider other)
        {
            Contact(other, ContactType.Exit);
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            Contact(collision.collider, ContactType.Exit);
        }
        private void OnCollisionExit(Collision collision)
        {
            Contact(collision.collider, ContactType.Exit);
        }

        #endregion Exit


        #endregion Triggers


        /// <summary>
        /// Converts Unity Collision/Trigger events into OnContact calls on both ContactTriggers involved.
        /// </summary>
        protected virtual void Contact(Component otherCollider, ContactType contactType)
        {
            //if (GetComponent<ContactProjectile>() && contactType == ContactType.Enter)
            //    Debug.Log(name + " <b>Prj Contact</b> " + otherCollider.name);

            var otherCT = otherCollider.transform.GetNestedComponentInParents<IContactTrigger, NetObject>();

            if (ReferenceEquals(otherCT, null))
                return;

            if (CheckIsNested(this, otherCT.Proxy))
                return;

            //Debug.Log(this._proxy.NetObj.name + " : " + (otherCT as Component).name);
            /// Ignore self collisions
            if (ReferenceEquals(this._proxy.NetObj, otherCT.Proxy.NetObj))
            {
                //Debug.Log("Early self-collide detect " + this._proxy.NetObj.name + " " + otherCT.Proxy.NetObj.name);
                return;
            }

            otherCT.Proxy.OnContact(this, contactType);
            this._proxy.OnContact(otherCT, contactType);

        }

        protected bool CheckIsNested(IContactTrigger first, IContactTrigger second)
        {
            var firstNO = first.NetObj;
            var secondNO = second.NetObj;

            var testNO = firstNO;

            while (!ReferenceEquals(testNO, null))
            {
                if (ReferenceEquals(testNO, secondNO))
                {
                    return true;
                }

                var par = testNO.transform.parent;

                if (ReferenceEquals(par, null))
                    break;

                testNO = par.GetParentComponent<NetObject>();
            }

            testNO = secondNO;

            while (!ReferenceEquals(testNO, null))
            {
                if (ReferenceEquals(testNO, firstNO))
                {
                    return true;
                }

                var par = testNO.transform.parent;

                if (ReferenceEquals(par, null))
                    break;

                testNO = par.GetParentComponent<NetObject>();
            }

            return false;
        }

        public virtual void OnContact(IContactTrigger otherCT, ContactType contactType)
        {

            if (GetComponent<ContactProjectile>() && contactType == ContactType.Enter)
                Debug.Log("Prj Contact");



            /// Check each system that is part of this contact event, to see if what it contacted with is applicable
            List<IContactSystem> systems = otherCT.Proxy.ContactSystems;

            int systemsCount = systems.Count;

            if (systemsCount == 0)
                return;

            /// May be important in preventing race conditions when objects first spawn in, where they might trigger contacts by starting in the wrong state.
            if (netObj != null && !this._proxy.NetObj.AllObjsAreReady)
            {
                //Debug.LogError(Time.time + name + " " + _proxy.NetObj.photonView.OwnerActorNr + " Not ready so ignoring contact");
                return;
            }

            var otherNetObj = otherCT.Proxy.NetObj;

            if (otherNetObj != null && !otherNetObj.AllObjsAreReady)
            {
                Debug.Log(Time.time + name + " " + otherNetObj.photonView.OwnerActorNr + " Other object not ready so ignoring contact");
                return;
            }

            for (int i = 0; i < systemsCount; i++)
            {
                var system = systems[i];

                if (!IsCompatibleSystem(system, otherCT))
                    continue;

                //Debug.Log(name + " " + GetType().Name + " <> " + ics.GetType().Name + " <b>PASSED</b>");
                /// Check to see if we have already reacted to this collision (multiple colliders/etc)
                if (preventRepeats)
                {
                    switch (contactType)
                    {
                        case ContactType.Enter:
                            {
                                if (triggeringEnters.Contains(system))
                                    continue;

                                triggeringEnters.Add(system);
                                break;
                            }
                        case ContactType.Stay:
                            {
                                if (triggeringStays.Contains(system))
                                    continue;

                                triggeringStays.Add(system);
                                break;
                            }
                        case ContactType.Exit:
                            {
                                if (!triggeringEnters.Contains(system))
                                    continue;

                                triggeringEnters.Remove(system);
                                break;
                            }
                        case ContactType.Hitscan:
                            {
                                if (triggeringHitscans.Contains(system))
                                    continue;

                                triggeringHitscans.Add(system);
                                break;
                            }
                    }
                }

                //Debug.Log("Other " + (otherCT as Component).name + " : " + (otherCT as Component).GetType().Name);

                /// Ignore contact types we have no reactors for. This runs after the above loop, because Enter/Stay/Exit all need to be processed for PreventRepeats to work.
                if ((usedContactTypes & contactType) == 0)
                {
                    return;
                }

                /// If there is an ISyncContact, pass contactEvents to it rather than executing them.
                var contactEvent = new ContactEvent(system, otherCT, contactType);

                if (ReferenceEquals(Proxy.SyncContact, null))
                {
                    ContactCallbacks(contactEvent);
                }
                else
                {
                    syncContact.SyncContactEvent(contactEvent);
                }
            }
        }

        public virtual Consumption ContactCallbacks(ContactEvent contactEvent)
        {
            //if (contactEvent.contactType == ContactType.Enter)
            //    Debug.Log("ContactCallbacks " + contactEvent);

            Consumption consumption = Consumption.None;

            for (int i = 0, cnt = this.OnContactEventCallbacks.Count; i < cnt; ++i)
            {
                consumption |= this.OnContactEventCallbacks[i].OnContactEvent(contactEvent);
                if (consumption == Consumption.All)
                    return Consumption.All;
            }

            return consumption;
        }

        /// <summary>
        /// This callback only is registered if preventRepeats is true.
        /// </summary>
        public void OnPreSimulate(int frameId, int subFrameId)
        {
            if (preventRepeats)
            {
                triggeringHitscans.Clear();
                triggeringStays.Clear();
            }
        }


        // Convert our serialized allowed hashes into a usable HashSet.
        internal void GetAllowedTypesFromHashes()
        {
            ignoredSystems.Clear();

            // No serialized system hashes to ignore. Do nothing.
            if (_ignoredSystems == null)
                return;

            foreach (var type in contactSystemTypes)
            {
                int hash = type.Name.GetHashCode();
                
                bool ignore = false;
                foreach (var allowed in _ignoredSystems)
                {
                    if (allowed == hash)
                        ignore = true;
                }

                if (ignore)
                    ignoredSystems.Add(type);
            }
        }

#if UNITY_EDITOR

        protected void SetAllowedType(params Type[] allowed)
        {
            List<int> disallowed = new List<int>();

            foreach (var type in contactSystemTypes)
            {
                bool allow = false;

                foreach (var a in allowed)
                {
                    if (a.CheckIsAssignableFrom(type))
                    {
                        allow = true;
                        break;
                    }
                }

                if (!allow)
                {
                    int hash = type.Name.GetHashCode();
                    disallowed.Add(hash);
                }
                
            }
            _ignoredSystems = disallowed.ToArray();
        }

#endif


        internal static List<Type> contactSystemTypes = new List<Type>();

        /// <summary>
        /// Find all IContactSystem derived interfaces that are not abstract.
        /// </summary>
        internal static void FindDerivedTypesFromAssembly()
        {
            contactSystemTypes.Clear();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsAbstract)
                        continue;

                    //bool assignable = type.CheckIsAssignableFrom(typeof(IContactSystem)); //.GetInterface(typeof(IContactSystem));
                    bool assignable = typeof(IContactSystem).CheckIsAssignableFrom(type); //.GetInterface(typeof(IContactSystem));

                    if (assignable)
                    {
                        contactSystemTypes.Add(type);
                    }
                }
            }
        }

        private bool IsCompatibleSystem(IContactSystem system, IContactTrigger ct)
        {
            var sysType = system.GetType();

            foreach (var ignored in ignoredSystems)
            {
                //Debug.Log(sysType + " <b> compare </b> " + ignored + " " + sysType.CheckIsAssignableFrom(ignored) + ":" + ignored.CheckIsAssignableFrom(sysType));

                //if (sysType.CheckIsAssignableFrom(ignored))
                if (ignored.CheckIsAssignableFrom(sysType))
                        return false;
            }

            //Debug.Log(name + " <> " + (ct as Component).name  + " <b>match</b> " + sysType.Name);
            return true;
        }

    }


#if UNITY_EDITOR

    [CustomEditor(typeof(ContactTrigger))]
    [CanEditMultipleObjects]
    public class ContactTriggerEditor : TriggerHeaderEditor
    {
        protected override string HelpURL
        {
            get { return SimpleDocsURLS.SYNCCOMPS_PATH + "#contacttrigger_component"; }
        }

        protected override string Instructions
        {
            get
            {
                return "Responds to Trigger/Collision/Scan events between this and other " + typeof(IContactTrigger).Name + " components. " +

                    typeof(IOnContactEvent).Name + " callbacks are generated for each applicable " + typeof(IContactSystem).Name + ".";
            }
        }

        protected override string TextTexturePath { get { return "Header/ContactTriggerText"; } }

        protected static GUIStyle ifaceBox;
        public override void OnEnable()
        {
            base.OnEnable();
            var _target = (target as ContactTrigger);
            _target.GetAllowedTypesFromHashes();
            _target.PollInterfaces();
        }

        private static HashSet<int> tempOldHashes = new HashSet<int>();
        private static List<int> tempNewHashes = new List<int>();

        protected override void OnInspectorGUIInjectMiddle()
        {
            if (ifaceBox == null)
                ifaceBox = new GUIStyle("HelpBox") { padding = new RectOffset(10, 6, 6, 6) };

            base.OnInspectorGUIInjectMiddle();
            var _target = (target as ContactTrigger);

            // If this generates a "hash already exists" error, then this is a freak occurrence of two interfaces producing the same hash.
            tempOldHashes.Clear();
            tempNewHashes.Clear();

            if (_target._ignoredSystems != null)
                foreach (var h in _target._ignoredSystems)
                    tempOldHashes.Add(h);


            EditorGUILayout.BeginVertical("HelpBox"); // ifaceBox);
            {
                bool systemChanged = false;
                foreach (var type in ContactTrigger.contactSystemTypes)
                {
                    int hash = type.Name.GetHashCode();
                    bool prev = !tempOldHashes.Contains(hash);
                    bool allowed = EditorGUILayout.ToggleLeft(type.Name, prev);

                    if (allowed != prev)
                        systemChanged = true;

                    if (!allowed)
                        tempNewHashes.Add(hash);
                }
                if (systemChanged)
                {
                    Undo.RecordObject(target, "Selected ContactSystems changed");
                    _target._ignoredSystems = tempNewHashes.ToArray();
                    _target.GetAllowedTypesFromHashes();
                    _target.PollInterfaces();
                }

            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(ifaceBox);
            {
                FoundInterfaces("Found " + typeof(IContactSystem).Name + "s:", _target.ContactSystems);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(ifaceBox);
            {
                FoundInterfaces("Found " + typeof(IOnContactEvent).Name + "s:", _target.OnContactEventCallbacks);
            }
            EditorGUILayout.EndVertical();
        }

        private void FoundInterfaces<T>(string label, List<T> list)
        {

            EditorGUILayout.LabelField(label);

            EditorGUI.BeginDisabledGroup(true);

            if (list.Count > 0)
                foreach (var c in list)
                {
                    var comp = c as Component;
                    EditorGUILayout.ObjectField(comp, typeof(Component), false);
                }
            else
                EditorGUILayout.LabelField("none", new GUIStyle("Label") { padding  =new RectOffset(6, 0, 0 , 0), fontSize = 11, fontStyle = FontStyle.Italic });

            EditorGUI.EndDisabledGroup();

        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
#endif
}

