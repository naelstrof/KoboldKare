//// ---------------------------------------------------------------------------------------------
//// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
//// <author>developer@exitgames.com</author>
//// ---------------------------------------------------------------------------------------------

//using UnityEngine;


//namespace Photon.Pun.Simple
//{
//	public static class StateReactors
//	{
//		///// <summary>
//		///// Convert mountId into a mount's transform, and attach attachment.
//		///// </summary>
//		///// <param name="state"></param>
//		///// <param name="mountId"></param>
//		///// <param name="attachmentTransform"></param>
//		///// <param name="attachedTo"></param>
//		//public static void AttachTransform(ObjState state, int mountId, Transform attachmentTransform, List<IOnTeleport> callbacks, Mount attachedTo = null)
//		//{
//		//	//Debug.Log(Time.time + " " +attachmentTransform.name + " " + state + " AttachTrans to " + attachedTo);

//		//	if ((state & ObjState.Attached) != 0)
//		//	{
//		//		var trans = StateReactors.GetAttachToTransform(attachedTo, mountId);
//		//		if (trans)
//		//		{
					
//		//			//attachmentTransform.localPosition = new Vector3(0, 0, 0);
//		//			//attachmentTransform.localRotation = new Quaternion(0, 0, 0, 1);

//		//			for (int i = 0; i < callbacks.Count; ++i)
//		//				callbacks[i].OnTeleport(new Vector3(), new Quaternion(), null);

//		//			attachmentTransform.parent = trans;
//		//			attachmentTransform.position = new Vector3();
//		//			attachmentTransform.rotation = new Quaternion();
//		//		}
//		//	}
//		//	else
//		//	{

//		//	}
//		//}

//		public static Transform GetAttachToTransform(Object attachedTo, int mountId)
//		{

//			if (attachedTo == null)
//				return null;

//			/// First see if this is the easiest case... it is a mount already.
//			Mount mount = attachedTo as Mount;
//			if (!ReferenceEquals(mount, null))
//			{
//				return mount.transform;
//			}

//			/// Otherwise try to find a MountLookup
//			else
//			{
//				var mountsLookup = attachedTo as MountsManager;

//				/// if not a Mounts... try getting Mounts component from the supplied object
//				if (!mountsLookup)
//				{
//					Debug.Log("Not Found MountLookup");

//					var comp = (attachedTo as Component);
//					if (comp)
//						mountsLookup = comp.GetComponent<MountsManager>();

//					var gobj = (attachedTo as GameObject);
//					if (gobj)
//						mountsLookup = gobj.GetComponent<MountsManager>();
//				}

//				/// If we found a mounts lookup with a search, try again to get a mount by ID
//				if (!ReferenceEquals(mountsLookup, null))
//				{
//					Debug.Log("Found MountLookup");

//					bool success = mountsLookup.mountIdLookup.TryGetValue(mountId, out mount);

//					if (success)
//					{
//						Debug.Log("Found Mount with ID " + mount.name);
//						return mount.transform;

//					}
//					else
//						return mountsLookup.transform;
//				}
//			}

			
//			/// All other cases we are after the transform
//			Transform tr = (attachedTo as Transform);
//			if (tr)
//				return tr;

//			GameObject go = attachedTo as GameObject;
//			if (go)
//				return go.transform;

//			if (tr)
//				return tr;

//			return null;
//		}
//	}
//}
