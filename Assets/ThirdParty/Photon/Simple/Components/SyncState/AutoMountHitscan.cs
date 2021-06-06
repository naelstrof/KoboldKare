// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
	/// <summary>
	/// This component will generate a hitscan based on the transform it is attached to. For each mount hit, 
	/// SyncState.SoftMount will be called to attempt to reparent to the transform of the Mount.
	/// </summary>
	public class AutoMountHitscan : HitscanComponent
	{
		//public ContactGroupMaskSelector contactGroups;

		protected SyncState syncState;
		public SyncState SyncState { get { return syncState; } }

#if UNITY_EDITOR
		protected override void Reset()
		{
			base.Reset();
			hitscanDefinition.distance = 1;
			visualize = true;

#if PUN_2_OR_NEWER
            var rootSyncTransform = netObj.GetComponent<SyncTransform>();
			if (rootSyncTransform)
			{
				if (!rootSyncTransform.transformCrusher.PosCrusher.local)
				{
					Debug.LogWarning(typeof(SyncTransform).Name + " on root of NetObject " + netObj.name +
						" does not have its position sync set to Local, which is the preferred setting when netObj is going to be changing parents. Setting to true for you.");
					rootSyncTransform.transformCrusher.PosCrusher.local = true;
				}
			}
#endif
        }
#endif


		public override void OnAwake()
		{
			base.OnAwake();

			if (netObj)
				syncState = netObj.GetComponent<SyncState>();
		}

		public override void OnAuthorityChanged(bool isMine, bool controllerChanged)
		{
			base.OnAuthorityChanged(isMine, controllerChanged);

			/// Rather than an IsMine test every tick, we are completely removing this object from the OnPreSiulate callback list when !IsMine
			var callbacklist = netObj.onPreSimulateCallbacks;
			bool containsThis = callbacklist.Contains(this);

			base.OnAuthorityChanged(isMine, controllerChanged);
			if (isMine)
			{
				if (!containsThis)
					callbacklist.Add(this);
			}
			else
			{
				if (containsThis)
					callbacklist.Remove(this);
			}
		}

		public override void OnPreSimulate(int frameId, int subFrameId)
		{

			if (subFrameId == TickEngineSettings.sendEveryXTick - 1)
			{
				triggerQueued = true;
				//foundMounts.Clear();

				base.OnPreSimulate(frameId, subFrameId);

				if (foundMounts.Count != 0)
				{
					do
					{
						var mount = foundMounts.Dequeue();
						syncState.SoftMount(mount);

					} while (foundMounts.Count != 0);
				}
				else
				{
					syncState.SoftMount(null);
				}
			}
		}

		Queue<Mount> foundMounts = new Queue<Mount>();

		public override bool ProcessHit(Collider hit)
		{
#if PUN_2_OR_NEWER
            var mount = hit.transform.GetNestedComponentInParents<Mount, NetObject>();

			//if (contactGroups != 0)
			//{
			//	var hga = hit.GetComponent<IContactGroupMask>();

			//	//Debug.Log(hit.name + " " + contactGroups.Mask + " : " + hga + " : " + hgaa + " : " + (hga as Component ? hga.Mask.ToString() : "???"));

			//	if (!ReferenceEquals(hga, null) && (hga.Mask & contactGroups) == 0)
			//		return false;

			//}

			if (mount)
			{
				//Debug.Log(Time.time + " " + name + " Mount to " + mount);
				foundMounts.Enqueue(mount);
			}
#endif
            return false;
		}
	}

#if UNITY_EDITOR
    [CustomEditor(typeof(AutoMountHitscan), true)]
    [CanEditMultipleObjects]
    public class AutoMountHitscanEditor : AccessoryHeaderEditor
    {
        //protected override string HelpURL
        //{
        //    get
        //    {
        //        return REFERENCE_DOCS_PATH + @"synccomponents#syncobject__isyncobject_";
        //    }
        //}

    }
#endif

}
