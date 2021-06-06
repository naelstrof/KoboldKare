// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

    public abstract class ContactReactorBase : NetComponent
        , IContactReactor
    {

#if UNITY_EDITOR
        public override bool AutoAddNetObj { get { return false; } }

        [Utilities.VersaMask(true)]
#endif
        [HideInInspector] public ContactType triggerOn = ContactType.Enter | ContactType.Stay | ContactType.Exit | ContactType.Hitscan;
        public ContactType TriggerOn { get { return triggerOn; } }

#if UNITY_EDITOR
        [SerializeField]
        protected bool DebugMismatches;
#endif

        public abstract bool IsPickup { get; }

        // cache
        protected SyncState syncState;
        protected int syncStateMountMask;

        public override void OnAwakeInitialize(bool isNetObject)
        {
            if (isNetObject)
            {
                syncState = transform.GetNestedComponentInParent<SyncState, NetObject>();
                syncStateMountMask = (syncState) ? syncState.mountableTo.mask : 0;
            }

            base.OnAwakeInitialize(isNetObject);
        }

        public virtual Consumption OnContactEvent(ContactEvent contactEvent)
        {
            // Don't react to contact events if we are using a sync. It will capture and manage contact events.
            //if (deferToISyncContact)
            //    return Consumption.None;

            //if (contactEvent.contactType == ContactType.Hitscan)
            //    Debug.Log(Time.time + " SCAN " + name + " OnContactEvent " + contactEvent);

            var contactType = contactEvent.contactType;

            if (triggerOn != 0 && (contactType & triggerOn) == 0)
                return Consumption.None;

            /// Pickup requires mount compatibility to be valid
            if (IsPickup)
            {
                var system = contactEvent.contactSystem;
                int systemMount = system.ValidMountsMask;
                int mountableTo = syncState.mountableTo;
                if (systemMount != 0 && (systemMount & mountableTo) == 0)
                {
#if UNITY_EDITOR
                    if (DebugMismatches)
                        Debug.LogWarning(name + " mount mask mismatch with: '" + system.NetObj.name + "':" + system.GetType().Name + "[" + system.SystemIndex + "] masks: " + systemMount + " <-> " + mountableTo);
#endif
                    return Consumption.None;
                }
            }

            return ProcessContactEvent(contactEvent);
        }

        protected abstract Consumption ProcessContactEvent(ContactEvent contactEvent);

    }

    public abstract class ContactReactorBase<T> : ContactReactorBase// NetComponent 
        , IOnContactEvent
        where T : class, IContactSystem
    {

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ContactReactorBase), true)]
    [CanEditMultipleObjects]
    public class ContactReactorsBaseEditor : ReactorHeaderEditor
    {

        protected override string Instructions
        {
            get
            {
                return "Reacts to <i>" + typeof(IOnContactEvent).Name + "</i> callbacks, " +
                    "by testing for a valid interaction with the <i>" + typeof(IContactSystem).Name + "</i> involved in the contact.";
            }
        }

        protected SyncState syncState;

        public override void OnEnable()
        {
            base.OnEnable();
            syncState = (target as ContactReactorBase).transform.GetNestedComponentInParent<SyncState, NetObject>();
        }

        protected override void OnInspectorGUIFooter()
        {
            base.OnInspectorGUIFooter();
            SyncStateWarnings();
        }

        protected virtual void SyncStateWarnings()
        {
            if (syncState == null)
            {
                EditorGUILayout.HelpBox("A SyncState is required for this Net Object to be mountable.", MessageType.Warning);
            }
            else if (syncState.mountableTo == 0)
            {
                EditorGUILayout.HelpBox("SyncState on this object has no allowed 'Mountable To' Types. This object will not be compatible with any system.", MessageType.Warning);
            }
        }
    }
#endif
}
