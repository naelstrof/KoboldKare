// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Compression;

namespace Photon.Pun.Simple
{
	/// <summary>
	/// These callbacks are called by NetMaster to serialize data to the tick stream after the FrameId value, and before all NetObject data is written.
	/// The primary use case for this would be serialization of user inputs, but can be used for any other global, non net entity specific serialization,
	/// such as scores, timers, global events, etc.
	/// </summary>
	public interface IOnTickPreSerialization
	{
		SerializationFlags OnPreSerializeTick(int frameId, byte[] buffer, ref int bitposition);
		//SerializationFlags OnPreDeserializeTick(int frameId, byte[] buffer, ref int bitposition);
	}

    /// <summary>
    /// Callback Interface for OnPreUpdate(). Called prior to all normal Update() calls in MonoBehaviours.
    /// </summary>
	public interface IOnPreUpdate { void OnPreUpdate(); }
    /// <summary>
    /// Callback Interface for OnPostUpdate(). Called after all normal Update() calls in MonoBehaviours.
    /// </summary>
	public interface IOnPostUpdate { void OnPostUpdate(); }

    /// <summary>
    /// Callback Interface for OnPreLateUpdate(). Called prior to all normal LateUpdate() calls in MonoBehaviours.
    /// </summary>
    public interface IOnPreLateUpdate { void OnPreLateUpdate(); }
    /// <summary>
    /// Callback Interface for OnPostLateUpdate(). Called after all normal LateUpdate() calls in MonoBehaviours.
    /// </summary>
	public interface IOnPostLateUpdate { void OnPostLateUpdate(); }

    /// <summary>
    /// Callback Interface for OnPreSimulate(). Called prior to all normal FixedUpdate() calls in MonoBehaviours.
    /// </summary>
    public interface IOnPreSimulate { void OnPreSimulate(int frameId, int subFrameId); }
    /// <summary>
    /// Callback Interface for OnPostSimulate(). Called after all normal FixedUpdate() calls in MonoBehaviours.
    /// </summary>
	public interface IOnPostSimulate { void OnPostSimulate(int frameId, int subFrameId, bool isNetTick); }

    /// <summary>
    /// Callback Interface for OnIncrementFrame(). Called when NetMaster increments the current FrameId and SubFrameId.
    /// </summary>
	public interface IOnIncrementFrame { void OnIncrementFrame(int newFrameId, int newSubFrameId, int previousFrameId, int prevSubFrameId); }

    /// <summary>
    /// Callback Interface for OnPreQuit(). Triggered when NetMaster OnApplicationQuit() is called. 
    /// NetMaster's script execution order is set to fire prior to all other MonoB scripts, so OnPreQuit() will fire prior to OnApplicationQuit() on all scripts.
    /// </summary>
    public interface IOnPreQuit { void OnPreQuit(); }


    public interface IOnPostCallbackLoop { void OnPostCallback(); }

    public interface IOnTickSnapshot { bool OnSnapshot(int newTargetFrameId); }
}
