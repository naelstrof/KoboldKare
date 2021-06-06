//// ---------------------------------------------------------------------------------------------
//// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
//// <author>developer@exitgames.com</author>
//// ---------------------------------------------------------------------------------------------

//using System.Collections.Generic;
//using UnityEngine;

//namespace Photon.Pun.Simple
//{
//	public static class StateSyncHelpers
//	{
//		/// <summary>
//		/// Apply the vectors (offset, pos, rot, velocity) in StateChangeInfo to the syncState object. Returns true if teleport is required for change.
//		/// </summary>
//		public static void ApplyVectors(this SyncState syncState, StateChangeInfo stateChangeInfo, Transform prevParent, List<IFlagTeleport> callbacks)
//		{

//			Transform transform = syncState.transform;

//			/// For respawns just the pos/rot values are NET relative to he old parent.
//			//if (stateChangeInfo.respawn)
//			//{
//			//	var mount = stateChangeInfo.mount;
//			//	var localPos = stateChangeInfo.offsetPos;
//			//	var localRot = stateChangeInfo.offsetRot;

//			//	int cnt = callbacks.Count;
//			//	for (int i = 0; i < cnt; i++)
//			//		callbacks[i].OnTeleport();

//			//	transform.parent = (mount) ? mount.transform : null;

//			//	if (localPos.HasValue)
//			//		transform.localPosition = localPos.Value;
//			//	if (localRot.HasValue)
//			//		transform.localEulerAngles = localRot.Value;

//			//	return;
//			//}
//			//else
//			//{
//				//if (stateChangeInfo.offsetPos.HasValue)
//				//{
//				//	{
//				//		var parRot = prevParent ? prevParent.rotation : transform.rotation;
//				//		/// TODO: offset and vel likely should be part of the ChangeState itself
//				//		/// Apply offset before changing state
//				//		var localOffset = stateChangeInfo.offsetPos;

//				//		//Debug.Log(Time.time + " " + transform.name + " Dequeue " + transform.position + " " + transform.position + prevParent.rotation * localOffset);
//				//		//transform.position = transform.position + prevParent.rotation * localOffset.Value;

//				//		Vector3? pos = localOffset.HasValue ? (transform.position + parRot * localOffset.Value) : (Vector3?)null;
//				//		Vector3? rot = stateChangeInfo.offsetRot;

//				//		//int cnt = callbacks.Count;
//				//		//for (int i = 0; i < cnt; i++)
//				//		//	callbacks[i].OnTeleport();

//				//		transform.parent = null;
//				//		if (pos.HasValue)
//				//			transform.position = pos.Value;
//				//		if (rot.HasValue)
//				//			transform.eulerAngles = rot.Value;
//				//	}
//				//}
//			//}

//			if (stateChangeInfo.velocity.HasValue)
//			{

//				var rb = syncState.Rb;
//				if (rb)
//				{
//					if (!rb.isKinematic)
//					{
//						rb.velocity = ((prevParent) ? prevParent.rotation : rb.rotation) * stateChangeInfo.velocity.Value;
//					}
//				}
//				else
//				{
//					var rb2d = syncState.Rb2d;
//					if (rb2d)
//					{
//						if (!rb2d.isKinematic)
//						{
//							rb2d.velocity = stateChangeInfo.velocity.Value;
//						}
//					}
//				}
//			}

			
//		}

//	}
//}

