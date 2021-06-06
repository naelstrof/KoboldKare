// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
	public interface IFlagTeleport
	{
        /// <summary>
        /// Call this BEFORE changing the parent and/or applying position and rotation changes. This will notify the SyncTransform that a teleport will be happening, and SyncTransform will record the pre-teleport state
        /// and send both the pre and post teleport state in the next send. 
        /// </summary>
		void FlagTeleport();

	}
}
