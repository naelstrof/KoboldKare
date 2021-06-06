// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------


using Photon.Compression;

/// <summary>
/// A collection of interfaces SyncObjects can implement.
/// </summary>
namespace Photon.Pun.Simple
{
	public interface ISyncAnimator { }

	
	//public delegate int ApplyOrderAction();
	
	public interface IApplyOrder
	{
		int ApplyOrder { get; }
	}
	/// <summary>
	/// Flags a ApplyOrder as adjustable in the inspector. 
	/// </summary>
	public interface IAdjustableApplyOrder : IApplyOrder
	{

	}


	/// <summary>
	/// Flags a SyncObject as needing to recieve a complete frame or manually call netObj.SyncObjSetReady() before the netObj is flagged as ready.
	/// </summary>
	public interface IReadyable
	{ 
		bool AlwaysReady { get; }
	}

	public interface IUseKeyframes
	{
		bool IsKeyframe(int frameId);
	}

	
	public interface IDeltaFrameChangeDetect : IUseKeyframes
	{
		bool UseDeltas { get; set; }
	}

	public interface ISerializationOptional : IOnNetSerialize
	{
		bool IncludeInSerialization { get; }
	}


	//public delegate SerializationFlags OnNetSerializeDelegate(int frameId, byte[] buffer, ref int bitposition);
	public interface IOnNetSerialize
	{
		SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags);
		SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival frameArrival);
        bool SkipWhenEmpty { get; }
	}

    public interface IOnCriticallyLateFrame
    {
        void HandleCriticallyLateFrame(int frameId);
    }

	//public delegate void OnSnapshotDelegate(int newTargetFrameId, bool initialize);
	public interface IOnSnapshot {  bool OnSnapshot(int pre1FrameId, int snapFrameId, int targFrameId, bool prevIsValid, bool snapIsValid, bool targIsValid); }

	//public delegate void OnInterpolateDelegate(float t);
	public interface IOnInterpolate { bool OnInterpolate(int snapFrameId, int targFrameId, float t); }

	//public delegate void OnQuantizeDelegate(int frameId, Realm realm);
	public interface IOnQuantize { void OnQuantize(int frameId); }

	//public delegate void OnCaptureCurrentValuesDelegate(int frameId, bool amActingAuthority, Realm realm);
	public interface IOnCaptureState { void OnCaptureCurrentState(int frameId); }
	public interface IOnAuthorityChanged { void OnAuthorityChanged(bool isMine, bool asServer); }

}
