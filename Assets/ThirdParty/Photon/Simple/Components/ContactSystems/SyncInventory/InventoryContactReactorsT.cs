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

	public abstract class InventoryContactReactors<T> : ContactReactorBase<IInventorySystem<T>>
		, IInventoryable<T>
	{
		public abstract T Size { get; }

		// cache
		protected int volume;
		public int Volume { get { return volume; } }

        public override bool IsPickup {  get { return true; } }

        protected override Consumption ProcessContactEvent(ContactEvent contactEvent)
        {
            //Debug.Log("Process " + contactEvent + " --  "  + contactEvent.contactSystem.GetType().Name + " : " + (contactEvent.contactSystem as IInventorySystem<T>));

            var system = (contactEvent.contactSystem as IInventorySystem<T>);
            if (system == null)
                return Consumption.None;


            ///// TEST - Notify other component of pickup condition
            //var onContact = transform.GetComponent<IOnPickup>();
            //Debug.Log("POST 2 " + (onContact != null));

            //if (onContact != null)
            //    syncState.HardMount(onContact.OnPickup(contactEvent));

            if (IsPickup)
            {
                Mount mount = system.TryPickup(this, contactEvent);
                if (mount)
                    syncState.HardMount(mount);

            }

            return Consumption.All;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(InventoryContactReactors<>), true)]
    [CanEditMultipleObjects]
    public class InventoryContactReactorsBaseEditor : ContactReactorsBaseEditor
    {
        protected override void OnInspectorGUIInjectMiddle()
        {
            base.OnInspectorGUIInjectMiddle();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerOn"));

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
