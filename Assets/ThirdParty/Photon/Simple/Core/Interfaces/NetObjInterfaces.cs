// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
	/// <summary>
	/// Callback Interface for when all SyncObjects on a NetObj have indicated Ready and are prepared for the NetObj to become visible.
	/// </summary>
	public interface IOnNetObjReady
	{
		
		void OnNetObjReadyChange(bool ready);

	}

	public interface IOnJoinedRoom
	{
		void OnJoinedRoom();
	}

	public interface IOnAwake
	{
		void OnAwake();
	}

	public interface IOnStart
	{
		void OnStart();
	}

	public interface IOnEnable
	{
		void OnPostEnable();
	}

	public interface IOnDisable
	{
		void OnPostDisable();
	}

    /// <summary>
    /// Callback Interface for OnPreNetDestroy(). Triggered when NetObject Destroy() is called. 
    /// </summary>
    public interface IOnPreNetDestroy { void OnPreNetDestroy(NetObject roothNetObj); }

}
