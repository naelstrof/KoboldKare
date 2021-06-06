// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
	public interface IProjectileCannon
	{
		PhotonView PhotonView { get; }
		NetObject NetObj { get; }
		IContactTrigger ContactTrigger { get; }
		int ViewID { get; }
	}
}
