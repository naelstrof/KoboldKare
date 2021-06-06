// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Pun.Simple.ContactGroups;

#if UNITY_EDITOR
using UnityEditor;
using Photon.Pun.Simple.Internal;
#endif

namespace Photon.Pun.Simple
{

    /// <summary>
    /// Base class for the inventory system. You can extend this class using your own T to define how capacity is defined, 
    /// and override any of the virtual methods to customize checks for triggering, pickup and having capacity.
    /// If you are feeling pro level, you can define your own class using the IInventory<> interface yourself.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Inventory<T> : NetComponent
        , IInventorySystem<T>
    {

        #region Inspector

        [SerializeField]
        protected MountSelector defaultMounting = new MountSelector(0);
        public Mount DefaultMount { get; set; }

        [SerializeField]
        protected ContactGroupMaskSelector contactGroups = new ContactGroupMaskSelector();
        public IContactGroupMask ValidContactGroups { get { return contactGroups; } }


        #endregion Inspector

        public byte SystemIndex { get; set; }
        
        //cache
        protected MountsManager mountsLookup;
        protected int defaultMountingMask;
        public int ValidMountsMask { get { return defaultMountingMask; } }

        public override void OnAwakeInitialize(bool isNetObject)
        {
            base.OnAwakeInitialize(isNetObject);

            this.transform.EnsureRootComponentExists<ContactManager, NetObject>();

            mountsLookup = netObj.transform.GetComponent<MountsManager>();
            defaultMountingMask = 1 << (defaultMounting.id);

        }
        public override void OnStart()
        {
            base.OnStart();

            if (mountsLookup)
                DefaultMount = mountsLookup.GetMount(defaultMounting);

            //Debug.Log("DEFMOUNT <b>"+ DefaultMount + " : " + mountIdx + " :</b> " + mountsLookup);
        }

        public virtual Consumption TryTrigger(IContactReactor reactor, ContactEvent contactEvent, int compatibleMounts)
        {
            //Debug.Log("TryTrigger Basic Inv. compat with: " + compatibleMounts + " defMountId: " + defaultMounting.id + " defMask: " + defaultMountingMask);

            IInventoryable<T> iven = reactor as IInventoryable<T>;

            if (ReferenceEquals(iven, null))
                return Consumption.None;

            if (contactGroups != 0)
            {
                IContactGroupsAssign groups = contactEvent.contactTrigger.ContactGroupsAssign;
                int triggermask = ReferenceEquals(groups, null) ? 0 : groups.Mask;
                if ((contactGroups.Mask & triggermask) == 0)
                {
                    //Debug.Log("Try trigger... ContactGroup mismatch " + contactGroups.Mask + "<>" + triggermask);
                    return Consumption.None;
                }
            }

            /// Return if the object being picked up exceeds remaining inventory.
            if (TestCapacity(reactor as IInventoryable<T>) == false)
            {
                //Debug.Log(name + " failed");
                return Consumption.None;
            }

            /// If both are set to 0 (Root) then consider that a match, otherwise zero for one but not the other is a mismatch (for now)
            if ((compatibleMounts == defaultMountingMask) || (compatibleMounts & defaultMountingMask) != 0)
            {
                // TODO: partial consumption handling needed

                //Debug.Log(name + " <> " + (trigger as Component).name + " <b>success</b>: " + compatibleMounts + " <> " + defaultMountingMask);
                return Consumption.All;
            }
            else
            {
                //Debug.Log(name + " <> " + (trigger as Component).name + " failed: " + compatibleMounts + " <> " + defaultMountingMask);
                return Consumption.None;
            }
        }


        public virtual Mount TryPickup(IContactReactor reactor, ContactEvent contactEvent)
        {
            return DefaultMount;
        }

        /// <summary>
        /// Return if the object being picked up exceeds remaining inventory. Default implementation always just returns true. Override to create real tests.
        /// </summary>
        /// <param name="inventoryable"></param>
        /// <returns></returns>
        public virtual bool TestCapacity(IInventoryable<T> inventoryable)
        {
            return true;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Inventory<>), true)]
    [CanEditMultipleObjects]
    public class InventoryTEditor : ContactSystemHeaderEditor
    {
        protected override string HelpURL
        {
            get { return SimpleDocsURLS.SYNCCOMPS_PATH + "#inventory_contact_system"; }
        }

        protected override string TextTexturePath
        {
            get
            {
                return "Header/InventorySystemText";
            }
        }
        protected override string Instructions
        {
            get
            {
                return "Associates a Mount with an Inventory. Picked up <i>IInventoryable</i> will attach to the associated mount.";
            }
        }
    }
#endif
}

