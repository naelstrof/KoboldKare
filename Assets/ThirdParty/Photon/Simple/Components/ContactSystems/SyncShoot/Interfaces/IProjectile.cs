// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;


namespace Photon.Pun.Simple
{
	public enum RespondTo { HitSelf = 1, IContactTrigger = 2, HitNetObj = 4, HitNonNetObj = 8, /*CheckNestedParents = 8 */}

	public interface IProjectile
	{
		IProjectileCannon Owner { get; set; }
		void Initialize(IProjectileCannon owner, int frameId, int subFrameId, Vector3 velocity, RespondTo terminateOn, RespondTo damageOn, float timeshift);
	}

}
